using System;
using System.Windows;

namespace L3.Cargo.Workstation.Plugins.Common.Interfaces
{
    public interface IBuffer : IPlugin
    {
        UIElement ToolBarItem { get; }

        void ApplyFilter (bool enable);
    }
}
