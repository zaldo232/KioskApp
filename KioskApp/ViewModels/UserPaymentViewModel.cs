using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KioskApp.Models;
using KioskApp.Services;
using System.Collections.ObjectModel;
using System.Windows.Threading; // 타이머용

namespace KioskApp.ViewModels
{
    // 사용자 결제(카드/카카오) 화면 뷰모델
    public partial class UserPaymentViewModel : ObservableObject
    {
        public ObservableCollection<OrderItem> OrderItems { get; }     // 주문 항목(장바구니)
        public int TotalPrice => OrderItems.Sum(x => x.TotalPrice);    // 결제 총액

        // 타이머(2분)
        private DispatcherTimer _timer;
        private int _remainSeconds = 120;
        public int RemainSeconds
        {
            get => _remainSeconds;
            set => SetProperty(ref _remainSeconds, value);
        }

        // 화면 전환용 콜백
        public Action? BackRequested { get; set; }
        public Action? HomeRequested { get; set; }
        public Action<string, string, string>? ShowQrPaymentRequested { get; set; } // payType, url, tid

        public UserPaymentViewModel(ObservableCollection<OrderItem> orderItems)
        {
            OrderItems = orderItems;

            // 타이머 초기화/시작
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
        // 타이머 시작
        public void StartTimer()
        {
            RemainSeconds = 120;
            _timer.Start();
        }

        // 타이머 멈춤
        public void StopTimer() => _timer.Stop();

        // 카드 결제(바로 완료) -> 단말기 없어서 임시로 구현
        [RelayCommand]
        private async void CardPay()
        {
            StopTimer(); // 결제 시작시 타이머 정지
            System.Diagnostics.Debug.WriteLine("카드결제 커맨드 실행!");
            var (orderId, todayOrderNo) = await OrderService.Instance.SaveOrderWithTodayNoAsync(OrderItems, "카드결제");
            var vm = new PaymentCompleteViewModel(orderId, todayOrderNo, "카드결제", TotalPrice);
            vm.HomeRequested = () => MainWindowViewModel.Instance.ShowHome();
            MainWindowViewModel.Instance.CurrentView = new KioskApp.Views.PaymentCompleteView { DataContext = vm };
        }

        // 카카오페이 결제(결제창-QR 흐름)
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
                // 실패시 타이머 재시작/메시지 표시 정책은 상황에 맞게 추가
                StartTimer(); // 실패시 타이머 재가동
            }
        }

        // [뒤로가기] 버튼
        [RelayCommand]
        private void Back()
        {
            StopTimer();    // 화면 전환시 타이머 멈춤
            BackRequested?.Invoke();
        }

        // [처음으로] 버튼
        [RelayCommand]
        private void Home()
        {
            StopTimer(); // 처음으로 눌러도 타이머 정지
            HomeRequested?.Invoke();
        }
    }
}
