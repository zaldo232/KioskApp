using KioskApp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace KioskApp.Views
{
    // 관리자 로그인 화면 코드비하인드
    public partial class AdminLoginView : UserControl
    {
        public AdminLoginView()
        {
            InitializeComponent();
        }

        // 로그인 버튼 클릭 이벤트 핸들러
        // PasswordBox는 MVVM 바인딩이 안 되므로 직접 ViewModel에 비밀번호 할당 후 Command 실행
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is KioskApp.ViewModels.AdminLoginViewModel vm)
            {
                vm.Password = pwdBox.Password;          // 비밀번호 직접 할당
                vm.LoginCommand.Execute(null);           // 로그인 커맨드 실행
            }
        }
    }

}
