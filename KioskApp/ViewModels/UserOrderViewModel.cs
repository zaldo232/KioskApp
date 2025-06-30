using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using KioskApp.Models;
using KioskApp.Repositories;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows;
using KioskApp.Views;

namespace KioskApp.ViewModels
{
    // 사용자 주문(메뉴선택+장바구니) 뷰모델
    public partial class UserOrderViewModel : ObservableObject
    {
        public ObservableCollection<Category> Categories { get; }                   // 카테고리 리스트
        public ObservableCollection<Menu> Menus { get; } = new();                   // 메뉴 리스트
        public ObservableCollection<OrderItem> OrderItems { get; } = new();         // 장바구니(주문 항목)

        [ObservableProperty] private Category selectedCategory;                     // 선택된 카테고리
        [ObservableProperty] private Menu selectedMenu;                             // 선택된 메뉴


        private readonly CategoryRepository _categoryRepo;
        private readonly MenuRepository _menuRepo;

        public int TotalQuantity => OrderItems.Sum(x => x.Quantity);                // 총 주문수량
        public int TotalPrice => OrderItems.Sum(x => x.UnitPrice * x.Quantity);     // 총 주문금액

        // 화면전환 콜백(홈)
        public Action GoHomeRequested { get; set; }

        // 타이머(2분)
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

            // 타이머 초기화/시작
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            StartTimer();

            // 카테고리 선택 시 메뉴 갱신
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedCategory))
                { 
                    UpdateMenus(); 
                }
            };

            // 장바구니 변경 시 합계 갱신/타이머 리셋
            OrderItems.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(TotalQuantity));
                OnPropertyChanged(nameof(TotalPrice));
                StartTimer(); // 주문(장바구니) 변경시 타이머 리셋 추가
            };

            UpdateMenus();
        }

        // 카테고리에 따른 메뉴 목록 갱신
        private void UpdateMenus()
        {
            Menus.Clear();
            if (SelectedCategory == null) return;

            var menus = _menuRepo.GetByCategory(SelectedCategory.CategoryId);
            foreach (var m in menus)
            {
                // 이미지 경로가 비었으면 디폴트로 세팅
                if (string.IsNullOrWhiteSpace(m.ImagePath))
                {
                    m.ImagePath = "Images/default.png"; 
                }

                // 상대경로면 절대경로로 변환
                if (!System.IO.Path.IsPathRooted(m.ImagePath))
                { 
                    m.ImagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, m.ImagePath); 
                }

                Menus.Add(m);
            }
        }

        // 메뉴 장바구니 담기 (동일메뉴+옵션 합치기)
        public void AddToCart(Menu menu, string optionText, int unitPrice, int quantity)
        {
            // 동일 메뉴+옵션이면 수량만 증가
            var item = OrderItems.FirstOrDefault(x => x.MenuId == menu.MenuId && x.OptionText == optionText);
            if (item != null)
            {
                item.Quantity += quantity;
                // 수량이 0 이하가 되면 삭제(예외 케이스)
                if (item.Quantity <= 0)
                { 
                    OrderItems.Remove(item); 
                }
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

        // 수량 증가
        [RelayCommand]
        public void IncreaseOrderItemQty(OrderItem item)
        {
            if (item == null) return;
            item.Quantity++;
            OnPropertyChanged(nameof(TotalQuantity));
            OnPropertyChanged(nameof(TotalPrice));
            StartTimer();
        }

        // 수량 감소(0이하일 경우 삭제)
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

        // 상품 삭제
        [RelayCommand]
        public void DeleteOrderItem(OrderItem item)
        {
            if (item == null) return;
            OrderItems.Remove(item);
            OnPropertyChanged(nameof(TotalQuantity));
            OnPropertyChanged(nameof(TotalPrice));
        }

        // (장바구니 항목 변경 감지용)
        private void OrderItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OrderItem.Quantity) || e.PropertyName == nameof(OrderItem.TotalPrice))
            {
                OnPropertyChanged(nameof(TotalQuantity));
                OnPropertyChanged(nameof(TotalPrice));
            }
        }

        // 1초마다 타이머 이벤트
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

        // 장바구니 전체 비우기
        [RelayCommand]
        public void ClearOrder()
        {
            OrderItems.Clear();
            StartTimer();
        }

        // 타이머 리셋
        public void StartTimer()
        {
            RemainSeconds = 120;
            _timer.Start();
        }
        // 타이머 리셋
        public void StopTimer()
        {
            _timer.Stop();
        }

        // 주문확인(장바구니→주문확인) 이동 콜백
        public Action<ObservableCollection<OrderItem>> GoOrderConfirmRequested { get; set; }

        // 주문확인 이동 (장바구니 비어있으면 경고)
        [RelayCommand]
        public void Order()
        {
            if (OrderItems.Count == 0)
            {
                StopTimer(); // 알림 전 타이머 중단

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var dialog = new AlertDialog("주문할 메뉴를 1개 이상 선택해주세요!", Application.Current.MainWindow);
                    dialog.ShowDialog();
                });

                StartTimer(); // 알림 닫힌 후 타이머 재시작

                return;
            }
            StopTimer();
            // 주문확인(옵션/수량 포함) 화면으로 넘어감
            GoOrderConfirmRequested?.Invoke(OrderItems);
        }


    }
}
