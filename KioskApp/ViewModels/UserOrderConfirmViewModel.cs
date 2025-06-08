using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using KioskApp.Models;

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
        public Action BackRequested { get; set; }
        public Action PayRequested { get; set; }

        public ICommand BackCommand { get; }
        public ICommand PayCommand { get; }

        public UserOrderConfirmViewModel(ObservableCollection<OrderItem> orderItems)
        {
            OrderItems = orderItems;

            RemoveOrderItemCommand = new RelayCommand<OrderItem>(RemoveOrderItem);
            BackCommand = new RelayCommand(() => BackRequested?.Invoke());
            PayCommand = new RelayCommand(() => PayRequested?.Invoke());
        }

        private void RemoveOrderItem(OrderItem item)
        {
            if (OrderItems.Contains(item))
                OrderItems.Remove(item);
            OnPropertyChanged(nameof(TotalPrice));
        }

        // 합계 바인딩 (자동 갱신)
        public int TotalPrice => OrderItems.Sum(x => x.TotalPrice);

        // 실제 이동/결제 로직은 외부에서 DI 혹은 콜백으로 처리 가능
        private void OnBack()
        {
            // ex: MainViewModel.CurrentView = new UserOrderViewModel();
        }
        private void OnPay()
        {
            // ex: 결제/주문 완료 처리
        }
    }
}