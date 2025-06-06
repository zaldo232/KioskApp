using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using KioskApp.Models;
using KioskApp.Repositories;
using Microsoft.Win32;

namespace KioskApp.ViewModels
{
    public partial class AdminCategoryMenuViewModel : ObservableObject
    {
        // 카테고리/메뉴
        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<Menu> Menus { get; } = new();

        [ObservableProperty] private Category selectedCategory;
        [ObservableProperty] private Menu selectedMenu;
        [ObservableProperty] private string newCategoryName;
        [ObservableProperty] private string newMenuName;
        [ObservableProperty] private string newMenuDesc;
        [ObservableProperty] private int newMenuPrice;
        [ObservableProperty] private string newMenuImagePath;

        private readonly CategoryRepository categoryRepo = new();
        private readonly MenuRepository menuRepo = new();

        // 옵션
        private readonly MenuOptionRepository optionRepo = new();

        [ObservableProperty] private ObservableCollection<MenuOption> menuOptions = new();
        [ObservableProperty] private MenuOption selectedMenuOption;
        [ObservableProperty] private string newOptionName;
        [ObservableProperty] private bool newOptionIsRequired;
        [ObservableProperty] private string newOptionImagePath;

        // 옵션값(선택지)
        [ObservableProperty] private string newOptionValueLabel;
        [ObservableProperty] private int newOptionExtraPrice;

        // 부모(MainWindowViewModel)에서 콜백 세팅
        public Action GoHomeRequested { get; set; }

        [RelayCommand]
        public void GoHome()
        {
            GoHomeRequested?.Invoke();
        }

        public AdminCategoryMenuViewModel()
        {
            LoadCategories();
        }

        // 카테고리 선택시 메뉴 로드
        partial void OnSelectedCategoryChanged(Category value)
        {
            LoadMenus();
            MenuOptions.Clear();
            SelectedMenu = null;
        }

        // 메뉴 선택시 옵션 로드
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

        public void LoadCategories()
        {
            Categories.Clear();
            foreach (var cat in categoryRepo.GetAll())
                Categories.Add(cat);
        }

        public void LoadMenus()
        {
            Menus.Clear();
            if (SelectedCategory == null) return;
            foreach (var menu in menuRepo.GetByCategory(SelectedCategory.CategoryId))
                Menus.Add(menu);
        }

        // 카테고리 CRUD
        [RelayCommand]
        public void AddCategory()
        {
            if (string.IsNullOrWhiteSpace(NewCategoryName)) return;
            categoryRepo.Add(NewCategoryName);
            NewCategoryName = "";
            LoadCategories();
        }

        [RelayCommand]
        public void UpdateCategory()
        {
            if (SelectedCategory == null || string.IsNullOrWhiteSpace(NewCategoryName)) return;
            SelectedCategory.Name = NewCategoryName;
            categoryRepo.Update(SelectedCategory);
            LoadCategories();
        }

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

        // 메뉴 CRUD 
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

        [RelayCommand]
        public void DeleteMenu()
        {
            if (SelectedMenu == null) return;
            menuRepo.Delete(SelectedMenu.MenuId);
            LoadMenus();
            MenuOptions.Clear();
            SelectedMenuOption = null;
        }

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

        // 옵션 관리

        public void LoadMenuOptions()
        {
            MenuOptions.Clear();
            if (SelectedMenu == null) return;
            foreach (var o in optionRepo.GetByMenuId(SelectedMenu.MenuId))
                MenuOptions.Add(o);
        }

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

        [RelayCommand]
        public void UpdateMenuOption()
        {
            if (SelectedMenuOption == null) return;
            optionRepo.Update(SelectedMenuOption);
            // UI 갱신 필요시 LoadMenuOptions(); 호출해도 됨
        }

        [RelayCommand]
        public void DeleteMenuOption()
        {
            if (SelectedMenuOption == null) return;
            optionRepo.Delete(SelectedMenuOption.OptionId);
            MenuOptions.Remove(SelectedMenuOption);
            SelectedMenuOption = null;
        }

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

        [RelayCommand]
        public void DeleteOptionValue(MenuOptionValue value)
        {
            if (SelectedMenuOption == null || value == null) return;
            optionRepo.DeleteValue(value.OptionValueId);
            SelectedMenuOption.Values.Remove(value);
        }

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
