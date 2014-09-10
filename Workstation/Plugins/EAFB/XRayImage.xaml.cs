using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using L3.Cargo.Common;
using L3.Cargo.Common.Xml.History_1_0;
using L3.Cargo.Workstation.SystemConfigurationCore;
using L3.Cargo.Workstation.Plugins.XRayImageBase;
using System.Windows.Documents;

namespace L3.Cargo.Workstation.Plugins.EAFB
{
    /// <summary>
    /// Interaction logic for XRayView.xaml
    /// </summary>
    public partial class XRayImage : UserControl
    {
        #region Private Members

        private struct MagnifierSettings
        {
            public double MagnFactor;
            public double Radius;
            public double AspectRatio;
            public bool IsMagniferEnabled;
            public Point Center;
        }

        private MagnifierSettings m_MagnifierSettings;

        private ViewObject m_ViewObject;

        private StatusBarItems m_statusBarItems;

        private uint m_MaxDetectorsPerBoard = 0;

        private uint m_bitsPerPixel = 0;

        #endregion Private Members


        #region Public Members

        public XRayView MainView
        {
            get
            {
                return MainXRayView;
            }
        }

        public BitmapSource RenderedImage
        {
            get
            {
                return MainXRayView.RenderedImage;
            }
        }

        public bool IsAnnotationsShown
        {
            get
            {
                return MainXRayView.adonerImageObject.IsAnnotationShown;
            }
        }

        public string ViewName
        {
            get
            {
                return m_ViewObject.Name;
            }
        }

        #endregion Public Members


        #region Constructor

        public XRayImage (ViewObject viewObject, StatusBarItems statusBarItems, History history, SysConfiguration SysConfig)
        {
            InitializeComponent();
            MainXRayView.Image.Visibility = System.Windows.Visibility.Hidden;

            m_MagnifierSettings.IsMagniferEnabled = false;
            m_MagnifierSettings.Radius = 0;
            m_MagnifierSettings.MagnFactor = 2.0;
            m_MagnifierSettings.AspectRatio = 0;
            m_MaxDetectorsPerBoard = viewObject.MaxDetectorsPerBoard;
            m_bitsPerPixel = viewObject.BitsPerPixel;
            m_ViewObject = viewObject;
            m_statusBarItems = statusBarItems;

            MainXRayView.Setup(viewObject, history, SysConfig);   
            

            MainXRayView.adonerImageObject.measureAdorner.SamplingSpace = viewObject.SamplingSpace;
            MainXRayView.adonerImageObject.measureAdorner.SamplingSpeed = viewObject.SamplingSpeed;

            MainXRayView.Cursor = Cursors.Hand;
            MainXRayView.Image.MouseMove += new MouseEventHandler(Img_MouseMove);
            MainXRayView.Image.MouseLeave += new MouseEventHandler(Img_MouseLeave);

            MainXRayView.MagnifierDockPanel.SizeChanged += new SizeChangedEventHandler(Magnifier_SizeChanged);
            MainXRayView.MagnifierDockPanel.MouseMove += new MouseEventHandler(Magnifier_MouseMove);
            MainXRayView.MagnifierDockPanel.MouseLeftButtonDown += new MouseButtonEventHandler(Magnifier_MouseMove);
            
        }

        #endregion Constructor


        #region Private Methods

        private void UnregisterMouseEvents ()
        {
            PanZoom_MenuItem.Icon = null;
            FragmentMark_MenuItem.Icon = null;
            FragmentUniformity_MenuItem.Icon = null;

            MainXRayView.PanAndZoomViewer.UnRegisterEvents();
            MainXRayView.adonerImageObject.FragmentButton_Clicked(false);
            MainXRayView.MagnifierDockPanel.Effect = null;
            m_MagnifierSettings.IsMagniferEnabled = false;
            MainXRayView.Image.Cursor = Cursors.Arrow;
            
            PanAndZoom_Normal.Visibility = System.Windows.Visibility.Collapsed;
            PanAndZoom_FitToScreen.Visibility = System.Windows.Visibility.Collapsed;
            CreateFramgmentMark_Ellipse.Visibility = System.Windows.Visibility.Collapsed;            
            FragmentRegisterView_Ellipse.Visibility = System.Windows.Visibility.Collapsed;            
            Options_Menu_Seperator.Visibility = Visibility.Collapsed;
        }

        private void Magnifier_MouseMove (object sender, MouseEventArgs e)
        {
            DockPanel panel = sender as DockPanel;

            if (panel != null)
            {
                if ((m_MagnifierSettings.IsMagniferEnabled == true) && (e.LeftButton == MouseButtonState.Pressed))
                {
                    if (m_MagnifierSettings.Radius == 0)
                    {
                        m_MagnifierSettings.Radius = 150.0 / panel.ActualWidth;
                    }

                    if (m_MagnifierSettings.AspectRatio == 0)
                    {
                        m_MagnifierSettings.AspectRatio = (panel.ActualWidth / panel.ActualHeight);
                    }

                    Point CurPos = e.GetPosition(panel);
					
					m_MagnifierSettings.Center = new Point(CurPos.X / panel.ActualWidth, CurPos.Y / panel.ActualHeight);

                    MagnifyEffect me = new MagnifyEffect();
                    me.Magnification = m_MagnifierSettings.MagnFactor;
                    me.Radius = m_MagnifierSettings.Radius;
                    me.AspectRatio = m_MagnifierSettings.AspectRatio;
                    me.Center = m_MagnifierSettings.Center;

                    panel.Effect = me;
                }
            }
        }

