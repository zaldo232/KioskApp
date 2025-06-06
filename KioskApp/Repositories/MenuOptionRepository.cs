// Repositories/MenuOptionRepository.cs
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using Dapper;
using KioskApp.Models;

namespace KioskApp.Repositories
{
    public class MenuOptionRepository
    {
        private readonly string _connStr = "Data Source=kiosk.db";

        public List<MenuOption> GetByMenuId(int menuId)
        {
            using var conn = new SqliteConnection(_connStr);
            var options = conn.Query<MenuOption>(
                "SELECT * FROM MenuOption WHERE MenuId = @MenuId", new { MenuId = menuId }).ToList();

            foreach (var option in options)
            {
                var values = conn.Query<MenuOptionValue>(
                    "SELECT * FROM MenuOptionValue WHERE OptionId = @OptionId", new { OptionId = option.OptionId }).ToList();
                option.Values = new System.Collections.ObjectModel.ObservableCollection<MenuOptionValue>(values);
            }
            return options;
        }

        public int Add(MenuOption option)
        {
            using var conn = new SqliteConnection(_connStr);
            var sql = "INSERT INTO MenuOption (MenuId, OptionName, IsRequired) VALUES (@MenuId, @OptionName, @IsRequired); SELECT last_insert_rowid();";
            return conn.ExecuteScalar<int>(sql, option);
        }

        public void Update(MenuOption option)
        {
            using var conn = new SqliteConnection(_connStr);
            conn.Execute("UPDATE MenuOption SET OptionName=@OptionName, IsRequired=@IsRequired WHERE OptionId=@OptionId", option);
        }

        public void Delete(int optionId)
        {
            using var conn = new SqliteConnection(_connStr);
            // 옵션 값 먼저 삭제
            conn.Execute("DELETE FROM MenuOptionValue WHERE OptionId = @OptionId", new { OptionId = optionId });
            // 옵션 삭제
            conn.Execute("DELETE FROM MenuOption WHERE OptionId = @OptionId", new { OptionId = optionId });
        }

        public int AddValue(MenuOptionValue value)
        {
            using var conn = new SqliteConnection(_connStr);
            var sql = @"INSERT INTO MenuOptionValue (OptionId, ValueLabel, ExtraPrice, ImagePath) VALUES (@OptionId, @ValueLabel, @ExtraPrice, @ImagePath); SELECT last_insert_rowid();";
            return conn.ExecuteScalar<int>(sql, value);
        }

        public void DeleteValue(int optionValueId)
        {
            using var conn = new SqliteConnection(_connStr);
            conn.Execute("DELETE FROM MenuOptionValue WHERE OptionValueId = @OptionValueId", new { OptionValueId = optionValueId });
        }
    }
}
