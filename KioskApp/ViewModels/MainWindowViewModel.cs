using CommunityToolkit.Mvvm.ComponentModel;
using KioskApp.Models;
using KioskApp.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace KioskApp.ViewModels
{
    // 메인 창 전체 흐름/화면전환 관리 뷰모델 (싱글톤)
    public class MainWindowViewModel : ObservableObject
    {
        private object _currentView;                    // 현재 보여줄 뷰(컨트롤)
        private UserOrderViewModel _userOrderViewModel; // 주문(사용자) 화면 상태 유지용
        public static MainWindowViewModel Instance { get; private set; }
        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public MainWindowViewModel()
        {
            Instance = this;
            ShowHome(); // 시작시 홈화면
        }

        // 홈(시작화면) 표시
        public void ShowHome()
        {
            _userOrderViewModel = null; // 홈 진입 시 주문상태 초기화
            var homeVM = new HomeViewModel();
            homeVM.GoAdminRequested = ShowAdminLogin;
            homeVM.GoOrderRequested = ShowUserOrder;
            CurrentView = new Views.HomeView { DataContext = homeVM };
        }

        // 관리자 로그인 화면 표시
        public void ShowAdminLogin()
        {
            var vm = new AdminLoginViewModel();
            vm.LoginSucceeded += ShowAdminCategoryMenu;
            vm.GoHomeRequested = ShowHome;
            CurrentView = new Views.AdminLoginView { DataContext = vm };
        }

        // 관리자 - 카테고리/메뉴 관리 화면 표시
        public void ShowAdminCategoryMenu()
        {
            var vm = new AdminCategoryMenuViewModel();
            vm.GoHomeRequested = ShowHome;
            vm.GoAdImageRequested = ShowAdminAdImage;
            CurrentView = new Views.AdminCategoryMenuView { DataContext = vm };
        }
        // 관리자 - 광고 이미지 관리 화면 표시
        public void ShowAdminAdImage()
        {
            var vm = new AdminAdImageViewModel();
            vm.GoHomeRequested = ShowHome;
            vm.GoMenuRequested = ShowAdminCategoryMenu;
            CurrentView = new Views.AdminAdImageView { DataContext = vm };
        }

        // 사용자 주문 화면 표시 (상태 재사용, 타이머 관리)
        public void ShowUserOrder()
        {
            if (_userOrderViewModel == null)
            {
                _userOrderViewModel = new UserOrderViewModel();
                _userOrderViewModel.GoHomeRequested = ShowHome;
                _userOrderViewModel.GoOrderConfirmRequested = ShowOrderConfirm;
            }
            else
            {
                _userOrderViewModel.StartTimer(); // 주문화면 재진입시 타이머 재시작
            }
            CurrentView = new Views.UserOrderView { DataContext = _userOrderViewModel };
        }

        // 주문내역 확인 화면 표시
        public void ShowOrderConfirm(ObservableCollection<OrderItem> orderItems)
        {
            var confirmVM = new UserOrderConfirmViewModel(orderItems);
            confirmVM.BackRequested = ShowUserOrder;
            confirmVM.PayRequested = () => ShowPaymentView(orderItems);
            confirmVM.HomeRequested = ShowHome;
            CurrentView = new Views.UserOrderConfirmView { DataContext = confirmVM };
        }

        // 결제수단 선택/결제 화면 표시
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

        // QR결제(카카오 등) 화면 표시
        public void ShowQrPaymentView(string payType, string url, string tid, ObservableCollection<OrderItem> orderItems)
        {
            Func<Task<bool>> pollPaymentStatus = payType switch
            {
                "카카오페이" => async () => await Services.PaymentService.Instance.PollKakaoPayApprovalAsync(tid),
                // "토스페이" => async () => await Services.PaymentService.Instance.PollTossPayApprovalAsync(tid), // 토스 추가시
                _ => async () => false // 기본(예외처리/지원X)
            };

            var vm = new QrPaymentViewModel(
                payType,
                url,
                pollPaymentStatus,
                async () =>
                {
                    var (orderId, todayOrderNo) = await Services.OrderService.Instance.SaveOrderWithTodayNoAsync(orderItems, payType);
                    ShowOrderComplete(orderId, todayOrderNo, payType, orderItems.Sum(x => x.TotalPrice));
                }
            );
            vm.CancelRequested = () => ShowPaymentView(orderItems);
            CurrentView = new Views.QrPaymentView { DataContext = vm };
        }

        // 결제 완료 화면 표시
        public void ShowOrderComplete(int orderId, int todayOrderNo, string payType, int amount)
        {
            var vm = new PaymentCompleteViewModel(orderId, todayOrderNo, payType, amount);
            vm.HomeRequested = ShowHome;
            CurrentView = new Views.PaymentCompleteView { DataContext = vm };
        }
    }
}