        private void Magnifier_SizeChanged (object sender, SizeChangedEventArgs e)
        {
            DockPanel panel = sender as DockPanel;

            if (panel != null)
            {
                if ((m_MagnifierSettings.IsMagniferEnabled == true))
                {
                    m_MagnifierSettings.Radius = 150.0 / panel.ActualWidth;
                    m_MagnifierSettings.AspectRatio = (panel.ActualWidth / panel.ActualHeight);

                    MagnifyEffect me = new MagnifyEffect();
                    me.Magnification = m_MagnifierSettings.MagnFactor;
                    me.Radius = m_MagnifierSettings.Radius;
                    me.AspectRatio = m_MagnifierSettings.AspectRatio;
                    me.Center = m_MagnifierSettings.Center;

                    panel.Effect = me;
                }
            }
        }

        private void Img_MouseLeave(object sender, MouseEventArgs e)
        {
            m_statusBarItems.ImageCursorCoordX = String.Empty;
            m_statusBarItems.ImageCursorCoordY = String.Empty;
            m_statusBarItems.ImageCursorDetectorVal = String.Empty;
            m_statusBarItems.ImageCursorBoardVal = String.Empty;
            m_statusBarItems.ImageCursorPixelVal = String.Empty;
        }

        private void Img_MouseMove (object sender, MouseEventArgs e)
        {
            Image img = sender as Image;

            if (img != null)
            {
                m_statusBarItems.ImageWidth = L3.Cargo.Common.Resources.Width_Colon + " " + img.Source.Width.ToString();
                m_statusBarItems.ImageHeight = L3.Cargo.Common.Resources.Height_Colon + " " + img.Source.Height.ToString();
                m_statusBarItems.ZoomFactor = L3.Cargo.Common.Resources.Zoom_Colon + " " + MainXRayView.PanAndZoomViewer.CurrentZoom.ToString("F");

                Size imgSize = new Size((double)img.ActualWidth, (double)img.ActualHeight);
                Size sourceSize = new Size(img.Source.Width, img.Source.Height);

                Point pt = GetCursorPosition(imgSize, sourceSize, e.GetPosition(img));                

                m_statusBarItems.ImageCursorCoordX = "Z val(mm) = " + Math.Round(pt.X * Conversion.SamplingSpace, 0).ToString();
                m_statusBarItems.ImageCursorCoordY = " Θ" + "1 (deg) = " + Math.Round(Conversion.ConvertY2Theta(pt.Y, img.ActualHeight), 0).ToString();                

                InvalidateVisual();
            }
        }

        private Point GetCursorPosition(Size imgSize, Size sourceSize, Point imgPoint)
        {
            //calculate cursor position on the Image
            Point point = new Point();

            double HeightRatio = imgSize.Height / sourceSize.Height;
            double WidthRatio = imgSize.Width / sourceSize.Width;

            point.X = (int)(imgPoint.X / WidthRatio);
            point.Y = (int)(imgPoint.Y / HeightRatio);

            if (point.X >= (int)sourceSize.Width)
                point.X = (int)sourceSize.Width;

            if (point.X < 0)
                point.X = 0;

            //point.Y = (int)(sourceSize.Height - point.Y);

            if (point.Y >= sourceSize.Height)
                point.Y = (int)sourceSize.Height;

            if (point.Y < 0)
                point.Y = 0;

            return point;
        }

        private int GetDataArrayOffset(Size sourceSize, Point point)
        {
            long DataArrayOffset = (long)(point.Y * sourceSize.Width + point.X);           

            if (DataArrayOffset >= (int)(sourceSize.Width * sourceSize.Height))
                DataArrayOffset = (int)(sourceSize.Width * sourceSize.Height) - 1;

            if (DataArrayOffset < 0)
                DataArrayOffset = 0;

            return (int)(MainXRayView.CurrentSource.Data[DataArrayOffset] * Math.Pow(2, m_bitsPerPixel));            
        }

        private int GetBoardDetector(Size sourceSize, Point point)
        {
            if (point.Y == sourceSize.Height)
                point.Y--;

            uint DetectorNum = (uint)(sourceSize.Height - point.Y) % m_MaxDetectorsPerBoard;
            double fBoardNum = (double)(((double)sourceSize.Height - point.Y) / (double)m_MaxDetectorsPerBoard);

            int dBoardNum = (int)fBoardNum;

            if (fBoardNum <= 1.00)
                dBoardNum = 1;
            else if (DetectorNum != 0)
                dBoardNum++;

            return dBoardNum;
        }        

        #endregion Private Methods


        #region Public Methods

