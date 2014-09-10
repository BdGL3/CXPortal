using System;
using System.Windows;
using System.Windows.Forms;

namespace L3.Cargo.Workstation.PresentationCore
{
    /// <summary>
    /// Interaction logic for SecondWindow.xaml
    /// </summary>
    public partial class DualViewWindow : Window
    {
        private Boolean m_IsWindowAvailable;

        public Boolean IsWindowAvailable
        {
            get
            {
                return m_IsWindowAvailable;
            }
        }

        public DualViewWindow () :
            base()
        {
            InitializeComponent();

            m_IsWindowAvailable = false;

            Screen[] screens = Screen.AllScreens;
            // TbD: Sort the screens so that the X-ray screen is last.
            // That way, when dual screens are presented, each one presents the same way.
            if (screens.Length > 1)
            {
                foreach (Screen screen in screens)
                {
                    if (!screen.Primary)
                    {
                        this.Top = screen.Bounds.Top;
                        this.Left = screen.Bounds.Left;
                        this.Width = screen.Bounds.Width;
                        this.Height = screen.Bounds.Height;
                        m_IsWindowAvailable = true;
                        break;
                    }
                }
            }
        }

        public new void Show ()
        {
            if (m_IsWindowAvailable)
            {
                base.Show();
            }
            else
            {
                base.Hide();
            }
        }
    }
}
