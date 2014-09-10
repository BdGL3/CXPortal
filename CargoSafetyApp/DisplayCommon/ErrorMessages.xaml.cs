using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using L3.Cargo.Common.Dashboard;
using L3.Cargo.Common.Dashboard.Display.Interfaces;
using L3.Cargo.Communications.Dashboard.Display.Host;

namespace L3.Cargo.Safety.Display.Common
{
    /// <summary>
    /// Interaction logic for ErrorMessages.xaml
    /// </summary>
    public partial class ErrorMessages : UserControl, IDisposable, IStatusMessage
    {
        #region Private Members

        private StatusHost _StatusHost;

        private Dispatcher _Dispatcher;

        private Object lockObj = new Object();

        #endregion Private Members


        #region Public Members

        public event VisibilityChangeHandler VisibilityChanged;

        #endregion Public Members


        #region Constructors

        public ErrorMessages (StatusHost statusHost, Dispatcher dispatcher)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            _Dispatcher = dispatcher;

            _StatusHost = statusHost;
            _StatusHost.ErrorMessageUpdate += new UpdateErrorMessageHandler(UpdateErrorMessage);
        }

        #endregion Constructors


        #region Private Methods

        private void UpdateErrorMessage(string[] messages)
        {
            lock (lockObj)
            {
                if (messages.Length > 0)
                {
                    _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                    {
                        ErrorsText.Items.Clear();
                        this.Visibility = Visibility.Visible;
                    }));

                    for (int index = 0; index < messages.Length; index++)
                    {
                        AddErrorMessage((index + 1), messages[index]);

                    }

                    if (VisibilityChanged != null)
                    {
                        VisibilityChanged();
                    }
                }
                else
                {
                    _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                    {
                        this.Visibility = Visibility.Collapsed;
                    }));
                }
            }
        }

        private void AddErrorMessage (int errorNum, string resourceName)
        {
            _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                NotificationHeaderControl errorItem = new NotificationHeaderControl(errorNum);
                var binding = new Binding("ERROR_HASH");
                binding.Source = L3.Cargo.Common.Dashboard.CultureResources.getDataProvider();
                BindingOperations.SetBinding(errorItem, NotificationHeaderControl.HeaderProperty, binding);

                string val = L3.Cargo.Safety.Display.Common.Resources.ResourceManager.GetString(resourceName);
                if (!String.IsNullOrWhiteSpace(val))
                {
                    binding = new Binding(resourceName);
                    binding.Source = CultureResources.getDataProvider();
                    BindingOperations.SetBinding(errorItem, HeaderedContentControl.ContentProperty, binding);
                }
                else
                {
                    errorItem.Content = L3.Cargo.Safety.Display.Common.Resources.UNKNOWN_RESOURCE + ": " + resourceName + " (" + errorNum.ToString() + ")";
                }
                ErrorsText.Items.Add(errorItem);
            }));
        }

        #endregion Private Methods


        #region Public Methods

        public void Dispose()
        {
            _StatusHost.ErrorMessageUpdate -= new UpdateErrorMessageHandler(UpdateErrorMessage);
        }

        #endregion Public Methods
    }
}
