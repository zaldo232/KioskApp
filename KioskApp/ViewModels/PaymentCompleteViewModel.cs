using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace KioskApp.ViewModels
{
    public partial class PaymentCompleteViewModel : ObservableObject
    {
        public int OrderId { get; }
        public string PayType { get; }
        public int Amount { get; }

        public Action? HomeRequested { get; set; }

        private bool _homeCalled = false; // 추가: 홈이동 이미 했는지 체크

        public PaymentCompleteViewModel(int orderId, string payType, int amount)
        {
            OrderId = orderId;
            PayType = payType;
            Amount = amount;
            AutoGoHomeAsync();
        }

        [RelayCommand]
        private void Home()
        {
            GoHomeOnce();
        }

        private async void AutoGoHomeAsync()
        {
            await Task.Delay(5000);
            GoHomeOnce();
        }

        private void GoHomeOnce()
        {
            if (_homeCalled) return; // 이미 호출됐으면 무시
            _homeCalled = true;
            HomeRequested?.Invoke();
        }
    }
}
