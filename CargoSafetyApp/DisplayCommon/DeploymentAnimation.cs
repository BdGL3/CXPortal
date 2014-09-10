using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace L3.Cargo.Safety.Display.Common
{
    internal class DeploymentAnimation
    {
        #region Private Members

        private bool _IsDeploying;

        private double _AnimationTime;

        private double _AnimationTimePerValue;

        private double _DeployedValue;

        private double _StowedValue;

        private Animatable _AnimationedObject;

        private DoubleAnimation _StopAnimation;

        private Dispatcher _Dispatcher;

        #endregion Private Members


        #region Public Members

        public double AnimationTime
        {
            set
            {
                _AnimationTime = value;
                _AnimationTimePerValue = value / Math.Abs(_DeployedValue - _StowedValue);
            }
        }

        public bool IsDeploying
        {
            get
            {
                return _IsDeploying;
            }
            set
            {
                _IsDeploying = value;
            }
        }

        public double DeployedValue
        {
            get
            {
                return _DeployedValue;
            }
            set
            {
                _DeployedValue = value;
                AnimationTime = _AnimationTime;
            }
        }

        public double StowedValue
        {
            get
            {
                return _StowedValue;
            }
            set
            {
                _StowedValue = value;
                AnimationTime = _AnimationTime;
            }
        }

        #endregion Public Members


        #region Constructors

        public DeploymentAnimation(Dispatcher dispatcher, Animatable animationedObject, bool isDeploying, double animationTime, double stowedValue, double deployedValue)
        {
            _Dispatcher = dispatcher;
            _AnimationedObject = animationedObject;
            _IsDeploying = isDeploying;
            _StowedValue = stowedValue;
            _DeployedValue = deployedValue;
            AnimationTime = animationTime;
            _StopAnimation = new DoubleAnimation();
            _StopAnimation.BeginTime = null;
        }

        #endregion Constructors


        #region Public Methods

        public void BeginAnimation(DependencyProperty dependencyProperty)
        {
            if ((_IsDeploying) && (this.GetPropertyValue(dependencyProperty) == _DeployedValue))
            {
                this.SetToStowedValue(dependencyProperty);
            }
            else if ((!_IsDeploying) && (this.GetPropertyValue(dependencyProperty) == _StowedValue))
            {
                this.SetToDeployedValue(dependencyProperty);
            }

            _Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    double toValue = _IsDeploying ? _DeployedValue : _StowedValue;
                    double currentValue = (double)_AnimationedObject.GetValue(dependencyProperty);
                    double timeRemaining = (_AnimationTimePerValue * Math.Abs(toValue - currentValue));
                    Duration duration = new Duration(TimeSpan.FromMilliseconds(timeRemaining));
                    DoubleAnimation animation = new DoubleAnimation(currentValue, toValue, duration);
                    _AnimationedObject.BeginAnimation(dependencyProperty, animation);
                }));
        }

        public void StopAnimation(DependencyProperty dependencyProperty)
        {
            _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    _AnimationedObject.BeginAnimation(dependencyProperty, _StopAnimation);
                }));
        }

        public void SetPropertyValue (DependencyProperty dependencyProperty, double value)
        {
            StopAnimation(dependencyProperty);

            _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                _AnimationedObject.SetCurrentValue(dependencyProperty, value);
            }));
        }

        public void SetToDeployedValue (DependencyProperty dependencyProperty)
        {
            StopAnimation(dependencyProperty);

            _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                _AnimationedObject.SetCurrentValue(dependencyProperty, _DeployedValue);
            }));
        }

        public void SetToStowedValue (DependencyProperty dependencyProperty)
        {
            StopAnimation(dependencyProperty);

            _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                _AnimationedObject.SetCurrentValue(dependencyProperty, _StowedValue);
            }));
        }

        public double GetPropertyValue (DependencyProperty dependencyProperty)
        {
            double value = 0.0;

            _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                value = (double)_AnimationedObject.GetValue(dependencyProperty);
            }));

            return value;
        }

        #endregion Public Methods
    }
}
