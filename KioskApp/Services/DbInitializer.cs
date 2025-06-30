using System;
using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;
using Dapper;

namespace KioskApp.Services
{
    // DB 파일 생성 및 테이블/샘플데이터 초기화 담당
    public static class DbInitializer
    {
        private const string DbFile = "kiosk.db"; // 실행파일 기준 위치
        private const string ConnectionString = "Data Source=" + DbFile;

        // DB,테이블,샘플데이터 생성 (최초 1회만)
        public static void Initialize()
        {
            if (!File.Exists(DbFile))
            {
                CreateDatabaseAndTables();
                InsertSampleData();
            }
        }

        // 테이블 전체 생성 쿼리
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
                        );

                        CREATE TABLE IF NOT EXISTS MenuOption (
                            OptionId INTEGER PRIMARY KEY AUTOINCREMENT,
                            MenuId INTEGER NOT NULL,
                            OptionName TEXT NOT NULL,
                            IsRequired INTEGER NOT NULL DEFAULT 0,
                            FOREIGN KEY(MenuId) REFERENCES Menu(MenuId)
                        );

                        CREATE TABLE IF NOT EXISTS MenuOptionValue (
                            OptionValueId INTEGER PRIMARY KEY AUTOINCREMENT,
                            OptionId INTEGER NOT NULL,
                            ValueLabel TEXT NOT NULL,
                            ExtraPrice INTEGER NOT NULL DEFAULT 0,  
                            ImagePath TEXT,
                            FOREIGN KEY(OptionId) REFERENCES MenuOption(OptionId)
                        );

            ";

            connection.Execute(sql);
        }

        // 샘플 데이터(최초 관리자 계정) 삽입
        private static void InsertSampleData()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            // 샘플 관리자 계정 추가 (비밀번호: admin123)
            string adminHash = BCrypt.Net.BCrypt.HashPassword("admin123"); // 비밀번호는 BCrypt 패키지 사용 추천
            connection.Execute("INSERT INTO User (Username, PasswordHash, IsAdmin) VALUES (@Username, @PasswordHash, 1);",
                new { Username = "admin", PasswordHash = adminHash });
        }
    }
}
