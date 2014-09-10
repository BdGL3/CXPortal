using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using L3.Cargo.Common.Xml.History_1_0;
using L3.Cargo.Workstation.Plugins.Common;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using System.Windows.Data;
using L3.Cargo.Common;

namespace L3.Cargo.Workstation.Filters.BlurEffect
{
    public class BlurEffect : IFilter
    {
        #region Private Members

        private Button m_ToolBarItem;

        private DockPanel dockPanel;

        private History m_History;

        private string m_Name = "Blur Effect";

        private string m_Version = "1.0.0";

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

        public BlurEffect ()
        {
            m_ToolBarItem = new Button();
            m_ToolBarItem.Margin = new Thickness(1);
            m_ToolBarItem.Padding = new Thickness(0);
            Image image = new Image();
            image.Width = 32;
            image.Height = 32;
            image.Source = new BitmapImage(new Uri(@"/L3Filter-BlurEffect;component/BlurOff.png", UriKind.Relative));
            m_ToolBarItem.Content = image;

            // Bind the tool tip to the resource
            var binding = new Binding("BlurEffect");
            binding.Source = CultureResources.getDataProvider();
            BindingOperations.SetBinding(m_ToolBarItem, Button.ToolTipProperty, binding);

            m_ToolBarItem.Click += new RoutedEventHandler(m_ToolBarItem_Click);
        }

        #endregion Constructors


        #region Private Methods

        private void m_ToolBarItem_Click(Object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            bool enable = false;

            if (dockPanel.Effect == null)
            {
                enable = true;
            }

            ApplyFilter(enable);

            HistoryFilter filter = new HistoryFilter();
            filter.name = m_Name;
            filter.parameter = enable.ToString();
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

                HistoryFilter filter = new HistoryFilter();
                filter.name = m_Name;
                filter.parameter = "false";
                m_History.SetFirstStep(filter);
            }
        }

        public void ApplyFilter (Object passedObj, Object dontCare = null)
        {
            bool enable = Convert.ToBoolean(passedObj);

            if (enable)
            {
                Image image = m_ToolBarItem.Content as Image;
                image.Source = new BitmapImage(new Uri(@"/L3Filter-BlurEffect;component/BlurOn.png", UriKind.Relative));

                dockPanel.Effect = new System.Windows.Media.Effects.BlurEffect();
            }
            else
            {
                Image image = m_ToolBarItem.Content as Image;
                image.Source = new BitmapImage(new Uri(@"/L3Filter-BlurEffect;component/BlurOff.png", UriKind.Relative));

                dockPanel.Effect = null;
            }
        }

        public void Dispose()
        {
        }

        #endregion Public Methods
    }
}
