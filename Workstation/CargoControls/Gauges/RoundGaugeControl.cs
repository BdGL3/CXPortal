using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace L3.Cargo.Controls
{
    public class RoundGaugeControl : Control
    {
        #region Dependency Property Definitions

        public static readonly DependencyProperty CurrentValueProperty =
            DependencyProperty.Register("CurrentValue", typeof(double), typeof(RoundGaugeControl),
                                        new FrameworkPropertyMetadata(0.0, CurrentValue_PropertyChanged));

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(double), typeof(RoundGaugeControl),
                                        new FrameworkPropertyMetadata(0.0, MinValue_PropertyChanged));

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(double), typeof(RoundGaugeControl),
                                        new FrameworkPropertyMetadata(0.0, MaxValue_PropertyChanged));

        public static readonly DependencyProperty MeasurementProperty =
            DependencyProperty.Register("Measurement", typeof(string), typeof(RoundGaugeControl),
                                        new FrameworkPropertyMetadata("PSI", Measurement_PropertyChanged));

        #endregion Dependency Property Definitions


        #region Private Members

        private RotateTransform _Angle;

        private TextBlock _Measurement;

        private TextBlock _CurrentText;

        #endregion Private Members


        #region Public Members

        public double CurrentValue
        {
            get
            {
                return (double)GetValue(CurrentValueProperty);
            }
            set
            {
                SetValue(CurrentValueProperty, value);
            }
        }

        public double MinValue
        {
            get
            {
                return (double)GetValue(MinValueProperty);
            }
            set
            {
                SetValue(MinValueProperty, value);
            }
        }

        public double MaxValue
        {
            get
            {
                return (double)GetValue(MaxValueProperty);
            }
            set
            {
                SetValue(MaxValueProperty, value);
            }
        }

        public string Measurement
        {
            get
            {
                return (string)GetValue(MeasurementProperty);
            }
            set
            {
                SetValue(MeasurementProperty, value);
            }
        }

        #endregion Public Members


        #region Constructors

        static RoundGaugeControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RoundGaugeControl), new FrameworkPropertyMetadata(typeof(RoundGaugeControl)));
        }

        #endregion Constructors


        #region Private Methods

        private static void CurrentValue_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RoundGaugeControl gauge = (RoundGaugeControl)d;
            gauge.SetPinAngle((double)e.NewValue);
            gauge.SetCurrentValueText((double)e.NewValue);
        }

        private static void MinValue_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RoundGaugeControl gauge = (RoundGaugeControl)d;
            gauge.SetMarkerValues();
        }

        private static void MaxValue_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RoundGaugeControl gauge = (RoundGaugeControl)d;
            gauge.SetMarkerValues();
        }

        private static void Measurement_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RoundGaugeControl gauge = (RoundGaugeControl)d;
            gauge.SetMeasurementText(gauge.Measurement);
        }

        private void SetCurrentValueText(double value)
        {
            if (_CurrentText != null)
            {
                _CurrentText.Text = value.ToString();
            }
        }

        private void SetMeasurementText(string value)
        {
            if (_Measurement != null)
            {
                _Measurement.Text = value;
            }
        }

        private void SetPinAngle(double value)
        {
            if (this._Angle != null)
            {
                value = (value > MaxValue) ? MaxValue : value;

                double angle = (300.0 / (this.MaxValue - this.MinValue)) * value;
                this._Angle.Angle = angle;
            }
        }

        private void SetMarkerValues()
        {
            if (this.Template != null)
            {
                int increment = (int)((MaxValue - MinValue) / 10.0);

                for (int i = 0; i <= 10; i++)
                {
                    TextBlock marker = this.Template.FindName("PART_Marker" + (i + 1).ToString(), this) as TextBlock;
                    if (marker != null)
                    {
                        marker.Text = (MinValue + (i * increment)).ToString();
                    }
                }
            }
        }

        #endregion Private Methods


        #region Public Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _Angle = (RotateTransform)this.Template.FindName("PART_PinAngle", this);

            _CurrentText = (TextBlock)this.Template.FindName("PART_CurrentValue", this);
            _Measurement = (TextBlock)this.Template.FindName("PART_Measurment", this);

            SetPinAngle(CurrentValue);
            SetCurrentValueText(CurrentValue);
            SetMarkerValues();
            SetMeasurementText(Measurement);
        }

        #endregion Public Methods
    }
}
