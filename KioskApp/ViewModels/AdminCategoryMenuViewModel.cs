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

        public AdminCategoryMenuViewModel()
        {
            LoadCategories();
        }

        partial void OnSelectedCategoryChanged(Category value)
        {
            LoadMenus();
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
        }

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

        // 메뉴 누를때 그 값 가져오게
        partial void OnSelectedMenuChanged(Menu value)
        {
            if (value != null)
            {
                NewMenuName = value.Name;
                NewMenuDesc = value.Description;
                NewMenuPrice = value.Price;
                NewMenuImagePath = value.ImagePath;
            }
            else
            {
                NewMenuName = "";
                NewMenuDesc = "";
                NewMenuPrice = 0;
                NewMenuImagePath = "";
            }
        }

    }
}
