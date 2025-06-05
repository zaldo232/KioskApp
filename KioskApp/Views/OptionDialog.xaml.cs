using KioskApp.Models;
using KioskApp.Repositories;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls; // 추가!

namespace KioskApp.Views
{
    public partial class OptionDialog : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public KioskApp.Models.Menu Menu { get; }
        public ObservableCollection<MenuOption> MenuOptions { get; }
        public Dictionary<int, MenuOptionValue> SelectedOptionValues { get; } = new(); // OptionId -> 선택값

        private int quantity = 1;
        public int Quantity
        {
            get => quantity;
            set { quantity = value; OnPropertyChanged(nameof(Quantity)); OnPropertyChanged(nameof(TotalPrice)); OnPropertyChanged(nameof(TotalPriceText)); }
        }

        public string TotalPriceText => $"총 {TotalPrice:N0}원";
        public int TotalPrice
        {
            get
            {
                int basePrice = Menu.Price;
                int extra = SelectedOptionValues.Values.Sum(v => v?.ExtraPrice ?? 0);
                return (basePrice + extra) * Quantity;
            }
        }

        public OptionDialog(KioskApp.Models.Menu menu)
        {
            InitializeComponent();
            Menu = menu;
            var repo = new MenuOptionRepository();
            MenuOptions = new ObservableCollection<MenuOption>(repo.GetByMenuId(menu.MenuId));

            foreach (var opt in MenuOptions)
            {
                if (!opt.IsRequired)
                {
                    if (opt.Values.All(v => v.OptionValueId != 0))
                        opt.Values.Insert(0, new MenuOptionValue { OptionValueId = 0, ValueLabel = "선택 안함", ExtraPrice = 0 });
                }
                // 기본 선택값: 필수는 첫 값, 아니면 '선택 안함'
                opt.SelectedValue = opt.IsRequired ? opt.Values.FirstOrDefault() : opt.Values.FirstOrDefault(v => v.OptionValueId == 0) ?? opt.Values.FirstOrDefault();
            }

            DataContext = this;
        }


        // 옵션 선택값 바뀔 때마다 합계 강제 갱신!
        private void OptionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox combo && combo.DataContext is MenuOption opt)
            {
                SelectedOptionValues[opt.OptionId] = combo.SelectedItem as MenuOptionValue;
                OnPropertyChanged(nameof(TotalPrice));
                OnPropertyChanged(nameof(TotalPriceText));
            }
        }


        private void DecreaseQty(object sender, RoutedEventArgs e)
        {
            if (Quantity > 1) Quantity--;
            OnPropertyChanged(nameof(Quantity));
            OnPropertyChanged(nameof(TotalPrice));
            OnPropertyChanged(nameof(TotalPriceText));
        }

        private void IncreaseQty(object sender, RoutedEventArgs e)
        {
            Quantity++;
            OnPropertyChanged(nameof(Quantity));
            OnPropertyChanged(nameof(TotalPrice));
            OnPropertyChanged(nameof(TotalPriceText));
        }

        private void AddToCart(object sender, RoutedEventArgs e)
        {
            var optSummary = string.Join(", ",
                MenuOptions.Select(opt => $"{opt.OptionName}:{SelectedOptionValues[opt.OptionId]?.ValueLabel}({(SelectedOptionValues[opt.OptionId]?.ExtraPrice ?? 0):N0}원)"));

            this.Tag = new OptionDialogResult
            {
                OptionText = optSummary,
                UnitPrice = Menu.Price + SelectedOptionValues.Values.Sum(v => v?.ExtraPrice ?? 0),
                Quantity = this.Quantity
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
