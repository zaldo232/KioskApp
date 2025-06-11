using KioskApp.Models;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;

namespace KioskApp.Services
{
    // 임시 메모리 매핑 클래스 (싱글턴)
    public class KakaoPayTidCache
    {
        private static KakaoPayTidCache? _instance;
        public static KakaoPayTidCache Instance => _instance ??= new KakaoPayTidCache();
        private Dictionary<string, string> _orderIdToTid = new Dictionary<string, string>();

        public void SetTid(string orderId, string tid) => _orderIdToTid[orderId] = tid;
        public string? GetTid(string orderId) =>
            _orderIdToTid.TryGetValue(orderId, out var tid) ? tid : null;
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; } // QR로 띄울 결제 URL
        public string? Tid { get; set; } // 결제 승인 Polling용
        public string? PartnerOrderId { get; set; } // (추가) Minimal API서버로 넘길 수 있도록
    }

    public class PaymentService
    {
        public static PaymentService Instance { get; } = new PaymentService();

        public async Task<PaymentResult> RequestKakaoPayAsync(ObservableCollection<OrderItem> items, int totalPrice)
        {
            var httpClient = new HttpClient();
            var requestUrl = "https://kapi.kakao.com/v1/payment/ready";
            var adminKey = "0f96e6b0ba4e4797fb92766da78409f3";
            var cid = "TC0ONETIME";

            string partnerOrderId = Guid.NewGuid().ToString();
            string ngrokUrl = "2b29-112-217-82-146.ngrok-free.app"; // ← ngrok 주소 매번 확인!

            var approvalUrl = $"https://{ngrokUrl}/approve?orderId={partnerOrderId}";

            var parameters = new Dictionary<string, string>
            {
                { "cid", cid },
                { "partner_order_id", partnerOrderId },
                { "partner_user_id", "kiosk-user" },
                { "item_name", items.Count == 1 ? items[0].MenuName : $"{items[0].MenuName} 외 {items.Count-1}건" },
                { "quantity", items.Sum(x => x.Quantity).ToString() },
                { "total_amount", totalPrice.ToString() },
                { "tax_free_amount", "0" },
                { "approval_url", approvalUrl },
                { "cancel_url", $"https://{ngrokUrl}/cancel" },
                { "fail_url", $"https://{ngrokUrl}/fail" }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new FormUrlEncodedContent(parameters)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("KakaoAK", adminKey);

            var response = await httpClient.SendAsync(request);
            var resultStr = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                return new PaymentResult { Success = false, Message = resultStr };

            var resultObj = JsonSerializer.Deserialize<KakaoPayReadyResult>(resultStr);

            // ★ Minimal API 서버로 tid 등록(반드시 있어야 approval_url 콜백이 동작함!)
            var regPayload = new { orderId = partnerOrderId, tid = resultObj.tid };
            var regJson = JsonSerializer.Serialize(regPayload);
            using var regClient = new HttpClient();
            var regRes = await regClient.PostAsync(
                $"https://{ngrokUrl}/register-tid",
                new StringContent(regJson, System.Text.Encoding.UTF8, "application/json")
            );
            if (!regRes.IsSuccessStatusCode)
                return new PaymentResult { Success = false, Message = "[서버에 tid등록 실패] " + await regRes.Content.ReadAsStringAsync() };

            return new PaymentResult
            {
                Success = true,
                Message = resultObj.next_redirect_mobile_url ?? resultObj.next_redirect_pc_url,
                Tid = resultObj.tid,
                PartnerOrderId = partnerOrderId
            };
        }


        // 페이코 QR 결제 API 연동(샘플)
        public async Task<PaymentResult> RequestPaycoAsync(ObservableCollection<OrderItem> items, int totalPrice)
        {
            await Task.Delay(500);

            string paycoUrl = "https://payco.com/fakeqr-url";
            string tid = Guid.NewGuid().ToString();

            return new PaymentResult
            {
                Success = true,
                Message = paycoUrl,
                Tid = tid
            };
        }

        // 카카오페이 결제 승인 Polling(실제 구현)
        public async Task<bool> PollKakaoPayApprovalAsync(string tid)
        {
            var httpClient = new HttpClient();
            var requestUrl = "https://kapi.kakao.com/v1/payment/order";
            var adminKey = "0f96e6b0ba4e4797fb92766da78409f3";
            var cid = "TC0ONETIME";

            // 최대 20초(10회) 동안 2초마다 결제 상태 확인
            for (int i = 0; i < 10; i++)
            {
                var parameters = new Dictionary<string, string>
                {
                    { "cid", cid },
                    { "tid", tid }
                };
                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
                {
                    Content = new FormUrlEncodedContent(parameters)
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("KakaoAK", adminKey);

                var response = await httpClient.SendAsync(request);
                var resultStr = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(resultStr);
                    if (doc.RootElement.TryGetProperty("status", out var statusElement))
                    {
                        var status = statusElement.GetString();
                        if (status == "SUCCESS_PAYMENT")
                            return true;
                    }
                }
                await Task.Delay(2000);
            }
            return false; // 타임아웃시 결제 실패 처리
        }

        // 페이코 결제 승인 Polling
        public async Task<bool> PollPaycoApprovalAsync(string tid)
        {
            await Task.Delay(5000);
            return true;
        }

        private class KakaoPayReadyResult
        {
            public string tid { get; set; }
            public string next_redirect_pc_url { get; set; }
            public string next_redirect_mobile_url { get; set; }
        }
    }
}
