using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace L3.Cargo.Workstation.Plugins.XRayImageBase.Adorners
{
    public class MeasurementLine
    {
        #region Private Members

        private const int _AreaBuffer = 5;

        private Point _StartPoint;

        private Point _EndPoint;

        private double _Thickness;

        private Brush _Brush;

        private CultureInfo _CultureInfo;

        private FlowDirection _FlowDirection;

        private Typeface _TypeFace;

        private double _FontSize;

        #endregion Private Members


        #region Public Members

        public Point StartPoint
        {
            get
            {
                return _StartPoint;
            }
            set
            {
                _StartPoint = value;
            }
        }

        public Point EndPoint
        {
            get
            {
                return _EndPoint;
            }
            set
            {
                _EndPoint = value;
            }
        }

        public Point MidPoint
        {
            get
            {
                return new Point((EndPoint.X + StartPoint.X) / 2, (EndPoint.Y + StartPoint.Y) / 2);
            }
        }

        public double Thickness
        {
            get
            {
                return _Thickness;
            }
            set
            {
                _Thickness = value;
            }
        }

        public Brush Brush
        {
            get
            {
                return _Brush;
            }
            set
            {
                _Brush = value;
            }
        }

        public CultureInfo CultureInfo
        {
            get
            {
                return _CultureInfo;
            }
            set
            {
                _CultureInfo = value;
            }
        }

        public FlowDirection FlowDirection
        {
            get
            {
                return _FlowDirection;
            }
            set
            {
                _FlowDirection = value;
            }
        }

        public Typeface Typeface
        {
            get
            {
                return _TypeFace;
            }
            set
            {
                _TypeFace = value;
            }
        }

        public double FontSize
        {
            get
            {
                return _FontSize;
            }
            set
            {
                _FontSize = value;
            }
        }

        public int Length
        {
            get
            {
                return (int)Math.Abs((Horizontal) ? EndPoint.X - StartPoint.X : EndPoint.Y - StartPoint.Y);
            }
        }

        public bool Horizontal
        {
            get
            {
                return (Math.Abs(EndPoint.X - StartPoint.X) > Math.Abs(EndPoint.Y - StartPoint.Y));
            }
        }

        #endregion Public Members


        #region Constructors

        public MeasurementLine (Point startPoint, Point endPoint)
        {
            _Thickness = 3;
            _Brush = Brushes.Green;
            _CultureInfo = CultureInfo.GetCultureInfo("en-us");
            _FlowDirection = FlowDirection.LeftToRight;
            _TypeFace = new Typeface("Verdana");
            _FontSize = 14;

            StartPoint = startPoint;
            EndPoint = endPoint;
        }

        #endregion Constructors


        #region Public Methods

        public bool Contains (Point pt, double zoom, double offsetX, double offsetY)
        {
            return (pt.X <= ((EndPoint.X * zoom) + (offsetX) + _AreaBuffer) && pt.X >= ((StartPoint.X * zoom) + (offsetX) - _AreaBuffer) &&
                    pt.Y <= ((EndPoint.Y * zoom) + (offsetY) + _AreaBuffer) && pt.Y >= ((StartPoint.Y * zoom) + (offsetY) - _AreaBuffer));
        }

        public void SetCorrectPoints (double zoom, double offsetX, double offsetY)
        {
            if (Horizontal)
            {
                _EndPoint.Y = _StartPoint.Y;

                if (_StartPoint.X > _EndPoint.X)
                {
                    double x = _EndPoint.X;
                    _EndPoint.X = _StartPoint.X;
                    _StartPoint.X = x;
                }
            }
            else
            {
                _EndPoint.X = _StartPoint.X;

                if (_StartPoint.Y > _EndPoint.Y)
                {
                    double y = _EndPoint.Y;
                    _EndPoint.Y = _StartPoint.Y;
                    _StartPoint.Y = y;
                }
            }

            _StartPoint.X = (_StartPoint.X / zoom) - (offsetX / zoom);
            _StartPoint.Y = (_StartPoint.Y / zoom) - (offsetY / zoom);

            _EndPoint.X = (_EndPoint.X / zoom) - (offsetX / zoom);
            _EndPoint.Y = (_EndPoint.Y / zoom) - (offsetY / zoom);
        }

        #endregion Public Methods
    }
}
