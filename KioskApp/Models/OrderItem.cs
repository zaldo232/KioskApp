using System.ComponentModel;

namespace KioskApp.Models
{
    public class OrderItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int MenuId { get; set; }
        public string MenuName { get; set; }

        private int quantity;
        public int Quantity
        {
            get => quantity;
            set
            {
                if (quantity != value)
                {
                    quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(TotalPrice)); // TotalPrice도 같이 갱신
                }
            }
        }

        public int UnitPrice { get; set; }
        public string OptionText { get; set; }
        public int TotalPrice => UnitPrice * Quantity;

        // 깊은 복사 Clone 메서드
        public OrderItem Clone()
        {
            return new OrderItem
            {
                OrderItemId = this.OrderItemId,
                OrderId = this.OrderId,
                MenuId = this.MenuId,
                MenuName = this.MenuName,
                Quantity = this.Quantity,
                UnitPrice = this.UnitPrice,
                OptionText = this.OptionText
            };
        }
    }
}
