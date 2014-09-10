using System;
using System.Globalization;
using System.Windows.Data;
using L3.Cargo.Common;

namespace L3.Cargo.Communications.Interfaces
{
    [ValueConversion(typeof(Int32), typeof(String))]
    public class IntToAWSDecisionStringConverter: IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            try
            {
                Int32 DecisionValue = (Int32)value;

                return Enum.GetName(typeof(WorkstationDecision), DecisionValue);
            }
            catch (InvalidCastException)
            {
                return Enum.GetName(typeof(WorkstationDecision), 0);
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = value as string;           
            return System.Convert.ToInt32(strValue);
        }
    }
}
