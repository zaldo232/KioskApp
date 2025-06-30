using System.IO;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace KioskApp.Views
{
    // 홈화면(광고 이미지, 관리자 트리거 등)
    public partial class HomeView : UserControl
    {
        private readonly List<string> _adImages = new(); // 광고 이미지 파일 리스트
        private int _adIndex = 0;                        // 현재 이미지 인덱스
        private DispatcherTimer _adTimer;                // 이미지 슬라이드 타이머

        // 관리자 진입 트리거(타이틀 연타 감지용)
        private int _adminClickCount = 0;
        private DateTime _lastClickTime = DateTime.MinValue;

        public HomeView()
        {
            InitializeComponent();

            // 광고 이미지 폴더(Images/Advertisement)에서 자동 로드
            var imagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Advertisement");
            var orderJson = Path.Combine(imagesDir, "ad_order.json");

            if (File.Exists(orderJson))
            {
                try
                {
                    var json = File.ReadAllText(orderJson);
                    var metaList = JsonSerializer.Deserialize<List<AdImageMeta>>(json);
                    if (metaList != null)
                    {
                        foreach (var meta in metaList)
                        {
                            var abs = Path.Combine(imagesDir, meta.FileName);
                            if (File.Exists(abs) && !meta.IsHidden)
                            {
                                _adImages.Add(abs); // 숨김이 아닌 것만 노출
                            }
                        }
                    }
                }
                catch { }
            }

            // json이 없으면 폴더 내 이미지 전체 사용
            if (_adImages.Count == 0 && Directory.Exists(imagesDir))
            {
                var imgFiles = Directory.GetFiles(imagesDir)
                    .Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                foreach (var file in imgFiles)
                { 
                    _adImages.Add(file); 
                }
            }
            // 이미지 없으면 기본 이미지(placeholder) 추가
            if (_adImages.Count == 0)
            { 
                _adImages.Add(Path.Combine(imagesDir, "default.png")); 
            }

            // 첫 이미지 표시
            if (_adImages.Count > 0)
            { 
                AdImage.Source = CreateBitmapImage(_adImages[0]); 
            }

            // 3.5초 간격 광고 슬라이드 시작
            _adTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3.5) };
            _adTimer.Tick += (s, e) => NextAd();
            _adTimer.Start();
        }

        // 광고 이미지 다음으로 넘김(슬라이드)
        private void NextAd()
        {
            _adIndex = (_adIndex + 1) % _adImages.Count;
            AdImage.Source = CreateBitmapImage(_adImages[_adIndex]);
        }

        // 이미지 경로 -> BitmapImage 변환 (락 최소화)
        private BitmapImage CreateBitmapImage(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                var bmp = new BitmapImage();
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = fs;
                    bmp.EndInit();
                    bmp.Freeze();
                }
                return bmp;
            }
            catch { return null; }
        }

        // 관리자 트리거: 타이틀 3초 내 5회 클릭 시 관리자 진입
        private void TitleText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var now = DateTime.Now;
            if ((now - _lastClickTime).TotalSeconds > 3)
            {
                _adminClickCount = 0;
            }
            _lastClickTime = now;
            _adminClickCount++;

            if (_adminClickCount >= 5)
            {
                _adminClickCount = 0;
                // 관리자 진입 (GoAdminCommand 호출)
                if (DataContext is KioskApp.ViewModels.HomeViewModel vm)
                { 
                    vm.GoAdminCommand.Execute(null); 
                }
            }
        }
    }

    // 광고 이미지 메타데이터(ad_order.json 구조체)
    public class AdImageMeta
    {
        public string FileName { get; set; }
        public bool IsHidden { get; set; }
    }
}
