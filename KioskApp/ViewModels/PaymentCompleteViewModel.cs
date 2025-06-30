using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace KioskApp.ViewModels
{
    // 결제 완료(주문완료) 화면 뷰모델
    public partial class PaymentCompleteViewModel : ObservableObject
    {
        public int OrderId { get; }        // 주문번호
        public string PayType { get; }     // 결제수단(예: 카카오페이/카드)
        public int Amount { get; }         // 결제 금액

        public Action? HomeRequested { get; set; } // 홈으로 이동 요청 콜백

        private bool _homeCalled = false;  // 홈 이동 이미 호출했는지 플래그

        public PaymentCompleteViewModel(int orderId, string payType, int amount)
        {
            OrderId = orderId;
            PayType = payType;
            Amount = amount;
            AutoGoHomeAsync();  // 생성시 5초 뒤 자동 홈이동
        }

        // 버튼 - 홈 이동 실행
        [RelayCommand]
        private void Home()
        {
            GoHomeOnce();
        }

        // 자동 - 5초 후 홈 이동
        private async void AutoGoHomeAsync()
        {
            await Task.Delay(5000);
            GoHomeOnce();
        }

        // 홈 이동 1회만 실행 (중복방지)
        private void GoHomeOnce()
        {
            if (_homeCalled) return; // 이미 호출됐으면 무시
            _homeCalled = true;
            HomeRequested?.Invoke();
        }
    }
}
