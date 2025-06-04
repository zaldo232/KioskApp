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
            CurrentView = new Views.HomeView { DataContext = homeVM };
        }

        public void ShowAdminLogin()
        {
            var adminLoginVM = new AdminLoginViewModel();
            adminLoginVM.LoginSucceeded += ShowAdminCategoryMenu;
            CurrentView = new Views.AdminLoginView { DataContext = adminLoginVM };
        }

        public void ShowAdminCategoryMenu()
        {
            CurrentView = new Views.AdminCategoryMenuView { DataContext = new AdminCategoryMenuViewModel() };
        }
    }
}
