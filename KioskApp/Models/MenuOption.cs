using System.Collections.ObjectModel;

namespace KioskApp.Models
{
    public class MenuOption
    {
        public int OptionId { get; set; }
        public int MenuId { get; set; }
        public string OptionName { get; set; }
        public bool IsRequired { get; set; }
        public ObservableCollection<MenuOptionValue> Values { get; set; } = new();
    }
}
