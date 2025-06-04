using System.Linq;
using Microsoft.Data.Sqlite;
using Dapper;
using KioskApp.Models;

namespace KioskApp.Repositories
{
    public class UserRepository
    {
        private readonly string _connStr = "Data Source=kiosk.db";

        public bool ValidateAdmin(string username, string password)
        {
            using var conn = new SqliteConnection(_connStr);
            var user = conn.QueryFirstOrDefault<User>("SELECT * FROM User WHERE Username=@Username AND IsAdmin=1", new { Username = username });
            if (user == null) return false;
            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }
    }
}
