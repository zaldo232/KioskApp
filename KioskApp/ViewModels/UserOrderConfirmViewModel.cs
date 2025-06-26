using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using KioskApp.Models;
using System.Windows.Threading;
using System;
using KioskApp.Views;

namespace KioskApp.ViewModels
{
    public class OrderItemViewModel : ObservableObject
    {
        public string MenuName { get; set; }
        public ObservableCollection<string> SelectedOptions { get; set; } = new();
        public int Quantity { get; set; }
        public int TotalPrice { get; set; }
    }

    public class UserOrderConfirmViewModel : ObservableObject
    {
        // 주문 항목 리스트
        public ObservableCollection<OrderItem> OrderItems { get; }

        // 명령들
        public ICommand RemoveOrderItemCommand { get; }

        public Action HomeRequested { get; set; }
        public Action BackRequested { get; set; }
        public Action PayRequested { get; set; }

        public ICommand BackCommand { get; }
        public ICommand PayCommand { get; }

        // 타이머 관련
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

        // 타이머 Tick
        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (RemainSeconds > 0)
                RemainSeconds--;
            else
            {
                _timer.Stop();
                HomeRequested?.Invoke(); // 타임아웃 시 이전으로
            }
        }

        public void StartTimer()
        {
            RemainSeconds = 15;
            _timer.Start();
        }

        public void StopTimer()
        {
            _timer.Stop();
        }

        private void RemoveOrderItem(OrderItem item)
        {
            // 메뉴가 1개뿐이면 삭제 막기
            if (OrderItems.Count <= 1)
            {
                StopTimer(); // 알림 보기 전 타이머 멈춤

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
                OrderItems.Remove(item);

            OnPropertyChanged(nameof(TotalPrice));
            StartTimer(); // 타이머 리셋
        }

        // 합계 바인딩 (자동 갱신)
        public int TotalPrice => OrderItems.Sum(x => x.TotalPrice);

        private void OnBack()
        {
            StopTimer();
            BackRequested?.Invoke();
        }
        private void OnPay()
        {
            StopTimer();
            PayRequested?.Invoke();
        }
    }
}
