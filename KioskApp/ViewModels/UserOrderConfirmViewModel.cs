using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KioskApp.Models;
using KioskApp.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace KioskApp.ViewModels
{
    // 주문 항목(뷰 표시용) 모델
    public class OrderItemViewModel : ObservableObject
    {
        public string MenuName { get; set; }                      // 메뉴명
        public ObservableCollection<string> SelectedOptions { get; set; } = new(); // 선택 옵션 리스트
        public int Quantity { get; set; }                         // 수량
        public int TotalPrice { get; set; }                       // 총 금액
    }

    // 사용자 주문 확인(장바구니) 뷰모델
    public class UserOrderConfirmViewModel : ObservableObject
    {
        // 주문 항목 리스트
        public ObservableCollection<OrderItem> OrderItems { get; }

        // 항목 삭제 커맨드
        public ICommand RemoveOrderItemCommand { get; }

        public Action HomeRequested { get; set; } // 홈 이동 콜백
        public Action BackRequested { get; set; } // 뒤로가기 콜백
        public Action PayRequested { get; set; }  // 결제진행 콜백

        public ICommand BackCommand { get; }
        public ICommand PayCommand { get; }

        // 타이머(자동 홈이동)
        private DispatcherTimer _timer;
        private int _remainSeconds = 120;
        public int RemainSeconds
        {
            get => _remainSeconds;
            set => SetProperty(ref _remainSeconds, value);
        }

        public UserOrderConfirmViewModel(ObservableCollection<OrderItem> orderItems)
        {
            OrderItems = orderItems;

            RemoveOrderItemCommand = new RelayCommand<OrderItem>(RemoveOrderItem);
            BackCommand = new RelayCommand(OnBack);
            PayCommand = new RelayCommand(OnPay);

            // 타이머 초기화 및 시작
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            StartTimer();
        }

        // 1초마다 실행되는 타이머 이벤트
        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (RemainSeconds > 0)
                RemainSeconds--;
            else
            {
                _timer.Stop();
                HomeRequested?.Invoke(); // 타임아웃시 홈으로
            }
        }

        // 타이머(2분) 시작
        public void StartTimer()
        {
            RemainSeconds = 120;
            _timer.Start();
        }

        // 타이머 중단
        public void StopTimer()
        {
            _timer.Stop();
        }

        // 주문 항목 삭제 처리
        private void RemoveOrderItem(OrderItem item)
        {
            // 메뉴가 1개뿐이면 삭제 불가(알림)
            if (OrderItems.Count <= 1)
            {
                StopTimer(); // 알림 전 타이머 중단

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var dialog = new AlertDialog("남은 메뉴가 한개 미만으로 줄어들 수 없습니다!", Application.Current.MainWindow);
                    dialog.ShowDialog();
                });

                StartTimer(); // 알림 닫힌 후 타이머 재시작
                return;
            }

            // 삭제 실행
            if (OrderItems.Contains(item))
            { 
                OrderItems.Remove(item); 
            }

            OnPropertyChanged(nameof(TotalPrice));
            StartTimer(); // 삭제 후 타이머 리셋
        }

        // 합계 금액(자동갱신)
        public int TotalPrice => OrderItems.Sum(x => x.TotalPrice);

        // 뒤로가기 처리
        private void OnBack()
        {
            StopTimer();
            BackRequested?.Invoke();
        }

        // 결제진행 처리
        private void OnPay()
        {
            StopTimer();
            PayRequested?.Invoke();
        }
    }
}
