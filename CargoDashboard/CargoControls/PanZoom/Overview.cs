using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Effects;

namespace L3.Cargo.Controls
{
    public class Overview : Control
    {
        #region Dependency Property Definitions

        public static readonly DependencyProperty ScreenPositionProperty =
                DependencyProperty.Register("ScreenPosition", typeof(ScreenPosition), typeof(Overview),
                                            new FrameworkPropertyMetadata(ScreenPosition.TopLeft, ScreenPosition_PropertyChanged));

        public new static readonly DependencyProperty EffectProperty =
                DependencyProperty.Register("Effect", typeof(Effect), typeof(Overview),
                                            new FrameworkPropertyMetadata(null, Effect_PropertyChanged));

        #endregion Dependency Property Definitions


        #region Private Members

        private Thumb _Thumb;

        private Canvas _Canvas;

        private Grid _Expand;

        private Grid _Collapse;

        #endregion Private Members


        #region Public Members

        public ScreenPosition ScreenPosition
        {
            get
            {
                return (ScreenPosition)GetValue(ScreenPositionProperty);
            }
            set
            {
                SetValue(ScreenPositionProperty, value);
            }
        }

        public new Effect Effect
        {
            get
            {
                return (Effect)GetValue(EffectProperty);
            }
            set
            {
                SetValue(EffectProperty, value);
            }
        }

        #endregion Public Members


        #region Constructors

        static Overview ()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Overview), new FrameworkPropertyMetadata(typeof(Overview)));
        }

        #endregion Constructors


        #region Private Methods

        private static void ScreenPosition_PropertyChanged (DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Overview overview = sender as Overview;
            ScreenPosition position = (ScreenPosition)e.NewValue;

            if (overview != null)
            {
                switch (position)
                {
                    case ScreenPosition.TopLeft:
                        overview.VerticalContentAlignment = VerticalAlignment.Top;
                        overview.HorizontalContentAlignment = HorizontalAlignment.Left;
                        break;
                    case ScreenPosition.TopRight:
                        overview.VerticalContentAlignment = VerticalAlignment.Top;
                        overview.HorizontalContentAlignment = HorizontalAlignment.Right;
                        break;
                    case ScreenPosition.BottomLeft:
                        overview.VerticalContentAlignment = VerticalAlignment.Bottom;
                        overview.HorizontalContentAlignment = HorizontalAlignment.Left;
                        break;
                    case ScreenPosition.BottomRight:
                        overview.VerticalContentAlignment = VerticalAlignment.Bottom;
                        overview.HorizontalContentAlignment = HorizontalAlignment.Right;
                        break;
                    default:
                        break;
                }
            }
        }

        private static void Thumb_DragDelta (object sender, DragDeltaEventArgs e)
        {
            Thumb t = sender as Thumb;

            double offsetX = Canvas.GetLeft(t) + e.HorizontalChange;
            double offsetY = Canvas.GetTop(t) + e.VerticalChange;

            Canvas.SetLeft(t, offsetX);
            Canvas.SetTop(t, offsetY);
        }

        private static void Effect_PropertyChanged (DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Overview overview = sender as Overview;
            Effect effect = e.NewValue as Effect;

            if (overview != null && effect != null && overview._Canvas != null)
            {
                overview._Canvas.Effect = effect;
            }
        }

        private void Expand_MouseUp (object sender, MouseButtonEventArgs e)
        {
            _Expand.Visibility = Visibility.Collapsed;
            _Collapse.Visibility = Visibility.Visible;
            _Canvas.Visibility = Visibility.Visible;
        }

        private void Collapse_MouseUp (object sender, MouseButtonEventArgs e)
        {
            _Expand.Visibility = Visibility.Visible;
            _Collapse.Visibility = Visibility.Collapsed;
            _Canvas.Visibility = Visibility.Collapsed;
        }

        #endregion Private Methods


        #region Protected Methods

        protected override Size MeasureOverride (Size constraint)
        {
            return constraint;
        }

        protected override Size ArrangeOverride (Size arrangeBounds)
        {
            return base.ArrangeOverride(this.DesiredSize);
        }

        #endregion Protected Methods


        #region Public Methods

        public override void OnApplyTemplate ()
        {
            base.OnApplyTemplate();

            _Canvas = this.Template.FindName("PART_Canvas", this) as Canvas;
            if (_Canvas != null)
            {
                _Canvas.Effect = Effect;
            }

            _Thumb = this.Template.FindName("PART_Thumb", this) as Thumb;
            if (_Thumb != null)
            {
                _Thumb.DragDelta += new DragDeltaEventHandler(Thumb_DragDelta);
            }

            _Expand = this.Template.FindName("PART_Expand", this) as Grid;
            if (_Expand != null)
            {
                _Expand.MouseUp += new MouseButtonEventHandler(Expand_MouseUp);
            }

            _Collapse = this.Template.FindName("PART_Collapse", this) as Grid;
            if (_Collapse != null)
            {
                _Collapse.MouseUp += new MouseButtonEventHandler(Collapse_MouseUp);
            }
        }

        #endregion Public Methods
    }
}
