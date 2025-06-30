using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KioskApp.Converters
{
    // int 값이 0이 아니면 Visible, 0이면 Collapsed로 변환하는 컨버터
    public class IntToVisibilityConverter : IValueConverter
    {
        // int가 0이 아니면 Visible, 0이면 Collapsed 반환
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is int i && i != 0 ? Visibility.Visible : Visibility.Collapsed;

        // 역변환은 사용 안 함 (예외 발생)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
