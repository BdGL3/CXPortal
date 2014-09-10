using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using L3.Cargo.Common.Xml.History_1_0;
using L3.Cargo.Workstation.Common;
using L3.Cargo.Workstation.Plugins.Common;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using L3.Cargo.Common;
using System.Windows.Data;

namespace L3.Cargo.Workstation.Buffers.NormalDensity
{
    public class NormalDensity : IBuffer
    {
        #region Private Members

        private Button m_ToolBarItem;

        private XrayImageEffect _Effect;

        private History m_History;

        private string m_Name = "NormalDensityCargo";

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

        public NormalDensity()
        {
            m_ToolBarItem = new Button();
            m_ToolBarItem.Margin = new Thickness(1);
            m_ToolBarItem.Padding = new Thickness(0);
            Image image = new Image();
            image.Width = 32;
            image.Height = 32;
            image.Source = new BitmapImage(new Uri(@"/L3Buffer-2NormalDensity;component/NormalDensityOn.png", UriKind.Relative));
            m_ToolBarItem.Content = image;
            m_ToolBarItem.Name = m_Name;

            // Bind the tool tip to the resource
            var binding = new Binding(m_Name);
            binding.Source = CultureResources.getDataProvider();
            BindingOperations.SetBinding(m_ToolBarItem, Button.ToolTipProperty, binding);

            m_ToolBarItem.Click += new RoutedEventHandler(m_ToolBarItem_Click);
        }

        #endregion Constructors


        #region Private Methods

        private void m_ToolBarItem_Click(Object sender, RoutedEventArgs e)
        {
            ApplyFilter(true);

            HistoryBuffer buffer = new HistoryBuffer();
            buffer.name = Name;
            m_History.AddStep(buffer);
        }

        #endregion Private Methods


        #region Public Methods

        public void Initialize(Object passedObj)
        {
            BufferParameter bufferParam = passedObj as BufferParameter;
            if (bufferParam != null)
            {
                _Effect = bufferParam.dockPanel.Effect as XrayImageEffect;
                m_History = bufferParam.History;
            }
        }

        public void ApplyFilter (bool enable)
        {
            Image image = m_ToolBarItem.Content as Image;
            
            if (enable)
            {
                image.Source = new BitmapImage(new Uri(@"/L3Buffer-2NormalDensity;component/NormalDensityOn.png", UriKind.Relative));

                if (_Effect != null)
                {
                    _Effect.EnableSquare = 0.0;
                    _Effect.EnableLog = 0.0;
                    _Effect.EnableSquareroot = 0.0;
                }
            }
            else
            {
                image.Source = new BitmapImage(new Uri(@"/L3Buffer-2NormalDensity;component/NormalDensityOff.png", UriKind.Relative));
            }
        }

        public void Dispose()
        {
        }

        #endregion Public Methods
    }
}
