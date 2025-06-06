namespace KioskApp.Models
{
    public class MenuOptionValue
    {
        public int OptionValueId { get; set; }
        public int OptionId { get; set; }
        public string ValueLabel { get; set; }
        public int ExtraPrice { get; set; }
        public string ImagePath { get; set; }
    }
}
