using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KioskApp.Models;
using KioskApp.ViewModels;

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
                var dlg = new KioskApp.Views.OptionDialog(menu)
                {
                    Owner = Window.GetWindow(this)
                };
                if (dlg.ShowDialog() == true)
                {
                    // 장바구니에 옵션/수량 포함해서 추가
                    (DataContext as UserOrderViewModel)?.AddToCart(menu, dlg.SelectedSize, dlg.Quantity);
                }
            }
        }

    }
}
