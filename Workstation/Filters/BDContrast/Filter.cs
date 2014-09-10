using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using L3.Cargo.Common.Xml.History_1_0;
using L3.Cargo.Workstation.Plugins.Common;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using System.Windows.Data;
using L3.Cargo.Common;

namespace L3.Cargo.Workstation.Filters.BDContrast
{
    public class BDContrast : IFilter
    {
        #region Private Members

        private DockPanel dockPanel;

        private Int32 Width;

        private Int32 Height;

        private History m_History;

        private Button m_ToolBarItem;

        private Popup m_PopUpWin;

        private Slider m_BrightnessControl;

        private Slider m_ContrastControl;

        private string m_Name = "Brightness/Darkness + Contrast";

        private string m_Version = "1.0.0";

        #endregion Private Members

        #region Public Members

        public const double DEFAULT_BRIGHTNESS = 0.0;
        public const double DEFAULT_CONTRAST = 1.0;

        public string Name
        {
            get
            {
                return m_Name;
            }
        }

        public string Version
        {
            get { return m_Version; }
        }

        public UIElement ToolBarItem
        {
            get
            {
                return m_ToolBarItem;
            }
        }

        #endregion Public Members


        #region Constructors

        public BDContrast ()
        {
            m_ToolBarItem = new Button();
            m_ToolBarItem.Margin = new Thickness(1);
            m_ToolBarItem.Padding = new Thickness(0);
            Image image = new Image();
            image.Width = 32;
            image.Height = 32;
            image.Source = new BitmapImage(new Uri(@"/L3Filter-BDContrast;component/BDContrastOff.png", UriKind.Relative));
            m_ToolBarItem.Content = image;

            // Bind the tool tip to the resource
            var binding = new Binding("BrightnessDarknessContrast");
            binding.Source = CultureResources.getDataProvider();
            BindingOperations.SetBinding(m_ToolBarItem, Button.ToolTipProperty, binding);

            m_ToolBarItem.Click += new RoutedEventHandler(m_ToolBarItem_Click);

            m_BrightnessControl = new Slider();
            m_BrightnessControl.Background = Brushes.White;

            m_BrightnessControl.Margin = new Thickness(10, 10, 10, 5);
            m_BrightnessControl.Width = 150;
            m_BrightnessControl.Height = 30;
            m_BrightnessControl.Minimum = -1;
            m_BrightnessControl.Maximum = 1;
            m_BrightnessControl.Value = DEFAULT_BRIGHTNESS;
            m_BrightnessControl.ValueChanged += new RoutedPropertyChangedEventHandler<double>(UIControl_ValueChanged);

            m_ContrastControl = new Slider();
            m_ContrastControl.Background = Brushes.White;

            m_ContrastControl.Margin = new Thickness(10, 10, 10, 5);
            m_ContrastControl.Width = 150;
            m_ContrastControl.Height = 30;
            m_ContrastControl.Minimum = 0;
            m_ContrastControl.Maximum = 2;
            m_ContrastControl.Value = DEFAULT_CONTRAST;
            m_ContrastControl.ValueChanged += new RoutedPropertyChangedEventHandler<double>(UIControl_ValueChanged);

            StackPanel stackPanel = new StackPanel();
            stackPanel.Background = Brushes.White;
            stackPanel.Children.Add(m_BrightnessControl);
            stackPanel.Children.Add(m_ContrastControl);

            m_PopUpWin = new Popup();
            m_PopUpWin.PlacementTarget = m_ToolBarItem;
            m_PopUpWin.Placement = PlacementMode.Bottom;
            m_PopUpWin.StaysOpen = false;
            m_PopUpWin.Child = stackPanel;
            m_PopUpWin.Margin = new Thickness(5);
            m_PopUpWin.Closed += new EventHandler(m_PopUpWin_Closed);
        }

        #endregion Constructors


        #region Private Methods

        private void m_ToolBarItem_Click(Object sender, RoutedEventArgs e)
        {
            m_PopUpWin.IsOpen = true;
        }

        private void UIControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ApplyFilter(m_BrightnessControl.Value, m_ContrastControl.Value);
        }

        private void m_PopUpWin_Closed (object sender, EventArgs e)
        {
            HistoryFilter filter = new HistoryFilter();
            filter.name = m_Name;
            filter.parameter = m_BrightnessControl.Value.ToString();
            filter.optparameter1 = m_ContrastControl.Value.ToString();
            m_History.AddStep(filter);
        }

        #endregion Private Methods


        #region Public Methods

        public void Initialize(Object passedObj)
        {
            FilterParameter filterParam = passedObj as FilterParameter;
            if (filterParam != null)
            {
                dockPanel = filterParam.dockPanel;
                Width = filterParam.Width;
                Height = filterParam.Height;
                m_History = filterParam.History;

                HistoryFilter filter = new HistoryFilter();
                filter.name = m_Name;
                filter.parameter = m_BrightnessControl.Value.ToString();
                filter.optparameter1 = m_ContrastControl.Value.ToString();
                m_History.SetFirstStep(filter);
            }
        }

        public void ApplyFilter (Object passedObj1, Object passedObj2)
        {
            double brightnessValue = Convert.ToDouble(passedObj1);
            double contrastValue = Convert.ToDouble(passedObj2);

            if (brightnessValue == 0 && contrastValue == 1)
            {
                Image image = m_ToolBarItem.Content as Image;
                image.Source = new BitmapImage(new Uri(@"/L3Filter-BDContrast;component/BDContrastOff.png", UriKind.Relative));

                dockPanel.Effect = null;
            }
            else
            {
                Image image = m_ToolBarItem.Content as Image;
                image.Source = new BitmapImage(new Uri(@"/L3Filter-BDContrast;component/BDContrastOn.png", UriKind.Relative));

                BDContrastEffect bdContrastEffect = new BDContrastEffect();
                bdContrastEffect.Brightness = brightnessValue;
                bdContrastEffect.Contrast = contrastValue;
                dockPanel.Effect = bdContrastEffect;
            }
            m_BrightnessControl.Value = brightnessValue;
            m_ContrastControl.Value = contrastValue;
        }

        public void Dispose()
        {
        }

        #endregion Public Methods
    }
}
