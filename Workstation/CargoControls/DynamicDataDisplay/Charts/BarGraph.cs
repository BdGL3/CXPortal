using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Microsoft.Research.DynamicDataDisplay.Common;
using Microsoft.Research.DynamicDataDisplay.DataSources;

namespace Microsoft.Research.DynamicDataDisplay.Charts
{
    public class BarGraph : PointsGraphBase
    {
        public static readonly DependencyProperty FillProperty =
                DependencyProperty.Register("Fill", typeof(Brush), typeof(BarGraph),
                                            new FrameworkPropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty StrokeProperty =
                DependencyProperty.Register("Stroke", typeof(Brush), typeof(BarGraph),
                                            new FrameworkPropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty StrokeThicknessProperty =
                DependencyProperty.Register("StrokeThickness", typeof(double), typeof(BarGraph),
                                            new FrameworkPropertyMetadata(1));

        public static readonly DependencyProperty BarWidthProperty =
                DependencyProperty.Register("BarWidth", typeof(double), typeof(BarGraph),
                                            new FrameworkPropertyMetadata(10));

        public Brush Fill
        {
            get
            {
                return (Brush)GetValue(FillProperty);
            }
            set
            {
                SetValue(FillProperty, value);
            }
        }

        public Brush Stroke
        {
            get
            {
                return (Brush)GetValue(StrokeProperty);
            }
            set
            {
                SetValue(StrokeProperty, value);
            }
        }

        public double StrokeThickness
        {
            get
            {
                return (double)GetValue(StrokeThicknessProperty);
            }
            set
            {
                SetValue(StrokeThicknessProperty, value);
            }
        }

        public double BarWidth
        {
            get
            {
                return (double)GetValue(BarWidthProperty);
            }
            set
            {
                SetValue(BarWidthProperty, value);
            }
        }

        protected override void  OnRenderCore(DrawingContext dc, RenderState state)
        {
            if (DataSource == null) return;
            var transform = Plotter2D.Viewport.Transform;

            Rect bounds = Rect.Empty;
            using (IPointEnumerator enumerator = DataSource.GetEnumerator(GetContext()))
            {
                Point point = new Point();
                while (enumerator.MoveNext())
                {
                    enumerator.GetCurrent(ref point);
                    enumerator.ApplyMappings(this);

                    Point zero = new Point(point.X, 0);
                    Point screenPoint = point.DataToScreen(transform);
                    Point screenZero = zero.DataToScreen(transform);

                    double height = screenPoint.Y = screenZero.Y;
                    height = (height >= 0) ? height : -height;

                    dc.DrawRectangle(Fill, new Pen(Stroke, StrokeThickness),
                                     new Rect(screenPoint.X - BarWidth / 2, screenZero.Y, BarWidth, height));

                    bounds = Rect.Union(bounds, point);
                }
            }

            ContentBounds = bounds;
        }
    }
}
