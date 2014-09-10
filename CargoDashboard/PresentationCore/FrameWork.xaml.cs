using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using L3.Cargo.Common.Dashboard;
using L3.Cargo.Common.Dashboard.Display;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Controls;

namespace L3.Cargo.Dashboard.PresentationCore
{
    /// <summary>
    /// Interaction logic for FrameWork.xaml
    /// </summary>
    internal partial class FrameWork : Window
    {
        #region Private Members

        private AdornerContentPresenter _StatusAdorner;

        private AdornerContentPresenter _AboutAdorner;

        private StatusInfoDisplay _StatusInfoDisplay;

        private AboutBox _AboutBox;

        private AdornerLayerManager _AdornerLayerManager;

        private List<WidgetPage> _WidgetPages;

        private EventLoggerAccess _Logger;

        #endregion Private Members


        #region Constructors

        public FrameWork(EventLoggerAccess logger)
        {
            try
            {

                InitializeComponent();
                CultureResources.registerDataProvider(this);

                _Logger = logger;
                _StatusInfoDisplay = new StatusInfoDisplay(logger);
                _StatusAdorner = new AdornerContentPresenter(DisplayArea, _StatusInfoDisplay);
                _AboutBox = new AboutBox();
                _AboutAdorner = new AdornerContentPresenter(DisplayArea, _AboutBox);
                _AdornerLayerManager = new AdornerLayerManager(AdornerLayer.GetAdornerLayer(DisplayArea));
                _WidgetPages = new List<WidgetPage>();
            }
            catch
            {
            }
        }

        #endregion Constructors


        #region Private Methods

        private void FrameWork_FlickCompleted (object sender, ManipulationCompletedEventArgs e)
        {
            if (e.TotalManipulation.Translation.X < -50 && WidgetTabControl.SelectedIndex < (WidgetTabControl.Items.Count - 1))
            {
                WidgetTabControl.SelectedIndex++;
            }
            else if (e.TotalManipulation.Translation.X > 50 && WidgetTabControl.SelectedIndex > 0)
            {
                WidgetTabControl.SelectedIndex--;
            }
        }

        private void FrameWork_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _AdornerLayerManager.Add(_StatusInfoDisplay.GetType().Name, _StatusAdorner);
                _AdornerLayerManager.Show(_StatusAdorner);
                _AdornerLayerManager.Show(_AboutAdorner);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
                throw ex;
            }
        }

        private void FrameWork_Closing(object sender, EventArgs e)
        {
            try
            {
                _AdornerLayerManager.Clear();
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
                throw ex;
            }
        }

        private void StatusTray_MouseOrTouchDown (object sender, RoutedEventArgs e)
        {
            try
            {
                _StatusInfoDisplay.ExpandNotifications();
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        private void AboutButton_MouseOrTouchDown (object sender, RoutedEventArgs e)
        {
            try
            {
                _AboutBox.ShowBox();
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        private void previewTabItem_TouchDown (object sender, TouchEventArgs e)
        {
            WidgetTabControl.SelectedItem = sender as PreviewTabItem;
        }

        private WidgetPage FindPage(int number)
        {
            WidgetPage ret = null;

            try
            {
                foreach (WidgetPage widgetPage in _WidgetPages)
                {
                    if (widgetPage.Number.Equals(number))
                    {
                        ret = widgetPage;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }

            return ret;
        }

        private WidgetPage FindWidgetLocation(string name)
        {
            WidgetPage ret = null;

            try
            {
                foreach (WidgetPage widgetPage in _WidgetPages)
                {
                    if (widgetPage.Widgets.ContainsKey(name))
                    {
                        ret = widgetPage;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }

            return ret;
        }

        private WidgetPage FindWidgetLocation(Widget widget)
        {
            WidgetPage ret = null;

            try
            {
                foreach (WidgetPage widgetPage in _WidgetPages)
                {
                    if (widgetPage.Widgets.ContainsValue(widget))
                    {
                        foreach (string name in widgetPage.Widgets.Keys)
                        {
                            if (widget.Name.Equals(name))
                            {
                                ret = widgetPage;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }

            return ret;
        }

        #endregion Private Methods


        #region Public Methods

        public void Add(string name, Status status)
        {
            try
            {
                StatusTray.Children.Add(status.Indicator);
                if (status.TroubleShooting != null)
                {
                    _AdornerLayerManager.Add(name, status.TroubleShooting);
                    _AdornerLayerManager.Show(status.TroubleShooting);
                }
                _StatusInfoDisplay.Add(status);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        public void Add(Widget widget)
        {
            try
            {
                WidgetPage widgetPage = FindWidgetLocation(widget.Name);

                if (widgetPage == null)
                {
                    widgetPage = FindPage(widget.Page);
                }

                if (widgetPage != null)
                {
                    if (widgetPage.Widgets.ContainsKey(widget.Name))
                    {
                        widget.Column = widgetPage.Widgets[widget.Name].Column;
                        widget.ColumnSpan = widgetPage.Widgets[widget.Name].ColumnSpan;
                        widget.Row = widgetPage.Widgets[widget.Name].Row;
                        widget.RowSpan = widgetPage.Widgets[widget.Name].RowSpan;

                        widgetPage.Grid.Children.Remove(widgetPage.Widgets[widget.Name].Display);
                        widgetPage.Widgets[widget.Name] = widget;
                        widgetPage.Grid.Children.Add(widget.Display);
                    }
                    else
                    {
                        widgetPage.Widgets.Add(widget.Name, widget);
                        widgetPage.Grid.Children.Add(widget.Display);
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        public void Add(CompleteInfo completeInfo)
        {
            try
            {
                AdornerContentPresenter adorner = new AdornerContentPresenter(DisplayArea, completeInfo.Display);
                _AdornerLayerManager.Add(completeInfo.Name, adorner);
                _AdornerLayerManager.Show(completeInfo.Name);
                CompleteInfoLinks.Children.Add(completeInfo.Link);
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
                StatusTray.Children.Remove(status.Indicator);
                if (status.TroubleShooting != null)
                {
                    _AdornerLayerManager.Remove(status.TroubleShooting);
                }
                _StatusInfoDisplay.Remove(status);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        public void Remove(Widget widget)
        {
            try
            {
                WidgetPage widgetPage = FindWidgetLocation(widget);

                if (widgetPage != null)
                {
                    widgetPage.Grid.Children.Remove(widget.Display);
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        public void Remove(CompleteInfo completeInfo)
        {
            try
            {
                _AdornerLayerManager.Remove(completeInfo.Name);
                CompleteInfoLinks.Children.Remove(completeInfo.Link);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        public void AddPage(int number, int rows, int columns)
        {
            try
            {
                PreviewTabItem previewTabItem = new PreviewTabItem();
                previewTabItem.PreviewWidth = 30;
                previewTabItem.PreviewHeight = 30;
                previewTabItem.TouchDown += new EventHandler<TouchEventArgs>(previewTabItem_TouchDown);

                Grid grid = new Grid();
                WidgetPage widgetPage = new WidgetPage();

                previewTabItem.Content = grid;
                WidgetTabControl.Items.Add(previewTabItem);

                widgetPage.Number = number;
                widgetPage.Grid = grid;
                _WidgetPages.Add(widgetPage);

                for (int row = 0; row < rows; row++)
                {
                    grid.RowDefinitions.Add(new RowDefinition());
                }

                for (int column = 0; column < columns; column++)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
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
