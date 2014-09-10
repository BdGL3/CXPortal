using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Animation;
using System.Windows;
using System.Diagnostics;

namespace L3.Cargo.Controls
{
    public class GridLengthAnimation : AnimationTimeline
    {
        #region Dependency Property Definitions

        public static readonly DependencyProperty FromProperty;

        public static readonly DependencyProperty ToProperty;

        #endregion Dependency Property Definitions


        #region Public Members

        public GridLength From
        {
            get
            {
                return (GridLength)GetValue(GridLengthAnimation.FromProperty);
            }
            set
            {
                SetValue(GridLengthAnimation.FromProperty, value);
            }
        }

        public GridLength To
        {
            get
            {
                return (GridLength)GetValue(GridLengthAnimation.ToProperty);
            }
            set
            {
                SetValue(GridLengthAnimation.ToProperty, value);
            }
        }

        #endregion Public Members


        #region Constructors

        static GridLengthAnimation()
        {
            FromProperty = DependencyProperty.Register("From", typeof(GridLength),
                typeof(GridLengthAnimation));

            ToProperty = DependencyProperty.Register("To", typeof(GridLength), 
                typeof(GridLengthAnimation));
        }

        #endregion Constructors


        #region Protected Methods
        
        protected override Freezable CreateInstanceCore()
        {
            return new GridLengthAnimation();
        }

        #endregion Protected Methods


        #region Public Methods

        public override Type TargetPropertyType
        {
            get
            {
                return typeof(GridLength);
            }
        }

        public override object GetCurrentValue(object defaultOriginValue, 
            object defaultDestinationValue, AnimationClock animationClock)
        {
            double fromVal = ((GridLength)GetValue(GridLengthAnimation.FromProperty)).Value;
            double toVal = ((GridLength)GetValue(GridLengthAnimation.ToProperty)).Value;

            if (fromVal > toVal)
            {
                return new GridLength((1.0 - animationClock.CurrentProgress.Value) * (fromVal - toVal) + toVal, GridUnitType.Pixel);
            }
            else
                return new GridLength(animationClock.CurrentProgress.Value * (toVal - fromVal) + fromVal, GridUnitType.Pixel);
        }

        #endregion Public Methods
    }
}
