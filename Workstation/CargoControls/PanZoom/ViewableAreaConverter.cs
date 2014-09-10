using System;
using System.Globalization;
using System.Windows.Data;

namespace L3.Cargo.Controls
{
    internal class ViewableAreaConverter : IMultiValueConverter
    {
        #region Private Members

        private double zoom;

        #endregion Private Members


        #region Public Methods

        public object Convert (object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            zoom = (double)values[1];
            return (double)values[0] / zoom;
        }

        public object[] ConvertBack (object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[2] { ((double)value * zoom), zoom };
        }

        #endregion Public Methods
    }
}
