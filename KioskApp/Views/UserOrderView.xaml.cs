using KioskApp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KioskApp.Views
{
    public partial class UserOrderView : UserControl
    {
        public UserOrderView()
        {
            InitializeComponent();
        }

        private void MenuCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is KioskApp.Models.Menu menu)
            {
                var dlg = new KioskApp.Views.OptionDialog(menu) { Owner = Window.GetWindow(this) };
                if (dlg.ShowDialog() == true && dlg.Tag is KioskApp.Views.OptionDialogResult result)
                {
                    (DataContext as UserOrderViewModel)?.AddToCart(menu, result.OptionText, result.UnitPrice, result.Quantity);
                }
            }
        }


    }
}
