namespace KioskApp.Models
{
    // 주문 정보 클래스
    public class Order
    {
        public int OrderId { get; set; }         // 주문 고유 ID
        public DateTime OrderTime { get; set; }  // 주문 시각
        public int TotalPrice { get; set; }      // 주문 총금액
        public string PaymentType { get; set; }  // 결제 수단(예: 카드, 카카오페이)
        public string Status { get; set; }       // 주문 상태(예: 완료, 취소)
    }
}
