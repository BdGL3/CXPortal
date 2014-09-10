using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using System;
using L3.Cargo.Common;
using System.Windows.Data;

namespace L3.Cargo.Workstation.Plugins.XRayImages
{
    /// <summary>
    /// Interaction logic for TIPPopUp.xaml
    /// </summary>
    public partial class TIPPopUp : Popup
    {
        public TIPPopUp ()
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
        }

        public void SetMessage (string message)
        {
            this.Message.Text = message;
        }

        public void SetIcon (bool isCorrect)
        {
            if (isCorrect)
            {
                MessageIcon.Source = new BitmapImage(new Uri(@"/L3Plugin-XRayImages;component/accept.png", UriKind.Relative));
            }
            else
            {
                MessageIcon.Source = new BitmapImage(new Uri(@"/L3Plugin-XRayImages;component/delete.png", UriKind.Relative));
            }
        }

        private void ToggleButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.IsOpen = false;
        }
    }
}
