using System.Collections.ObjectModel;
using System.ComponentModel;

namespace KioskApp.Models
{
    public class MenuOption : INotifyPropertyChanged
    {
        public int OptionId { get; set; }

        public int MenuId { get; set; }
        public string OptionName { get; set; }
        public bool IsRequired { get; set; }
        public ObservableCollection<MenuOptionValue> Values { get; set; }

        private MenuOptionValue _selectedValue;
        public MenuOptionValue SelectedValue
        {
            get => _selectedValue;
            set
            {
                if (_selectedValue != value)
                {
                    _selectedValue = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedValue)));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
