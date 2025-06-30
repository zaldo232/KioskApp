using KioskApp.Models;
using System.Globalization;
using System.Windows.Data;

namespace KioskApp.Converters
{
    // (MenuOption, MenuOptionValue) 튜플을 커맨드 파라미터로 넘기기 위한 컨버터
    public class OptionValuePairConverter : IMultiValueConverter, IValueConverter
    {
        // 여러 값([MenuOption, MenuOptionValue])을 받아 튜플로 반환 (멀티바인딩용)
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // 둘 다 있으면 튜플로 반환
            if (values.Length == 2 && values[0] is MenuOption option && values[1] is MenuOptionValue value)
            {
                return (option, value);
            }

            return null;
        }

        // 멀티바인딩 역변환은 사용하지 않음
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();

        // 단일 값(MenuOption)과 파라미터(MenuOptionValue)를 받아 튜플로 반환 (싱글바인딩용)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 둘 다 있으면 튜플로 반환
            if (value is MenuOption option && parameter is MenuOptionValue val)
            {
                return (option, val);
            }
            return null;
        }

        // 싱글바인딩 역변환은 사용하지 않음
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
