using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace KioskApp.ViewModels
{
    // 홈 화면 뷰모델 (메인 진입점)
    public partial class HomeViewModel : ObservableObject
    {
        public Action GoAdminRequested { get; set; }   // 관리자 로그인 이동 콜백
        public Action GoOrderRequested { get; set; }   // 주문 시작 이동 콜백

        // 관리자 로그인 화면 이동
        [RelayCommand]
        private void GoAdmin()
        {
            GoAdminRequested?.Invoke();
        }

        // 주문 시작 화면 이동
        [RelayCommand]
        private void GoOrder()
        {
            GoOrderRequested?.Invoke();
        }
    }
}
