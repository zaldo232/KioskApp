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
            // 디폴트 이미지 경로
            string defaultImage = "Images/default.png";
            if (string.IsNullOrWhiteSpace(menu.ImagePath))
                menu.ImagePath = defaultImage;

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

            // 메뉴의 모든 옵션ID 조회
            var optionIds = conn.Query<int>("SELECT OptionId FROM MenuOption WHERE MenuId = @MenuId", new { MenuId = menuId }).ToList();

            foreach (var optionId in optionIds)
            {
                // 선택지 모두 삭제
                conn.Execute("DELETE FROM MenuOptionValue WHERE OptionId = @OptionId", new { OptionId = optionId });
                // 옵션 삭제
                conn.Execute("DELETE FROM MenuOption WHERE OptionId = @OptionId", new { OptionId = optionId });
            }

            // 메뉴 삭제
            conn.Execute("DELETE FROM Menu WHERE MenuId = @MenuId", new { MenuId = menuId });
        }
    }
}
