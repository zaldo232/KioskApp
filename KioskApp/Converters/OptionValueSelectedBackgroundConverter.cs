using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using KioskApp.Models;

namespace KioskApp.Converters
{
    // MultiBinding: [MenuOptionValue, MenuOption.SelectedValue]
    public class OptionValueSelectedBackgroundConverter : IMultiValueConverter
    {
        // 선택된 경우 연한 파랑, 아니면 흰색 등으로
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var value = values[0] as MenuOptionValue;
            var selected = values[1] as MenuOptionValue;

            // value, selected가 null일 경우 예외처리
            if (value == null || selected == null)
                return Brushes.White;

            return value.OptionValueId == selected.OptionValueId
                ? (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFE8F3FF"))
                : Brushes.White;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
