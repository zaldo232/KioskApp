using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using KioskApp.Models;

namespace KioskApp.Converters
{
    // MultiBinding: [MenuOptionValue, MenuOption.SelectedValue, Button]
    public class OptionValueSelectedStyleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var value = values[0] as MenuOptionValue;
            var selected = values[1] as MenuOptionValue;

            // 스타일 리소스명 (Selected/Default)
            string key = (value != null && selected != null && value.OptionValueId == selected.OptionValueId)
                ? "SelectedOptionCardButtonStyle" : "OptionCardButtonStyle";
            // 반환: 해당 스타일
            return Application.Current.Resources[key] as Style;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
