using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Documents;
using L3.Cargo.Workstation.Plugins.XRayImageBase;

namespace L3.Cargo.Workstation.Plugins.EAFB
{
    public class SimpleRulerAdorner : Adorner
    {
        private uint _MaxLength;

        private uint _SamplingSpeed;

        private float _SamplingSpace;

        private float _TickHeight = 5;

        private double _Zoom;

        private double _ImageWidth;
		
        private Point _RulerStartPoint;
        
        private Point _RulerEndPoint;

        public double ImageWidth
        {
            set { _ImageWidth = value; }
        }

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

        public SimpleRulerAdorner(UIElement adornedElement, PanAndZoomViewer viewer)
            : base(adornedElement)
        {
            _Zoom = 1;
            _MaxLength = 100;    // 100 pixels max length

            viewer.ZoomTransform.Changed += new EventHandler(ZoomTransform_Changed);
            FrameworkElement frameworkElement = adornedElement as FrameworkElement;
            frameworkElement.Loaded += new RoutedEventHandler(AdornedElement_Loaded);
        }

        void AdornedElement_Loaded (object sender, RoutedEventArgs e)
        {
            XRayView xrayView = sender as XRayView;
            if (xrayView != null)
            {
                _RulerStartPoint = new Point(0.025 * xrayView.ActualWidth, 0.975 * xrayView.ActualHeight);
                _RulerEndPoint = new Point(0.025 * xrayView.ActualWidth, 0.975 * xrayView.ActualHeight);
            }
        }

        void ZoomTransform_Changed(object sender, EventArgs e)
        {
            ScaleTransform transform = sender as ScaleTransform;
            if (transform != null)
            {
                //We can use either ScaleX or Y since they are the same value in the end
                _Zoom = transform.ScaleX;
                InvalidateVisual();
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            double mmPerPixel = (1.0 / _Zoom) * _SamplingSpace;
            double textLength = 1000; // 1 meter
            double lineLength = textLength / mmPerPixel;

            while (lineLength < _MaxLength)
            {
                lineLength *= 2;
                textLength *= 2;
            }

            while (lineLength > _MaxLength)
            {
                lineLength /= 2;
                textLength /= 2;
            }

            _RulerEndPoint.X = _RulerStartPoint.X + lineLength;

            // Draw the Line
            Pen LinePen = new Pen(new SolidColorBrush(Colors.ForestGreen), 2);
            drawingContext.DrawLine(LinePen, _RulerStartPoint, _RulerEndPoint);
            drawingContext.DrawLine(LinePen, new Point(_RulerStartPoint.X, _RulerStartPoint.Y - _TickHeight), new Point(_RulerStartPoint.X, _RulerStartPoint.Y + _TickHeight));
            drawingContext.DrawLine(LinePen, new Point(_RulerEndPoint.X, _RulerEndPoint.Y - _TickHeight), new Point(_RulerEndPoint.X, _RulerEndPoint.Y + _TickHeight));

            // Draw the Length Text
            string lengthText = Math.Ceiling(textLength).ToString() + " mm";
            FormattedText formattedText = new FormattedText(lengthText, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 10, Brushes.ForestGreen);
            drawingContext.DrawText(formattedText, new Point(_RulerEndPoint.X + 4, _RulerEndPoint.Y + 4));
        }
    }
}