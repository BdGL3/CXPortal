using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;
using System.Threading;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Dashboard.Common;
using L3.Cargo.Communications.Dashboard.Display.Client;
using L3.Cargo.Communications.EventsLogger.Client;
using System.ServiceModel.Channels;
using System.Net;
using System.Threading.Tasks;

namespace L3.Cargo.Subsystem.DataAccessCore
{
    public delegate void PLCTagUpdateHandler(string name, int value);

    public class DataAccessBase
    {
        #region Protected Members

        protected object _AddressLock;

        protected DashboardAccess _DashboardAccess;

        protected SubsystemDisplayAccess _SubsystemDisplayAccess;

        protected EventLoggerAccess _Logger;

        protected List<EndpointAddress> _AvailableConnections;

        protected TaskFactory _TaskFactory;

        protected Task _LastTask;

        #endregion Protected Members


        #region Public Members

        public event DashboardUpdateRequestHandler DisplayUpdateRequest;

        public event DashboardControlUpdateHandler DisplayControlUpdateRequest;

        #endregion Public Members


        #region Constructors

        public DataAccessBase(EventLoggerAccess logger)
        {
            _AddressLock = new object();
            _TaskFactory = new TaskFactory(TaskCreationOptions.PreferFairness, TaskContinuationOptions.ExecuteSynchronously);
            _LastTask = new Task(new Action(() => { }));
            _LastTask.RunSynchronously();

            _Logger = logger;
            _AvailableConnections = new List<EndpointAddress>();
            _DashboardAccess = new DashboardAccess();
            _DashboardAccess.Start();



            _SubsystemDisplayAccess = new SubsystemDisplayAccess();
            _SubsystemDisplayAccess.DisplayUpdateRequest += new DashboardUpdateRequestHandler(DisplaySendUpdate);
            _SubsystemDisplayAccess.ControlUpdateRequest += new DashboardControlUpdateHandler(DisplayControlUpdate);
        }

        #endregion Constructors


        #region Protected Methods

        protected void DisplaySendUpdate()
        {
            if (DisplayUpdateRequest != null)
            {
                DisplayUpdateRequest();
            }
        }

        protected void DisplayControlUpdate(string name, int value)
        {
            if (DisplayControlUpdateRequest != null)
            {
                DisplayControlUpdateRequest(name, value);
            }
        }

        protected void AddConnection(OperationContext context)
        {
            if (context != null)
            {
                RemoteEndpointMessageProperty remp = OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                if (remp != null)
                {

                    string host = remp.Address;
                    Uri via = OperationContext.Current.IncomingMessageProperties.Via;
                    // string host = via.Host;
                    string port = ConfigurationManager.AppSettings["DisplayPort"];
                    string alias = ConfigurationManager.AppSettings["Alias"];
                    EndpointAddress address = new EndpointAddress("net.tcp://" + host + ":" + port + "/" + alias + "Comm");

                    lock (_AddressLock)
                    {
                        if (!_AvailableConnections.Contains(address))
                        {
                            _AvailableConnections.Add(address);
                        }
                    }
                }
            }
        }

        #endregion Protected Methods


        #region Public Methods

        public void UpdateStatusErrorMessages(string[] messages)
        {
            AddConnection(OperationContext.Current);

            lock (_AddressLock)
            {
                for (int index = _AvailableConnections.Count - 1; index >= 0; index--)
                {
                    EndpointAddress address = _AvailableConnections[index];

                    Action<Task[]> action = new Action<Task[]>(delegate(Task[] t)
                    {
                        try
                        {
                            using (StatusEndpoint endpoint = new StatusEndpoint(new UpdateTcpBinding(), address))
                            {
                                endpoint.Open();
                                endpoint.UpdateErrorMessage(messages);
                            }
                        }
                        catch { }
                    });

                    Task[] lastTasks = new Task[] { _LastTask };
                    _LastTask = _TaskFactory.ContinueWhenAll(lastTasks, action);

                }
            }
        }

        public void UpdateStatusWarningMessages(string[] messages)
        {
            AddConnection(OperationContext.Current);

            lock (_AddressLock)
            {
                for (int index = _AvailableConnections.Count - 1; index >= 0; index--)
                {
                    EndpointAddress address = _AvailableConnections[index];

                    Action<Task[]> action = new Action<Task[]>(delegate(Task[] t)
                    {
                        try
                        {
                            using (StatusEndpoint endpoint = new StatusEndpoint(new UpdateTcpBinding(), address))
                            {
                                endpoint.Open();
                                endpoint.UpdateWarningMessage(messages);
                            }
                        }
                        catch { }
                    });

                    Task[] lastTasks = new Task[] { _LastTask };
                    _LastTask = _TaskFactory.ContinueWhenAll(lastTasks, action);
                }
            }
        }

        public void UpdateStatusIndicator(string color)
        {
            AddConnection(OperationContext.Current);

            lock (_AddressLock)
            {
                for (int index = _AvailableConnections.Count - 1; index >= 0; index--)
                {
                    EndpointAddress address = _AvailableConnections[index];

                    Action<Task[]> action = new Action<Task[]>(delegate(Task[] t)
                    {
                        try
                        {
                            using (StatusEndpoint endpoint = new StatusEndpoint(new UpdateTcpBinding(), address))
                            {
                                endpoint.Open();
                                endpoint.UpdateIndicator(color);
                            }
                        }
                        catch { }
                    });

                    Task[] lastTasks = new Task[] { _LastTask };
                    _LastTask = _TaskFactory.ContinueWhenAll(lastTasks, action);
                }
            }
        }

        public void UpdateWidgets(string name, int value)
        {
            AddConnection(OperationContext.Current);

            lock (_AddressLock)
            {
                for (int index = _AvailableConnections.Count - 1; index >= 0; index--)
                {
                    EndpointAddress address = _AvailableConnections[index];

                    Action<Task[]> action = new Action<Task[]>(delegate(Task[] t)
                    {
                        try
                        {
                            using (WidgetStatusEndpoint endpoint = new WidgetStatusEndpoint(new UpdateTcpBinding(), address))
                            {
                                endpoint.Open();
                                endpoint.Update(name, value);
                            }
                        }
                        catch { }
                    });

                    Task[] lastTasks = new Task[] { _LastTask };
                    _LastTask = _TaskFactory.ContinueWhenAll(lastTasks, action);
                }
            }
        }

        #endregion Public Methods
    }
}
