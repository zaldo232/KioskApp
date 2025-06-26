using System.Windows;

namespace KioskApp.Views
{
    public partial class AlertDialog : Window
    {
        public AlertDialog(string message)
        {
            InitializeComponent();
            MessageText.Text = message;
        }

        public AlertDialog(string message, Window owner = null)
        {
            InitializeComponent();
            MessageText.Text = message;
            if (owner != null)
                this.Owner = owner;
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}