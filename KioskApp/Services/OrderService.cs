using KioskApp.Models;
using Microsoft.Data.Sqlite;
using Dapper;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace KioskApp.Services
{
    public class OrderService
    {
        private const string ConnectionString = "Data Source=kiosk.db";
        public static OrderService Instance { get; } = new OrderService();

        // 주문/주문항목 저장. 결제종류(paymentType) 포함
        public async Task<int> SaveOrderAsync(ObservableCollection<OrderItem> items, string paymentType)
        {
            using var connection = new SqliteConnection(ConnectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. 주문(Order) 테이블에 INSERT
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

                // 2. 주문항목(OrderItem) 테이블에 여러건 INSERT
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
                return (int)orderId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
