using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace KioskApp.Converters
{
    public class PriceToColorAndTextConverter : IValueConverter
    {
        // 가격(int) -> 텍스트(예: +1,000원) 또는 색상(초록/빨강/검정) 변환 컨버터
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            int price = 0;
            if (value is int i) price = i;

            // Text/Color 동시 반환용 튜플: (text, color), 파라미터가 "color"면 색상 반환 아니면 텍스트 반환
            if (parameter as string == "color")
            {
                if (price > 0) return Brushes.Green;    // 추가금: 초록색
                if (price < 0) return Brushes.Red;      // 할인/마이너스: 빨강
                return Brushes.Black;                   // 0원: 검정
            }
            else 
            {
                if (price > 0) return $"+{price:N0}원";  // +1,000원 형식
                if (price < 0) return $"{price:N0}원";   // -1,000원 형식
                return "0원";                            // 0원
            }
        }

        // 역변환은 사용 안 함
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
