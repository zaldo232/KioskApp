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
    // 카카오페이 TID 메모리 저장용 (싱글톤)
    public class KakaoPayTidCache
    {
        private static KakaoPayTidCache? _instance;
        public static KakaoPayTidCache Instance => _instance ??= new KakaoPayTidCache();
        private Dictionary<string, string> _orderIdToTid = new Dictionary<string, string>();

        public void SetTid(string orderId, string tid) => _orderIdToTid[orderId] = tid;// tid 등록
        public string? GetTid(string orderId) => _orderIdToTid.TryGetValue(orderId, out var tid) ? tid : null;// tid 조회
    }

    // 결제 처리 결과 DTO
    public class PaymentResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; } // QR로 띄울 결제 URL
        public string? Tid { get; set; } // 결제 승인 Polling용 TID
        public string? PartnerOrderId { get; set; } // Minimal API서버용 주문ID
    }

    // 결제 처리 서비스 (카카오페이 연동)
    public class PaymentService
    {
        public static PaymentService Instance { get; } = new PaymentService();

        // 카카오페이 결제 요청 (결제 준비)
        public async Task<PaymentResult> RequestKakaoPayAsync(ObservableCollection<OrderItem> items, int totalPrice)
        {
            var httpClient = new HttpClient();
            var requestUrl = "https://kapi.kakao.com/v1/payment/ready";
            var adminKey = "0f96e6b0ba4e4797fb92766da78409f3";
            var cid = "TC0ONETIME";

            string partnerOrderId = Guid.NewGuid().ToString();
            string ngrokUrl = "https://e2369a5df79a.ngrok-free.app"; // <- ngrok 주소 매번 확인 필요함

            var approvalUrl = $"{ngrokUrl}/approve?orderId={partnerOrderId}";

            // 카카오 API 파라미터 구성
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
                { "cancel_url", $"{ngrokUrl}/cancel" },
                { "fail_url", $"{ngrokUrl}/fail" }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new FormUrlEncodedContent(parameters)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("KakaoAK", adminKey);

            var response = await httpClient.SendAsync(request);
            var resultStr = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) 
            {
                return new PaymentResult { Success = false, Message = resultStr }; 
            }


            var resultObj = JsonSerializer.Deserialize<KakaoPayReadyResult>(resultStr);

            // Minimal API 서버로 tid 등록 (필수 approval_url 콜백 처리를 위해)
            var regPayload = new { orderId = partnerOrderId, tid = resultObj.tid };
            var regJson = JsonSerializer.Serialize(regPayload);
            using var regClient = new HttpClient();
            var regRes = await regClient.PostAsync(
                $"{ngrokUrl}/register-tid",
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


        // 카카오페이 결제 승인 여부 Polling (최대 20초, 2초 간격)
        public async Task<bool> PollKakaoPayApprovalAsync(string tid)
        {
            var httpClient = new HttpClient();
            var requestUrl = "https://kapi.kakao.com/v1/payment/order";
            var adminKey = "0f96e6b0ba4e4797fb92766da78409f3";
            var cid = "TC0ONETIME";

            // 2초마다 결제 상태 확인, 최대 10회(20초)
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
                        if (status == "SUCCESS_PAYMENT")// 결제 성공
                            return true;
                    }
                }
                await Task.Delay(2000); // 2초 대기
            }
            return false; // 타임아웃시 결제 실패 처리
        }

        // 카카오페이 결제 준비 응답 DTO (내부용)
        private class KakaoPayReadyResult
        {
            public string tid { get; set; }
            public string next_redirect_pc_url { get; set; }
            public string next_redirect_mobile_url { get; set; }
        }
    }
}
