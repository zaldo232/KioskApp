using System.Collections.ObjectModel;
using System.ComponentModel;

namespace KioskApp.Models
{
    // 메뉴 옵션 정보 (예: 사이즈, 추가 토핑 등)
    public class MenuOption : INotifyPropertyChanged
    {
        public MenuOption()
        {
            Values = new ObservableCollection<MenuOptionValue>();
        }

        public int OptionId { get; set; }                  // 옵션 고유 ID
        public int MenuId { get; set; }                    // 소속 메뉴 ID
        public string OptionName { get; set; }             // 옵션명
        public bool IsRequired { get; set; }               // 필수 선택 여부
        public ObservableCollection<MenuOptionValue> Values { get; set; } // 옵션 선택지 리스트

        private MenuOptionValue _selectedValue;

        public MenuOptionValue SelectedValue                // 현재 선택된 옵션 값
        {
            get => _selectedValue;
            set
            {
                if (_selectedValue != value)
                {
                    _selectedValue = value;
                    // 바인딩 갱신 알림
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedValue)));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged; // 값 변경 이벤트
    }
}
