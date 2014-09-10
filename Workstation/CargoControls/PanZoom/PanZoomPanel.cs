using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Ink;
using System.Configuration;
using System.Windows.Media.Animation;

namespace L3.Cargo.Controls
{
    public partial class PanZoomPanel : ContentControl
    {
        #region Dependency Property Definitions

        public static readonly DependencyProperty ZoomProperty =
                DependencyProperty.Register("Zoom", typeof(double), typeof(PanZoomPanel),
                                            new FrameworkPropertyMetadata(1.0, Zoom_PropertyChanged, Zoom_Coerce));

        public static readonly DependencyProperty OffsetXProperty =
                DependencyProperty.Register("OffsetX", typeof(double), typeof(PanZoomPanel),
                                            new FrameworkPropertyMetadata(0.0, OffsetX_PropertyChanged, OffsetX_Coerce));

        public static readonly DependencyProperty OffsetYProperty =
                DependencyProperty.Register("OffsetY", typeof(double), typeof(PanZoomPanel),
                                            new FrameworkPropertyMetadata(0.0, OffsetY_PropertyChanged, OffsetY_Coerce));

        public static readonly DependencyProperty MouseEventsEnabledProperty =
                DependencyProperty.Register("MouseEnabled", typeof(bool), typeof(PanZoomPanel),
                                            new FrameworkPropertyMetadata(false, MouseEventsEnabled_PropertyChanged));

        public static readonly DependencyProperty MinZoomProperty =
                DependencyProperty.Register("MinContentScale", typeof(double), typeof(PanZoomPanel),
                                            new FrameworkPropertyMetadata(0.0625));

        public static readonly DependencyProperty MaxZoomProperty =
                DependencyProperty.Register("MaxContentScale", typeof(double), typeof(PanZoomPanel),
                                            new FrameworkPropertyMetadata(16.0));

        public static readonly DependencyProperty ZoomFactorProperty =
                DependencyProperty.Register("ZoomFactor", typeof(double), typeof(PanZoomPanel),
                                            new FrameworkPropertyMetadata(1.3));

        public static readonly DependencyProperty ZoomAnimationDurationProperty =
                DependencyProperty.Register("ZoomAnimationDuration", typeof(double), typeof(PanZoomPanel),
                                            new FrameworkPropertyMetadata(400.0));

        public static readonly DependencyProperty PanAnimationDurationProperty =
                DependencyProperty.Register("PanAnimationDuration", typeof(double), typeof(PanZoomPanel),
                                            new FrameworkPropertyMetadata(400.0));

        public static readonly DependencyProperty ZoomAccelerationProperty =
                DependencyProperty.Register("ZoomAcceleration", typeof(double), typeof(PanZoomPanel),
                                            new FrameworkPropertyMetadata(0.01));

        public static readonly DependencyProperty ZoomDecelerationProperty =
                DependencyProperty.Register("ZoomDeceleration", typeof(double), typeof(PanZoomPanel),
                                            new FrameworkPropertyMetadata(0.5));

        public static readonly DependencyProperty PanAccelerationProperty =
                DependencyProperty.Register("PanAcceleration", typeof(double), typeof(PanZoomPanel),
                                            new FrameworkPropertyMetadata(0.1));

        public static readonly DependencyProperty PanDecelerationProperty =
                DependencyProperty.Register("PanDeceleration", typeof(double), typeof(PanZoomPanel),
                                            new FrameworkPropertyMetadata(0.9));

        public static readonly DependencyProperty OverviewProperty =
                DependencyProperty.Register("Overview", typeof(Overview), typeof(PanZoomPanel),
                                            new FrameworkPropertyMetadata(null, Overview_PropertyChanged));

        #endregion Dependency Property Definitions


        #region Private Members

        private FrameworkElement _Content;

        private AdornerContentPresenter _Adorner;

        private ScaleTransform _ZoomTransform;

        private TranslateTransform _OffsetTransform;

        private Point _MousePosition;

        private bool _Panning;

        private bool _Zooming;

        #endregion Private Members


        #region Public Members

        public event DoublePropertyEventHandler ZoomChanged;

        public event DoublePropertyEventHandler OffsetXChanged;

        public event DoublePropertyEventHandler OffsetYChanged;

        public double Zoom
        {
            get
            {
                return (double)GetValue(ZoomProperty);
            }
            set
            {
                SetValue(ZoomProperty, value);
            }
        }

        public double OffsetX
        {
            get
            {
                return (double)GetValue(OffsetXProperty);
            }
            set
            {
                SetValue(OffsetXProperty, value);
            }
        }

