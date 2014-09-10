using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Threading;
using System.Xml.Linq;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Dashboard.Client;
using L3.Cargo.Communications.Dashboard.Interfaces;
using L3.Cargo.Communications.EventsLogger.Client;

namespace L3.Cargo.Dashboard.DataAccessCore
{
    public class SubsystemServices : SynchronizedCollection<SubsystemService>
    {
        #region Private Members

        private bool _Shutdown;

        private Thread _ListenerThread;

        private EventLoggerAccess _Logger;

        private uint _SubsystemCommTimeoutMsec;

        #endregion Private Members


        #region Public Members

        public event SubsystemServiceListUpdateHandler SubsystemServiceListUpdateEvent;

        #endregion Public Members


        #region Constructors

        public SubsystemServices(EventLoggerAccess logger) :
            base()
        {
            _Shutdown = false;
            _Logger = logger;
            _SubsystemCommTimeoutMsec = 30000;
        }

        #endregion Constructors


        #region Private Methods

        private void ListenForServices ()
        {
            HostDiscovery serviceDiscovery = new HostDiscovery(typeof(ISubsystem), new TimeSpan(0, 0, 0, 2, 0));

            while (!_Shutdown)
            {
                try
                {
                    Collection<EndpointDiscoveryMetadata> sourceEndpoints = serviceDiscovery.GetAvailableConnections();

                    if (sourceEndpoints.Count > 0)
                    {
                        for (int index = 0; index < sourceEndpoints.Count; index++)
                        {
                            string alias = GetAlias(sourceEndpoints[index].Extensions);
                            string assemblyTag = GetAssemblyTag(sourceEndpoints[index].Extensions);
                            SubsystemService subsystemService = GetService(alias);

                            if (subsystemService == null)
                            {
                                subsystemService = AddSubsystem(alias, assemblyTag, sourceEndpoints[index].Address);

                                Thread assemblyThread = new Thread(new ParameterizedThreadStart(delegate
                                {
                                    GetAssembly(subsystemService);
                                }));
                                assemblyThread.IsBackground = true;
                                assemblyThread.Start();
                            }
                            else
                            {
                                subsystemService.CommCheckTimer.Change(_SubsystemCommTimeoutMsec, Timeout.Infinite);
                            }
                        }
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    _Logger.LogError(ex);
                }
            }
        }

        private SubsystemService AddSubsystem (string alias, string assemblyTag, EndpointAddress address)
        {
            SubsystemService subsystemService = null;

            try
            {
                subsystemService = new SubsystemService(alias, assemblyTag, address);
                subsystemService.CommCheckTimer = new Timer(new TimerCallback(ServiceCommCheck), subsystemService, _SubsystemCommTimeoutMsec, Timeout.Infinite);
                Add(subsystemService);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }

            return subsystemService;
        }

        private void RemoveSubsystem(SubsystemService subsystemService)
        {
            try
            {
                string alias = subsystemService.Alias;

                subsystemService.Dispose();
                Remove(subsystemService);

                if (SubsystemServiceListUpdateEvent != null)
                {
                    SubsystemServiceListUpdateEvent(alias, SubsystemUpdateEnum.SubsystemDisconnect, string.Empty);
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        private void ServiceCommCheck(Object stateInfo)
        {
            try
            {
                // it has been X seconds since "I'm Alive" broadcast message is received from the SubsystemApplication
                // inform the assembly manager of this and dispose this timer
                RemoveSubsystem((SubsystemService)stateInfo);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        private void GetAssembly(SubsystemService subsystemService)
        {
            try
            {
                EnumSystemOperationMode mode = (EnumSystemOperationMode)Enum.Parse(typeof(EnumSystemOperationMode),
                                                                                   ConfigurationManager.AppSettings["SystemOperationMode"]);

                if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, mode.ToString())))
                {
                    Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, mode.ToString()));
                }

                using (SubsystemEndpoint subsystemEndPoint = new SubsystemEndpoint(new TCPBinding(), subsystemService.EndpointAddress))
                {
                    subsystemEndPoint.Open();
                    GetAssemblyParameterMessage msg = new GetAssemblyParameterMessage(mode);
                    SubsystemAssembly assembly = subsystemEndPoint.GetAssembly(msg);

                    //save the assembly locally
                    subsystemService.ZippedFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, mode.ToString(), assembly.filename);
                    using (FileStream stream = new FileStream(subsystemService.ZippedFilename, FileMode.Create))
                    {
                        assembly.file.CopyTo(stream);
                    }
                }

                if (SubsystemServiceListUpdateEvent != null)
                {
                    SubsystemServiceListUpdateEvent(subsystemService.Alias, SubsystemUpdateEnum.SubsystemConnect, subsystemService.ZippedFilename);
                }
            }
            catch (Exception ex)
            {
                RemoveSubsystem(subsystemService);
                _Logger.LogError(ex);
            }
        }

        private SubsystemService GetService(string alias)
        {
            SubsystemService ret = null;

            try
            {
                foreach (SubsystemService subsystemService in this)
                {
                    if (String.Compare(subsystemService.Alias, alias, true) == 0)
                    {
                        ret = subsystemService;
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

        private string GetAlias(Collection<XElement> extensions)
        {
            string alias = string.Empty;

            try
            {
                foreach (XElement xElement in extensions)
                {
                    try
                    {
                        if (xElement.Element(CommuncationInfo.MetaDataAlias) != null)
                        {
                            alias = xElement.Element(CommuncationInfo.MetaDataAlias).Value;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _Logger.LogError(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }

            return alias;
        }

        private string GetAssemblyTag(Collection<XElement> extensions)
        {
            string tag = string.Empty;

            try
            {
                foreach (XElement xElement in extensions)
                {
                    try
                    {
                        if (xElement.Element(CommuncationInfo.MetaDataAssemblyTag) != null)
                        {
                            tag = xElement.Element(CommuncationInfo.MetaDataAssemblyTag).Value;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _Logger.LogError(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }

            return tag;
        }

        #endregion Private Methods


        #region Public Methods

        public void StartUp ()
        {
            try
            {
                if (_ListenerThread == null)
                {
                    _Shutdown = false;
                    _ListenerThread = new Thread(new ThreadStart(ListenForServices));
                    _ListenerThread.IsBackground = true;
                    _ListenerThread.Start();
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        public void ShutDown ()
        {
            try
            {
                if (_ListenerThread != null)
                {
                    _Shutdown = true;
                    _ListenerThread.Abort();
                    _ListenerThread.Join();
                    _ListenerThread = null;
                }

                foreach (SubsystemService subsystemService in this)
                {
                    subsystemService.Dispose();
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
