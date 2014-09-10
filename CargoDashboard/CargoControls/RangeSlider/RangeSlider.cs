using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace L3.Cargo.Controls
{
    /// <summary>
    /// A slider that provides the a range
    /// </summary>
    public sealed class RangeSlider : Control
    {
        #region Dependency Property Definitions

        public static readonly DependencyProperty RangeStartProperty =
                DependencyProperty.Register("RangeStart", typeof(long), typeof(RangeSlider),
                                            new FrameworkPropertyMetadata((long)0, Range_PropertyChanged));

        public static readonly DependencyProperty RangeStopProperty =
                DependencyProperty.Register("RangeStop", typeof(long), typeof(RangeSlider),
                                            new FrameworkPropertyMetadata((long)1, Range_PropertyChanged));

        public static readonly DependencyProperty RangeStartSelectedProperty =
                DependencyProperty.Register("RangeStartSelected", typeof(long), typeof(RangeSlider),
                                            new FrameworkPropertyMetadata((long)0, RangeSelected_PropertyChanged));
        

        public static readonly DependencyProperty RangeStopSelectedProperty =
                DependencyProperty.Register("RangeStopSelected", typeof(long), typeof(RangeSlider),
                                            new FrameworkPropertyMetadata((long)1, RangeSelected_PropertyChanged));

        public static readonly DependencyProperty MinRangeProperty =
                DependencyProperty.Register("MinRange", typeof(long), typeof(RangeSlider),
                                            new FrameworkPropertyMetadata((long)1, MinRange_PropertyChanged));

        public static readonly DependencyProperty RangeSelectorsColorProperty =
                DependencyProperty.Register("RangeSelectorsColor", typeof(Brush), typeof(RangeSlider),
                                            new FrameworkPropertyMetadata((Brush)Brushes.WhiteSmoke));

        public static readonly DependencyProperty RangeColorProperty =
                DependencyProperty.Register("RangeColor", typeof(Brush), typeof(RangeSlider),
                                            new FrameworkPropertyMetadata((Brush)Brushes.WhiteSmoke));

        #endregion Dependency Property Definitions


        #region Private Members

        private bool internalUpdate = false;

        private const double RepeatButtonMoveRatio = 0.1;

        private long _MovableRange;

        private double _MovableWidth;

        private Thumb _CenterThumb;

        private Thumb _LeftThumb;

        private Thumb _RightThumb;

        private Grid _LeftButton;

        private Grid _RightButton;

        #endregion Private Members


        #region Public Members

        public long RangeStart
        {
            get
            {
                return (long)GetValue(RangeStartProperty);
            }
            set
            {
                SetValue(RangeStartProperty, value);
            }
        }

        public long RangeStop
        {
            get
            {
                return (long)GetValue(RangeStopProperty);
            }
            set
            {
                SetValue(RangeStopProperty, value);
            }
        }

        public long RangeStartSelected
        {
            get
            {
                return (long)GetValue(RangeStartSelectedProperty);
            }
            set
            {
                SetValue(RangeStartSelectedProperty, value);
            }
        }

        public long RangeStopSelected
        {
            get
            {
                return (long)GetValue(RangeStopSelectedProperty);
            }
            set
            {
                SetValue(RangeStopSelectedProperty, value);
            }
        }

        public long MinRange
        {
            get
            {
                return (long)GetValue(MinRangeProperty);
            }
            set
            {
                SetValue(MinRangeProperty, value);
            }
        }

        public Brush RangeSelectorsColor
        {
            get
            {
                return (Brush)GetValue(RangeSelectorsColorProperty);
            }
            set
            {
                SetValue(RangeSelectorsColorProperty, value);
            }
        }

        public Brush RangeColor
        {
            get
            {
                return (Brush)GetValue(RangeColorProperty);
            }
            set
            {
                SetValue(RangeColorProperty, value);
            }
        }

        public static readonly RoutedEvent RangeSelectionChangedEvent =
                EventManager.RegisterRoutedEvent("RangeSelectionChanged", RoutingStrategy.Bubble,
                                                 typeof(RangeSelectionChangedEventHandler), typeof(RangeSlider));

        public event RangeSelectionChangedEventHandler RangeSelectionChanged
        {
            add
            {
                AddHandler(RangeSelectionChangedEvent, value);
            }
            remove
            {
                RemoveHandler(RangeSelectionChangedEvent, value);
            }
        }

        #endregion Public Member


        #region Constructors

        public RangeSlider()
        {
            DependencyPropertyDescriptor.FromProperty(ActualWidthProperty, typeof(RangeSlider)).
                AddValueChanged(this, delegate { ReCalculateWidths(); });

            _MovableRange = 0;
            _MovableWidth = 0;
        }

        static RangeSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RangeSlider), new FrameworkPropertyMetadata(typeof(RangeSlider)));
        }

        #endregion Constructors


        #region Private Methods

        private void OnRangeSelectionChanged (RangeSelectionChangedEventArgs e)
        {
            e.RoutedEvent = RangeSelectionChangedEvent;
            RaiseEvent(e);
        }

        private static void Range_PropertyChanged (DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            RangeSlider slider = sender as RangeSlider;
            if (slider!= null && !slider.internalUpdate)
            {
                slider.ReCalculateRanges();
                slider.ReCalculateWidths();
            }
        }

        private static void RangeSelected_PropertyChanged (DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            RangeSlider slider = sender as RangeSlider;
            if (slider != null && !slider.internalUpdate)
            {
                slider.ReCalculateWidths();
                slider.OnRangeSelectionChanged(new RangeSelectionChangedEventArgs(slider));
            }
        }

        private static void MinRange_PropertyChanged (DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((long)e.NewValue < 0)
                throw new ArgumentOutOfRangeException("value", "value for MinRange cannot be less than 0");

            RangeSlider slider = sender as RangeSlider;
            if (slider != null && !slider.internalUpdate)
            {
                slider.internalUpdate = true;//set flag to signal that the properties are being set by the object itself
                slider.RangeStopSelected = Math.Max(slider.RangeStopSelected, slider.RangeStartSelected + (long)e.NewValue);
                slider.RangeStop = Math.Max(slider.RangeStop, slider.RangeStopSelected);
                slider.internalUpdate = false;//set flag to signal that the properties are being set by the object itself

                slider.ReCalculateRanges();
                slider.ReCalculateWidths();
            }
        }

        private void RightThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            MoveThumb(_CenterThumb, _RightButton, e.HorizontalChange);
            ReCalculateRangeSelected(false, true);
        }

        private void LeftThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            MoveThumb(_LeftButton, _CenterThumb, e.HorizontalChange);
            ReCalculateRangeSelected(true, false);
        }

        private void CenterThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            MoveThumb(_LeftButton, _RightButton, e.HorizontalChange);
            ReCalculateRangeSelected(true, true);
        }

        private static void MoveThumb(FrameworkElement x, FrameworkElement y, double horizonalChange)
        {
            double change = 0;
            if (horizonalChange < 0)
                change = GetChangeKeepPositive(x.Width, horizonalChange);
            else if (horizonalChange > 0)
                change = -GetChangeKeepPositive(y.Width, -horizonalChange);

            x.Width += change;
            y.Width -= change;
        }

        private static double GetChangeKeepPositive(double width, double increment)
        {
            return Math.Max(width + increment, 0) - width;
        }

        private void ReCalculateRanges()
        {
           _MovableRange = RangeStop - RangeStart - MinRange;
        }

        private void ReCalculateWidths()
        {
            if (_LeftButton != null && _RightButton != null && _CenterThumb != null)
            {
                _MovableWidth = Math.Max(ActualWidth - _RightThumb.ActualWidth - _LeftThumb.ActualWidth - _CenterThumb.MinWidth, 1);
                _LeftButton.Width = Math.Max(_MovableWidth * (RangeStartSelected - RangeStart) / _MovableRange, 0);
                _RightButton.Width = Math.Max(_MovableWidth * (RangeStop - RangeStopSelected) / _MovableRange, 0);
                _CenterThumb.Width = Math.Max(ActualWidth - _LeftButton.Width - _RightButton.Width - _RightThumb.ActualWidth - _LeftThumb.ActualWidth, 0);
            }
        }

        private void ReCalculateRangeSelected(bool reCalculateStart, bool reCalculateStop)
        {
            internalUpdate = true;
            if (reCalculateStart)
            {
                if (_LeftButton.Width == 0.0)
                    RangeStartSelected = RangeStart;
                else
                {
                    RangeStartSelected =
                        Math.Max(RangeStart, (long)(RangeStart + _MovableRange * _LeftButton.Width / _MovableWidth));

                    if (RangeStartSelected > RangeStopSelected)
                        RangeStartSelected = RangeStopSelected;
                }
            }

            if (reCalculateStop)
            {
                if (_RightButton.Width == 0.0)
                    RangeStopSelected = RangeStop;
                else
                {
                    RangeStopSelected =
                        Math.Min(RangeStop, (long)(RangeStop - _MovableRange * _RightButton.Width / _MovableWidth));

                    if (RangeStopSelected < RangeStartSelected)
                        RangeStopSelected = RangeStartSelected;
                }
            }

            internalUpdate = false;

            if (reCalculateStart || reCalculateStop)
            {
                OnRangeSelectionChanged(new RangeSelectionChangedEventArgs(this));
            }
        }

        #endregion Private Methods


        #region Public Methods

        public void ResetSelection(bool isStart)
        {
            double widthChange = RangeStop - RangeStart;
            widthChange = isStart ? -widthChange : widthChange;

            MoveThumb(_LeftButton, _RightButton, widthChange);
            ReCalculateRangeSelected(true, true);
        }

        public void SetSelectedRange(long selectionStart, long selectionStop)
        {
            long start = Math.Max(RangeStart, selectionStart);
            long stop = Math.Min(selectionStop, RangeStop);
            start = Math.Min(start, RangeStop - MinRange);
            stop = Math.Max(RangeStart + MinRange, stop);

            if (stop >= start + MinRange)
            {
                internalUpdate = true;
                RangeStartSelected = start;
                RangeStopSelected = stop;
                ReCalculateWidths();
                internalUpdate = false;
                OnRangeSelectionChanged(new RangeSelectionChangedEventArgs(this));
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _CenterThumb = this.Template.FindName("PART_MiddleThumb", this) as Thumb;
            if (_CenterThumb != null)
            {
                _CenterThumb.DragDelta += CenterThumbDragDelta;
            }

            _LeftThumb = this.Template.FindName("PART_LeftThumb", this) as Thumb;
            if (_LeftThumb != null)
            {
                _LeftThumb.DragDelta += LeftThumbDragDelta;
            }

            _RightThumb = this.Template.FindName("PART_RightThumb", this) as Thumb;
            if (_RightThumb != null)
            {
                _RightThumb.DragDelta += RightThumbDragDelta;
            }

            _LeftButton = this.Template.FindName("PART_LeftEdge", this) as Grid;
            _RightButton = this.Template.FindName("PART_RightEdge", this) as Grid;

            ReCalculateWidths();
        }

        #endregion Public Methods
    }
}