using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.Sqlite;
using Dapper;
using KioskApp.Models;

namespace KioskApp.Repositories
{
    public class CategoryRepository
    {
        private readonly string _connStr = "Data Source=kiosk.db";

        public List<Category> GetAll()
        {
            using var conn = new SqliteConnection(_connStr);
            return conn.Query<Category>("SELECT * FROM Category").ToList();
        }

        public void Add(string name)
        {
            using var conn = new SqliteConnection(_connStr);
            conn.Execute("INSERT INTO Category (Name) VALUES (@Name)", new { Name = name });
        }

        public void Update(Category cat)
        {
            using var conn = new SqliteConnection(_connStr);
            conn.Execute("UPDATE Category SET Name=@Name WHERE CategoryId=@CategoryId", cat);
        }

        public void Delete(int categoryId)
        {
            using var conn = new SqliteConnection(_connStr);
            // 먼저 메뉴들 삭제
            conn.Execute("DELETE FROM Menu WHERE CategoryId = @CategoryId", new { CategoryId = categoryId });
            // 카테고리 삭제
            conn.Execute("DELETE FROM Category WHERE CategoryId = @CategoryId", new { CategoryId = categoryId });
        }

    }
}
