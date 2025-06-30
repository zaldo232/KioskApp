using System.Windows;

namespace KioskApp.Views
{
    // 경고/알림 팝업 다이얼로그
    public partial class AlertDialog : Window
    {
        // 일반 팝업 생성자
        public AlertDialog(string message)
        {
            InitializeComponent();
            MessageText.Text = message; // 메시지 바인딩
        }

        // 소유자(owner) 지정 가능 팝업 생성자
        public AlertDialog(string message, Window owner = null)
        {
            InitializeComponent();
            MessageText.Text = message;
            if (owner != null)
            { 
                this.Owner = owner; // 부모 윈도우(모달 띄우기)
            }
        }

        // 확인 버튼 클릭 시 닫기
        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}