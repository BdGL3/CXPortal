using System;
using System.Windows.Controls;

namespace L3.Cargo.Workstation.PresentationCore.Common
{
    public class PanelLayout : IDisposable
    {
        public TabControl MainTabControl { get; set; }

        public TabControl InfoTabControl { get; set; }

        public TabControl SubTabControl { get; set; }

        public PanelLayout ()
        {
            MainTabControl = new TabControl();
            InfoTabControl = new TabControl();
            SubTabControl = new TabControl();
        }

        public void Dispose()
        {
            MainTabControl.Items.Clear();
            InfoTabControl.Items.Clear();
            SubTabControl.Items.Clear();

            MainTabControl = null;
            InfoTabControl = null;
            SubTabControl = null;
        }
    }
}
