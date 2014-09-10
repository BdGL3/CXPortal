using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3.Cargo.Common.Dashboard.Display.Interfaces
{
    public delegate void VisibilityChangeHandler ();

    public interface IStatusMessage
    {
        event VisibilityChangeHandler VisibilityChanged;
    }
}
