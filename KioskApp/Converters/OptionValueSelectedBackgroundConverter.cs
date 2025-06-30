using KioskApp.Models;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace KioskApp.Converters
{
    // MultiBinding: [MenuOptionValue, MenuOption.SelectedValue]를 받아 선택된 옵션이면 파란 배경, 아니면 흰색 배경 반환
    public class OptionValueSelectedBackgroundConverter : IMultiValueConverter
    {
        // 선택된 값이면 연한 파랑, 아니면 흰색 반환
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var value = values[0] as MenuOptionValue;
            var selected = values[1] as MenuOptionValue;

            // 값이 없으면 흰색 반환 (예외처리)
            if (value == null || selected == null)
            {
                return Brushes.White;
            }

            // 선택된 옵션이면 연한 파랑, 아니면 흰색
            return value.OptionValueId == selected.OptionValueId ? (SolidColorBrush) (new BrushConverter().ConvertFrom("#FFE8F3FF")) : Brushes.White;
        }

        // 역변환은 사용 안 함
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
