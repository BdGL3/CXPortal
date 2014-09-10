using System;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using System.Windows;
using l3.cargo.corba;

namespace LoginManager
{
    public class AccessToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility ret = Visibility.Collapsed;

            AuthenticationLevel accessLevel = (AuthenticationLevel)value;
            AuthenticationLevel minAccessLevel = (AuthenticationLevel)Enum.Parse(typeof(AuthenticationLevel), (string)parameter, true);

            if ((accessLevel.CompareTo(minAccessLevel) >= 0 && accessLevel != AuthenticationLevel.NONE) ||
                (accessLevel.Equals(minAccessLevel)))
            {
                ret = Visibility.Visible;
            }


            //if ((AuthenticationLevel)value == AuthenticationLevel.ENGINEER && minAccessLevel != "NONE")
            //{
            //    ret = Visibility.Visible;
            //}
            //else if ((AuthenticationLevel)value == AuthenticationLevel.MAINTENANCE && minAccessLevel != "NONE" && minAccessLevel != "ENGINEER")
            //{
            //    ret = Visibility.Visible;
            //}
            //else if ((AuthenticationLevel)value == AuthenticationLevel.SUPERVISOR && minAccessLevel != "NONE" && minAccessLevel != "ENGINEER" && minAccessLevel != "MAINTENANCE")
            //{
            //    ret = Visibility.Visible;
            //}
            //else if ((AuthenticationLevel)value == AuthenticationLevel.OPERATOR && minAccessLevel != "NONE" && minAccessLevel != "ENGINEER" && minAccessLevel != "MAINTENANCE" && minAccessLevel != "SUPERVISOR")
            //{
            //    ret = Visibility.Visible;
            //}
            //else if ((AuthenticationLevel)value == AuthenticationLevel.NONE && minAccessLevel == "NONE")
            //{
            //    ret = Visibility.Visible;
            //}

            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
