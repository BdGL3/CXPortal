using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Windows.Threading;
using L3.Cargo.Common.Configurations;
using L3.Cargo.Common.Dashboard.Display;
using L3.Cargo.Communications.EventsLogger.Client;

namespace L3.Cargo.Dashboard.PresentationCore
{
    public class UIManager
    {
        #region Private Members

        private FrameWork _FrameWork;

        private List<AssemblyDisplays> _AssemblyDisplays;

        private object _DisplayLock;

        private EventLoggerAccess _Logger;

        private delegate void RemoveWidgetDelegate (Widget w);

        private delegate void RemoveStatusDelegate (Status s);

        private delegate void RemoveCompleteInfoDelegate (CompleteInfo c);

        #endregion Private Members


        #region Public Members

        public Dispatcher Dispatcher
        {
            get
            {
                return _FrameWork.Dispatcher;
            }
        }

        #endregion Public Members


        #region Constructors

        public UIManager(EventLoggerAccess logger)
        {
            _Logger = logger;
            _FrameWork = new FrameWork(logger);
            _AssemblyDisplays = new List<AssemblyDisplays>();
            _DisplayLock = new object();
        }

        #endregion Constructors


        #region Private Methods

        private void InitializeDisplay()
        {
            try
            {
                BackgroundWorker backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += new DoWorkEventHandler(SetupDisplay);
                backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SetupDisplayCompleted);
                backgroundWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        private void SetupDisplay(object sender, DoWorkEventArgs e)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                WidgetAreaSection widgetDisplayAreaSection = config.GetSection(ConfigurationManager.AppSettings["SystemOperationMode"].ToLower() + "WidgetArea") as WidgetAreaSection;

                if (widgetDisplayAreaSection != null)
                {
                    foreach (WidgetPageElement wpe in widgetDisplayAreaSection.WidgetPage)
                    {
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                            {
                                _FrameWork.AddPage(wpe.Page, wpe.Rows, wpe.Columns);
                            }));

                        foreach (WidgetDisplayElement wde in wpe.WidgetDisplay)
                        {
                            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                                {
                                    Widget widget = new Widget(wde.Name, new DefaultWidget(), wde.Row, wde.Column, wde.RowSpan, wde.ColumnSpan, wpe.Page);
                                    _FrameWork.Add(widget);
                                }));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        private void SetupDisplayCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    _FrameWork.Show();
                }));
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        private AssemblyDisplays FindDisplay(string name)
        {
            AssemblyDisplays ret = null;

            try
            {

                lock (_DisplayLock)
                {
                    foreach (AssemblyDisplays display in _AssemblyDisplays)
                    {
                        if (display.Name.Equals(name))
                        {
                            ret = display;
                            break;
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

        private void AddingAssemblyDisplay(object sender, DoWorkEventArgs e)
        {
            try
            {
                AssemblyDisplays display = e.Argument as AssemblyDisplays;

                if (display != null)
                {
                    lock (_DisplayLock)
                    {
                        _AssemblyDisplays.Add(display);
                    }

                    foreach (CompleteInfo completeInfo in display.CompleteInfos)
                    {
                        Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate()
                            {
                                _FrameWork.Add(completeInfo);
                            }));
                    }

                    foreach (Status status in display.Statuses)
                    {
                        Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate()
                            {
                                _FrameWork.Add(display.Name, status);
                            }));
                    }

                    foreach (Widget widget in display.Widgets)
                    {
                        Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate()
                            {
                                _FrameWork.Add(widget);
                            }));
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        private void RemoveWidget(Widget widget)
        {
            try
            {
				_FrameWork.Remove(widget);
				Widget defaultWidget = new Widget(widget.Name, new DefaultWidget(), widget.Row, widget.Column, widget.RowSpan, widget.ColumnSpan, widget.Page);
				_FrameWork.Add(defaultWidget);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        private void RemoveCompleteInfo(CompleteInfo completeInfo)
        {
            try
            {
                _FrameWork.Remove(completeInfo);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        private void RemoveStatus(Status status)
        {
            try
            {
                _FrameWork.Remove(status);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        private void RemovingAssemblyDisplay(object sender, DoWorkEventArgs e)
        {
            try
            {
                string name = e.Argument as string;

                if (!String.IsNullOrWhiteSpace(name))
                {
                    AssemblyDisplays display = null;

                    lock (_DisplayLock)
                    {
                        display = FindDisplay(name);
                    }

                    if (display != null)
                    {
                        foreach (CompleteInfo completeInfo in display.CompleteInfos)
                        {
                            Dispatcher.BeginInvoke(DispatcherPriority.Background, new RemoveCompleteInfoDelegate(RemoveCompleteInfo), completeInfo);
                        }

                        foreach (Status status in display.Statuses)
                        {
                            Dispatcher.BeginInvoke(DispatcherPriority.Background, new RemoveStatusDelegate(RemoveStatus), status);
                        }

                        foreach (Widget widget in display.Widgets)
                        {
                            Dispatcher.BeginInvoke(DispatcherPriority.Background, new RemoveWidgetDelegate(RemoveWidget), widget);
                        }

                        lock (_DisplayLock)
                        {
	                        _AssemblyDisplays.Remove(display);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        #endregion Private Methods


        #region Public Methods

        public void Add(AssemblyDisplays displays)
        {
            try
            {
                BackgroundWorker backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += new DoWorkEventHandler(AddingAssemblyDisplay);

                backgroundWorker.RunWorkerAsync(displays);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        public void Remove(string name)
        {
            try
            {
                BackgroundWorker backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += new DoWorkEventHandler(RemovingAssemblyDisplay);

                backgroundWorker.RunWorkerAsync(name);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        public void Show()
        {
            try
            {
                InitializeDisplay();
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
                throw ex;
            }
        }

        #endregion Public Methods
    }
}
