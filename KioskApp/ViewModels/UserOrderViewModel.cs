using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using KioskApp.Models;
using KioskApp.Repositories;

namespace KioskApp.ViewModels
{
    public partial class UserOrderViewModel : ObservableObject
    {
        public ObservableCollection<Category> Categories { get; }
        public ObservableCollection<Menu> Menus { get; } = new();
        public ObservableCollection<OrderItem> OrderItems { get; } = new();

        [ObservableProperty]
        private Category selectedCategory;

        [ObservableProperty]
        private Menu selectedMenu;

        private readonly CategoryRepository _categoryRepo;
        private readonly MenuRepository _menuRepo;

        public int TotalQuantity => OrderItems.Sum(x => x.Quantity);
        public int TotalPrice => OrderItems.Sum(x => x.UnitPrice * x.Quantity);

        public UserOrderViewModel()
        {
            _categoryRepo = new CategoryRepository();
            _menuRepo = new MenuRepository();

            // DB에서 카테고리 불러오기
            Categories = new ObservableCollection<Category>(_categoryRepo.GetAll());
            SelectedCategory = Categories.FirstOrDefault();

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedCategory))
                    UpdateMenus();
            };
            OrderItems.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(TotalQuantity));
                OnPropertyChanged(nameof(TotalPrice));
            };
            UpdateMenus();
        }

        private void UpdateMenus()
        {
            Menus.Clear();
            if (SelectedCategory == null) return;

            var menus = _menuRepo.GetByCategory(SelectedCategory.CategoryId);
            foreach (var m in menus)
                Menus.Add(m);
        }

        // 메뉴를 장바구니에 담기 (카드 클릭 시)
        public void AddToCart(Menu menu, string optionText, int unitPrice, int quantity)
        {
            var item = OrderItems.FirstOrDefault(x => x.MenuId == menu.MenuId && x.OptionText == optionText);
            if (item != null)
                item.Quantity += quantity;
            else
                OrderItems.Add(new OrderItem
                {
                    MenuId = menu.MenuId,
                    MenuName = menu.Name,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    OptionText = optionText
                });
            OnPropertyChanged(nameof(TotalQuantity));
            OnPropertyChanged(nameof(TotalPrice));
        }

        [RelayCommand]
        public void ClearOrder() => OrderItems.Clear();

        [RelayCommand]
        public void Order()
        {
            // TODO: 주문 처리 (DB저장, 완료 메시지 등)
            System.Windows.MessageBox.Show("주문이 완료되었습니다!");
            OrderItems.Clear();
        }
    }
}