        public double OffsetY
        {
            get
            {
                return (double)GetValue(OffsetYProperty);
            }
            set
            {
                SetValue(OffsetYProperty, value);
            }
        }

        public bool MouseEventsEnabled
        {
            get
            {
                return (bool)GetValue(MouseEventsEnabledProperty);
            }
            set
            {
                SetValue(MouseEventsEnabledProperty, value);
            }
        }

        public double MinZoom
        {
            get
            {
                return (double)GetValue(MinZoomProperty);
            }
            set
            {
                SetValue(MinZoomProperty, value);
            }
        }

        public double MaxZoom
        {
            get
            {
                return (double)GetValue(MaxZoomProperty);
            }
            set
            {
                SetValue(MaxZoomProperty, value);
            }
        }

        public double ZoomFactor
        {
            get
            {
                return (double)GetValue(ZoomFactorProperty);
            }
            set
            {
                SetValue(ZoomFactorProperty, value);
            }
        }

        public double ZoomAnimationDuration
        {
            get
            {
                return (double)GetValue(ZoomAnimationDurationProperty);
            }
            set
            {
                SetValue(ZoomAnimationDurationProperty, value);
            }
        }

        public double PanAnimationDuration
        {
            get
            {
                return (double)GetValue(PanAnimationDurationProperty);
            }
            set
            {
                SetValue(PanAnimationDurationProperty, value);
            }
        }

        public double ZoomAcceleration
        {
            get
            {
                return (double)GetValue(ZoomAccelerationProperty);
            }
            set
            {
                SetValue(ZoomAccelerationProperty, value);
            }
        }

        public double ZoomDeceleration
        {
            get
            {
                return (double)GetValue(ZoomDecelerationProperty);
            }
            set
            {
                SetValue(ZoomDecelerationProperty, value);
            }
        }

        public double PanAcceleration
        {
            get
            {
                return (double)GetValue(PanAccelerationProperty);
            }
            set
            {
                SetValue(PanAccelerationProperty, value);
            }
        }

        public double PanDeceleration
        {
            get
            {
                return (double)GetValue(PanDecelerationProperty);
            }
            set
            {
                SetValue(PanDecelerationProperty, value);
            }
        }

        public Overview Overview
        {
            get
            {
                return GetValue(OverviewProperty) as Overview;
            }
            set
            {
                if (Overview == null)
                {
                    SetValue(OverviewProperty, value);
                }
            }
        }

        #endregion Public Member


        #region Constructors

