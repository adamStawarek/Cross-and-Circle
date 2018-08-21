using System;
using System.Globalization;
using System.Windows.Data;

namespace SimpleGame.Helpers
{
    public class InverseBoolConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            return parameter != null && (value != null && (!((bool) value||(bool)parameter)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
