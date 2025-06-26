using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using KioskApp.Models;
using KioskApp.Repositories;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows;

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

        // 타이머 관련 필드/프로퍼티
        private DispatcherTimer _timer;
        private int _remainSeconds = 120;
        public int RemainSeconds
        {
            get => _remainSeconds;
            set => SetProperty(ref _remainSeconds, value);
        }

        [RelayCommand]
        public void GoHome()
        {
            StopTimer();
            GoHomeRequested?.Invoke();
        }
        public UserOrderViewModel()
        {
            _categoryRepo = new CategoryRepository();
            _menuRepo = new MenuRepository();

            // DB에서 카테고리 불러오기
            Categories = new ObservableCollection<Category>(_categoryRepo.GetAll());
            SelectedCategory = Categories.FirstOrDefault();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            StartTimer();

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedCategory))
                    UpdateMenus();
            };
            OrderItems.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(TotalQuantity));
                OnPropertyChanged(nameof(TotalPrice));
                StartTimer(); // 주문(장바구니) 변경시 타이머 리셋 추가
            };

            UpdateMenus();
        }

        private void UpdateMenus()
        {
            Menus.Clear();
            if (SelectedCategory == null) return;

            var menus = _menuRepo.GetByCategory(SelectedCategory.CategoryId);
            foreach (var m in menus)
            {
                // 이미지 경로가 비었으면 디폴트로 세팅
                if (string.IsNullOrWhiteSpace(m.ImagePath))
                    m.ImagePath = "Images/default.png";

                // 상대경로면 절대경로로 변환
                if (!System.IO.Path.IsPathRooted(m.ImagePath))
                    m.ImagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, m.ImagePath);

                Menus.Add(m);
            }
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
            StartTimer();
        }

        // 상품 감소
        [RelayCommand]
        public void IncreaseOrderItemQty(OrderItem item)
        {
            if (item == null) return;
            item.Quantity++;
            OnPropertyChanged(nameof(TotalQuantity));
            OnPropertyChanged(nameof(TotalPrice));
            StartTimer();
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
            StartTimer();
        }

        private void OrderItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OrderItem.Quantity) || e.PropertyName == nameof(OrderItem.TotalPrice))
            {
                OnPropertyChanged(nameof(TotalQuantity));
                OnPropertyChanged(nameof(TotalPrice));
            }
        }

        // 타이머 Tick 메서드
        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (RemainSeconds > 0)
            {
                RemainSeconds--;
            }
            else
            {
                StopTimer();
                GoHome();
            }
        }

        [RelayCommand]
        public void ClearOrder()
        {
            OrderItems.Clear();
            StartTimer();
        }

        public void StartTimer()
        {
            RemainSeconds = 15;
            _timer.Start();
        }
        public void StopTimer()
        {
            _timer.Stop();
        }

        public Action<ObservableCollection<OrderItem>> GoOrderConfirmRequested { get; set; }

        [RelayCommand]
        public void Order()
        {
            if (OrderItems.Count == 0)
            {
                MessageBox.Show("주문할 메뉴를 1개 이상 선택해주세요!", "알림", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            StopTimer();
            // 주문확인(옵션/수량 포함) 화면으로 넘어감
            GoOrderConfirmRequested?.Invoke(OrderItems);
        }


    }
}
