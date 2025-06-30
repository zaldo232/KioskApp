using System.Windows.Media.Imaging;
using System.IO;

namespace KioskApp.Models
{
    // 메뉴 정보 클래스
    public class Menu
    {
        public int MenuId { get; set; }         // 메뉴 고유 ID
        public int CategoryId { get; set; }     // 소속 카테고리 ID
        public string Name { get; set; }        // 메뉴명
        public string Description { get; set; } // 메뉴 설명
        public int Price { get; set; }          // 가격 (원)
        public string ImagePath { get; set; }   // 이미지 파일 경로

        // 썸네일 이미지를 반환 (경로가 유효하면 이미지 로딩)
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
