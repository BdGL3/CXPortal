using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace L3.Cargo.Workstation.Plugins.Common.Interfaces
{
    public interface IContent : IPlugin
	{
        List<LayoutInfo> UserControlDisplays { get; }

        PrinterObject PrinterObject { get; }
	}
}
