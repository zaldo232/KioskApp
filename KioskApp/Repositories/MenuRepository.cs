using Dapper;
using KioskApp.Models;
using Microsoft.Data.Sqlite;

namespace KioskApp.Repositories
{
    // 메뉴(Menu) 관련 DB 처리 클래스
    public class MenuRepository
    {
        private readonly string _connStr = "Data Source=kiosk.db";

        // 전체 메뉴 리스트 조회
        public List<Menu> GetAll()
        {
            using var conn = new SqliteConnection(_connStr);
            return conn.Query<Menu>("SELECT * FROM Menu").ToList();
        }

        // 특정 카테고리의 메뉴 리스트 조회
        public List<Menu> GetByCategory(int categoryId)
        {
            using var conn = new SqliteConnection(_connStr);
            return conn.Query<Menu>("SELECT * FROM Menu WHERE CategoryId = @CategoryId", new { CategoryId = categoryId }).ToList();
        }

        // 메뉴 추가 (이미지 없으면 기본 이미지로)
        public void Add(Menu menu)
        {
            // 디폴트 이미지 경로
            string defaultImage = "Images/default.png";
            if (string.IsNullOrWhiteSpace(menu.ImagePath))
            {
                menu.ImagePath = defaultImage;
            }
            
            using var conn = new SqliteConnection(_connStr);
            conn.Execute(@"INSERT INTO Menu (CategoryId, Name, Description, Price, ImagePath) VALUES (@CategoryId, @Name, @Description, @Price, @ImagePath)", menu);
        }

        // 메뉴 정보 수정
        public void Update(Menu menu)
        {
            using var conn = new SqliteConnection(_connStr);
            conn.Execute(@"UPDATE Menu SET CategoryId=@CategoryId, Name=@Name, Description=@Description, Price=@Price, ImagePath=@ImagePath 
                           WHERE MenuId=@MenuId", menu);
        }

        // 메뉴 및 하위 옵션/선택지 모두 삭제
        public void Delete(int menuId)
        {
            using var conn = new SqliteConnection(_connStr);

            // 해당 메뉴가 포함된 모든 주문내역 삭제 (OrderItem)
            conn.Execute("DELETE FROM OrderItem WHERE MenuId = @MenuId", new { MenuId = menuId });

            // 메뉴의 모든 옵션ID 조회
            var optionIds = conn.Query<int>("SELECT OptionId FROM MenuOption WHERE MenuId = @MenuId", new { MenuId = menuId }).ToList();

            foreach (var optionId in optionIds)
            {
                // 옵션에 딸린 선택지 모두 삭제
                conn.Execute("DELETE FROM MenuOptionValue WHERE OptionId = @OptionId", new { OptionId = optionId });
                // 옵션 삭제
                conn.Execute("DELETE FROM MenuOption WHERE OptionId = @OptionId", new { OptionId = optionId });
            }

            // 메뉴 삭제
            conn.Execute("DELETE FROM Menu WHERE MenuId = @MenuId", new { MenuId = menuId });
        }
    }
}
