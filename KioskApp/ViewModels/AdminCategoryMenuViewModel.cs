using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using KioskApp.Models;
using KioskApp.Repositories;
using Microsoft.Win32;

namespace KioskApp.ViewModels
{
    // 관리자 카테고리/메뉴/옵션 관리 뷰모델
    public partial class AdminCategoryMenuViewModel : ObservableObject
    {
        // 카테고리/메뉴 바인딩 리스트
        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<Menu> Menus { get; } = new();

        [ObservableProperty] private Category selectedCategory;  // 선택된 카테고리
        [ObservableProperty] private Menu selectedMenu;          // 선택된 메뉴
        [ObservableProperty] private string newCategoryName;     // 신규 카테고리명
        [ObservableProperty] private string newMenuName;         // 신규 메뉴명
        [ObservableProperty] private string newMenuDesc;         // 신규 메뉴 설명
        [ObservableProperty] private int newMenuPrice;           // 신규 메뉴 가격
        [ObservableProperty] private string newMenuImagePath;    // 신규 메뉴 이미지 경로

        private readonly CategoryRepository categoryRepo = new();
        private readonly MenuRepository menuRepo = new();

        // 옵션 관련
        private readonly MenuOptionRepository optionRepo = new();

        [ObservableProperty] private ObservableCollection<MenuOption> menuOptions = new();
        [ObservableProperty] private MenuOption selectedMenuOption;      // 선택된 옵션
        [ObservableProperty] private string newOptionName;               // 신규 옵션명
        [ObservableProperty] private bool newOptionIsRequired;           // 신규 옵션 필수여부
        [ObservableProperty] private string newOptionImagePath;          // 신규 옵션 이미지 경로

        // 옵션값(선택지)
        [ObservableProperty] private string newOptionValueLabel;         // 신규 옵션값 라벨
        [ObservableProperty] private int newOptionExtraPrice;            // 신규 옵션값 추가금

        // 부모(MainWindowViewModel)에서 콜백 세팅
        public Action GoHomeRequested { get; set; }
        public Action GoAdImageRequested { get; set; }
        
        // 홈으로 이동
        [RelayCommand]
        public void GoHome() => GoHomeRequested?.Invoke();
        
        // 메뉴로 이동
        [RelayCommand]
        public void GoAdImage() => GoAdImageRequested?.Invoke();
        

        public AdminCategoryMenuViewModel()
        {
            LoadCategories();       // 시작시 카테고리 목록 불러오기
        }

        // 카테고리 선택시 해당 메뉴 불러옴
        partial void OnSelectedCategoryChanged(Category value)
        {
            LoadMenus();
            MenuOptions.Clear();
            SelectedMenu = null;
        }

        // 메뉴 선택시 상세/옵션 불러옴
        partial void OnSelectedMenuChanged(Menu value)
        {
            if (value != null)
            {
                NewMenuName = value.Name;
                NewMenuDesc = value.Description;
                NewMenuPrice = value.Price;
                NewMenuImagePath = value.ImagePath;
                LoadMenuOptions();
            }
            else
            {
                NewMenuName = "";
                NewMenuDesc = "";
                NewMenuPrice = 0;
                NewMenuImagePath = "";
                MenuOptions.Clear();
                SelectedMenuOption = null;
            }
        }

        // 카테고리 전체 로드
        public void LoadCategories()
        {
            Categories.Clear();
            foreach (var cat in categoryRepo.GetAll())
                Categories.Add(cat);
        }

        // 선택된 카테고리의 메뉴 전체 로드
        public void LoadMenus()
        {
            Menus.Clear();
            if (SelectedCategory == null) return;
            foreach (var menu in menuRepo.GetByCategory(SelectedCategory.CategoryId))
                Menus.Add(menu);
        }

        // 카테고리 추가
        [RelayCommand]
        public void AddCategory()
        {
            if (string.IsNullOrWhiteSpace(NewCategoryName)) return;
            categoryRepo.Add(NewCategoryName);
            NewCategoryName = "";
            LoadCategories();
        }

        // 카테고리 수정
        [RelayCommand]
        public void UpdateCategory()
        {
            if (SelectedCategory == null || string.IsNullOrWhiteSpace(NewCategoryName)) return;
            SelectedCategory.Name = NewCategoryName;
            categoryRepo.Update(SelectedCategory);
            LoadCategories();
        }

