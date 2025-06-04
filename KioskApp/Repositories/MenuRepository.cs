using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.Sqlite;
using Dapper;
using KioskApp.Models;

namespace KioskApp.Repositories
{
    public class MenuRepository
    {
        private readonly string _connStr = "Data Source=kiosk.db";

        public List<Menu> GetAll()
        {
            using var conn = new SqliteConnection(_connStr);
            return conn.Query<Menu>("SELECT * FROM Menu").ToList();
        }

        public List<Menu> GetByCategory(int categoryId)
        {
            using var conn = new SqliteConnection(_connStr);
            return conn.Query<Menu>("SELECT * FROM Menu WHERE CategoryId = @CategoryId", new { CategoryId = categoryId }).ToList();
        }

        public void Add(Menu menu)
        {
            using var conn = new SqliteConnection(_connStr);
            conn.Execute(@"INSERT INTO Menu (CategoryId, Name, Description, Price, ImagePath) 
                           VALUES (@CategoryId, @Name, @Description, @Price, @ImagePath)", menu);
        }

        public void Update(Menu menu)
        {
            using var conn = new SqliteConnection(_connStr);
            conn.Execute(@"UPDATE Menu SET CategoryId=@CategoryId, Name=@Name, Description=@Description, Price=@Price, ImagePath=@ImagePath 
                           WHERE MenuId=@MenuId", menu);
        }

        public void Delete(int menuId)
        {
            using var conn = new SqliteConnection(_connStr);
            conn.Execute("DELETE FROM Menu WHERE MenuId=@MenuId", new { MenuId = menuId });
        }
    }
}
