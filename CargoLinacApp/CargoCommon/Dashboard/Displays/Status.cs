using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace L3.Cargo.Common.Dashboard.Display
{
    public class Status
    {
        public UIElement Indicator;

        public UIElement ErrorMessages;

        public UIElement WarningMessages;

        public Adorner TroubleShooting;

        public Status()
        {
            // Not available for initial release
            //ErrorMessages.MouseLeftButtonDown += new MouseButtonEventHandler(ErrorMessages_MouseLeftButtonDown);
            //WarningMessages.MouseLeftButtonDown += new MouseButtonEventHandler(WarningMessages_MouseLeftButtonDown);
        }

        void WarningMessages_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TroubleShooting.Visibility = Visibility.Visible;
        }

        void ErrorMessages_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TroubleShooting.Visibility = Visibility.Visible;
        }
    }
}
