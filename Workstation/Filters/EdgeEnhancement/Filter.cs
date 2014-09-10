using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using L3.Cargo.Workstation.Plugins.Common;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using L3.Cargo.Common.Xml.History_1_0;
using System.Windows.Data;
using L3.Cargo.Common;

namespace L3.Cargo.Workstation.Filters.EdgeEnhancement
{
    public class EdgeEnhancement : IFilter
    {
        #region Private Members

        private Button m_ToolBarItem;

        private DockPanel dockPanel;

        private Popup m_PopUpWin;

        private Slider m_PopUpItem;

        private History m_History;

        private string m_Name = "Edge Enhancement";

        private string m_Version = "1.0.0";

        private int Width;

        private int Height;

        #endregion Private Members


        #region Public Members

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

        public EdgeEnhancement ()
        {
            m_ToolBarItem = new Button();
            m_ToolBarItem.Margin = new Thickness(1);
            m_ToolBarItem.Padding = new Thickness(0);
            Image image = new Image();
            image.Width = 32;
            image.Height = 32;
            image.Source = new BitmapImage(new Uri(@"/L3Filter-EdgeEnhancement;component/EdgeEnhancementOff.png", UriKind.Relative));
            m_ToolBarItem.Content = image;

            // Bind the tool tip to the resource
            var binding = new Binding("EdgeEnhancement");
            binding.Source = CultureResources.getDataProvider();
            BindingOperations.SetBinding(m_ToolBarItem, Button.ToolTipProperty, binding);


            m_ToolBarItem.Click += new RoutedEventHandler(m_ToolBarItem_Click);

            m_PopUpItem = new Slider();
            m_PopUpItem.Background = Brushes.White;

            m_PopUpItem.Margin = new Thickness(10, 10, 10, 5);
            m_PopUpItem.Width = 150;
            m_PopUpItem.Height = 30;
            m_PopUpItem.Maximum = 10;
            m_PopUpItem.Minimum = 0;
            m_PopUpItem.ValueChanged += new RoutedPropertyChangedEventHandler<double>(UIControl_ValueChanged);

            StackPanel stackPanel = new StackPanel();
            stackPanel.Background = Brushes.White;
            stackPanel.Children.Add(m_PopUpItem);

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
            ApplyFilter(m_PopUpItem.Value);
        }

        private void m_PopUpWin_Closed (object sender, EventArgs e)
        {
            HistoryFilter filter = new HistoryFilter();
            filter.name = m_Name;
            filter.parameter = m_PopUpItem.Value.ToString();
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
                m_History = filterParam.History;
                Width = filterParam.Width;
                Height = filterParam.Height;

                HistoryFilter filter = new HistoryFilter();
                filter.name = m_Name;
                filter.parameter = m_PopUpItem.Value.ToString();
                m_History.SetFirstStep(filter);
            }
        }

        public void ApplyFilter (Object passedObj, Object dontCare = null)
        {
            double value = Convert.ToDouble(passedObj);

            if (value == 0)
            {
                Image image = m_ToolBarItem.Content as Image;
                image.Source = new BitmapImage(new Uri(@"/L3Filter-EdgeEnhancement;component/EdgeEnhancementOff.png", UriKind.Relative));

                dockPanel.Effect = null;
                m_PopUpItem.Value = 0;
            }
            else
            {
                Image image = m_ToolBarItem.Content as Image;
                image.Source = new BitmapImage(new Uri(@"/L3Filter-EdgeEnhancement;component/EdgeEnhancementOn.png", UriKind.Relative));

                ParametricEdgeDetectionA pes = new ParametricEdgeDetectionA();
                // set default
                pes.Threshhold = 1.0;
                pes.K00 = m_PopUpItem.Maximum - value;
                pes.K01 = m_PopUpItem.Maximum - value;
                pes.K02 = m_PopUpItem.Maximum - value;
                pes.TextureSize = new Point(Width, Height);
                pes.StartPoint = new Point(0, 0);
                pes.EndPoint = new Point(1, 1);
                dockPanel.Effect = pes;
                m_PopUpItem.Value = value;
            }
        }

        public void Dispose()
        {
        }

        #endregion Public Methods
    }
}
