using KioskApp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KioskApp.Views
{
    // 사용자가 메뉴 주문을 진행하는 메인 주문화면(카테고리, 메뉴, 장바구니, 주문)
    public partial class UserOrderView : UserControl
    {
        public UserOrderView()
        {
            InitializeComponent();
        }

        // 메뉴 카드 클릭 시 옵션 다이얼로그 오픈 -> 장바구니에 추가
        private void MenuCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is KioskApp.Models.Menu menu)
            {
                // 옵션 선택 다이얼로그 띄우기
                var dlg = new KioskApp.Views.OptionDialog(menu) { Owner = Window.GetWindow(this) };
                if (dlg.ShowDialog() == true && dlg.Tag is KioskApp.Views.OptionDialogResult result)
                {
                    // 선택 결과를 장바구니에 추가
                    (DataContext as UserOrderViewModel)?.AddToCart(menu, result.OptionText, result.UnitPrice, result.Quantity);
                }
            }
        }


    }
}
