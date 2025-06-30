using KioskApp.ViewModels;
using System.Windows.Controls;

namespace KioskApp.Views
{
    // 관리자 카테고리/메뉴/옵션 관리 화면 코드비하인드
    public partial class AdminCategoryMenuView : UserControl
    {
        public AdminCategoryMenuView()
        {
            InitializeComponent();  // XAML UI 초기화
            this.DataContext = new AdminCategoryMenuViewModel();    // ViewModel 바인딩(테스트/단독 호출용)
        }   
    }
}