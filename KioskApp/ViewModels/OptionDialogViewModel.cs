using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using KioskApp.Models;
using KioskApp.Repositories;
using System.Linq;
using System.Windows.Input;

namespace KioskApp.ViewModels
{
    public partial class OptionDialogViewModel : ObservableObject
    {
        [ObservableProperty] private KioskApp.Models.Menu menu;
        public ObservableCollection<MenuOption> MenuOptions { get; } = new();

        [ObservableProperty] private int quantity = 1;

        public string TotalPriceText => $"총 {TotalPrice:N0}원";
        public int TotalPrice
        {
            get
            {
                int basePrice = Menu.Price;
                int extra = MenuOptions.Sum(opt =>
                    opt.SelectedValue != null ? opt.SelectedValue.ExtraPrice : 0);
                return (basePrice + extra) * Quantity;
            }
        }

        public OptionDialogViewModel(Menu menu)
        {
            Menu = menu;

            // 옵션 및 값들 불러오기
            var optionRepo = new MenuOptionRepository();
            var options = optionRepo.GetByMenuId(menu.MenuId);

            foreach (var opt in options)
            {
                // 옵션값을 가격순으로 정렬
                opt.Values = new ObservableCollection<MenuOptionValue>(
                    opt.Values.OrderBy(v => v.ExtraPrice)
                );

                // 필수 옵션이 아니면 "선택 안함" 추가
                if (!opt.IsRequired)
                {
                    if (opt.Values.All(v => v.OptionValueId != 0))
                        opt.Values.Insert(0, new MenuOptionValue
                        {
                            OptionValueId = 0,
                            ValueLabel = "선택 안함",
                            ExtraPrice = 0,
                            ImagePath = "/Images/none.png" // 이미지 경로가 있으면 넣기(없으면 삭제)
                        });
                }
                // 기본 선택값: 필수는 첫 값, 아니면 "선택 안함"
                opt.SelectedValue = opt.IsRequired
                    ? opt.Values.FirstOrDefault()
                    : opt.Values.FirstOrDefault(v => v.OptionValueId == 0) ?? opt.Values.FirstOrDefault();

                MenuOptions.Add(opt);
            }

            // 변경감지 이벤트 등록
            foreach (var opt in MenuOptions)
                opt.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(MenuOption.SelectedValue))
                    {
                        OnPropertyChanged(nameof(TotalPrice));
                        OnPropertyChanged(nameof(TotalPriceText));
                    }
                };
        }


        // 옵션 카드 클릭 시 실행
        [RelayCommand]
        public void SelectOption((MenuOption Option, MenuOptionValue Value) param)
        {
            if (param.Option.SelectedValue != param.Value)
                param.Option.SelectedValue = param.Value;
        }

        [RelayCommand]
        public void DecreaseQty()
        {
            if (Quantity > 1)
                Quantity--;
            OnPropertyChanged(nameof(TotalPrice));
            OnPropertyChanged(nameof(TotalPriceText));
        }

        [RelayCommand]
        public void IncreaseQty()
        {
            Quantity++;
            OnPropertyChanged(nameof(TotalPrice));
            OnPropertyChanged(nameof(TotalPriceText));
        }
    }
}
