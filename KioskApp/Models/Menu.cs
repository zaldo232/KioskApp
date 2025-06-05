namespace KioskApp.Models
{
    public class Menu
    {
        public int MenuId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public string ImagePath { get; set; }
    }
}
