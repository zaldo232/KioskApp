using KioskApp.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KioskApp.Converters
{
    // MultiBinding: [MenuOptionValue, MenuOption.SelectedValue, Button]을 받아 선택 여부에 따라 다른 스타일 반환 (버튼 스타일 변경용)
    public class OptionValueSelectedStyleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var value = values[0] as MenuOptionValue;
            var selected = values[1] as MenuOptionValue;

            // 선택된 옵션이면 "SelectedOptionCardButtonStyle", 아니면 "OptionCardButtonStyle" 리턴, 스타일 리소스명 (Selected/Default)
            string key = (value != null && selected != null && value.OptionValueId == selected.OptionValueId) ? "SelectedOptionCardButtonStyle" : "OptionCardButtonStyle";
            
            // Application 자원에서 해당 스타일 반환
            return Application.Current.Resources[key] as Style;
        }

        // 역변환은 안 씀
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
