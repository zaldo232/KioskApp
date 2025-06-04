using KioskApp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace KioskApp.Views
{
    public partial class AdminLoginView : UserControl
    {
        public AdminLoginView()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is KioskApp.ViewModels.AdminLoginViewModel vm)
            {
                vm.Password = pwdBox.Password;
                vm.LoginCommand.Execute(null);
            }
        }
    }

}
