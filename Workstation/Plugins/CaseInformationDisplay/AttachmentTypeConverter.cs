using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using L3.Cargo.Common;

namespace L3.Cargo.Workstation.Plugins.CaseInformationDisplay
{
    public class AttachmentTypeConverter : IValueConverter
    {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
        {
            AttachmentType attachmentType = (AttachmentType)value;
            string translated = L3.Cargo.Common.Resources.ResourceManager.GetString(attachmentType.ToString(), CultureResources.getCultureSetting());
            if (String.IsNullOrEmpty(translated))
            {
                // there is no translation for the type
                translated = attachmentType.ToString();
            }
            return translated;
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value;
        }
    }
}
