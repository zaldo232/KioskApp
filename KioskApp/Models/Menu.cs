using System.Windows.Media.Imaging;
using System.IO;

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

        // 썸네일 이미지
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
                    return null;
                }
            }
        }
    }
}
