using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

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
        public ObservableCollection<OrderItemViewModel> OrderItems { get; } = new();

        // 명령들
        public ICommand RemoveOrderItemCommand { get; }
        public Action BackRequested { get; set; }
        public Action PayRequested { get; set; }

        public ICommand BackCommand { get; }
        public ICommand PayCommand { get; }

        public UserOrderConfirmViewModel(IEnumerable<OrderItemViewModel> items)
        {
            foreach (var i in items)
                OrderItems.Add(i);

            foreach (var item in items)
            {
                foreach (var opt in item.SelectedOptions)
                {
                    System.Diagnostics.Debug.WriteLine($"[SelectedOption] '{opt}' ({opt?.GetType()})");
                }
            }

            RemoveOrderItemCommand = new RelayCommand<OrderItemViewModel>(RemoveOrderItem);
            BackCommand = new RelayCommand(() => BackRequested?.Invoke());
            PayCommand = new RelayCommand(() => PayRequested?.Invoke());
        }

        private void RemoveOrderItem(OrderItemViewModel item)
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