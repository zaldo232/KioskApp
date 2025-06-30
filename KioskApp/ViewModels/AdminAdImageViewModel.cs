using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace KioskApp.ViewModels
{
    // 관리자 광고 이미지 관리 뷰모델
    public partial class AdminAdImageViewModel : ObservableObject
    {

        public ObservableCollection<AdImage> AdImages { get; } = new();

        [ObservableProperty] private AdImage selectedAdImage;   // 현재 선택된 광고 이미지
        [ObservableProperty] private string newAdImagePath;     // 추가할 이미지 경로

        public Action GoHomeRequested { get; set; }             // 홈화면 이동 액션
        public Action GoMenuRequested { get; set; }             // 메뉴관리 이동 액션

        private readonly string _imagesDir;                     // 이미지 저장 폴더
        private readonly string _orderJsonFile;                 // 순서/메타 json 경로

        public AdminAdImageViewModel()
        {
            _imagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Advertisement");
            _orderJsonFile = Path.Combine(_imagesDir, "ad_order.json");
            LoadOrderJson();    // 시작시 광고 이미지/메타 불러오기
        }

        // 홈으로 이동
        [RelayCommand]
        public void GoHome() => GoHomeRequested?.Invoke();
        
        // 메뉴로 이동
        [RelayCommand]
        public void GoMenu() => GoMenuRequested?.Invoke();

        // 광고 이미지 파일 선택
        [RelayCommand]
        public void BrowseAdImage()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*"
            };
            if (dlg.ShowDialog() == true)
            {
                NewAdImagePath = dlg.FileName;
            }
        }

        // 광고 이미지 추가(복사 후 목록/메타 저장)
        [RelayCommand]
        public void AddAdImage()
        {
            if (string.IsNullOrWhiteSpace(NewAdImagePath) || !File.Exists(NewAdImagePath))
                return;

            if (!Directory.Exists(_imagesDir)) Directory.CreateDirectory(_imagesDir);

            var fileName = Path.GetFileName(NewAdImagePath);
            var destPath = Path.Combine(_imagesDir, fileName);
            File.Copy(NewAdImagePath, destPath, true);

            if (AdImages.Any(x => x.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase))) return;

            AdImages.Add(new AdImage { FileName = fileName, IsHidden = false });
            SaveOrderJson();

            NewAdImagePath = "";
        }

        // 광고 이미지 삭제
        [RelayCommand]
        public void DeleteAdImage()
        {
            if (SelectedAdImage == null) return;

            var filePath = Path.Combine(_imagesDir, SelectedAdImage.FileName);
            if (File.Exists(filePath)) File.Delete(filePath);

            AdImages.Remove(SelectedAdImage);
            SaveOrderJson();
            SelectedAdImage = null;
        }

        // 광고 이미지 한 칸 위로 이동
        [RelayCommand]
        public void MoveUpAdImage()
        {
            if (SelectedAdImage == null) return;
            int idx = AdImages.IndexOf(SelectedAdImage);
            if (idx > 0)
            {
                AdImages.Move(idx, idx - 1);
                SaveOrderJson();
            }
        }

        // 광고 이미지 한 칸 아래로 이동
        [RelayCommand]
        public void MoveDownAdImage()
        {
            if (SelectedAdImage == null) return;
            int idx = AdImages.IndexOf(SelectedAdImage);
            if (idx < AdImages.Count - 1 && idx >= 0)
            {
                AdImages.Move(idx, idx + 1);
                SaveOrderJson();
            }
        }

        // 광고 이미지 순서/메타 정보 json 파일 저장
        private void SaveOrderJson()
        {
            var metaList = AdImages.Select(x => new AdImageMeta { FileName = x.FileName, IsHidden = x.IsHidden }).ToList();
            File.WriteAllText(_orderJsonFile, JsonSerializer.Serialize(metaList, new JsonSerializerOptions { WriteIndented = true }));
        }

        // 광고 이미지/메타 정보 불러오기
        private void LoadOrderJson()
        {
            AdImages.Clear();
            var metaList = new List<AdImageMeta>();

            if (File.Exists(_orderJsonFile))
            {
                try
                {
                    var json = File.ReadAllText(_orderJsonFile);
                    metaList = JsonSerializer.Deserialize<List<AdImageMeta>>(json) ?? new();
                }
                catch { metaList = new List<AdImageMeta>(); }
            }

            // json 파일 기준으로 순서 복원
            foreach (var meta in metaList)
            {
                var abs = Path.Combine(_imagesDir, meta.FileName);
                if (File.Exists(abs))
                    AdImages.Add(new AdImage { FileName = meta.FileName, IsHidden = meta.IsHidden });
            }
            // 폴더 내 실제 파일 중 json에 없는 건 신규로 추가
            var allFiles = Directory.GetFiles(_imagesDir)
                .Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                .Select(f => Path.GetFileName(f));
            foreach (var f in allFiles)
            {
                if (!AdImages.Any(x => x.FileName == f))
                    AdImages.Add(new AdImage { FileName = f });
            }
        }
    }

    // 광고 이미지 메타 정보(json 저장용)
    public class AdImageMeta
    {
        public string FileName { get; set; }
        public bool IsHidden { get; set; }
    }

    // 광고 이미지 뷰모델용 데이터
    public class AdImage : ObservableObject
    {
        public string FileName { get; set; }
        public bool IsHidden { get; set; }

        // 락 없이 썸네일 읽기 (읽기 실패시 null)
        public System.Windows.Media.Imaging.BitmapImage FileImage
        {
            get
            {
                var abs = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Advertisement", FileName ?? ""));
                if (!System.IO.File.Exists(abs)) return null;

                try
                {
                    using (var stream = new System.IO.FileStream(abs, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                    {
                        var bmp = new System.Windows.Media.Imaging.BitmapImage();
                        bmp.BeginInit();
                        bmp.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                        bmp.StreamSource = stream;
                        bmp.EndInit();
                        bmp.Freeze();
                        return bmp;
                    }
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
