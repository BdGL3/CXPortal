using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using L3.Cargo.Common;
using L3.Cargo.Workstation.Plugins.XRayImageBase.Common;
using L3.Cargo.Common.Xml.Annotations_1_0;
using L3.Cargo.Controls;

namespace L3.Cargo.Workstation.Plugins.XRayImageBase.Adorners
{
    class AOIAdorner : AdornerBase, IDisposable
    {
        #region Private Members

        private bool LeftMouseBtnWasPressed;

        private Annotation m_CurrentAOI;

        private UIElement m_element;

        private ViewObject m_ViewObject;

        private List<AOIWindow> m_AOIWindows;

        private Point _StartPoint;

        #endregion Private Members

        #region Events

        public event AlgServerRequestEventHandler AlgServerRequestEvent;

        #endregion

        #region Constructors

        public AOIAdorner(UIElement element, PanZoomPanel panZoomPanel)
            : base(element, panZoomPanel)
        {
            m_AOIWindows = new List<AOIWindow>();
            LeftMouseBtnWasPressed = false;
            m_element = element;
        }

        #endregion Constructors


        #region Protected Methods

        public void Setup (ViewObject viewObj)
        {
            m_ViewObject = viewObj;
        }

        protected Annotation Create(Rect rct, String txt, bool IsReadOnly)
        {
            Pen pen = new Pen(Brushes.Yellow, 3);        
            RectangleGeometry geo;       
            geo = new RectangleGeometry(rct);
            Annotation obj = new Annotation(pen, geo, null, IsReadOnly);
       
            return obj;
        }

        #endregion Protected Methods


        #region Layer Events
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            AdornedElement_MouseMove(null, e);
        }

        protected override void AdornedElement_MouseMove (object sender, MouseEventArgs e)
        {
            base.AdornedElement_MouseMove(sender, e);

            if (LeftMouseBtnWasPressed)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point CurrentPoint = PointToImageCoordinates(e.GetPosition(this));

                    if (m_CurrentAOI != null)
                    {
                        m_CurrentAOI.EndPoint = CurrentPoint;
                    }
                    else
                    {
                        Rect tmpRect = new Rect(_StartPoint, CurrentPoint);

                        if (tmpRect.Width > 10)
                            m_CurrentAOI = Create(tmpRect, null, false);
                    }
                    InvalidateVisual();
                }

