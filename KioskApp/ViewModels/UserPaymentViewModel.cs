using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KioskApp.Models;
using KioskApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace KioskApp.ViewModels
{
    public partial class UserPaymentViewModel : ObservableObject
    {
        public ObservableCollection<OrderItem> OrderItems { get; }
        public int TotalPrice => OrderItems.Sum(x => x.TotalPrice);

        public Action? BackRequested { get; set; }
        public Action? HomeRequested { get; set; }
        public Action<string, string, string>? ShowQrPaymentRequested { get; set; } // payType, url, tid

        public UserPaymentViewModel(ObservableCollection<OrderItem> orderItems)
        {
            OrderItems = orderItems;
        }

        [RelayCommand]
        private async void CardPay()
        {
            System.Diagnostics.Debug.WriteLine("카드결제 커맨드 실행!");
            var orderId = await OrderService.Instance.SaveOrderAsync(OrderItems, "카드결제");
            var vm = new PaymentCompleteViewModel(orderId, "카드결제", TotalPrice);
            vm.HomeRequested = () => MainWindowViewModel.Instance.ShowHome();
            MainWindowViewModel.Instance.CurrentView = new KioskApp.Views.PaymentCompleteView { DataContext = vm };
        }

        [RelayCommand]
        private async void KakaoPay()
        {
            System.Diagnostics.Debug.WriteLine("카카오페이 커맨드 실행!");
            var result = await PaymentService.Instance.RequestKakaoPayAsync(OrderItems, TotalPrice);
            System.Diagnostics.Debug.WriteLine($"카카오페이 result.Success: {result.Success}, Message: {result.Message}, Tid: {result.Tid}");
            if (result.Success && !string.IsNullOrEmpty(result.Message))
            {
                System.Diagnostics.Debug.WriteLine("ShowQrPaymentRequested 호출!");
                ShowQrPaymentRequested?.Invoke("카카오페이", result.Message, result.Tid);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("카카오페이 결제 실패: " + result.Message);
            }
        }

        [RelayCommand]
        private void Back() => BackRequested?.Invoke();

        [RelayCommand]
        private void Home() => HomeRequested?.Invoke();
    }
}
