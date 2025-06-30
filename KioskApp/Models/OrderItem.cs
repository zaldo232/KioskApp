using System.Collections.ObjectModel;
using System.ComponentModel;

namespace KioskApp.Models
{
    // 주문 상세(한 메뉴+옵션+수량) 정보 클래스
    public class OrderItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public int OrderItemId { get; set; }      // 주문 상세 고유 ID
        public int OrderId { get; set; }          // 소속 주문 ID
        public int MenuId { get; set; }           // 메뉴 ID
        public string MenuName { get; set; }      // 메뉴명

        private int quantity;
        public int Quantity
        {
            get => quantity;
            set
            {
                if (quantity != value)
                {
                    quantity = value;
                    OnPropertyChanged(nameof(Quantity));    // 수량 변경 알림
                    OnPropertyChanged(nameof(TotalPrice));  // 총금액도 갱신 알림
                }
            }
        }

        public int UnitPrice { get; set; }        // 단가(옵션 포함)
        public string OptionText { get; set; }    // 옵션 설명(문자열, 쉼표구분)
        public int TotalPrice => UnitPrice * Quantity; // 총 금액

        // 옵션 설명을 줄단위로 분리 (쉼표 구분)
        public ObservableCollection<string> OptionList
            => new ObservableCollection<string>(
                (OptionText ?? "")
                .Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
            );
    }
}
