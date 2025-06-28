using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace KioskApp.ViewModels
{
    public partial class AdminAdImageViewModel : ObservableObject
    {
        public ObservableCollection<AdImage> AdImages { get; } = new();

        [ObservableProperty] private AdImage selectedAdImage;
        [ObservableProperty] private string newAdImagePath;

        public Action GoHomeRequested { get; set; }
        public Action GoMenuRequested { get; set; }

        private readonly string _imagesDir;
        private readonly string _orderJsonFile;

        public AdminAdImageViewModel()
        {
            _imagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Advertisement");
            _orderJsonFile = Path.Combine(_imagesDir, "ad_order.json");
            LoadOrderJson();
        }

        [RelayCommand]
        public void GoHome() => GoHomeRequested?.Invoke();

        [RelayCommand]
        public void GoMenu() => GoMenuRequested?.Invoke();

        [RelayCommand]
        public void BrowseAdImage()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*"
            };
            if (dlg.ShowDialog() == true)
                NewAdImagePath = dlg.FileName;
        }

        [RelayCommand]
        public void AddAdImage()
        {
            if (string.IsNullOrWhiteSpace(NewAdImagePath) || !File.Exists(NewAdImagePath))
                return;

            if (!Directory.Exists(_imagesDir)) Directory.CreateDirectory(_imagesDir);

            var fileName = Path.GetFileName(NewAdImagePath);
            var destPath = Path.Combine(_imagesDir, fileName);
            File.Copy(NewAdImagePath, destPath, true);

            if (AdImages.Any(x => x.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                return;

            AdImages.Add(new AdImage { FileName = fileName, IsHidden = false });
            SaveOrderJson();

            NewAdImagePath = "";
        }

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

        // json 저장
        private void SaveOrderJson()
        {
            var metaList = AdImages.Select(x => new AdImageMeta { FileName = x.FileName, IsHidden = x.IsHidden }).ToList();
            File.WriteAllText(_orderJsonFile, JsonSerializer.Serialize(metaList, new JsonSerializerOptions { WriteIndented = true }));
        }

        // json 불러오기
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

            // 1. json에 정의된 순서대로
            foreach (var meta in metaList)
            {
                var abs = Path.Combine(_imagesDir, meta.FileName);
                if (File.Exists(abs))
                    AdImages.Add(new AdImage { FileName = meta.FileName, IsHidden = meta.IsHidden });
            }
            // 2. 폴더 내 파일 중 빠진 것 보충 (신규파일)
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

    public class AdImageMeta
    {
        public string FileName { get; set; }
        public bool IsHidden { get; set; }
    }

    public class AdImage : ObservableObject
    {
        public string FileName { get; set; }
        public bool IsHidden { get; set; }

        // 락 없는 썸네일
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
