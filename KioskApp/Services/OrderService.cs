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
        public async Task<(int OrderId, int TodayOrderNo)> SaveOrderWithTodayNoAsync(ObservableCollection<OrderItem> items, string paymentType)
        {
            using var connection = new SqliteConnection(ConnectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                int totalPrice = items.Sum(item => item.TotalPrice);

                // 오늘 날짜 기준 오늘의 주문번호 계산
                var today = DateTime.Today.ToString("yyyy-MM-dd");
                var todayCount = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM [Order] WHERE OrderTime >= @Start",
                    new { Start = today + " 00:00:00" }, transaction
                );
                int todayOrderNo = todayCount + 1;

                // 주문(Order) 테이블에 INSERT (TodayOrderNo 추가)
                string insertOrder = @"
                                    INSERT INTO [Order] (OrderTime, TotalPrice, PaymentType, Status, TodayOrderNo)
                                    VALUES (@OrderTime, @TotalPrice, @PaymentType, @Status, @TodayOrderNo);
                                    SELECT last_insert_rowid();";
                var orderId = await connection.ExecuteScalarAsync<long>(insertOrder, new
                {
                    OrderTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    TotalPrice = totalPrice,
                    PaymentType = paymentType,
                    Status = "결제완료",
                    TodayOrderNo = todayOrderNo
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
                return ((int)orderId, todayOrderNo); // ★ orderId와 todayOrderNo 둘 다 반환!
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

    }
}