        static PanZoomPanel ()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PanZoomPanel), new FrameworkPropertyMetadata(typeof(PanZoomPanel)));
        }

        #endregion Constructors


        #region Private Methods

        private void OffsetTransform_Changed (object sender, EventArgs e)
        {
            TranslateTransform translateTransform = sender as TranslateTransform;

            if (translateTransform != null && OffsetXChanged != null && OffsetYChanged != null)
            {
                OffsetXChanged(this, new DoublePropertyEventArgs(translateTransform.X));
                OffsetYChanged(this, new DoublePropertyEventArgs(translateTransform.Y));
            }
        }

        private void ZoomTransform_Changed (object sender, EventArgs e)
        {
            ScaleTransform scaleTransform = sender as ScaleTransform;

            if (scaleTransform != null && ZoomChanged != null)
            {
                ZoomChanged(this, new DoublePropertyEventArgs(scaleTransform.ScaleX));
            }
        }

        private static void Zoom_PropertyChanged (DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PanZoomPanel panZoomPanel = sender as PanZoomPanel;

            Point contentMousePosition = ((TransformGroup)panZoomPanel._Content.RenderTransform).Inverse.Transform(panZoomPanel._MousePosition);

            AnimationHelper.StartAnimation(panZoomPanel._ZoomTransform, ScaleTransform.ScaleXProperty, (double)e.NewValue, panZoomPanel.ZoomAnimationDuration, panZoomPanel.ZoomAcceleration, panZoomPanel.ZoomDeceleration);
            AnimationHelper.StartAnimation(panZoomPanel._ZoomTransform, ScaleTransform.ScaleYProperty, (double)e.NewValue, panZoomPanel.ZoomAnimationDuration, panZoomPanel.ZoomAcceleration, panZoomPanel.ZoomDeceleration);

            panZoomPanel._Zooming = true;

            panZoomPanel.OffsetX = -1.0 * (contentMousePosition.X * panZoomPanel.Zoom - panZoomPanel._MousePosition.X);
            panZoomPanel.OffsetY = -1.0 * (contentMousePosition.Y * panZoomPanel.Zoom - panZoomPanel._MousePosition.Y);

            panZoomPanel._Zooming = false;
        }

        private static object Zoom_Coerce (DependencyObject sender, object baseValue)
        {
            return Math.Min(Math.Max((double)baseValue, ((PanZoomPanel)sender).MinZoom), ((PanZoomPanel)sender).MaxZoom);
        }

        private static void OffsetX_PropertyChanged (DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PanZoomPanel panZoomPanel = sender as PanZoomPanel;

            double duration = panZoomPanel._Zooming ? panZoomPanel.ZoomAnimationDuration : panZoomPanel.PanAnimationDuration;
            double acceleration = panZoomPanel._Zooming ? panZoomPanel.ZoomAcceleration : panZoomPanel.PanAcceleration;
            double deceleration = panZoomPanel._Zooming ? panZoomPanel.ZoomDeceleration : panZoomPanel.PanDeceleration;

            AnimationHelper.StartAnimation(panZoomPanel._OffsetTransform, TranslateTransform.XProperty, (double)e.NewValue, duration, acceleration, deceleration);
        }

        private static object OffsetX_Coerce (DependencyObject sender, object baseValue)
        {
            PanZoomPanel panZoomPanel = (PanZoomPanel)sender;

            double value = (double)baseValue;
            double minOffsetX = -(panZoomPanel._Content.DesiredSize.Width * panZoomPanel.Zoom);
            value = Math.Min(Math.Max(value, minOffsetX), panZoomPanel.DesiredSize.Width);

            return value;
        }

        private static void OffsetY_PropertyChanged (DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PanZoomPanel panZoomPanel = sender as PanZoomPanel;

            double duration = panZoomPanel._Zooming ? panZoomPanel.ZoomAnimationDuration : panZoomPanel.PanAnimationDuration;
            double acceleration = panZoomPanel._Zooming ? panZoomPanel.ZoomAcceleration : panZoomPanel.PanAcceleration;
            double deceleration = panZoomPanel._Zooming ? panZoomPanel.ZoomDeceleration : panZoomPanel.PanDeceleration;

            AnimationHelper.StartAnimation(panZoomPanel._OffsetTransform, TranslateTransform.YProperty, (double)e.NewValue, duration, acceleration, deceleration);
        }

        private static object OffsetY_Coerce (DependencyObject sender, object baseValue)
        {
            PanZoomPanel panZoomPanel = (PanZoomPanel)sender;

            double value = (double)baseValue;
            double minOffsetY = -(panZoomPanel._Content.DesiredSize.Height * panZoomPanel.Zoom);
            value = Math.Min(Math.Max(value, minOffsetY), panZoomPanel.DesiredSize.Height);

            return value;
        }

        private static void MouseEventsEnabled_PropertyChanged (DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PanZoomPanel panZoomPanel = sender as PanZoomPanel;
            panZoomPanel.IsManipulationEnabled = true;
            bool isEnabled = (bool)e.NewValue;

            if (panZoomPanel != null)
            {
                if (isEnabled)
                {
                    panZoomPanel.MouseWheel += new MouseWheelEventHandler(PanZoomPanel_MouseWheel);
                    panZoomPanel.MouseLeftButtonDown += new MouseButtonEventHandler(PanZoomPanel_MouseLeftButtonDown);
                    panZoomPanel.MouseLeftButtonUp += new MouseButtonEventHandler(PanZoomPanel_MouseLeftButtonUp);
                    panZoomPanel.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(panZoomPanel_ManipulationDelta);
                    panZoomPanel.MouseMove += new MouseEventHandler(PanZoomPanel_MouseMove);
                }
                else
                {
                    panZoomPanel.MouseWheel -= new MouseWheelEventHandler(PanZoomPanel_MouseWheel);
                    panZoomPanel.MouseLeftButtonDown -= new MouseButtonEventHandler(PanZoomPanel_MouseLeftButtonDown);
                    panZoomPanel.MouseLeftButtonUp -= new MouseButtonEventHandler(PanZoomPanel_MouseLeftButtonUp);
                    panZoomPanel.ManipulationDelta -= new EventHandler<ManipulationDeltaEventArgs>(panZoomPanel_ManipulationDelta);
                    panZoomPanel.MouseMove -= new MouseEventHandler(PanZoomPanel_MouseMove);
                }
            }
        }

        static void panZoomPanel_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            PanZoomPanel panZoomPanel = sender as PanZoomPanel;

            if (panZoomPanel != null)
            {
                var scale = e.DeltaManipulation.Scale;

                if (scale.X != 1 && scale.Y != 1)
                {
                    var zoomfactor = 1 - (scale.Length / Math.Sqrt(2));
                    panZoomPanel.Zoom -= zoomfactor;
                }
                else
                {
                    panZoomPanel.OffsetX += e.DeltaManipulation.Translation.X;
                    panZoomPanel.OffsetY += e.DeltaManipulation.Translation.Y;
                }
            }
        }

        private static void Overview_PropertyChanged (DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PanZoomPanel panZoomPanel = sender as PanZoomPanel;
            Overview overview = e.NewValue as Overview;

            if (panZoomPanel != null && overview != null)
            {
                overview.DataContext = panZoomPanel;
            }
        }

        private static void PanZoomPanel_MouseWheel (object sender, MouseWheelEventArgs e)
        {
            PanZoomPanel panZoomPanel = sender as PanZoomPanel;

            if (panZoomPanel != null)
            {
                if (e.Delta > 0)
                {
                    panZoomPanel.Zoom *= panZoomPanel.ZoomFactor;
                }
                else if (e.Delta < 0)
                {
                    panZoomPanel.Zoom *= 1 / panZoomPanel.ZoomFactor;
                }
            }
        }

        private static void PanZoomPanel_MouseLeftButtonDown (object sender, MouseEventArgs e)
        {
            PanZoomPanel panZoomPanel = sender as PanZoomPanel;

            if (panZoomPanel != null)
            {
                Mouse.Capture(panZoomPanel);
                panZoomPanel._Panning = true;
            }
        }

        private static void PanZoomPanel_MouseLeftButtonUp (object sender, MouseEventArgs e)
        {
            PanZoomPanel panZoomPanel = sender as PanZoomPanel;

            if (panZoomPanel != null)
            {
                Mouse.Capture(null);
                panZoomPanel._Panning = false;
            }
        }

        private static void PanZoomPanel_MouseMove (object sender, MouseEventArgs e)
        {
            PanZoomPanel panZoomPanel = sender as PanZoomPanel;

            if (panZoomPanel != null)
            {
                if (panZoomPanel._Panning == true)
                {
                    Point currentPoint = e.GetPosition(panZoomPanel);

                    panZoomPanel.OffsetX += currentPoint.X - panZoomPanel._MousePosition.X;
                    panZoomPanel.OffsetY += currentPoint.Y - panZoomPanel._MousePosition.Y;

                    panZoomPanel._MousePosition = currentPoint;
                }
                else
                {
                    panZoomPanel._MousePosition = e.GetPosition(panZoomPanel);
                }
            }
        }

        #endregion Private Methods


        #region Protected Methods

        protected override Size MeasureOverride (Size constraint)
        {
            return constraint;
        }

        protected override Size ArrangeOverride (Size arrangeBounds)
        {
            Size ret = base.ArrangeOverride(this.DesiredSize);
            ScaleToFit();
            return ret;
        }

        protected override void OnContentChanged (object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            if (Overview != null)
            {
                Overview.DataContext = null;
                Overview.DataContext = this;
            }
        }

        #endregion Protected Methods


        #region Public Methods

        public override void OnApplyTemplate ()
        {
            base.OnApplyTemplate();

            _Content = this.Template.FindName("PART_Content", this) as FrameworkElement;
            if (_Content != null)
            {
                this._ZoomTransform = new ScaleTransform(Zoom, Zoom);
                this._OffsetTransform = new TranslateTransform();

                this._ZoomTransform.Changed += new EventHandler(ZoomTransform_Changed);
                this._OffsetTransform.Changed += new EventHandler(OffsetTransform_Changed);

                TransformGroup transformGroup = new TransformGroup();
                transformGroup.Children.Add(this._ZoomTransform);
                transformGroup.Children.Add(this._OffsetTransform);
                _Content.RenderTransform = transformGroup;
            }

            AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
            if (layer != null && _Adorner == null)
            {
                _Adorner = new AdornerContentPresenter(this, Overview);
                layer.Add(_Adorner);
            }
        }

        public void ScaleToFit ()
        {
            Zoom = Math.Min(this.DesiredSize.Width / _Content.DesiredSize.Width, this.DesiredSize.Height / _Content.DesiredSize.Height);
            OffsetX = (this.DesiredSize.Width / 2) - ((_Content.DesiredSize.Width / 2) * Zoom);
            OffsetY = (this.DesiredSize.Height / 2) - ((_Content.DesiredSize.Height / 2) * Zoom);
        }

        public void Scale1to1 ()
        {
            Zoom = 1.0;
            OffsetX = (this.DesiredSize.Width / 2) - ((_Content.DesiredSize.Width / 2) * Zoom);
            OffsetY = (this.DesiredSize.Height / 2) - ((_Content.DesiredSize.Height / 2) * Zoom);
        }

        #endregion Public Methods
    }
}
