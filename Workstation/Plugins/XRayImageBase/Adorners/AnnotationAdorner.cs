using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using L3.Cargo.Controls;
using System.Windows.Data;
using L3.Cargo.Common;

namespace L3.Cargo.Workstation.Plugins.XRayImageBase.Adorners
{
    public class AnnotationAdorner : AdornerBase
    {
        #region Private Members

        private List<Annotation> _Annotations;

        private Annotation _FocusedAnnotation;

        private bool _IsEllipse;

        private Point _StartPoint;

        private bool _CanCreateAnnotations = true;

        private LineGeometry ResizeTopLine;
        private LineGeometry ResizeLeftLine;
        private LineGeometry ResizeBottomLine;
        private LineGeometry ResizeRightLine;
        private bool m_MoveFocusedObject;
        private bool m_ResizeFocusedObject;

        #endregion Private Members

        #region Constructors

        public AnnotationAdorner (UIElement element, PanZoomPanel panZoomPanel)
            : base(element, panZoomPanel)
        {
            _Annotations = new List<Annotation>();

            MenuItem mi = new MenuItem();
            var binding = new Binding("Comment");
            binding.Source = CultureResources.getDataProvider();
            BindingOperations.SetBinding(mi, MenuItem.HeaderProperty, binding);
            mi.Click += new RoutedEventHandler(CommentItem_Click);
            ContextMenu.Items.Add(mi);

            MainContextMenuOpen = false;
        }

        #endregion Constructors

        #region Public Methods

        public List<Annotation> GetAnnotations()
        {
            return _Annotations;
        }

        public bool MainContextMenuOpen
        {
            get;
            set;
        }

        #endregion 

        #region Private Methods

        private Annotation Create (Rect rect, string commentText, bool IsReadOnly, double radiusX, double radiusY)
        {
            Pen pen = new Pen(Brushes.Goldenrod, 3);
            pen.LineJoin = PenLineJoin.Round;

            CommentBoxControl commentBox = new CommentBoxControl();
            commentBox.IsReadOnly = IsReadOnly;
            commentBox.Text = commentText;
            commentBox.Width = 275;
            commentBox.Height = 165;
            visualChildren.Add(commentBox);

            Annotation annotation = new Annotation(pen, new RectangleGeometry(rect, radiusX, radiusY), commentBox, commentText, IsReadOnly);

            return annotation;
        }

        private void GetFocusedObject (Point cursorPosition)
        {
            if (_FocusedAnnotation != null)
            {
                CheckResizeLines(cursorPosition);
                if (!_FocusedAnnotation.Marking.FillContains(cursorPosition, 2.0, ToleranceType.Absolute) 
                    && ResizeTopLine == null 
                    && ResizeBottomLine == null 
                    && ResizeLeftLine == null 
                    && ResizeRightLine == null)
                {
                    SetAnnotationDeselected(_FocusedAnnotation);
                    ResizeTopLine = null;
                    ResizeLeftLine = null;
                    ResizeBottomLine = null;
                    ResizeRightLine = null;
                    m_ResizeFocusedObject = false;
                    m_MoveFocusedObject = false;
                    _FocusedAnnotation = null;
                    this.Cursor = System.Windows.Input.Cursors.SizeAll;
                }
            }

            if (_FocusedAnnotation == null)
            {
                foreach (Annotation annotation in _Annotations)
                {
                    if (_FocusedAnnotation == null && !annotation.IsReadOnly && annotation.Marking.FillContains(cursorPosition, 2.0, ToleranceType.Absolute))
                    {
                        _FocusedAnnotation = annotation;
                        _FocusedAnnotation.CommentBox.Visibility = System.Windows.Visibility.Visible;
                        _FocusedAnnotation.Pen.Brush = Brushes.Goldenrod;
                    }
                    else
                    {
                        SetAnnotationDeselected(annotation);
                    }
                }
            }
            InvalidateVisual();
        }


        #endregion Private Methods


        #region Layer Events

