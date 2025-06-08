using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

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
                    OnPropertyChanged(nameof(TotalPrice));
                }
            }
        }

        public int UnitPrice { get; set; }
        public string OptionText { get; set; }
        public int TotalPrice => UnitPrice * Quantity;

        // 옵션 여러 줄로 쪼개기 (쉼표 기준)
        public ObservableCollection<string> OptionList
            => new ObservableCollection<string>(
                (OptionText ?? "")
                .Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
            );
    }
}
