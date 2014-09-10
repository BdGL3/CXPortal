using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3.Cargo.Controls
{
    public delegate void DoublePropertyEventHandler (object sender, DoublePropertyEventArgs e);

    public class DoublePropertyEventArgs
    {
        public double NewValue;

        public DoublePropertyEventArgs (double value)
        {
            NewValue = value;
        }
    }
}
