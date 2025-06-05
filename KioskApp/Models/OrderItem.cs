namespace KioskApp.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int MenuId { get; set; }
        public string MenuName { get; set; }   // UI/그리드 표시용
        public int Quantity { get; set; }
        public int UnitPrice { get; set; }
        public string OptionText { get; set; }
        public int TotalPrice => UnitPrice * Quantity;  // UI 계산용
    }
}
