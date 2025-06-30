using Dapper;
using KioskApp.Models;
using Microsoft.Data.Sqlite;

namespace KioskApp.Repositories
{
    // 사용자(관리자) 인증 관련 DB 처리 클래스
    public class UserRepository
    {
        private readonly string _connStr = "Data Source=kiosk.db";

        // 관리자 계정 유효성 검증 (비밀번호 해시 비교)
        public bool ValidateAdmin(string username, string password)
        {
            using var conn = new SqliteConnection(_connStr);
            
            // 관리자 계정 조회 (IsAdmin=1)
            var user = conn.QueryFirstOrDefault<User>(
                "SELECT * FROM User WHERE Username=@Username AND IsAdmin=1",
                new { Username = username }
            );
            if (user == null) return false;

            // 비밀번호 해시 비교
            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }
    }
}
