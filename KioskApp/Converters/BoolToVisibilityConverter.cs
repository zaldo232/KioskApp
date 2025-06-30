using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KioskApp.Converters
{
    // bool 값을 WPF의 Visibility로 변환하는 컨버터
    public class BoolToVisibilityConverter : IValueConverter
    {
        // true면 Visible, 아니면 Collapsed 반환
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (value is bool b && b) ? Visibility.Visible : Visibility.Collapsed;

        // Visible이면 true, 아니면 false 반환 (역변환)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is Visibility v && v == Visibility.Visible;
    }
}
