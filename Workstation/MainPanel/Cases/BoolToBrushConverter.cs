using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;

namespace L3.Cargo.Workstation.MainPanel.Cases
{
    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isTrue = System.Convert.ToBoolean(value);

            if (isTrue)
            {
                return Brushes.Green;
            }
            else
            {
                return Brushes.Red;
            }
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brushes brush = value as Brushes;

            if (brush != null && brush.Equals(Brushes.Green))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
