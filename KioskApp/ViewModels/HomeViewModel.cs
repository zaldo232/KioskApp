using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace KioskApp.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        public Action GoAdminRequested { get; set; }

        public Action GoOrderRequested { get; set; }

        [RelayCommand]
        private void GoAdmin()
        {
            GoAdminRequested?.Invoke();
        }

        [RelayCommand]
        private void GoOrder()
        {
            GoOrderRequested?.Invoke();
        }
    }
}
