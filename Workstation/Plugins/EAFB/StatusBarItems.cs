using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace L3.Cargo.Workstation.Plugins.XRayImageBase
{
    public class StatusBarItems : INotifyPropertyChanged
    {
        #region Private Members

        private String m_ZoomFactor;
        private String m_ImageWidth;
        private String m_ImageHeight;
        private String m_ImageCursorPixelVal;
        private String m_ImageCursorBoardVal;
        private String m_ImageCursorDetectorVal;
        private String m_ImageCursorCoordX;
        private String m_ImageCursorCoordY;
        private String m_ImageMagFactor;
        private List<StatusBarItem> m_StatusDisplay;

        #endregion

        #region Public Members

        public String ZoomFactor
        {
            get { return m_ZoomFactor; }
            set
            {
                m_ZoomFactor = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("ZoomFactor"));
            }
        }
        
        public String ImageWidth
        {
            get { return m_ImageWidth; }
            set
            {
                m_ImageWidth = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("ImageWidth"));
            }
        }
        
        public String ImageHeight
        {
            get { return m_ImageHeight; }
            set
            {
                m_ImageHeight = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("ImageHeight"));
            }
        }
        
        public String ImageCursorPixelVal
        {
            get { return m_ImageCursorPixelVal; }
            set
            {
                m_ImageCursorPixelVal = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("ImageCursorPixelVal"));
            }
        }
        
        public String ImageCursorBoardVal
        {
            get { return m_ImageCursorBoardVal; }
            set
            {
                m_ImageCursorBoardVal = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("ImageCursorBoardVal"));
            }
        }
        
        public String ImageCursorDetectorVal
        {
            get { return m_ImageCursorDetectorVal; }
            set
            {
                m_ImageCursorDetectorVal = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("ImageCursorDetectorVal"));
            }
        }
               
        public String ImageCursorCoordX
        {
            get { return m_ImageCursorCoordX; }
            set
            {
                m_ImageCursorCoordX = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("ImageCursorCoordX"));
            }
        }
        
        public String ImageCursorCoordY
        {
            get { return m_ImageCursorCoordY; }
            set
            {
                m_ImageCursorCoordY = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("ImageCursorCoordY"));
            }
        }
        
        public String ImageMagFactor
        {
            get { return m_ImageMagFactor; }
            set
            {
                m_ImageMagFactor = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("ImageMagFactor"));
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, e);
        }

        #endregion        

        public List<StatusBarItem> StatusDisplay
        {
            get { return m_StatusDisplay; }
            set { m_StatusDisplay = value; }
        }

        #endregion

        #region Constructor

        public StatusBarItems()
        {
            m_StatusDisplay = new List<StatusBarItem>();

            StatusBarItem item = new StatusBarItem();

            StackPanel panel = new StackPanel();

            TextBlock blck = new TextBlock();
            Binding textBinding = new Binding("ImageCursorCoordX") { Source = this, };
            textBinding.Mode = BindingMode.OneWay;
            blck.SetBinding(TextBlock.TextProperty, textBinding);
            blck.DataContext = ImageCursorCoordX;
            blck.Width = 200;//50;
            panel.Children.Add(blck);
       
            Separator separator = new Separator();
            panel.Children.Add(separator);
            
            blck = new TextBlock();
            textBinding = new Binding("ImageCursorCoordY") { Source = this, };
            textBinding.Mode = BindingMode.OneWay;
            blck.SetBinding(TextBlock.TextProperty, textBinding);
            blck.DataContext = ImageCursorCoordY;
            blck.Width = 200;//50;
            panel.Children.Add(blck);
            
            separator = new Separator();
            panel.Children.Add(separator);
            
            blck = new TextBlock();
            textBinding = new Binding("ImageCursorPixelVal") { Source = this, };
            textBinding.Mode = BindingMode.OneWay;
            blck.SetBinding(TextBlock.TextProperty, textBinding);
            blck.DataContext = ImageCursorPixelVal;
            blck.Width = 80;
            panel.Children.Add(blck);
           
            separator = new Separator();
            panel.Children.Add(separator);
            
            blck = new TextBlock();
            textBinding = new Binding("ImageCursorBoardVal") { Source = this, };
            textBinding.Mode = BindingMode.OneWay;
            blck.SetBinding(TextBlock.TextProperty, textBinding);
            blck.DataContext = ImageCursorBoardVal;
            blck.Width = 50;
            panel.Children.Add(blck);
           
            separator = new Separator();
            panel.Children.Add(separator);
            
            blck = new TextBlock();
            textBinding = new Binding("ImageCursorDetectorVal") { Source = this, };
            textBinding.Mode = BindingMode.OneWay;
            blck.SetBinding(TextBlock.TextProperty, textBinding);
            blck.DataContext = ImageCursorDetectorVal;
            blck.Width = 45;
            panel.Children.Add(blck);
            
            separator = new Separator();
            panel.Children.Add(separator);
            
            blck = new TextBlock();
            textBinding = new Binding("ImageWidth") { Source = this, };
            textBinding.Mode = BindingMode.OneWay;
            blck.SetBinding(TextBlock.TextProperty, textBinding);
            blck.DataContext = ImageWidth;
            blck.Width = 200;//75;
            panel.Children.Add(blck);
           
            separator = new Separator();
            panel.Children.Add(separator);
            
            blck = new TextBlock();
            textBinding = new Binding("ImageHeight") { Source = this, };
            textBinding.Mode = BindingMode.OneWay;
            blck.SetBinding(TextBlock.TextProperty, textBinding);
            blck.DataContext = ImageHeight;
            blck.Width =200; //75;
            panel.Children.Add(blck);
            
            separator = new Separator();
            panel.Children.Add(separator);
            
            blck = new TextBlock();
            textBinding = new Binding("ZoomFactor") { Source = this, };
            textBinding.Mode = BindingMode.OneWay;
            blck.SetBinding(TextBlock.TextProperty, textBinding);
            blck.DataContext = ZoomFactor;
            blck.Width = 75;
            panel.Children.Add(blck);           

            panel.Orientation = Orientation.Horizontal;
            item.Content = panel;
            item.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
            m_StatusDisplay.Add(item);
        }

        #endregion
    }
}
