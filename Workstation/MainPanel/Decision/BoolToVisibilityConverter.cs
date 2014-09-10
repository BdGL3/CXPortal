using System;
using System.Windows;
using System.Globalization;
using System.Windows.Data;
using L3.Cargo.Common;

namespace L3.Cargo.Workstation.MainPanel.Decision
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility ret = Visibility.Collapsed;
            try
            {
                ret = (System.Convert.ToBoolean(value)) ? Visibility.Visible : Visibility.Collapsed;
            }
            catch
            {
                //TODO:  Log error here
            }

            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool ret = false;

            try
            {
                Visibility visibility = (Visibility)value;

                if (visibility == Visibility.Visible)
                {
                    ret = true;
                }
            }
            catch
            {
                //TODO:  Log error here
            }
         
            return ret;
        }
    }
}
