using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3.Cargo.Common.Dashboard.Display.Interfaces
{
    public interface IDisplays
    {
        string Name { get; }

        string Version { get; }

        AssemblyDisplays Initialize (Object passedObj);

        void Dispose ();
    }
}
