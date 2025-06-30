using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.QrCode;

namespace KioskApp.ViewModels
{    
    // QR 결제(카카오/페이코 등) 뷰모델
    public partial class QrPaymentViewModel : ObservableObject
    {
        public string PayType { get; }            // 결제 종류(카카오페이 등)
        public BitmapImage QrImage { get; }       // QR 이미지 바인딩용
        public Action? CancelRequested { get; set; } // 취소 요청 콜백

        private readonly Func<Task<bool>> _pollPaymentStatus; // 결제 승인 폴링 함수
        private readonly Func<Task> _onPaymentApproved;       // 결제 승인 콜백

        // 생성자: 결제 URL -> QR생성, 폴링 시작
        public QrPaymentViewModel(string payType, string paymentUrl, Func<Task<bool>> pollPaymentStatus, Func<Task> onPaymentApproved)
        {
            PayType = payType;
            QrImage = GenerateQrCode(paymentUrl);         // QR 이미지 생성
            _pollPaymentStatus = pollPaymentStatus;       // 결제 상태 체크 함수
            _onPaymentApproved = onPaymentApproved;       // 결제 승인 시 콜백
            StartPollingAsync();                          // 결제 승인 폴링 시작
        }

        // 취소 버튼 클릭 시 실행
        [RelayCommand]
        private void Cancel() => CancelRequested?.Invoke();

        // 결제 승인 폴링(최대 30초, 2초마다 체크)
        private async void StartPollingAsync()
        {
            // 15회(=30초) 동안 2초마다 결제 승인 여부 확인
            for (int i = 0; i < 15; i++)
            {
                await Task.Delay(2000);
                if (await _pollPaymentStatus())
                {
                    await _onPaymentApproved(); // 승인시 콜백 실행(주문저장 등)
                    return;
                }
            }
            // 타임아웃: 필요하면 알림/취소
        }

        // 결제 URL -> QR 이미지 생성
        private BitmapImage GenerateQrCode(string url)
        {
            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = 256,
                    Width = 256,
                    Margin = 1
                }
            };
            var pixelData = writer.Write(url);
            var bitmap = new WriteableBitmap(pixelData.Width, pixelData.Height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null);
            bitmap.WritePixels(new System.Windows.Int32Rect(0, 0, pixelData.Width, pixelData.Height), pixelData.Pixels, pixelData.Width * 4, 0);
            var ms = new System.IO.MemoryStream();
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            encoder.Save(ms);
            ms.Position = 0;
            var img = new BitmapImage();
            img.BeginInit();
            img.StreamSource = ms;
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();
            return img;
        }
    }
}
