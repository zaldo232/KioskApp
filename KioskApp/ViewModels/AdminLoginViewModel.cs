using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KioskApp.Repositories;
using System.Windows;

namespace KioskApp.ViewModels
{
    public partial class AdminLoginViewModel : ObservableObject
    {
        [ObservableProperty] private string username;
        [ObservableProperty] private string password;
        [ObservableProperty] private string errorMessage;

        public RelayCommand LoginCommand { get; }

        public event Action LoginSucceeded;

        public AdminLoginViewModel()
        {
            LoginCommand = new RelayCommand(Login);
        }

        private void Login()
        {
            var repo = new UserRepository();
            if (repo.ValidateAdmin(Username, Password))
            {
                LoginSucceeded?.Invoke(); // 성공시 이벤트 (화면전환)
            }
            else
            {
                ErrorMessage = "아이디 또는 비밀번호가 잘못됨";
            }
        }
    }
}
