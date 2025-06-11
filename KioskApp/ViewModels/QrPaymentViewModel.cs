using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace KioskApp.ViewModels
{
    public partial class QrPaymentViewModel : ObservableObject
    {
        public string PayType { get; }
        public BitmapImage QrImage { get; }
        public Action? CancelRequested { get; set; }
        private readonly Func<Task<bool>> _pollPaymentStatus;
        private readonly Func<Task> _onPaymentApproved;

        public QrPaymentViewModel(string payType, string paymentUrl, Func<Task<bool>> pollPaymentStatus, Func<Task> onPaymentApproved)
        {
            PayType = payType;
            QrImage = GenerateQrCode(paymentUrl);
            _pollPaymentStatus = pollPaymentStatus;
            _onPaymentApproved = onPaymentApproved;
            StartPollingAsync();
        }

        [RelayCommand]
        private void Cancel() => CancelRequested?.Invoke();

        private async void StartPollingAsync()
        {
            // 30초간(예시) 2초마다 결제완료 여부 체크
            for (int i = 0; i < 15; i++)
            {
                await Task.Delay(2000);
                if (await _pollPaymentStatus())
                {
                    await _onPaymentApproved();
                    return;
                }
            }
            // 타임아웃: 필요하면 알림/취소
        }

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
