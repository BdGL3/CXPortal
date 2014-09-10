using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace L3.Cargo.Controls
{
    public static class AnimationHelper
    {
        public static void StartAnimation (UIElement animatableElement, DependencyProperty dependencyProperty, double toValue, double durationMilliseconds, double accelerationRatio, double decelerationRatio)
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.To = toValue;
            animation.AccelerationRatio = accelerationRatio;
            animation.DecelerationRatio = decelerationRatio;
            animation.FillBehavior = FillBehavior.HoldEnd;
            animation.Duration = TimeSpan.FromMilliseconds(durationMilliseconds);
            animation.Freeze();

            animatableElement.BeginAnimation(dependencyProperty, animation, HandoffBehavior.Compose);
        }

        public static void StartAnimation (UIElement animatableElement, DependencyProperty dependencyProperty, double fromValue, double toValue, double durationMilliseconds, double accelerationRatio, double decelerationRatio)
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = fromValue;
            animation.To = toValue;
            animation.AccelerationRatio = accelerationRatio;
            animation.DecelerationRatio = decelerationRatio;
            animation.FillBehavior = FillBehavior.HoldEnd;
            animation.Duration = TimeSpan.FromMilliseconds(durationMilliseconds);
            animation.Freeze();

            animatableElement.BeginAnimation(dependencyProperty, animation, HandoffBehavior.Compose);
        }

        public static void StartAnimation (Transform animatableElement, DependencyProperty dependencyProperty, double toValue, double durationMilliseconds, double accelerationRatio, double decelerationRatio)
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.To = toValue;
            animation.AccelerationRatio = accelerationRatio;
            animation.DecelerationRatio = decelerationRatio;
            animation.FillBehavior = FillBehavior.HoldEnd;
            animation.Duration = TimeSpan.FromMilliseconds(durationMilliseconds);
            animation.Freeze();

            animatableElement.BeginAnimation(dependencyProperty, animation, HandoffBehavior.Compose);
        }

        public static void StartAnimation (Transform animatableElement, DependencyProperty dependencyProperty, double toValue, double durationMilliseconds, double accelerationRatio, double decelerationRatio, EventHandler callback)
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.To = toValue;
            animation.AccelerationRatio = accelerationRatio;
            animation.DecelerationRatio = decelerationRatio;
            animation.FillBehavior = FillBehavior.HoldEnd;
            animation.Duration = TimeSpan.FromMilliseconds(durationMilliseconds);
            animation.Changed += new EventHandler(callback);
            animation.Completed += new EventHandler(callback);
            animation.Freeze();

            animatableElement.BeginAnimation(dependencyProperty, animation, HandoffBehavior.Compose);
        }

        public static void CancelAnimation(UIElement animatableElement, DependencyProperty dependencyProperty)
        {
            animatableElement.BeginAnimation(dependencyProperty, null);
        }
    }
}
