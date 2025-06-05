using System.Collections.Generic;
using System.Windows;
using KioskApp.Models;

namespace KioskApp.Views
{
    public partial class OptionDialog : Window
    {
        public Menu Menu { get; }
        public List<string> SizeOptions { get; } = new List<string> { "Small", "Medium", "Large" };
        public string SelectedSize { get; set; }
        public int Quantity { get; set; } = 1;

        public string OptionText => $"사이즈:{SelectedSize}";

        public OptionDialog(Menu menu)
        {
            InitializeComponent();
            Menu = menu;
            SelectedSize = SizeOptions[1];
            DataContext = this;
        }

        private void DecreaseQty(object sender, RoutedEventArgs e)
        {
            if (Quantity > 1) Quantity--;
            DataContext = null; DataContext = this;
        }

        private void IncreaseQty(object sender, RoutedEventArgs e)
        {
            Quantity++;
            DataContext = null; DataContext = this;
        }

        private void AddToCart(object sender, RoutedEventArgs e)
        {
            DialogResult = true; // 창 닫기 + 성공
        }
    }
}
