using System.Windows.Controls;

namespace KioskApp.Views
{
    // 주문 결제수단 선택 화면(카드, 카카오페이 등)
    public partial class UserPaymentView : UserControl
    {
        public UserPaymentView()
        {
            InitializeComponent();
            // 디버깅용: 현재 DataContext가 어떤 ViewModel인지 로그 출력
            System.Diagnostics.Debug.WriteLine("UserPaymentView DataContext: " + (this.DataContext?.GetType().FullName ?? "null"));

        }
    }
}
