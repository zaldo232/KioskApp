using KioskApp.Models;
using Microsoft.Data.Sqlite;
using Dapper;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace KioskApp.Services
{
    // 주문 저장/처리 담당 서비스 (싱글톤)
    public class OrderService
    {
        private const string ConnectionString = "Data Source=kiosk.db";
        public static OrderService Instance { get; } = new OrderService();

        // 주문과 주문 항목 전체 저장 (비동기, 트랜잭션 처리)
        // items: 주문 항목 리스트, paymentType: 결제 수단
        public async Task<int> SaveOrderAsync(ObservableCollection<OrderItem> items, string paymentType)
        {
            using var connection = new SqliteConnection(ConnectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                // 주문(Order) 테이블에 INSERT
                int totalPrice = 0;
                foreach (var item in items)
                    totalPrice += item.TotalPrice;

                string insertOrder = @"
                                    INSERT INTO [Order] (OrderTime, TotalPrice, PaymentType, Status)
                                    VALUES (@OrderTime, @TotalPrice, @PaymentType, @Status);
                                    SELECT last_insert_rowid();";
                var orderId = await connection.ExecuteScalarAsync<long>(insertOrder, new
                {
                    OrderTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    TotalPrice = totalPrice,
                    PaymentType = paymentType,
                    Status = "결제완료"
                }, transaction);

                // 주문항목(OrderItem) 테이블에 여러건 INSERT
                string insertItem = @"
                                    INSERT INTO OrderItem (OrderId, MenuId, Quantity, UnitPrice, OptionText)
                                    VALUES (@OrderId, @MenuId, @Quantity, @UnitPrice, @OptionText);";

                foreach (var item in items)
                {
                    await connection.ExecuteAsync(insertItem, new
                    {
                        OrderId = orderId,
                        MenuId = item.MenuId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        OptionText = item.OptionText
                    }, transaction);
                }

                transaction.Commit();
                return (int)orderId; // 생성된 주문ID 반환
            }
            catch
            {
                transaction.Rollback(); // 실패시 롤백
                throw;
            }
        }
    }
}
