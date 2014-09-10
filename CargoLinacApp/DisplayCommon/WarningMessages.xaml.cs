using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using L3.Cargo.Common.Dashboard;
using L3.Cargo.Common.Dashboard.Display.Interfaces;
using L3.Cargo.Communications.Dashboard.Display.Host;

namespace L3.Cargo.Linac.Display.Common
{
    /// <summary>
    /// Interaction logic for WarningMessages.xaml
    /// </summary>
    public partial class WarningMessages : UserControl, IDisposable, IStatusMessage
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

        public WarningMessages (StatusHost statusHost, Dispatcher dispatcher)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            _Dispatcher = dispatcher;

            _StatusHost = statusHost;
            _StatusHost.WarningMessageUpdate += new UpdateWarningMessageHandler(UpdateWarningMessage);
        }

        #endregion Constructors


        #region Private Methods

        private void UpdateWarningMessage(string[] messages)
        {
            lock (lockObj)
            {
				if (messages.Length > 0)
				{
					_Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
					{
						WarningText.Items.Clear();
						this.Visibility = Visibility.Visible;
					}));

					for (int index = 0; index < messages.Length; index++)
					{
						AddWarningMessage((index + 1), messages[index]);
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

        private void AddWarningMessage (int errorNum, string resourceName)
        {
            _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                NotificationHeaderControl warningItem = new NotificationHeaderControl(errorNum);
                var binding = new Binding("WARNING_HASH");
                binding.Source = L3.Cargo.Common.Dashboard.CultureResources.getDataProvider();
                BindingOperations.SetBinding(warningItem, NotificationHeaderControl.HeaderProperty, binding);

                string val = L3.Cargo.Linac.Display.Common.Resources.ResourceManager.GetString(resourceName);
                if (!String.IsNullOrWhiteSpace(val))
                {
                    binding = new Binding(resourceName);
                    binding.Source = CultureResources.getDataProvider();
                    BindingOperations.SetBinding(warningItem, HeaderedContentControl.ContentProperty, binding);
                }
                else
                {
                    warningItem.Content = L3.Cargo.Linac.Display.Common.Resources.UNKNOWN_RESOURCE + ": " + resourceName + " (" + errorNum.ToString() + ")";
                }
                WarningText.Items.Add(warningItem);
            }));
        }

        #endregion Private Methods


        #region Public Methods

        public void Dispose()
        {
            _StatusHost.WarningMessageUpdate -= new UpdateWarningMessageHandler(UpdateWarningMessage);
        }

        #endregion Public Methods
    }
}