        // 카테고리 삭제
        [RelayCommand]
        public void DeleteCategory()
        {
            if (SelectedCategory == null) return;
            categoryRepo.Delete(SelectedCategory.CategoryId);
            LoadCategories();
            Menus.Clear();
            MenuOptions.Clear();
            SelectedMenu = null;
            SelectedMenuOption = null;
        }

        // 메뉴 추가
        [RelayCommand]
        public void AddMenu()
        {
            if (SelectedCategory == null || string.IsNullOrWhiteSpace(NewMenuName)) return;
            var menu = new Menu
            {
                CategoryId = SelectedCategory.CategoryId,
                Name = NewMenuName,
                Description = NewMenuDesc,
                Price = NewMenuPrice,
                ImagePath = NewMenuImagePath
            };
            menuRepo.Add(menu);
            NewMenuName = NewMenuDesc = NewMenuImagePath = "";
            NewMenuPrice = 0;
            LoadMenus();
        }

        // 메뉴 수정
        [RelayCommand]
        public void UpdateMenu()
        {
            if (SelectedMenu == null || string.IsNullOrWhiteSpace(NewMenuName)) return;
            SelectedMenu.Name = NewMenuName;
            SelectedMenu.Description = NewMenuDesc;
            SelectedMenu.Price = NewMenuPrice;
            SelectedMenu.ImagePath = NewMenuImagePath;
            menuRepo.Update(SelectedMenu);
            LoadMenus();
        }

        // 메뉴 삭제
        [RelayCommand]
        public void DeleteMenu()
        {
            if (SelectedMenu == null) return;
            menuRepo.Delete(SelectedMenu.MenuId);
            LoadMenus();
            MenuOptions.Clear();
            SelectedMenuOption = null;
        }

        // 메뉴 이미지 파일 선택
        [RelayCommand]
        public void BrowseImage()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*"
            };
            if (dlg.ShowDialog() == true)
            {
                NewMenuImagePath = dlg.FileName;
            }
        }

        // 옵션 전체 로드
        public void LoadMenuOptions()
        {
            MenuOptions.Clear();
            if (SelectedMenu == null) return;
            foreach (var o in optionRepo.GetByMenuId(SelectedMenu.MenuId))
                MenuOptions.Add(o);
        }

        // 옵션 추가
        [RelayCommand]
        public void AddMenuOption()
        {
            if (SelectedMenu == null || string.IsNullOrWhiteSpace(NewOptionName)) return;
            var option = new MenuOption
            {
                MenuId = SelectedMenu.MenuId,
                OptionName = NewOptionName,
                IsRequired = NewOptionIsRequired
            };
            option.OptionId = optionRepo.Add(option);
            MenuOptions.Add(option);

            // 입력 초기화
            NewOptionName = "";
            NewOptionIsRequired = false;
        }

        // 옵션 수정
        [RelayCommand]
        public void UpdateMenuOption()
        {
            if (SelectedMenuOption == null) return;
            optionRepo.Update(SelectedMenuOption);
            // UI 갱신 필요시 LoadMenuOptions(); 호출해도 됨
        }

        // 옵션 삭제
        [RelayCommand]
        public void DeleteMenuOption()
        {
            if (SelectedMenuOption == null) return;
            optionRepo.Delete(SelectedMenuOption.OptionId);
            MenuOptions.Remove(SelectedMenuOption);
            SelectedMenuOption = null;
        }

        // 옵션값 추가
        [RelayCommand]
        public void AddOptionValue()
        {
            if (SelectedMenuOption == null || string.IsNullOrWhiteSpace(NewOptionValueLabel)) return;
            var value = new MenuOptionValue
            {
                OptionId = SelectedMenuOption.OptionId,
                ValueLabel = NewOptionValueLabel,
                ExtraPrice = NewOptionExtraPrice,
                ImagePath = NewOptionImagePath
            };

            value.OptionValueId = optionRepo.AddValue(value);
            SelectedMenuOption.Values.Add(value);
            NewOptionValueLabel = "";
            NewOptionExtraPrice = 0;
            NewOptionImagePath = "";
        }

        // 옵션값 삭제
        [RelayCommand]
        public void DeleteOptionValue(MenuOptionValue value)
        {
            if (SelectedMenuOption == null || value == null) return;
            optionRepo.DeleteValue(value.OptionValueId);
            SelectedMenuOption.Values.Remove(value);
        }

        // 옵션 이미지 파일 선택
        [RelayCommand]
        public void BrowseOptionImage()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*"
            };
            if (dlg.ShowDialog() == true)
            {
                NewOptionImagePath = dlg.FileName;
            }
        }

    }
}
