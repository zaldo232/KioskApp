using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KioskApp.Repositories;

namespace KioskApp.ViewModels
{
    // 관리자 로그인 뷰모델
    public partial class AdminLoginViewModel : ObservableObject
    {
        [ObservableProperty] private string username;       // 입력 아이디
        [ObservableProperty] private string password;       // 입력 비밀번호
        [ObservableProperty] private string errorMessage;   // 에러 메시지

        public RelayCommand LoginCommand { get; }           // 로그인 커맨드

        public event Action LoginSucceeded;                 // 로그인 성공 이벤트

        // 부모(MainWindowViewModel)에서 콜백 (홈 이동)
        public Action GoHomeRequested { get; set; }
        
        // 홈 화면 이동
        [RelayCommand]
        public void GoHome()
        {
            GoHomeRequested?.Invoke();
        }

        public AdminLoginViewModel()
        {
            LoginCommand = new RelayCommand(Login);
        }

        // 로그인 시도 (아이디/비번 검증)
        private void Login()
        {
            var repo = new UserRepository();
            if (repo.ValidateAdmin(Username, Password))
            {
                LoginSucceeded?.Invoke(); // 성공시 콜백 호출(화면전환)
            }
            else
            {
                ErrorMessage = "아이디 또는 비밀번호가 잘못됨";
            }
        }
    }
}