        protected override void AdornedElement_MouseLeftButtonDown (object sender, MouseButtonEventArgs e)
        {
            base.AdornedElement_MouseLeftButtonDown(sender, e);

            Point pt = PointToImageCoordinates(e.GetPosition(this));

            GetFocusedObject(pt);

            if (_FocusedAnnotation == null)
            {
                _StartPoint = pt;

                foreach (Annotation annotation in _Annotations)
                {
                    annotation.CommentBox.Visibility = System.Windows.Visibility.Hidden;
                }
                
                InvalidateVisual();
            }
        }

        protected override void AdornedElement_MouseMove (object sender, MouseEventArgs e)
        {
            base.AdornedElement_MouseMove(sender, e);

            if (Enabled && !m_MoveFocusedObject && !m_ResizeFocusedObject && MainContextMenuOpen == false)
            {
                if (e.LeftButton == MouseButtonState.Pressed && !ContextMenu.IsLoaded && _CanCreateAnnotations)
                {
                    Point CurrentPoint = PointToImageCoordinates(e.GetPosition(this));

                    if (_Annotations.Count < 20)
                    {
                        Rect rect = new Rect(_StartPoint, CurrentPoint);

                        if (rect.Width > 10)
                        {
                            _FocusedAnnotation = (_IsEllipse) ? Create(rect, String.Empty, false, Double.MaxValue, Double.MaxValue) : Create(rect, String.Empty, false, 0, 0);
                            _Annotations.Add(_FocusedAnnotation);
                            _FocusedAnnotation.EndPoint = CurrentPoint;
                            ResizeLeftLine = null;
                            ResizeTopLine = null;
                            ResizeBottomLine = new LineGeometry(_FocusedAnnotation.Marking.Rect.BottomLeft, _FocusedAnnotation.Marking.Rect.BottomRight);
                            ResizeRightLine = new LineGeometry(_FocusedAnnotation.Marking.Rect.TopRight, _FocusedAnnotation.Marking.Rect.BottomRight);
                            m_ResizeFocusedObject = true;
                            Cursor = System.Windows.Input.Cursors.Pen;
                            CaptureMouse();
                        }
                    }
                    else
                    {
                        MessageBox.Show(L3.Cargo.Common.Resources.MaxAnnotationErrorMessage);
                    }
                }
                InvalidateVisual();
            }

        }

