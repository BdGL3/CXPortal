using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using L3.Cargo.Controls;

namespace L3.Cargo.Workstation.Plugins.XRayImageBase.Adorners
{
    public class MeasureAdorner : AdornerBase
    {
        #region Private Members

        private List<MeasurementLine> _MeasurementLines;

        private MeasurementLine _CurrentLine;

        private MeasurementLine _FocusedLine;

        private uint _SamplingSpeed;

        private float _SamplingSpace;

        private Cursor _Cursor;

        #endregion Private Members
        

        #region Public Members

        public uint SamplingSpeed
        {
            set { _SamplingSpeed = value; }
            get { return _SamplingSpeed; }
        }

        public float SamplingSpace
        {
            set { _SamplingSpace = value; }
            get { return _SamplingSpace; }
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;
                this.Cursor = (value) ? _Cursor : null;               
            }
        }

        public List<MeasurementLine> GetMeasurementLines()
        {
            return _MeasurementLines;
        }

        #endregion Public Members


        #region Constructor

        public MeasureAdorner (UIElement element, PanZoomPanel panZoomPanel) :
            base(element, panZoomPanel)
        {
            _MeasurementLines = new List<MeasurementLine>();
            _Cursor = new Cursor(Assembly.GetExecutingAssembly().GetManifestResourceStream("L3.Cargo.Workstation.Plugins.XRayImageBase.Resources.Cursors.ruler.cur"));
        }

        #endregion Constructor


        #region Private Methods

        #endregion Private Methods


        #region Layer Events

        protected override void AdornedElement_MouseMove (object sender, MouseEventArgs e)
        {
            Point CurrentPoint = e.GetPosition(this);

            if (_CurrentLine != null)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    _CurrentLine.EndPoint = CurrentPoint;
                }
                else if (e.LeftButton == MouseButtonState.Released)
                {
                    if (_CurrentLine.Length > 1)
                    {
                        _CurrentLine.SetCorrectPoints(_Zoom, _OffsetX, _OffsetY);
                        _MeasurementLines.Add(_CurrentLine);
                    }
                    _CurrentLine = null;
                }

