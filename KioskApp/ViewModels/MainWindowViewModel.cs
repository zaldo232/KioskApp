using CommunityToolkit.Mvvm.ComponentModel;
using KioskApp.Models;
using KioskApp.Views;
using System.Collections.ObjectModel;

namespace KioskApp.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private object _currentView;
        private UserOrderViewModel _userOrderViewModel;

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
            _userOrderViewModel = null; // 홈으로 갈 때만 주문 상태 완전 초기화
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
            if (_userOrderViewModel == null)
            {
                _userOrderViewModel = new UserOrderViewModel();
                _userOrderViewModel.GoHomeRequested = ShowHome;
                _userOrderViewModel.GoOrderConfirmRequested = ShowOrderConfirm;
            }
            CurrentView = new Views.UserOrderView { DataContext = _userOrderViewModel };
        }

        public void ShowOrderConfirm(ObservableCollection<OrderItem> orderItems)
        {
            var confirmVM = new UserOrderConfirmViewModel(orderItems);
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