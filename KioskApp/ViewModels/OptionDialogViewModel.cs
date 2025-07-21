using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KioskApp.Models;
using KioskApp.Repositories;
using System.Collections.ObjectModel;

namespace KioskApp.ViewModels
{
    // 옵션, 수량 선택 다이얼로그 뷰모델
    public partial class OptionDialogViewModel : ObservableObject
    {
        [ObservableProperty] private KioskApp.Models.Menu menu; // 대상 메뉴
        public ObservableCollection<MenuOption> MenuOptions { get; } = new(); // 옵션 리스트

        [ObservableProperty] private int quantity = 1; // 주문 수량

        // 총 가격 텍스트 (ex: "총 5,500원")
        public string TotalPriceText => $"총 {TotalPrice:N0}원";

        // 총 가격 계산 (메뉴 가격 + 옵션 추가금) * 수량
        public int TotalPrice
        {
            get
            {
                int basePrice = Menu.Price;
                int extra = MenuOptions.Sum(opt => opt.SelectedValue != null ? opt.SelectedValue.ExtraPrice : 0);
                return (basePrice + extra) * Quantity;
            }
        }
        
        // 생성자: 옵션, 선택지 셋팅, 기본 선택값 등 처리
        public OptionDialogViewModel(Menu menu)
        {
            Menu = menu;

            // 메뉴 이미지 절대경로 변환
            if (!string.IsNullOrWhiteSpace(Menu.ImagePath) && !System.IO.Path.IsPathRooted(Menu.ImagePath))
            { 
                Menu.ImagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Menu.ImagePath); 
            }

            // 옵션 및 값들 불러오기
            var optionRepo = new MenuOptionRepository();
            var options = optionRepo.GetByMenuId(menu.MenuId);

            foreach (var opt in options)
            {
                // 옵션값을 가격순으로 정렬
                opt.Values = new ObservableCollection<MenuOptionValue>(opt.Values.OrderBy(v => v.ExtraPrice));

                // 각 옵션값 이미지 절대경로 변환
                foreach (var v in opt.Values)
                {
                    if (!string.IsNullOrWhiteSpace(v.ImagePath) && !System.IO.Path.IsPathRooted(v.ImagePath))
                    { 
                        v.ImagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, v.ImagePath);
                    }
                }

                // 필수 옵션이 아니면 선택 안함 추가
                if (!opt.IsRequired)
                {
                    if (opt.Values.All(v => v.OptionValueId != 0))
                        opt.Values.Insert(0, new MenuOptionValue
                        {
                            OptionValueId = 0,
                            ValueLabel = "선택 안함",
                            ExtraPrice = 0,
                            ImagePath = "/Images/default.png" // 이미지 경로가 있으면 넣기(없으면 삭제)
                        });
                }
                // 기본 선택값 지정: 필수->첫값, 선택옵션->선택 안함 또는 첫값
                opt.SelectedValue = opt.IsRequired ? opt.Values.FirstOrDefault() : opt.Values.FirstOrDefault(v => v.OptionValueId == 0) ?? opt.Values.FirstOrDefault();

                MenuOptions.Add(opt);
            }

            // 옵션 변경 시 총금액/텍스트 자동 갱신
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

        // 옵션 카드 클릭 시 실행 (옵션 값 선택)
        [RelayCommand]
        public void SelectOption((MenuOption Option, MenuOptionValue Value) param)
        {
            if (param.Option.SelectedValue != param.Value)
            {
                param.Option.SelectedValue = param.Value;
            }
        }

        // 수량 감소 (최소 1)
        [RelayCommand]
        public void DecreaseQty()
        {
            if (Quantity > 1)
            {
                Quantity--;
            }

            OnPropertyChanged(nameof(TotalPrice));
            OnPropertyChanged(nameof(TotalPriceText));
        }

        // 수량 증가
        [RelayCommand]
        public void IncreaseQty()
        {
            Quantity++;
            OnPropertyChanged(nameof(TotalPrice));
            OnPropertyChanged(nameof(TotalPriceText));
        }
    }
}
