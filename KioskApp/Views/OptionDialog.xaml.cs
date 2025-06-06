using System.Windows;
using KioskApp.Models;
using KioskApp.ViewModels;

namespace KioskApp.Views
{
    public partial class OptionDialog : Window
    {
        private readonly OptionDialogViewModel _vm;
        public OptionDialog(Menu menu)
        {
            InitializeComponent();
            _vm = new OptionDialogViewModel(menu);
            DataContext = _vm;
        }

        private void AddToCart(object sender, RoutedEventArgs e)
        {
            // 모든 필수 옵션이 선택됐는지 체크
            if (_vm.MenuOptions.Any(o => o.IsRequired && o.SelectedValue == null))
            {
                MessageBox.Show("모든 필수 옵션을 선택하세요.");
                return;
            }

            // 옵션 요약
            var optSummary = string.Join(", ",
                _vm.MenuOptions
                    .Where(opt => opt.SelectedValue != null && opt.SelectedValue.OptionValueId != 0)
                    .Select(opt => $"{opt.OptionName}:{opt.SelectedValue.ValueLabel}({opt.SelectedValue.ExtraPrice:N0}원)"));

            this.Tag = new OptionDialogResult
            {
                OptionText = optSummary,
                UnitPrice = _vm.Menu.Price + _vm.MenuOptions.Sum(o => o.SelectedValue?.ExtraPrice ?? 0),
                Quantity = _vm.Quantity
            };
            DialogResult = true;
        }
    }

    public class OptionDialogResult
    {
        public string OptionText { get; set; }
        public int UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
