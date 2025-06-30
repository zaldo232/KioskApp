using Dapper;
using KioskApp.Models;
using Microsoft.Data.Sqlite;

namespace KioskApp.Repositories
{
    // 카테고리(Cateogry) 관련 DB 처리 클래스
    public class CategoryRepository
    {
        private readonly string _connStr = "Data Source=kiosk.db";
        
        // 모든 카테고리 조회
        public List<Category> GetAll()
        {
            using var conn = new SqliteConnection(_connStr);
            return conn.Query<Category>("SELECT * FROM Category").ToList();
        }
        
        // 카테고리 추가
        public void Add(string name)
        {
            using var conn = new SqliteConnection(_connStr);
            conn.Execute("INSERT INTO Category (Name) VALUES (@Name)", new { Name = name });
        }

        // 카테고리 정보 수정
        public void Update(Category cat)
        {
            using var conn = new SqliteConnection(_connStr);
            conn.Execute("UPDATE Category SET Name=@Name WHERE CategoryId=@CategoryId", cat);
        }

        // 카테고리 및 관련 데이터(메뉴, 옵션, 선택지) 전체 삭제
        public void Delete(int categoryId)
        {
            using var conn = new SqliteConnection(_connStr);

            // 이 카테고리의 모든 메뉴ID 조회
            var menuIds = conn.Query<int>("SELECT MenuId FROM Menu WHERE CategoryId = @CategoryId", new { CategoryId = categoryId }).ToList();

            // 각 메뉴ID별로 옵션/선택지까지 전부 삭제
            foreach (var menuId in menuIds)
            {
                // 메뉴의 모든 옵션ID 조회
                var optionIds = conn.Query<int>("SELECT OptionId FROM MenuOption WHERE MenuId = @MenuId", new { MenuId = menuId }).ToList();

                foreach (var optionId in optionIds)
                {
                    // 옵션의 모든 선택지 삭제
                    conn.Execute("DELETE FROM MenuOptionValue WHERE OptionId = @OptionId", new { OptionId = optionId });
                    // 옵션 삭제
                    conn.Execute("DELETE FROM MenuOption WHERE OptionId = @OptionId", new { OptionId = optionId });
                }

                // 메뉴 삭제
                conn.Execute("DELETE FROM Menu WHERE MenuId = @MenuId", new { MenuId = menuId });
            }

            // 카테고리 삭제
            conn.Execute("DELETE FROM Category WHERE CategoryId = @CategoryId", new { CategoryId = categoryId });
        }

    }
}
