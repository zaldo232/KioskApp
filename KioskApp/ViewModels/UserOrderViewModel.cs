using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using KioskApp.Models;
using KioskApp.Repositories;
using System.ComponentModel;

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

        // 부모(MainWindowViewModel)에서 콜백 세팅
        public Action GoHomeRequested { get; set; }

        [RelayCommand]
        public void GoHome()
        {
            GoHomeRequested?.Invoke();
        }
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
            // 같은 메뉴 + 같은 옵션 조합이 있는지 찾음
            var item = OrderItems.FirstOrDefault(x => x.MenuId == menu.MenuId && x.OptionText == optionText);
            if (item != null)
            {
                item.Quantity += quantity;
                // 혹시 수량이 0 이하로 되면 삭제 (일반적으로 - 담기에서는 안 일어나지만, 혹시 음수 담기 허용시)
                if (item.Quantity <= 0)
                    OrderItems.Remove(item);
            }
            else
            {
                if (quantity > 0) // 0 이하 수량은 아예 추가 안 함
                    OrderItems.Add(new OrderItem
                    {
                        MenuId = menu.MenuId,
                        MenuName = menu.Name,
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        OptionText = optionText
                    });
            }
            OnPropertyChanged(nameof(TotalQuantity));
            OnPropertyChanged(nameof(TotalPrice));
        }

        // 상품 감소
        [RelayCommand]
        public void IncreaseOrderItemQty(OrderItem item)
        {
            if (item == null) return;
            item.Quantity++;
            OnPropertyChanged(nameof(TotalQuantity));
            OnPropertyChanged(nameof(TotalPrice));
        }

        // 상품 증가
        [RelayCommand]
        public void DecreaseOrderItemQty(OrderItem item)
        {
            if (item == null) return;
            item.Quantity--;
            if (item.Quantity <= 0)
                OrderItems.Remove(item);
            OnPropertyChanged(nameof(TotalQuantity));
            OnPropertyChanged(nameof(TotalPrice));
        }

        private void OrderItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OrderItem.Quantity) || e.PropertyName == nameof(OrderItem.TotalPrice))
            {
                OnPropertyChanged(nameof(TotalQuantity));
                OnPropertyChanged(nameof(TotalPrice));
            }
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
