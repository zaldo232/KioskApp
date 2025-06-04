using System;
using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;
using Dapper;

namespace KioskApp.Services
{
    public static class DbInitializer
    {
        private const string DbFile = "kiosk.db"; // 실행파일 기준 위치
        private const string ConnectionString = "Data Source=" + DbFile;

        public static void Initialize()
        {
            if (!File.Exists(DbFile))
            {
                CreateDatabaseAndTables();
                InsertSampleData();
            }
        }

        private static void CreateDatabaseAndTables()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var sql = @"
            CREATE TABLE IF NOT EXISTS Category (
                CategoryId INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Menu (
                MenuId INTEGER PRIMARY KEY AUTOINCREMENT,
                CategoryId INTEGER NOT NULL,
                Name TEXT NOT NULL,
                Description TEXT,
                Price INTEGER NOT NULL,
                ImagePath TEXT,
                FOREIGN KEY(CategoryId) REFERENCES Category(CategoryId)
            );

            CREATE TABLE IF NOT EXISTS [Order] (
                OrderId INTEGER PRIMARY KEY AUTOINCREMENT,
                OrderTime TEXT NOT NULL,
                TotalPrice INTEGER NOT NULL,
                PaymentType TEXT,
                Status TEXT
            );

            CREATE TABLE IF NOT EXISTS OrderItem (
                OrderItemId INTEGER PRIMARY KEY AUTOINCREMENT,
                OrderId INTEGER NOT NULL,
                MenuId INTEGER NOT NULL,
                Quantity INTEGER NOT NULL,
                UnitPrice INTEGER NOT NULL,
                OptionText TEXT,
                FOREIGN KEY(OrderId) REFERENCES [Order](OrderId),
                FOREIGN KEY(MenuId) REFERENCES Menu(MenuId)
            );

            CREATE TABLE IF NOT EXISTS User (
                UserId INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL,
                PasswordHash TEXT NOT NULL,
                IsAdmin INTEGER NOT NULL DEFAULT 0
            );";

            connection.Execute(sql);
        }

        private static void InsertSampleData()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            // 샘플 카테고리
            connection.Execute("INSERT INTO Category (Name) VALUES (@Name);",
                new[] {
                    new { Name = "버거" },
                    new { Name = "사이드" },
                    new { Name = "음료" }
                });

            // 샘플 메뉴
            connection.Execute("INSERT INTO Menu (CategoryId, Name, Description, Price, ImagePath) VALUES (@CategoryId, @Name, @Description, @Price, @ImagePath);",
                new[] {
                    new { CategoryId = 1, Name = "치즈버거", Description = "치즈 가득 버거", Price = 5000, ImagePath = "burger_cheese.png" },
                    new { CategoryId = 1, Name = "불고기버거", Description = "달콤 불고기 소스", Price = 5200, ImagePath = "burger_bulgogi.png" },
                    new { CategoryId = 2, Name = "감자튀김", Description = "바삭한 감자", Price = 2000, ImagePath = "side_fries.png" },
                    new { CategoryId = 3, Name = "콜라", Description = "시원한 콜라", Price = 1800, ImagePath = "drink_coke.png" }
                });

            // 샘플 관리자
            string adminHash = BCrypt.Net.BCrypt.HashPassword("admin123"); // 비밀번호는 BCrypt 패키지 사용 추천
            connection.Execute("INSERT INTO User (Username, PasswordHash, IsAdmin) VALUES (@Username, @PasswordHash, 1);",
                new { Username = "admin", PasswordHash = adminHash });
        }
    }
}
