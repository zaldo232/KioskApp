using KioskApp.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace KioskApp.Services
{
    public class PaymentResult { public bool Success { get; set; } public string? Message { get; set; } }

    public class PaymentService
    {
        // 싱글톤 패턴
        public static PaymentService Instance { get; } = new PaymentService();

        // 카카오페이 모의 결제 (실제 연동시 여기에 REST 호출)
        public async Task<PaymentResult> RequestKakaoPayAsync(ObservableCollection<OrderItem> items, int totalPrice)
        {
            // TODO: 실제 카카오페이 연동시
            // 1. 서버에 결제요청(REST API) 보내기
            // 2. QR결제창 띄우기 (url 응답값 사용)
            // 3. 결제 성공 콜백 or 폴링으로 상태 체크

            // 여기선 모의 성공
            await Task.Delay(1000);
            return new PaymentResult { Success = true };
        }

        // 페이코 모의 결제
        public async Task<PaymentResult> RequestPaycoAsync(ObservableCollection<OrderItem> items, int totalPrice)
        {
            // TODO: PAYCO 결제 연동시
            // (카카오와 거의 동일)
            await Task.Delay(1000);
            return new PaymentResult { Success = true };
        }
    }
}
