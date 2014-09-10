using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace L3.Cargo.Safety.Display.Common.PortalObjects
{
    public interface ObjectBehavior
    {
        void applyBehavior(UserControl control, string name, int value);
    }
}
