using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using KioskApp.Models;
using System.Linq;
using KioskApp.Repositories;

namespace KioskApp.ViewModels
{
    public partial class UserOrderViewModel : ObservableObject
    {
        public ObservableCollection<Category> Categories { get; }
        public ObservableCollection<Menu> Menus { get; } = new();

        [ObservableProperty]
        private Category selectedCategory;

        [ObservableProperty]
        private Menu selectedMenu;

        private readonly CategoryRepository _categoryRepo;
        private readonly MenuRepository _menuRepo;

        public UserOrderViewModel()
        {
            // DB 연결 문자열(전역에서 가져오거나 상수로 관리)
            _categoryRepo = new CategoryRepository();
            _menuRepo = new MenuRepository();

            // DB에서 카테고리 불러오기
            Categories = new ObservableCollection<Category>(_categoryRepo.GetAll());
            SelectedCategory = Categories.FirstOrDefault();

            // 카테고리 변경 이벤트 연결
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedCategory))
                    UpdateMenus();
            };
            UpdateMenus();
        }

        private void UpdateMenus()
        {
            Menus.Clear();
            if (SelectedCategory == null) return;

            // DB에서 메뉴 불러오기
            var menus = _menuRepo.GetByCategory(SelectedCategory.CategoryId);
            foreach (var m in menus)
                Menus.Add(m);
        }
    }
}
