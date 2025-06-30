using System.Windows.Media.Imaging;
using System.IO;

namespace KioskApp.Models
{
    // 메뉴 옵션의 개별 선택지(예: 사이즈-라지, 토핑-치즈 등)
    public class MenuOptionValue
    {
        public int OptionValueId { get; set; }    // 옵션 값 고유 ID
        public int OptionId { get; set; }         // 소속 옵션 ID
        public string ValueLabel { get; set; }    // 표시 라벨(예: '라지', '치즈')
        public int ExtraPrice { get; set; }       // 추가 금액(없으면 0)
        public string ImagePath { get; set; }     // 이미지 파일 경로

        // 썸네일 이미지 반환 (유효한 경로일 때만 로딩)
        public BitmapImage FileImage
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ImagePath) || !File.Exists(ImagePath))
                    return null;
                try
                {
                    var bmp = new BitmapImage();
                    using (var fs = new FileStream(ImagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        bmp.BeginInit();
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.StreamSource = fs;
                        bmp.EndInit();
                        bmp.Freeze();
                    }
                    return bmp;
                }
                catch
                {
                    // 이미지 로드 실패 시 null 반환
                    return null;
                }
            }
        }
    }
}
