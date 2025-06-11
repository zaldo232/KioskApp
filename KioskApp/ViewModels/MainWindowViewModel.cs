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
        // 싱글턴
        public static MainWindowViewModel Instance { get; private set; }
        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public MainWindowViewModel()
        {
            Instance = this;
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
            // 변경: PayRequested에 결제수단 화면으로!
            confirmVM.PayRequested = () => ShowPaymentView(orderItems);
            CurrentView = new Views.UserOrderConfirmView { DataContext = confirmVM };
        }

        public void ShowPaymentView(ObservableCollection<OrderItem> orderItems)
        {
            var paymentVM = new UserPaymentViewModel(orderItems);
            paymentVM.BackRequested = () => ShowOrderConfirm(orderItems); // 이전으로
            paymentVM.HomeRequested = ShowHome; // 처음으로
            //paymentVM.PaymentCompleted = ShowOrderComplete; // 결제 완료시(->홈 or 완료뷰)
            CurrentView = new Views.UserPaymentView { DataContext = paymentVM };
        }

        public void ShowOrderComplete(int orderId, string payType, int amount)
        {
            var vm = new PaymentCompleteViewModel(orderId, payType, amount);
            vm.HomeRequested = ShowHome;
            CurrentView = new Views.PaymentCompleteView { DataContext = vm };
        }

    }
}