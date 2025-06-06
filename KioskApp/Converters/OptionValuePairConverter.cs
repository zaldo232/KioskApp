using System;
using System.Globalization;
using System.Windows.Data;
using KioskApp.Models;

namespace KioskApp.Converters
{
    // (MenuOption, MenuOptionValue) 튜플 커맨드 파라미터로 넘기기
    public class OptionValuePairConverter : IMultiValueConverter, IValueConverter
    {
        // For Button CommandParameter
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is MenuOption option && values[1] is MenuOptionValue value)
                return (option, value);
            return null;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

        // For single value case
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MenuOption option && parameter is MenuOptionValue val)
                return (option, val);
            return null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
