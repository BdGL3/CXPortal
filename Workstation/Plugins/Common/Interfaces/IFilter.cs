using System;
using System.Windows;

namespace L3.Cargo.Workstation.Plugins.Common.Interfaces
{
    public interface IFilter : IPlugin
    {
        UIElement ToolBarItem { get; }

        void ApplyFilter (Object passedObj, Object optPassedObj = null);
    }
}
