using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace L3.Cargo.Safety.Display.Common
{
    /// <summary>
    /// Interaction logic for FadedGradientBorder.xaml
    /// </summary>
    public partial class FadedGradientBorder : UserControl
    {
        private double thickness;

        public FadedGradientBorder()
        {
            InitializeComponent();

            thickness = 0.03;

            MainCanvas.Opacity = 0.0;

            this.Loaded += new RoutedEventHandler(FadedGradientBorder_Loaded);
            this.SizeChanged += new SizeChangedEventHandler(FadedGradientBorder_SizeChanged);
        }
        
        // thickness in percentage
        public double Thickness
        {
            get { return thickness; }
            set
            {
                thickness = value;
                UpdateResourceValues();
            }
        }

        public void Fade(bool makeVisible)
        {
            Dispatcher.BeginInvoke(new Action(delegate()
                {
                    DoubleAnimation animation = null;

                    if (MainCanvas.Opacity == 0 && makeVisible)
                    {
                        this.Visibility = System.Windows.Visibility.Visible;
                        MainCanvas.Visibility = System.Windows.Visibility.Visible;
                        animation = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(1));
                    }
                    else if (MainCanvas.Opacity != 0 && !makeVisible)
                    {
                        animation = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5));
                    }
                    if (animation != null)
                    {
                        //commented out so the border doesn't show.
                        //MainCanvas.BeginAnimation(Canvas.OpacityProperty, animation);
                    }
                }));
        }

        private void FadedGradientBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateResourceValues();
        }

        private void FadedGradientBorder_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateResourceValues();
        }

        private void UpdateResourceValues()
        {
            Dispatcher.BeginInvoke(new Action(delegate()
            {
                this.Resources["TwiceBorderThickness"] = this.ActualWidth * thickness * 2;
            }));
        }
    }
}