        public List<AnnotationInfo> GetAnnotations()
        {
            return MainXRayView.adonerImageObject.annotationAdorner.GetAnnotations();
        }

        public void SetCreateNewAnnotation(Boolean CanCreateNewAnnot)
        {
            MainXRayView.adonerImageObject.annotationAdorner.CreateNewAnnotationObject = CanCreateNewAnnot;
        }
        

        public void PanAndZoom_Normal_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainXRayView.PanAndZoomViewer.ZoomNormal();

        }

        public void PanAndZoom_FitToScreen_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainXRayView.PanAndZoomViewer.ZoomFitToScreen();
        }                

        public void PanZoom_MenuItem_Click (object sender, RoutedEventArgs e)
        {
            UnregisterMouseEvents();
            MainXRayView.PanAndZoomViewer.RegisterEvents();
            Image image = new Image();
            image.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/checkmark.gif", UriKind.Relative));
            PanZoom_MenuItem.Icon = image;            

            PanAndZoom_FitToScreen.Visibility = System.Windows.Visibility.Visible;
            PanAndZoom_Normal.Visibility = System.Windows.Visibility.Visible;
            Options_Menu_Seperator.Visibility = Visibility.Visible;

            MainXRayView.Image.Cursor = Cursors.Hand;
        }

        public void FragmentMark_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            UnregisterMouseEvents();         
           
            FragmentRegisterView_Ellipse.Visibility = System.Windows.Visibility.Visible;
            CreateFramgmentMark_Ellipse.Visibility = System.Windows.Visibility.Visible;
            Options_Menu_Seperator.Visibility = Visibility.Visible;            

            Image image = new Image();
            image.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/checkmark.gif", UriKind.Relative));
            FragmentMark_MenuItem.Icon = image;           

            image = new Image();
            image.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/checkmark.gif", UriKind.Relative));
            CreateFramgmentMark_Ellipse.Icon = image;

            FragmentRegisterView_Ellipse.Icon = null;
        }

        public void FragmentRegisterView_Ellipse_MenuItem_Click(object sender, RoutedEventArgs e)
        {           
            FragmentRegisterView_Ellipse.Visibility = System.Windows.Visibility.Visible;
            CreateFramgmentMark_Ellipse.Visibility = System.Windows.Visibility.Visible;
            Options_Menu_Seperator.Visibility = Visibility.Visible;

            Image image = new Image();
            image.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/checkmark.gif", UriKind.Relative));            
            CreateFramgmentMark_Ellipse.Icon = null;
            FragmentRegisterView_Ellipse.Icon = image;
        }

        public void FragmentUniformity_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            UnregisterMouseEvents(); 
            Image image = new Image();
            image.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/checkmark.gif", UriKind.Relative));
            
            FragmentUniformity_MenuItem.Icon = image;
        }

        public void CreateFramgmentMark_Ellipse_MenuItem_Click(object sender, RoutedEventArgs e)
        {            
            FragmentRegisterView_Ellipse.Visibility = System.Windows.Visibility.Visible;
            CreateFramgmentMark_Ellipse.Visibility = System.Windows.Visibility.Visible;
            Options_Menu_Seperator.Visibility = Visibility.Visible;

            Image image = new Image();
            image.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/checkmark.gif", UriKind.Relative));
            FragmentRegisterView_Ellipse.Icon = null;            
            CreateFramgmentMark_Ellipse.Icon = image;
        }

        public void FragmentAddMarks_Hide_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainXRayView.adonerImageObject.LoadFragmentAdorner(false);
            FragmentMarks_Show.Visibility = Visibility.Visible;
            FragmentMarks_Hide.Visibility = Visibility.Collapsed;
        }

        public void FragmentAddMarks_Show_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainXRayView.adonerImageObject.LoadFragmentAdorner(true);
            FragmentMarks_Show.Visibility = Visibility.Collapsed;
            FragmentMarks_Hide.Visibility = Visibility.Visible;
        }

        public void Dispose ()
        {
            MainXRayView.Image.MouseMove -= new MouseEventHandler(Img_MouseMove);
            MainXRayView.Image.MouseLeave -= new MouseEventHandler(Img_MouseLeave);

            MainXRayView.MagnifierDockPanel.SizeChanged -= new SizeChangedEventHandler(Magnifier_SizeChanged);
            MainXRayView.MagnifierDockPanel.MouseMove -= new MouseEventHandler(Magnifier_MouseMove);
            MainXRayView.MagnifierDockPanel.MouseLeftButtonDown -= new MouseButtonEventHandler(Magnifier_MouseMove);

            m_statusBarItems = null;

            MainXRayView.Dispose();
            MainXRayView = null;
        }

        #endregion Public Methods

        private void XrayImage_Loaded(object sender, RoutedEventArgs e)
        {
            SimpleRulerAdorner ruler = new SimpleRulerAdorner(MainXRayView, MainXRayView.PanAndZoomViewer);
            ruler.SamplingSpace = Conversion.SamplingSpace;
            ruler.ImageWidth = MainXRayView.Image.Source.Width;

            AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(MainXRayView);
            myAdornerLayer.Add(ruler);
        }
    }
}
