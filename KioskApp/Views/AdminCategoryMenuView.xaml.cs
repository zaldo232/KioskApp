using KioskApp.ViewModels;
using System.Windows.Controls;

namespace KioskApp.Views
{
    public partial class AdminCategoryMenuView : UserControl
    {
        public AdminCategoryMenuView()
        {
            InitializeComponent();
            this.DataContext = new AdminCategoryMenuViewModel();
        }
    }
}