        protected override void AdornedElement_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Enabled)
            {
                _FocusedAnnotation = null;
                InvalidateVisual();
            }
        }

        #endregion Layer Events


        #region Menu Events

        protected override void RemoveItem_Click (object sender, EventArgs e)
        {
            base.RemoveItem_Click(sender, e);

            if (_FocusedAnnotation != null && !_FocusedAnnotation.IsReadOnly)
            {
                _Annotations.Remove(_FocusedAnnotation);
                visualChildren.Remove(_FocusedAnnotation.CommentBox);
                _FocusedAnnotation = null;
            }

            InvalidateVisual();
        }

        protected void CommentItem_Click (object sender, EventArgs e)
        {
            if (_FocusedAnnotation != null)
            {
                _FocusedAnnotation.CommentBox.Visibility = _FocusedAnnotation.CommentBox.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                InvalidateVisual();
            }
        }

        public void DrawEllip_Click ()
        {
            _IsEllipse = true;
        }

        public void DrawRect_Click ()
        {
            _IsEllipse = false;
        }

        #endregion Menu Events


        #region Object Events

        protected override void OnMouseEnter (MouseEventArgs e)
        {
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave (MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (Enabled && _FocusedAnnotation == null)
            {
                foreach (Annotation annotation in _Annotations)
                {
                    annotation.CommentBox.Visibility = System.Windows.Visibility.Hidden;
                }
                InvalidateVisual();
            }
        }

        protected override void OnMouseLeftButtonDown (MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (Enabled)
            {
                Pen pen = new Pen(Brushes.Goldenrod, 3);
                Point pt = PointToImageCoordinates(e.GetPosition(this));
                
                m_MoveFocusedObject = false;
                m_ResizeFocusedObject = false;

                GetFocusedObject(pt);

                this.CaptureMouse();

                if (_FocusedAnnotation != null && !_FocusedAnnotation.IsReadOnly)
                {
                    if (ResizeTopLine != null ||
                        ResizeBottomLine != null ||
                        ResizeLeftLine != null ||
                        ResizeRightLine != null)
                    {
                        m_ResizeFocusedObject = true;
                        _StartPoint = pt;
                    }

                    if (!m_ResizeFocusedObject)
                    {
                        if (_FocusedAnnotation.Marking.FillContains(pt))
                        {
                            m_MoveFocusedObject = true;
                            _StartPoint = pt;
                        }
                    }

                }
            }
            bool handled = e.Handled;
        }

        protected override void OnMouseLeftButtonUp (MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (Enabled)
            {
                this.ReleaseMouseCapture();
                m_ResizeFocusedObject = false;
                m_MoveFocusedObject = false;
            }
            InvalidateVisual();
        }

        protected override void OnMouseMove (MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (Enabled)
            {
                Point pt = PointToImageCoordinates(e.GetPosition(this));

                if (e.LeftButton == MouseButtonState.Pressed && _FocusedAnnotation != null && !_FocusedAnnotation.IsReadOnly)
                {
                    Point pt1 = _StartPoint;

                    if (m_MoveFocusedObject)
                    {
                        this.Cursor = System.Windows.Input.Cursors.SizeAll;
                        double xValue = _FocusedAnnotation.Marking.Rect.X + (pt.X - pt1.X);
                        double yValue = _FocusedAnnotation.Marking.Rect.Y + (pt.Y - pt1.Y);

                        if (xValue < 0)
                            xValue = 0;
                        else if ((xValue + _FocusedAnnotation.Marking.Rect.Width) > AdornedElement.RenderSize.Width)
                            xValue = AdornedElement.RenderSize.Width - _FocusedAnnotation.Marking.Rect.Width;

                        if (yValue < 0)
                            yValue = 0;
                        else if ((yValue + _FocusedAnnotation.Marking.Rect.Height) > AdornedElement.RenderSize.Height)
                            yValue = AdornedElement.RenderSize.Height - _FocusedAnnotation.Marking.Rect.Height;

                        _FocusedAnnotation.Marking.Rect = new Rect(xValue,
                                                            yValue,
                                                            _FocusedAnnotation.Marking.Rect.Width,
                                                            _FocusedAnnotation.Marking.Rect.Height);

                        if (_FocusedAnnotation.Marking.RadiusX != 0 && _FocusedAnnotation.Marking.RadiusY != 0)
                        {
                            _FocusedAnnotation.Marking.RadiusX = Double.MaxValue;
                            _FocusedAnnotation.Marking.RadiusY = Double.MaxValue;
                        }
                    }
                    else if (m_ResizeFocusedObject)
                    {
                        double xValue = _FocusedAnnotation.Marking.Rect.X;
                        double yValue = _FocusedAnnotation.Marking.Rect.Y;
                        double width = _FocusedAnnotation.Marking.Rect.Width;
                        double height = _FocusedAnnotation.Marking.Rect.Height;

                        if (ResizeTopLine != null)
                        {
                            yValue = _FocusedAnnotation.Marking.Rect.Y + (pt.Y - pt1.Y);

                            if (yValue > 0)
                                height = _FocusedAnnotation.Marking.Rect.Height + -(pt.Y - pt1.Y);
                            else
                                yValue = 0;
                        }

                        if (ResizeLeftLine != null)
                        {
                            xValue = _FocusedAnnotation.Marking.Rect.X + (pt.X - pt1.X);
                            if (xValue > 0)
                                width = _FocusedAnnotation.Marking.Rect.Width + -(pt.X - pt1.X);
                        }

                        if (ResizeBottomLine != null)
                        {
                            height = _FocusedAnnotation.Marking.Rect.Height + (pt.Y - pt1.Y);
                            if (height < 0)
                            {
                                yValue += (pt.Y - pt1.Y);
                            }
                        }

                        if (ResizeRightLine != null)
                        {
                            width = _FocusedAnnotation.Marking.Rect.Width + (pt.X - pt1.X);
                            if (width < 0)
                            {
                                xValue += (pt.X - pt1.X);
                            }
                        }

                        if (height < 1)
                            height = 1;

                        if (width < 1)
                            width = 1;

                        if (xValue <= 0)
                            xValue = 0;
                        else if (xValue >= AdornedElement.RenderSize.Width)
                            xValue = AdornedElement.RenderSize.Width;

                        if ((xValue + _FocusedAnnotation.Marking.Rect.Width) > AdornedElement.RenderSize.Width)
                            width = AdornedElement.RenderSize.Width - xValue;

                        if (yValue <= 0)
                            yValue = 0;
                        else if (yValue >= AdornedElement.RenderSize.Height)
                            yValue = AdornedElement.RenderSize.Height;

                        if ((yValue + _FocusedAnnotation.Marking.Rect.Height) > AdornedElement.RenderSize.Height)
                            height = AdornedElement.RenderSize.Height - yValue;

                        _FocusedAnnotation.Marking.Rect = new Rect(xValue,
                                                            yValue,
                                                            width,
                                                            height);

                        if (_FocusedAnnotation.Marking.RadiusX != 0 && _FocusedAnnotation.Marking.RadiusY != 0)
                        {
                            _FocusedAnnotation.Marking.RadiusX = Double.MaxValue;
                            _FocusedAnnotation.Marking.RadiusY = Double.MaxValue;
                        }
                    }
                    _StartPoint = pt;
                    InvalidateVisual();
                }
                else if (_FocusedAnnotation != null && !_FocusedAnnotation.IsReadOnly)
                {
                    // mouse isn't being held, check resize lines
                    this.Cursor = System.Windows.Input.Cursors.SizeAll;
                    CheckResizeLines(pt);
                }
                else if (_FocusedAnnotation == null)
                {
                    bool commentOpen = false;
                    foreach (Annotation annotation in _Annotations)
                    {
                        if (annotation.Marking.FillContains(pt, 20.0, ToleranceType.Absolute) && !commentOpen)
                        {
                            annotation.CommentBox.Visibility = System.Windows.Visibility.Visible;
                            commentOpen = true;
                        }
                        else
                        {
                            annotation.CommentBox.Visibility = System.Windows.Visibility.Hidden;
                        }
                    }
                    InvalidateVisual();
                }
            }
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            if (Enabled)
            {
                Point pt = PointToImageCoordinates(e.GetPosition(this));
                GetFocusedObject(pt);
            }
            base.OnMouseRightButtonDown(e);
            InvalidateVisual();
        }

        protected void CheckResizeLines(Point cursorPt)
        {
            // Determine if the cursor is over a line for resizing
            ResizeTopLine = new LineGeometry(_FocusedAnnotation.Marking.Rect.TopLeft, _FocusedAnnotation.Marking.Rect.TopRight);
            ResizeLeftLine = new LineGeometry(_FocusedAnnotation.Marking.Rect.TopLeft, _FocusedAnnotation.Marking.Rect.BottomLeft);
            ResizeBottomLine = new LineGeometry(_FocusedAnnotation.Marking.Rect.BottomLeft, _FocusedAnnotation.Marking.Rect.BottomRight);
            ResizeRightLine = new LineGeometry(_FocusedAnnotation.Marking.Rect.TopRight, _FocusedAnnotation.Marking.Rect.BottomRight);

            if (ResizeBottomLine.GetWidenedPathGeometry(_FocusedAnnotation.Pen).StrokeContains(_FocusedAnnotation.Pen, cursorPt))
            {
                this.Cursor = System.Windows.Input.Cursors.SizeNS;
                ResizeTopLine = null;
            }
            else if (ResizeTopLine.GetWidenedPathGeometry(_FocusedAnnotation.Pen).StrokeContains(_FocusedAnnotation.Pen, cursorPt))
            {
                this.Cursor = System.Windows.Input.Cursors.SizeNS;
                ResizeBottomLine = null;
            }
            else
            {
                ResizeBottomLine = null;
                ResizeTopLine = null;
            }

            if (ResizeRightLine.GetWidenedPathGeometry(_FocusedAnnotation.Pen).StrokeContains(_FocusedAnnotation.Pen, cursorPt))
            {
                if (ResizeBottomLine != null)
                {
                    this.Cursor = System.Windows.Input.Cursors.SizeNWSE;
                }
                else if (ResizeTopLine != null)
                {
                    this.Cursor = System.Windows.Input.Cursors.SizeNESW;
                }
                else
                {
                    this.Cursor = System.Windows.Input.Cursors.SizeWE;
                }
                ResizeLeftLine = null;
            }
            else if (ResizeLeftLine.GetWidenedPathGeometry(_FocusedAnnotation.Pen).StrokeContains(_FocusedAnnotation.Pen, cursorPt))
            {

                if (ResizeBottomLine != null)
                {
                    this.Cursor = System.Windows.Input.Cursors.SizeNESW;
                }
                else if (ResizeTopLine != null)
                {
                    this.Cursor = System.Windows.Input.Cursors.SizeNWSE;
                }
                else
                {
                    this.Cursor = System.Windows.Input.Cursors.SizeWE;
                }
                ResizeRightLine = null;
            }
            else
            {
                ResizeRightLine = null;
                ResizeLeftLine = null;
            }
        }

        protected override Size ArrangeOverride (Size finalSize)
        {
            foreach (Annotation annotObj in _Annotations)
            {
                if (annotObj != null && annotObj.CommentBox.Visibility == System.Windows.Visibility.Visible)
                {
                    Point StartPt = PointToWindowCoordinates(annotObj.CommentBoxBottomLeft);
                    Rect rect = new Rect(StartPt.X, StartPt.Y, annotObj.CommentBox.Width, annotObj.CommentBox.Height);
                    rect.Y = rect.Y - annotObj.CommentBox.Height;

                    if (rect.Top < 0) 
                    {
                        rect.Y = 0;
                    }
                    double right = ((FrameworkElement)this.Parent).ActualWidth;
                    if (rect.Right > right)
                    {
                        rect.X = right - rect.Width;
                    }

                    annotObj.CommentBox.Arrange(rect);
                }
            }

            return base.ArrangeOverride(finalSize);
        }

        public override GeneralTransform GetDesiredTransform (GeneralTransform transform)
        {
            return base.GetDesiredTransform(new MatrixTransform(1, 0, 0, 1, 0, 0));
        }

        public void AddAnnotationInfo(AnnotationInfo info, bool isReadOnly)
        {
            var newAnnot = Create(new Rect(info.TopLeft.X, info.TopLeft.Y, info.Width, info.Height), info.Comment, isReadOnly, info.RadiusX, info.RadiuxY);
            SetAnnotationDeselected(newAnnot);
            _Annotations.Add(newAnnot);
        }

        public void SetCanCreateAnnotation(bool canCreateNewAnnot)
        {
            _CanCreateAnnotations = true;
        }

        protected override void OnRender (DrawingContext drawingContext)
        {
            foreach (Annotation annotation in _Annotations)
            {
                DrawAnnotation(annotation, drawingContext);
            }

            base.OnRender(drawingContext);
        }

        private void SetAnnotationDeselected(Annotation annotation)
        {
            annotation.CommentBox.Visibility = System.Windows.Visibility.Hidden;
            annotation.Pen = new Pen(Brushes.Green, 3);
            annotation.Pen.LineJoin = PenLineJoin.Round;
        }

        private void DrawAnnotation(Annotation annotation, DrawingContext drawingContext)
        {
            Point startPoint = PointToWindowCoordinates(annotation.StartPoint);
            Point endPoint = PointToWindowCoordinates(annotation.EndPoint);
            RectangleGeometry rectGeo = new RectangleGeometry(new Rect(startPoint, endPoint), annotation.Marking.RadiusX, annotation.Marking.RadiusY);
            Brush fill = (Enabled) ? Brushes.Transparent : null;
            drawingContext.DrawGeometry(fill, annotation.Pen, rectGeo);

            if (annotation.CommentBox.Visibility == Visibility.Visible)
            {
                Point commentBoxPoint = PointToWindowCoordinates(annotation.CommentBoxBottomLeft);
                if (annotation.Marking.RadiusX != 0)
                {
                    drawingContext.DrawLine(annotation.Pen, new Point(rectGeo.Rect.TopRight.X, rectGeo.Rect.TopRight.Y + rectGeo.Rect.Height / 2), commentBoxPoint);
                }
                else
                {
                    drawingContext.DrawLine(annotation.Pen, rectGeo.Rect.TopRight, commentBoxPoint);
                }
            }
        }

        #endregion Object Events

    }
}
