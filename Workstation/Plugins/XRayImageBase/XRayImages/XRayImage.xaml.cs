using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using L3.Cargo.Common;
using L3.Cargo.Common.Xml.History_1_0;
using L3.Cargo.Workstation.Plugins.XRayImageBase.Adorners;
using L3.Cargo.Workstation.Plugins.XRayImageBase.Common;
using L3.Cargo.Workstation.SystemConfigurationCore;
using L3.Cargo.Common.Xml.Annotations_1_0;
using System.Windows.Data;
using L3.Cargo.Controls;

namespace L3.Cargo.Workstation.Plugins.XRayImageBase
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
                return MainXRayView.IsAnnotationsShowing();
            }
        }

        public string ViewName
        {
            get
            {
                return m_ViewObject.Name;
            }
        }

        public bool IsContextMenuOpen
        {
            get
            {
                return XRayView_ContextMenu.IsLoaded;
            }
        }

        public event AlgServerRequestEventHandler AlgServerRequestEvent;

        #endregion Public Members


        #region Constructor

        public XRayImage (ViewObject viewObject, StatusBarItems statusBarItems, History history, SysConfiguration SysConfig)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            m_MagnifierSettings.IsMagniferEnabled = false;
            m_MagnifierSettings.Radius = 0;
            m_MagnifierSettings.MagnFactor = 2.0;
            m_MagnifierSettings.AspectRatio = 0;
            m_MaxDetectorsPerBoard = viewObject.MaxDetectorsPerBoard;
            m_bitsPerPixel = viewObject.BitsPerPixel;
            m_ViewObject = viewObject;
            m_statusBarItems = statusBarItems;

            XRayView_ContextMenu.Opened += new RoutedEventHandler(XRayView_ContextMenu_Changed);
            XRayView_ContextMenu.Closed += new RoutedEventHandler(XRayView_ContextMenu_Changed);

            MainXRayView.Setup(viewObject, history, SysConfig);

            MainXRayView.Cursor = Cursors.Hand;
            MainXRayView.MainImage.MouseMove += new MouseEventHandler(Img_MouseMove);
            MainXRayView.MainImage.MouseLeave += new MouseEventHandler(Img_MouseLeave);

            MainXRayView.Magnifier_Panel.SizeChanged += new SizeChangedEventHandler(Magnifier_SizeChanged);
            MainXRayView.Magnifier_Panel.MouseMove += new MouseEventHandler(Magnifier_MouseMove);
            MainXRayView.Magnifier_Panel.MouseLeftButtonDown += new MouseButtonEventHandler(Magnifier_MouseMove);

            if (viewObject.TIPMarkings != null)
            {
                HideFTILocations();

                //foreach (Rect rect in viewObject.TIPMarkings)
                //{
                //    MainXRayView.adonerImageObject.TIPAdorner.Add(rect);
                //}
            }

            MainXRayView.AlgServerRequestEvent += new AlgServerRequestEventHandler(MainXRayView_AlgServerRequestEvent);
        }

        #endregion Constructor


        #region Private Methods

        private void XRayView_ContextMenu_Changed(object sender, RoutedEventArgs e)
        {
            if (MainXRayView != null)
            {
                bool loaded = (sender as ContextMenu).IsOpen;
                MainXRayView.XRayView_ContextMenu_Changed(loaded);
            }
        }

        private void MainXRayView_AlgServerRequestEvent (object sender, AlgServerRequestEventArgs e)
        {
            if (AlgServerRequestEvent != null)
            {
                AlgServerRequestEvent(sender, e);
            }
        }

        private void UnregisterMouseEvents ()
        {
            PanZoom_MenuItem.Icon = null;
            Annotation_MenuItem.Icon = null;
            Measurements_MenuItem.Icon = null;
            Magnifier_MenuItem.Icon = null;
            AOI_MenuItem.Icon = null;

            MainXRayView.PanAndZoomControl.MouseEventsEnabled = false;

            AnnotationAdorner annotationAdorner = (MainXRayView.AdornerLayerManager[AdornerLayerManager.ANNOTATION_ADORNER] as AnnotationAdorner);
            annotationAdorner.Enabled = false;

            MeasureAdorner measureAdorner = (MainXRayView.AdornerLayerManager[AdornerLayerManager.MEASUREMENT_ADORNER] as MeasureAdorner);
            measureAdorner.Enabled = false;

            AOIAdorner aoiAdorner = (MainXRayView.AdornerLayerManager[AdornerLayerManager.AOI_ADORNER] as AOIAdorner);
            aoiAdorner.Enabled = false;

            //MainXRayView.adonerImageObject.AOIButton_Clicked(m_ViewObject, false);

            MainXRayView.Magnifier_Panel.Effect = null;
            m_MagnifierSettings.IsMagniferEnabled = false;
            MainXRayView.MainImage.Cursor = Cursors.Arrow;

            Annotation_Rectangle.Visibility = Visibility.Collapsed;
            Annotation_Ellipse.Visibility = Visibility.Collapsed;
            PanAndZoom_Normal.Visibility = System.Windows.Visibility.Collapsed;
            PanAndZoom_FitToScreen.Visibility = System.Windows.Visibility.Collapsed;
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
                m_statusBarItems.ImageWidth = L3.Cargo.Common.Resources.Width_Colon + " " + img.Source.Width.ToString(CultureResources.getDefaultDisplayCulture());
                m_statusBarItems.ImageHeight = L3.Cargo.Common.Resources.Height_Colon + " " + img.Source.Height.ToString(CultureResources.getDefaultDisplayCulture());
                m_statusBarItems.ZoomFactor = L3.Cargo.Common.Resources.Zoom_Colon + " " + MainXRayView.PanAndZoomControl.Zoom.ToString("F", CultureResources.getDefaultDisplayCulture());

                Size imgSize = new Size((double)img.ActualWidth, (double)img.ActualHeight);
                Size sourceSize = new Size(img.Source.Width, img.Source.Height);

                Point pt = GetCursorPosition(imgSize, sourceSize, e.GetPosition(img));

                int DataArrayOffset = GetDataArrayOffset(sourceSize, pt);

                m_statusBarItems.ImageCursorPixelVal = L3.Cargo.Common.Resources.Value_Colon + " " + DataArrayOffset.ToString(CultureResources.getDefaultDisplayCulture());

                int dBoardNum = GetBoardDetector(sourceSize, pt);

                m_statusBarItems.ImageCursorBoardVal = L3.Cargo.Common.Resources.BD_Colon + " " + dBoardNum.ToString(CultureResources.getDefaultDisplayCulture());

                uint DetectorNum = (uint)(sourceSize.Height - pt.Y) % m_MaxDetectorsPerBoard;
                if (DetectorNum == 0)
                {
                    DetectorNum = m_MaxDetectorsPerBoard;
                }

                m_statusBarItems.ImageCursorDetectorVal = L3.Cargo.Common.Resources.D_Colon + " " + DetectorNum.ToString(CultureResources.getDefaultDisplayCulture());

               // pt.Y = (int)(sourceSize.Height - pt.Y);

                m_statusBarItems.ImageCursorCoordX = L3.Cargo.Common.Resources.X_Colon + " " + pt.X.ToString(CultureResources.getDefaultDisplayCulture());
                m_statusBarItems.ImageCursorCoordY = L3.Cargo.Common.Resources.Y_Colon + " " + pt.Y.ToString(CultureResources.getDefaultDisplayCulture());

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

        public List<Annotation> GetAnnotations()
        {
            return MainXRayView.GetAnnotations();
        }

        public void SetCreateNewAnnotation(Boolean CanCreateNewAnnot)
        {
            MainXRayView.CanCreateNewAnnot = CanCreateNewAnnot;
        }

        public void Annotation_Ellipse_MenuItem_Click (object sender, RoutedEventArgs e)
        {
            (MainXRayView.AdornerLayerManager.GetAdorner(AdornerLayerManager.ANNOTATION_ADORNER) as AnnotationAdorner).DrawEllip_Click();

            Image image = new Image();
            image.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/checkmark.gif", UriKind.Relative));
            Annotation_Rectangle.Icon = null;
            Annotation_Ellipse.Icon = image;
        }

        public void Annotation_Rectangle_MenuItem_Click (object sender, RoutedEventArgs e)
        {
            (MainXRayView.AdornerLayerManager.GetAdorner(AdornerLayerManager.ANNOTATION_ADORNER) as AnnotationAdorner).DrawRect_Click();

            Image image = new Image();
            image.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/checkmark.gif", UriKind.Relative));
            Annotation_Rectangle.Icon = image;
            Annotation_Ellipse.Icon = null;
        }

        public void PanAndZoom_Normal_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainXRayView.PanAndZoomControl.Scale1to1();

        }

        public void PanAndZoom_FitToScreen_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainXRayView.PanAndZoomControl.ScaleToFit();
        }

        public void Annotation_Show_MenuItem_Click (object sender, RoutedEventArgs e)
        {
            MainXRayView.AdornerLayerManager.Show(AdornerLayerManager.ANNOTATION_ADORNER);
            Annotation_Show.Visibility = Visibility.Collapsed;
            Annotation_Hide.Visibility = Visibility.Visible;
        }

        public void Annotation_Hide_MenuItem_Click (object sender, RoutedEventArgs e)
        {
            MainXRayView.AdornerLayerManager.Hide(AdornerLayerManager.ANNOTATION_ADORNER);
            Annotation_Show.Visibility = Visibility.Visible;
            Annotation_Hide.Visibility = Visibility.Collapsed;
        }

        public void Measurement_Show_MenuItem_Click (object sender, RoutedEventArgs e)
        {
            MainXRayView.AdornerLayerManager.Show(AdornerLayerManager.MEASUREMENT_ADORNER);
            Measurement_Show.Visibility = Visibility.Collapsed;
            Measurement_Hide.Visibility = Visibility.Visible;
        }

        public void Measurement_Hide_MenuItem_Click (object sender, RoutedEventArgs e)
        {
            MainXRayView.AdornerLayerManager.Hide(AdornerLayerManager.MEASUREMENT_ADORNER);
            Measurement_Show.Visibility = Visibility.Visible;
            Measurement_Hide.Visibility = Visibility.Collapsed;
        }

        public void AOI_Show_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainXRayView.AdornerLayerManager.Show(AdornerLayerManager.AOI_ADORNER);
        }

        public void AOI_Hide_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainXRayView.AdornerLayerManager.Hide(AdornerLayerManager.AOI_ADORNER);
        }

        public void ShowFTILocations ()
        {
            //MainXRayView.adonerImageObject.LoadTIPAdorner(true);
            //MainXRayView.adonerImageObject.TIPDisplay(true);
        }

        public void HideFTILocations ()
        {
            //MainXRayView.adonerImageObject.LoadTIPAdorner(false);
            //MainXRayView.adonerImageObject.TIPDisplay(false);
        }

        public void Magnifier_MenuItem_Click (object sender, RoutedEventArgs e)
        {
            UnregisterMouseEvents();
            m_MagnifierSettings.IsMagniferEnabled = true;
            Image image = new Image();
            image.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/checkmark.gif", UriKind.Relative));
            Magnifier_MenuItem.Icon = image;

            Stream cursorStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("L3.Cargo.Workstation.Plugins.XRayImageBase.Resources.Cursors.magnify.cur");
            Cursor CustomCursor = new System.Windows.Input.Cursor(cursorStream);
            MainXRayView.MainImage.Cursor = CustomCursor;
        }

        public void Measurements_MenuItem_Click (object sender, RoutedEventArgs e)
        {
            UnregisterMouseEvents();

            Measurement_Show_MenuItem_Click(sender, e);
            MeasureAdorner adorner = (MainXRayView.AdornerLayerManager[AdornerLayerManager.MEASUREMENT_ADORNER] as MeasureAdorner);
            adorner.Enabled = true;

            Image image = new Image();
            image.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/checkmark.gif", UriKind.Relative));
            Measurements_MenuItem.Icon = image;

            Stream cursorStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("L3.Cargo.Workstation.Plugins.XRayImageBase.Resources.Cursors.ruler.cur");
            Cursor CustomCursor = new System.Windows.Input.Cursor(cursorStream);
            MainXRayView.MainImage.Cursor = CustomCursor;
        }

        public void Annotation_MenuItem_Click (object sender, RoutedEventArgs e)
        {
            UnregisterMouseEvents();

            Annotation_Show_MenuItem_Click(sender, e);
            AnnotationAdorner annotationAdorner = (MainXRayView.AdornerLayerManager[AdornerLayerManager.ANNOTATION_ADORNER] as AnnotationAdorner);
            annotationAdorner.Enabled = true;

            Annotation_Rectangle.Visibility = Visibility.Visible;
            Annotation_Ellipse.Visibility = Visibility.Visible;
            Options_Menu_Seperator.Visibility = Visibility.Visible;
            Image image = new Image();
            image.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/checkmark.gif", UriKind.Relative));
            Annotation_MenuItem.Icon = image;

            MainXRayView.MainImage.Cursor = Cursors.Pen;
        }

        public void AOI_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            UnregisterMouseEvents();

            AOI_Show_MenuItem_Click(sender, e);
            AOIAdorner aoiAdorner = (MainXRayView.AdornerLayerManager[AdornerLayerManager.AOI_ADORNER] as AOIAdorner);
            aoiAdorner.Enabled = true;
            aoiAdorner.Setup(m_ViewObject);

            Image image = new Image();
            image.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/checkmark.gif", UriKind.Relative));
            AOI_MenuItem.Icon = image;

            MainXRayView.MainImage.Cursor = Cursors.Cross;
        }

        public void PanZoom_MenuItem_Click (object sender, RoutedEventArgs e)
        {
            UnregisterMouseEvents();
            MainXRayView.PanAndZoomControl.MouseEventsEnabled = true;
            Image image = new Image();
            image.Source = new BitmapImage(new Uri(@"/L3.Cargo.Workstation.Plugins.XRayImageBase;component/Resources/Icons/checkmark.gif", UriKind.Relative));
            PanZoom_MenuItem.Icon = image;
            PanAndZoom_FitToScreen.Visibility = System.Windows.Visibility.Visible;
            PanAndZoom_Normal.Visibility = System.Windows.Visibility.Visible;
            Options_Menu_Seperator.Visibility = Visibility.Visible;

            MainXRayView.MainImage.Cursor = Cursors.Hand;
        }

        public void Dispose ()
        {
            MainXRayView.MainImage.MouseMove -= new MouseEventHandler(Img_MouseMove);
            MainXRayView.MainImage.MouseLeave -= new MouseEventHandler(Img_MouseLeave);

            MainXRayView.Magnifier_Panel.SizeChanged -= new SizeChangedEventHandler(Magnifier_SizeChanged);
            MainXRayView.Magnifier_Panel.MouseMove -= new MouseEventHandler(Magnifier_MouseMove);
            MainXRayView.Magnifier_Panel.MouseLeftButtonDown -= new MouseButtonEventHandler(Magnifier_MouseMove);

            m_statusBarItems = null;

            MainXRayView.Dispose();
            MainXRayView = null;
        }

        #endregion Public Methods
    }
}
