using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace KioskApp.Converters
{
    public class PriceToColorAndTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            int price = 0;
            if (value is int i) price = i;

            // Text/Color 동시 반환용 튜플: (text, color)
            if (parameter as string == "color")
            {
                if (price > 0) return Brushes.Green;
                if (price < 0) return Brushes.Red;
                return Brushes.Black;
            }
            else // Text
            {
                if (price > 0) return $"+{price:N0}원";
                if (price < 0) return $"{price:N0}원";
                return "0원";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
