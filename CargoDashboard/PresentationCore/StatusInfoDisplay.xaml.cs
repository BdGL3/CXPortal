using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using L3.Cargo.Common.Dashboard.Display;
using L3.Cargo.Common.Dashboard.Display.Interfaces;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Controls;
using L3.Cargo.Common.Dashboard;
using System.Windows.Data;

namespace L3.Cargo.Dashboard.PresentationCore
{
    internal partial class StatusInfoDisplay : UserControl
    {
        #region Private Members
		
        private EventLoggerAccess _Logger;

        #endregion Private Members


        #region Constructors

        public StatusInfoDisplay(EventLoggerAccess logger)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            _Logger = logger;

            CriticalErrorsArea.SizeChanged += new SizeChangedEventHandler(CriticalErrorsArea_SizeChanged);
            WarningMessagesArea.SizeChanged += new SizeChangedEventHandler(WarningMessagesArea_SizeChanged);
        }

        #endregion Constructors


        #region Private Methods

        private void ClosingArea_MouseOrTouchDown (object sender, RoutedEventArgs e)
        {
            try
            {
                CollapseNotifications();
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        private void CriticalErrorsArea_SizeChanged (object sender, SizeChangedEventArgs e)
        {
            try
            {
                DefaultErrorDisplay.Visibility = (e.NewSize.Height == 0) ? Visibility.Visible : Visibility.Collapsed;
                ResizeNotificationArea();
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        private void WarningMessagesArea_SizeChanged (object sender, SizeChangedEventArgs e)
        {
            try
            {
                DefaultWarningDisplay.Visibility = (e.NewSize.Height == 0) ? Visibility.Visible : Visibility.Collapsed;
                ResizeNotificationArea();
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        private void ScrollViewer_ManipulationDelta (object sender, ManipulationDeltaEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + e.DeltaManipulation.Translation.Y);
        }

        private void ResizeNotificationArea ()
        {
            if (ClosingArea.Visibility == Visibility.Visible)
            {
                double newSize = 0;

                foreach (RowDefinition row in StatusMessageArea.RowDefinitions)
                {
                    newSize += row.ActualHeight;
                }

                AnimationHelper.StartAnimation(StatusMessageArea, Grid.HeightProperty, newSize, 250, 0.9, 0.1);
            }
        }

        #endregion Private Methods


        #region Public Methods

        public void ExpandNotifications()
        {
            try
            {
                AnimationHelper.StartAnimation(StatusMessageArea, Grid.HeightProperty, StatusMessageArea.ActualHeight, 250, 0.9, 0.1);
                ClosingArea.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        public void CollapseNotifications()
        {
            try
            {
                AnimationHelper.StartAnimation(StatusMessageArea, Grid.HeightProperty, 0, 250, 0.9, 0.1);
                ClosingArea.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        public void Add(Status status)
        {
            try
            {
                if (!CriticalErrorsArea.Children.Contains(status.ErrorMessages))
                {
                    CriticalErrorsArea.Children.Add(status.ErrorMessages);
                }

                if (!WarningMessagesArea.Children.Contains(status.WarningMessages))
                {
                    WarningMessagesArea.Children.Add(status.WarningMessages);
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        public void Remove(Status status)
        {
            try
            {
                if (CriticalErrorsArea.Children.Contains(status.ErrorMessages))
                {
                    CriticalErrorsArea.Children.Remove(status.ErrorMessages);
                }

                if (WarningMessagesArea.Children.Contains(status.WarningMessages))
                {
                    WarningMessagesArea.Children.Remove(status.WarningMessages);
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        #endregion Public Methods
    }
}
