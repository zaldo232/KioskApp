namespace KioskApp.Models
{
    // 사용자(관리자) 정보 클래스
    public class User
    {
        public int UserId { get; set; }          // 사용자 고유 ID
        public string Username { get; set; }     // 사용자명(로그인 ID)
        public string PasswordHash { get; set; } // 비밀번호 해시값
        public bool IsAdmin { get; set; }        // 관리자 여부
    }
}
