using CommunityToolkit.Mvvm.ComponentModel;
using KioskApp.Models;
using KioskApp.Views;
using System.Collections.ObjectModel;

namespace KioskApp.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public MainWindowViewModel()
        {
            ShowHome();
        }

        public void ShowHome()
        {
            var homeVM = new HomeViewModel();
            homeVM.GoAdminRequested = ShowAdminLogin;
            homeVM.GoOrderRequested = ShowUserOrder;
            CurrentView = new Views.HomeView { DataContext = homeVM };
        }

        public void ShowAdminLogin()
        {
            var vm = new AdminLoginViewModel();
            vm.LoginSucceeded += ShowAdminCategoryMenu;
            vm.GoHomeRequested = ShowHome;
            CurrentView = new Views.AdminLoginView { DataContext = vm };
        }

        public void ShowAdminCategoryMenu()
        {
            var vm = new AdminCategoryMenuViewModel();
            vm.GoHomeRequested = ShowHome;
            CurrentView = new Views.AdminCategoryMenuView { DataContext = vm };
        }

        public void ShowUserOrder()
        {
            var vm = new UserOrderViewModel();
            vm.GoHomeRequested = ShowHome;
            vm.GoOrderConfirmRequested = ShowOrderConfirm; // 추가
            CurrentView = new Views.UserOrderView { DataContext = vm };
        }

        public void ShowOrderConfirm(ObservableCollection<OrderItem> orderItems)
        {
            var list = orderItems.Select(x => new OrderItemViewModel
            {
                MenuName = x.MenuName,
                SelectedOptions = new ObservableCollection<string>(
                    (x.OptionText ?? "")
                        .Split(',')
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                ),
                Quantity = x.Quantity,
                TotalPrice = x.UnitPrice * x.Quantity
            }).ToList();

            // ★ 여기서 실제 값을 로그로 출력!
            foreach (var item in list)
            {
                foreach (var opt in item.SelectedOptions)
                {
                    System.Diagnostics.Debug.WriteLine($"[옵션실제값] '{opt}' ({opt?.GetType()})");
                }
            }

            var confirmVM = new UserOrderConfirmViewModel(list);
            // ...
            confirmVM.BackRequested = ShowUserOrder;
            confirmVM.PayRequested = ShowOrderComplete;
            CurrentView = new Views.UserOrderConfirmView { DataContext = confirmVM };
        }


        public void ShowOrderComplete()
        {
            // 결제완료 뷰로 전환 또는 홈으로
            ShowHome();
        }

    }
}