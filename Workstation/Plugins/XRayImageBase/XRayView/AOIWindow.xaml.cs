using System.Windows;
using L3.Cargo.Common.Xml.History_1_0;
using L3.Cargo.Workstation.Plugins.XRayImageBase.Common;

namespace L3.Cargo.Workstation.Plugins.XRayImageBase
{
    /// <summary>
    /// Interaction logic for AOIWindow.xaml
    /// </summary>
    public partial class AOIWindow : Window
    {
        public event AlgServerRequestEventHandler AlgServerRequestEvent;

        public void Setup (ViewObject content)
        {
            AOIXRayView.Setup(content, new History(), null);
            AOIXRayView.CollapseDualEnergy(true);
        }

        public AOIWindow ()
        {
            InitializeComponent();
            AOIXRayView.AlgServerRequestEvent += new AlgServerRequestEventHandler(AOIXRayView_AlgServerRequestEvent);
        }

        private void AOIXRayView_AlgServerRequestEvent(object sender, AlgServerRequestEventArgs e)
        {
            if (AlgServerRequestEvent != null)
            {
                AlgServerRequestEvent(sender, e);
            }
        }
    }
}
