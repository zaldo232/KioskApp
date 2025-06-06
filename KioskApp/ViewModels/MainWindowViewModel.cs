using CommunityToolkit.Mvvm.ComponentModel;
using KioskApp.Views;

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
            CurrentView = new Views.UserOrderView { DataContext = vm };
        }



    }
}
