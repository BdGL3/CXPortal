using System.Collections.Generic;
using System.Windows.Controls;
using L3.Cargo.Common.Dashboard.Display;

namespace L3.Cargo.Dashboard.PresentationCore
{
    public class WidgetPage
    {
        public int Number;

        public Dictionary<string, Widget> Widgets;

        public Grid Grid;


        public WidgetPage()
        {
            Widgets = new Dictionary<string, Widget>();
        }
    }
}
