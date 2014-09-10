using System;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using System.Windows;
using l3.cargo.corba;

namespace LoginManager
{
    public class BooleanToModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool ret = false;

            if (((string)value) == (string)parameter)
            {
                ret = true;
            }

            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string ret = "Operator";

            if ((bool)value)
            {
                ret = (string)parameter;
            }

            return ret;
        }
    }
}
