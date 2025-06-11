using CommunityToolkit.Mvvm.ComponentModel;
using KioskApp.Models;
using KioskApp.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace KioskApp.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private object _currentView;
        private UserOrderViewModel _userOrderViewModel;
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
            confirmVM.PayRequested = () => ShowPaymentView(orderItems);
            CurrentView = new Views.UserOrderConfirmView { DataContext = confirmVM };
        }

        public void ShowPaymentView(ObservableCollection<OrderItem> orderItems)
        {
            var paymentVM = new UserPaymentViewModel(orderItems);
            paymentVM.BackRequested = () => ShowOrderConfirm(orderItems);
            paymentVM.HomeRequested = ShowHome;
            paymentVM.ShowQrPaymentRequested = (payType, url, tid) =>
            {
                ShowQrPaymentView(payType, url, tid, orderItems);
            };
            CurrentView = new Views.UserPaymentView { DataContext = paymentVM };

            System.Diagnostics.Debug.WriteLine("CurrentView UserPaymentView 할당(DataContext: " + paymentVM.GetType().FullName + ")");
        }

        public void ShowQrPaymentView(string payType, string url, string tid, ObservableCollection<OrderItem> orderItems)
        {
            var vm = new QrPaymentViewModel(
                payType,
                url,
                payType == "카카오페이"
                    ? async () => await Services.PaymentService.Instance.PollKakaoPayApprovalAsync(tid)
                    : async () => await Services.PaymentService.Instance.PollPaycoApprovalAsync(tid),
                async () =>
                {
                    var orderId = await Services.OrderService.Instance.SaveOrderAsync(orderItems, payType);
                    ShowOrderComplete(orderId, payType, orderItems.Sum(x => x.TotalPrice));
                }
            );
            vm.CancelRequested = () => ShowPaymentView(orderItems);
            CurrentView = new Views.QrPaymentView { DataContext = vm };
        }

        public void ShowOrderComplete(int orderId, string payType, int amount)
        {
            var vm = new PaymentCompleteViewModel(orderId, payType, amount);
            vm.HomeRequested = ShowHome;
            CurrentView = new Views.PaymentCompleteView { DataContext = vm };
        }
    }
}
