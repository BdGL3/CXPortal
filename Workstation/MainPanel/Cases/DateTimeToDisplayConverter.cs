using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using L3.Cargo.Communications.Interfaces;
using L3.Cargo.Common;

namespace L3.Cargo.Workstation.MainPanel.Cases
{
    public class DateTimeToDisplayConverter : IValueConverter
    {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime time = (DateTime)value;
            return CultureResources.ConvertDateTimeToStringForDisplay(time);
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value;
        }
    }
}
