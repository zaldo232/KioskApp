using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using Dapper;
using KioskApp.Models;

namespace KioskApp.Repositories
{
    // 메뉴 옵션/선택지 관련 DB 처리 클래스
    public class MenuOptionRepository
    {
        private readonly string _connStr = "Data Source=kiosk.db";

        // 특정 메뉴의 모든 옵션(+옵션별 선택지) 조회
        public List<MenuOption> GetByMenuId(int menuId)
        {
            using var conn = new SqliteConnection(_connStr);
            var options = conn.Query<MenuOption>("SELECT * FROM MenuOption WHERE MenuId = @MenuId", new { MenuId = menuId }).ToList();

            foreach (var option in options)
            {
                // 옵션별로 선택지 리스트도 조회
                var values = conn.Query<MenuOptionValue>("SELECT * FROM MenuOptionValue WHERE OptionId = @OptionId", new { OptionId = option.OptionId }).ToList();
                option.Values = new System.Collections.ObjectModel.ObservableCollection<MenuOptionValue>(values);
            }
            return options;
        }

        // 옵션 추가, 새 OptionId 반환
        public int Add(MenuOption option)
        {
            using var conn = new SqliteConnection(_connStr);
            var sql = "INSERT INTO MenuOption (MenuId, OptionName, IsRequired) VALUES (@MenuId, @OptionName, @IsRequired); SELECT last_insert_rowid();";
            return conn.ExecuteScalar<int>(sql, option);
        }

        // 옵션 정보 수정
        public void Update(MenuOption option)
        {
            using var conn = new SqliteConnection(_connStr);
            conn.Execute("UPDATE MenuOption SET OptionName=@OptionName, IsRequired=@IsRequired WHERE OptionId=@OptionId", option);
        }

        // 옵션 삭제(하위 선택지도 같이 삭제)
        public void Delete(int optionId)
        {
            using var conn = new SqliteConnection(_connStr);
            // 옵션 값 먼저 삭제
            conn.Execute("DELETE FROM MenuOptionValue WHERE OptionId = @OptionId", new { OptionId = optionId });
            // 옵션 삭제
            conn.Execute("DELETE FROM MenuOption WHERE OptionId = @OptionId", new { OptionId = optionId });
        }

        // 옵션 값(선택지) 추가, 새 OptionValueId 반환
        public int AddValue(MenuOptionValue value)
        {
            // 이미지 경로가 없으면 디폴트 이미지 사용
            string defaultImage = "Images/default.png";
            if (string.IsNullOrWhiteSpace(value.ImagePath))
            {
                value.ImagePath = defaultImage;
            }
            
            using var conn = new SqliteConnection(_connStr);
            var sql = @"INSERT INTO MenuOptionValue (OptionId, ValueLabel, ExtraPrice, ImagePath) 
                        VALUES (@OptionId, @ValueLabel, @ExtraPrice, @ImagePath); 
                        SELECT last_insert_rowid();";
            return conn.ExecuteScalar<int>(sql, value);
        }

        // 옵션 값(선택지) 삭제
        public void DeleteValue(int optionValueId)
        {
            using var conn = new SqliteConnection(_connStr);
            conn.Execute("DELETE FROM MenuOptionValue WHERE OptionValueId = @OptionValueId", new { OptionValueId = optionValueId });
        }
    }
}