                if (e.LeftButton == MouseButtonState.Released)
                {
                    if (m_CurrentAOI != null)
                    {
                        Point startPoint = m_CurrentAOI.StartPoint;
                        Point endPoint = m_CurrentAOI.EndPoint;

                        CreateWindow(startPoint, endPoint);

                        m_CurrentAOI = null;
                        LeftMouseBtnWasPressed = false;
                        InvalidateVisual();
                    }
                }
            }
        }

        protected override void AdornedElement_MouseLeftButtonDown (object sender, MouseButtonEventArgs e)
        {
            base.AdornedElement_MouseLeftButtonDown(sender, e);
            if (m_CurrentAOI == null)
            {
                _StartPoint = PointToImageCoordinates(e.GetPosition(this));
            }
            LeftMouseBtnWasPressed = true;
        }
        
        #endregion Layer Events


        #region Object Events
        
        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            return base.GetDesiredTransform(new MatrixTransform(1, 0, 0, 1, 0, 0));
        }
        
        protected override void OnMouseLeftButtonUp (MouseButtonEventArgs e)
        {
            
            if (m_CurrentAOI != null)
            {
                Point startPoint = m_CurrentAOI.StartPoint;
                Point endPoint = m_CurrentAOI.EndPoint;

                CreateWindow(startPoint, endPoint);

                m_CurrentAOI = null;
                LeftMouseBtnWasPressed = false;
                InvalidateVisual();
            }
        }

        protected Point FitPointInImage(Point pt)
        {
            
            if (pt.X < 0)
            {
                pt = new Point(0, pt.Y);
            }
            if (pt.X > m_ViewObject.HighEnergy.Width)
            {
                pt = new Point(m_ViewObject.HighEnergy.Width, pt.Y);
            }
            if (pt.Y < 0)
            {
                pt = new Point(pt.X, pt.Y);
            }
            if (pt.Y > m_ViewObject.HighEnergy.Height)
            {
                pt = new Point(pt.X, m_ViewObject.HighEnergy.Height);
            }
            return pt;
        }

        protected void CreateWindow (Point startPoint, Point endPoint)
        {
            startPoint = FitPointInImage(startPoint);
            endPoint = FitPointInImage(endPoint);
            
            for (int index = m_AOIWindows.Count - 1; index >= 0; index--)
            {
                if (m_AOIWindows[index].IsLoaded == false)
                {
                    m_AOIWindows.RemoveAt(index);
                }
            }

            //TODO: This needs to be configurable
            if (m_AOIWindows.Count >= 3)
            {
                return;
            }


            if (startPoint.X > endPoint.X)
            {
                double temp = endPoint.X;
                endPoint.X = startPoint.X;
                startPoint.X = temp;
            }

            if (startPoint.Y > endPoint.Y)
            {
                double temp = endPoint.Y;
                endPoint.Y = startPoint.Y;
                startPoint.Y = temp;
            }

            SourceObject highEnergy = null;
            SourceObject lowEnergy = null;
            SourceObject trimat = null;
            ViewType viewType = ViewType.Unknown;

            if (m_ViewObject.HighEnergy != null)
            {
                double heightratio = m_ViewObject.HighEnergy.Height / base.ActualHeight;
                double widthratio = m_ViewObject.HighEnergy.Width / base.ActualWidth;

                int StartX = (int)(startPoint.X * widthratio);
                int StartY = (int)(startPoint.Y * heightratio);
                int EndX = (int)(endPoint.X * widthratio);
                int EndY = (int)(endPoint.Y * heightratio);

                int width = EndX - StartX;
                int height = EndY - StartY;

                float[] data = new float[width * height];

                for (int j = StartY, k = 0; j < EndY; j++, k++)
                {
                    for (int i = StartX, l = 0; i < EndX; i++, l++)
                    {
                        data[(k * width) + l] = m_ViewObject.HighEnergy.Data[(j * m_ViewObject.HighEnergy.Width) + i];
                    }
                }

                highEnergy = new SourceObject(data, width, height, false, false);
                viewType = ViewType.HighEnergy;
            }

            if (m_ViewObject.LowEnergy != null)
            {
                double heightratio = m_ViewObject.LowEnergy.Height / base.ActualHeight;
                double widthratio = m_ViewObject.LowEnergy.Width / base.ActualWidth;

                int StartX = (int)(startPoint.X * widthratio);
                int StartY = (int)(startPoint.Y * heightratio);
                int EndX = (int)(endPoint.X * widthratio);
                int EndY = (int)(endPoint.Y * heightratio);

                int width = EndX - StartX;
                int height = EndY - StartY;

                float[] data = new float[width * height];

                for (int j = StartY, k = 0; j < EndY; j++, k++)
                {
                    for (int i = StartX, l = 0; i < EndX; i++, l++)
                    {
                        data[(k * width) + l] = m_ViewObject.LowEnergy.Data[(j * m_ViewObject.LowEnergy.Width) + i];
                    }
                }

                lowEnergy = new SourceObject(data, width, height, false, false);
                viewType = ViewType.LowEnergy;
            }

            ViewObject viewObj = new ViewObject(m_ViewObject.Name, m_ViewObject.ImageIndex, viewType, highEnergy, lowEnergy, m_ViewObject.MaxDetectorsPerBoard, m_ViewObject.BitsPerPixel, m_ViewObject.SamplingSpeed, m_ViewObject.SamplingSpace, new List<AnnotationInfo>());
            try
            {
                AOIWindow window = new AOIWindow();
                window.AlgServerRequestEvent += new AlgServerRequestEventHandler(AOIWindow_AlgServerRequestEvent);
                window.Setup(viewObj);
                window.AOIXRayView.MainImage.Width = (int)(endPoint.X - startPoint.X);
                window.AOIXRayView.MainImage.Height = (int)(endPoint.Y - startPoint.Y);
                window.Show();

                foreach (AOIWindow aoiWindow in m_AOIWindows)
                {
                    if (window.Top == aoiWindow.Top && (window.Left == aoiWindow.Left))
                    {
                        window.Top = aoiWindow.Top + 25;
                        window.Left = aoiWindow.Left + 25;
                    }
                }

                m_AOIWindows.Add(window);
            }
            catch (Exception ex)
            {

            }
        }

        void AOIWindow_AlgServerRequestEvent(object sender, AlgServerRequestEventArgs e)
        {
            if (AlgServerRequestEvent != null)
            {
                AlgServerRequestEvent(sender, e);
            }
        }

        protected override void OnRender (System.Windows.Media.DrawingContext drawingContext)
        {
            if (m_CurrentAOI != null)
            {
                Point startPoint = PointToWindowCoordinates(m_CurrentAOI.StartPoint);
                Point endPoint = PointToWindowCoordinates(m_CurrentAOI.EndPoint);
                RectangleGeometry rectGeo = new RectangleGeometry(new Rect(startPoint, endPoint), m_CurrentAOI.Marking.RadiusX, m_CurrentAOI.Marking.RadiusY);
                Brush fill = (Enabled) ? Brushes.Transparent : null;
                drawingContext.DrawGeometry(fill, m_CurrentAOI.Pen, rectGeo);
            }
            base.OnRender(drawingContext);
        }

        #endregion Object Events


        #region Public Methods

        public void Dispose ()
        {
            foreach (AOIWindow aoiWindow in m_AOIWindows)
            {
                aoiWindow.Close();
            }

            m_AOIWindows.Clear();
            m_AOIWindows = null;
        }

        #endregion Public Methods
    }
}
