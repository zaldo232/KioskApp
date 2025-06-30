using System.Windows;
using KioskApp.Models;
using KioskApp.ViewModels;
using System.Linq;

namespace KioskApp.Views
{
    // 메뉴 옵션 선택 팝업(장바구니 추가 등)
    public partial class OptionDialog : Window
    {
        private readonly OptionDialogViewModel _vm;

        // 메뉴 모델을 받아 ViewModel 바인딩
        public OptionDialog(Menu menu)
        {
            InitializeComponent();
            _vm = new OptionDialogViewModel(menu);
            DataContext = _vm;
        }

        // 장바구니 담기 버튼 클릭 시
        private void AddToCart(object sender, RoutedEventArgs e)
        {
            // 모든 필수 옵션이 선택됐는지 체크
            if (_vm.MenuOptions.Any(o => o.IsRequired && o.SelectedValue == null))
            {
                MessageBox.Show("모든 필수 옵션을 선택하세요.");
                return;
            }

            // 옵션 요약 문자열 생성 (옵션명:값, ...)
            var optSummary = string.Join(",",
                _vm.MenuOptions
                    .Where(opt => opt.SelectedValue != null && opt.SelectedValue.OptionValueId != 0)
                    .Select(opt => $"{opt.OptionName}:{opt.SelectedValue.ValueLabel}")
            );

            // 옵션 문자열 로그(디버깅용)
            System.Diagnostics.Debug.WriteLine($"[OptionDialog] optSummary = '{optSummary}'");

            // 결과값(Tag로 반환)
            this.Tag = new OptionDialogResult
            {
                OptionText = optSummary,
                UnitPrice = _vm.Menu.Price + _vm.MenuOptions.Sum(o => o.SelectedValue?.ExtraPrice ?? 0),
                Quantity = _vm.Quantity
            };
            DialogResult = true; // OK 반환
        }

        // 주문화면으로 버튼 클릭 시
        private void GoBackToOrder(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // 취소, 닫기
        }

    }

    // 옵션 선택 결과(장바구니에 전달)
    public class OptionDialogResult
    {
        public string OptionText { get; set; }
        public int UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
