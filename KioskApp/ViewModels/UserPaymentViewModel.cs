using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KioskApp.Models;
using KioskApp.Services;
using KioskApp.Views;
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
        public Action? PaymentCompleted { get; set; }

        // 커맨드 내부에서 호출
        private void OnBack() => BackRequested?.Invoke();
        private void OnHome() => HomeRequested?.Invoke();
        private void OnPaymentCompleted() => PaymentCompleted?.Invoke();


        public UserPaymentViewModel(ObservableCollection<OrderItem> orderItems)
        {
            OrderItems = orderItems;
        }

        // 카드 결제
        [RelayCommand]
        private async void CardPay()
        {
            var orderId = await OrderService.Instance.SaveOrderAsync(OrderItems, "카드결제");
            var vm = new PaymentCompleteViewModel(orderId, "카드결제", TotalPrice);
            vm.HomeRequested = () => MainWindowViewModel.Instance.ShowHome();
            MainWindowViewModel.Instance.CurrentView = new PaymentCompleteView { DataContext = vm };
        }

        // 카카오페이 결제
        [RelayCommand]
        private async void KakaoPay()
        {
            var result = await PaymentService.Instance.RequestKakaoPayAsync(OrderItems, TotalPrice);
            if (result.Success)
            {
                var orderId = await OrderService.Instance.SaveOrderAsync(OrderItems, "카카오페이");
                var vm = new PaymentCompleteViewModel(orderId, "카카오페이", TotalPrice);
                vm.HomeRequested = () => MainWindowViewModel.Instance.ShowHome();
                MainWindowViewModel.Instance.CurrentView = new PaymentCompleteView { DataContext = vm };
            }
            // else: 실패처리
        }

        // 페이코 결제
        [RelayCommand]
        private async void PaycoPay()
        {
            var result = await PaymentService.Instance.RequestPaycoAsync(OrderItems, TotalPrice);
            if (result.Success)
            {
                var orderId = await OrderService.Instance.SaveOrderAsync(OrderItems, "페이코");
                var vm = new PaymentCompleteViewModel(orderId, "페이코", TotalPrice);
                vm.HomeRequested = () => MainWindowViewModel.Instance.ShowHome();
                MainWindowViewModel.Instance.CurrentView = new PaymentCompleteView { DataContext = vm };
            }
            // else: 실패처리
        }

        [RelayCommand]
        private void Back() => OnBack();

        [RelayCommand]
        private void Home() => OnHome();
    }
}
