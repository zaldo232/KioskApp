using System.Windows.Controls;

namespace KioskApp.Views
{
    public partial class UserPaymentView : UserControl
    {
        public UserPaymentView()
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("UserPaymentView DataContext: " + (this.DataContext?.GetType().FullName ?? "null"));

        }
    }
}
