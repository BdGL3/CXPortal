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
    public class IntToDecisionConverter : IValueConverter
    {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
        {
            WorkstationDecision decision = (WorkstationDecision)value;
            string translated = L3.Cargo.Common.Resources.ResourceManager.GetString(decision.ToString(), CultureResources.getCultureSetting());
            if (String.IsNullOrEmpty(translated))
            {
                return decision.ToString();
            }
            else
            {
                return translated;
            }
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value;
        }
    }
}
