using System;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;

namespace LoginManager
{
    public class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color color = Colors.Red;

            if ((bool)value)
            {
                color = Colors.Green;
            }

            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
