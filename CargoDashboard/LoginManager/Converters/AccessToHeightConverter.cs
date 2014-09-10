using System;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using System.Windows;
using l3.cargo.corba;

namespace LoginManager
{
    public class AccessToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GridLength ret = new GridLength(0);

            AuthenticationLevel accessLevel = (AuthenticationLevel)value;
            AuthenticationLevel minAccessLevel = (AuthenticationLevel)Enum.Parse(typeof(AuthenticationLevel), (string)parameter, true);

            if ((accessLevel.CompareTo(minAccessLevel) >= 0 && accessLevel != AuthenticationLevel.NONE) ||
                (accessLevel.Equals(minAccessLevel)))
            {
                ret = new GridLength(1, GridUnitType.Star);
            }

            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