                InvalidateVisual();
            }

            base.AdornedElement_MouseMove(sender, e);
        }

        protected override void AdornedElement_MouseLeftButtonDown (object sender, MouseButtonEventArgs e)
        {
            base.AdornedElement_MouseLeftButtonDown(sender, e);

            Point CurrentPoint = e.GetPosition(this);
            _CurrentLine = new MeasurementLine(CurrentPoint, CurrentPoint);
            _FocusedLine = null;

            InvalidateVisual();
        }

        #endregion Layer Events


        #region Menu Events

        protected override void RemoveItem_Click (object sender, EventArgs e)
        {
            base.RemoveItem_Click(sender, e);

            _MeasurementLines.Remove(_FocusedLine);
            _FocusedLine = null;

            InvalidateVisual();
        }

        #endregion Menu Events


        #region Object Events

        protected override void OnMouseEnter (MouseEventArgs e)
        {
            if (Enabled)
            {
                Point CurrentPoint = e.GetPosition(this);

                foreach (MeasurementLine line in _MeasurementLines)
                {
                    if (line.Contains(CurrentPoint, _Zoom, _OffsetX, _OffsetY))
                    {
                        _FocusedLine = line;
                        line.Brush = Brushes.DarkGoldenrod;
                        InvalidateVisual();
                        break;
                    }
                }

                base.OnMouseEnter(e);
            }            
        }

        protected override void OnMouseLeave (MouseEventArgs e)
        {
            if (Enabled)
            {
                if (_FocusedLine != null)
                {
                    _FocusedLine.Brush = Brushes.Green;
                    InvalidateVisual();
                }

                base.OnMouseLeave(e);
            }
        }

        protected override void OnContextMenuOpening (ContextMenuEventArgs e)
        {
            if (Enabled)
            {
                base.OnContextMenuOpening(e);
            }
            else
            {
                e.Handled = true;
            }
        }

        public override GeneralTransform GetDesiredTransform (GeneralTransform transform)
        {
            return base.GetDesiredTransform(new MatrixTransform(1, 0, 0, 1, 0 ,0));
        }
        
        protected override void OnRender (DrawingContext drawingContext)
        {
            if (_CurrentLine != null && _CurrentLine.Length > 0)
            {
                Point endPoint = (_CurrentLine.Horizontal) ? new Point(_CurrentLine.EndPoint.X, _CurrentLine.StartPoint.Y) : new Point(_CurrentLine.StartPoint.X, _CurrentLine.EndPoint.Y);

                var mouseLinePen = new Pen(_CurrentLine.Brush, _CurrentLine.Thickness);
                mouseLinePen.DashCap = PenLineCap.Flat;
                mouseLinePen.DashStyle = new DashStyle(new double[] { 5, 2 }, 0);

                drawingContext.DrawLine(mouseLinePen, _CurrentLine.StartPoint, _CurrentLine.EndPoint);


                var finalLinePen = new Pen(_CurrentLine.Brush, _CurrentLine.Thickness);

                drawingContext.DrawLine(finalLinePen, _CurrentLine.StartPoint, endPoint);


                double length = (_CurrentLine.Length * _SamplingSpace) / (_Zoom * 1000);

                FormattedText formattedText =
                    new FormattedText(length.ToString("F") + "m",
                                      _CurrentLine.CultureInfo,
                                      _CurrentLine.FlowDirection,
                                      _CurrentLine.Typeface,
                                      _CurrentLine.FontSize,
                                      _CurrentLine.Brush);
                drawingContext.DrawText(formattedText, endPoint);
            }

            foreach (MeasurementLine line in _MeasurementLines)
            {
                double length = (line.Length * _SamplingSpace) / 1000;

                FormattedText formattedText =
                    new FormattedText(length.ToString("F") + "m",
                                      line.CultureInfo,
                                      line.FlowDirection,
                                      line.Typeface,
                                      line.FontSize,
                                      line.Brush);

                Point TextStartPt = new Point((line.MidPoint.X * _Zoom) + (_OffsetX) - (formattedText.WidthIncludingTrailingWhitespace / 2),
                                              (line.MidPoint.Y * _Zoom) + (_OffsetY) - (formattedText.Height / 2));

                drawingContext.DrawText(formattedText, TextStartPt);


                var linePen = new Pen(line.Brush, line.Thickness);

                Point firstHalfStartPoint = new Point(line.StartPoint.X, line.StartPoint.Y);

                firstHalfStartPoint.X = (firstHalfStartPoint.X * _Zoom) + (_OffsetX);
                firstHalfStartPoint.Y = (firstHalfStartPoint.Y * _Zoom) + (_OffsetY);

                Point firstHalfEndPoint = (line.Horizontal) ?
                    new Point((line.MidPoint.X * _Zoom) + (_OffsetX) - (formattedText.WidthIncludingTrailingWhitespace / 2), (line.MidPoint.Y * _Zoom) + (_OffsetY)) :
                    new Point((line.MidPoint.X * _Zoom) + (_OffsetX), (line.MidPoint.Y * _Zoom) + (_OffsetY) - (formattedText.Height / 2));

                drawingContext.DrawLine(linePen, firstHalfStartPoint, firstHalfEndPoint);


                Point SecondHalfStartPointt = (line.Horizontal) ?
                    new Point((line.MidPoint.X * _Zoom) + (_OffsetX) + (formattedText.WidthIncludingTrailingWhitespace / 2), (line.MidPoint.Y * _Zoom) + (_OffsetY)) :
                    new Point((line.MidPoint.X * _Zoom) + (_OffsetX), (line.MidPoint.Y * _Zoom) + (_OffsetY) + (formattedText.Height / 2));

                Point SecondHalfEndPoint = new Point(line.EndPoint.X, line.EndPoint.Y);

                SecondHalfEndPoint.X = (SecondHalfEndPoint.X * _Zoom) + (_OffsetX);
                SecondHalfEndPoint.Y = (SecondHalfEndPoint.Y * _Zoom) + (_OffsetY);

                drawingContext.DrawLine(linePen, SecondHalfStartPointt, SecondHalfEndPoint);
            }

            base.OnRender(drawingContext);
        }

        #endregion Object Events
    }
}
