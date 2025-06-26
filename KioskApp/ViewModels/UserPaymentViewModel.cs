using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KioskApp.Models;
using KioskApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading; // 타이머용

namespace KioskApp.ViewModels
{
    public partial class UserPaymentViewModel : ObservableObject
    {
        public ObservableCollection<OrderItem> OrderItems { get; }
        public int TotalPrice => OrderItems.Sum(x => x.TotalPrice);

        // 타이머 관련
        private DispatcherTimer _timer;
        private int _remainSeconds = 120;
        public int RemainSeconds
        {
            get => _remainSeconds;
            set => SetProperty(ref _remainSeconds, value);
        }

        public Action? BackRequested { get; set; }
        public Action? HomeRequested { get; set; }
        public Action<string, string, string>? ShowQrPaymentRequested { get; set; } // payType, url, tid

        public UserPaymentViewModel(ObservableCollection<OrderItem> orderItems)
        {
            OrderItems = orderItems;

            // 타이머 셋업
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) =>
            {
                if (RemainSeconds > 0)
                    RemainSeconds--;
                else
                {
                    _timer.Stop();
                    HomeRequested?.Invoke(); // 시간초과 시 처음으로 이동
                }
            };
            StartTimer();
        }

        public void StartTimer()
        {
            RemainSeconds = 120;
            _timer.Start();
        }
        public void StopTimer() => _timer.Stop();

        [RelayCommand]
        private async void CardPay()
        {
            StopTimer(); // 결제 진입 시 타이머 완전 멈춤
            System.Diagnostics.Debug.WriteLine("카드결제 커맨드 실행!");
            var orderId = await OrderService.Instance.SaveOrderAsync(OrderItems, "카드결제");
            var vm = new PaymentCompleteViewModel(orderId, "카드결제", TotalPrice);
            vm.HomeRequested = () => MainWindowViewModel.Instance.ShowHome();
            MainWindowViewModel.Instance.CurrentView = new KioskApp.Views.PaymentCompleteView { DataContext = vm };
        }

        [RelayCommand]
        private async void KakaoPay()
        {
            StopTimer(); // 결제 진입 시 타이머 완전 멈춤
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
                // 실패시 타이머 재시작할지, 메시지 보여줄지는 정책에 따라 추가!
                StartTimer(); // 원하면 실패시 타이머 다시 흐르게
            }
        }

        [RelayCommand]
        private void Back()
        {
            StopTimer(); // 뒤로가기 눌러도 타이머 멈추는게 일반적 (화면 전환시)
            BackRequested?.Invoke();
        }

        [RelayCommand]
        private void Home()
        {
            StopTimer(); // 처음으로 눌러도 타이머 정지
            HomeRequested?.Invoke();
        }
    }
}
