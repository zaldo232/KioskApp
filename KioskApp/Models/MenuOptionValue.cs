using System.Windows.Media.Imaging;
using System.IO;

namespace KioskApp.Models
{
    public class MenuOptionValue
    {
        public int OptionValueId { get; set; }
        public int OptionId { get; set; }
        public string ValueLabel { get; set; }
        public int ExtraPrice { get; set; }
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
