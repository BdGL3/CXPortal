using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;

namespace LoginManager
{
    public class ButtonIsEnabledConverter : IMultiValueConverter
    {
        public object Convert (object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 3) return false;
            if (!(values[0] is bool) && !(values[1] is string) && !(values[2] is string)) return false;

            bool ret = false;

            if ((bool)values[0] && !String.IsNullOrWhiteSpace((string)values[1]) && !String.IsNullOrWhiteSpace((string)values[2]))
            {
                ret = true;
            }

            return ret;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
