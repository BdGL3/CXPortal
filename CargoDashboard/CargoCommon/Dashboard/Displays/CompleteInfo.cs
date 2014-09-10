using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace L3.Cargo.Common.Dashboard.Display
{
    public class CompleteInfo
    {
        private string _name;

        private UIElement _link;

        private UIElement _display;


        public string Name
        {
            get
            {
                return _name;
            }
        }

        public UIElement Link
        {
            get
            {
                return _link;
            }
        }

        public UIElement Display
        {
            get
            {
                return _display;
            }
        }

        public CompleteInfo (string name, UIElement link, UIElement display)
        {
            _name = name;
            _display = display;
            _link = link;
            _link.MouseLeftButtonDown += new MouseButtonEventHandler(Link_MouseLeftButtonDown);
            _link.TouchDown += new EventHandler<TouchEventArgs>(_link_TouchDown);
        }

        void _link_TouchDown (object sender, TouchEventArgs e)
        {
            _display.Visibility = Visibility.Visible;
        }

        private void Link_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _display.Visibility = Visibility.Visible;
        }
    }
}
