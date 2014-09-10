using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.Threading;
using System.Xml.Linq;
using L3.Cargo.Common;
using L3.Cargo.Communications.Client;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Interfaces;

namespace L3.Cargo.Workstation.DataSourceCore
{
    public class CaseSources<T> : SynchronizedCollection<CaseSource<T>>
        where T : ICaseRequestManager
    {
        #region Private Members

        private bool m_Shutdown;

        private Type m_ContractType;

        private string m_WSId;

        private SourceType m_SourceType;

        private string m_AliasElementTag = "HostAlias";

        private string m_LoginRequiredElementTag = "LoginRequired";

        private Thread m_ListenerThread;

        private SynchronizedCollection<Thread> m_PingThreads;

        private int _wcfDiscoveryProbeTimeoutPeriodSec;

        private HostDiscovery.EnableDiscoveryManagedModeEnum _enableDiscoveryManagedMode;

        private string _discoveryProxyUri;

        private int _wcfTcpBindingReceiveTimeoutMin;

        private int _wcfTcpBindingSendTimeoutMin;

        private int _wsCommPingTimeoutMsec;

        private object _lockUpdateEventObject = new object();

        #endregion Private Members


        #region Public Members

        public delegate void SourceListUpdateHandler(string alias, bool isAdding, bool isLogingRequired);

        public event SourceListUpdateHandler SourceListUpdateEvent;

        public int WcfTcpBindingReceiveTimeoutMin
        {
            set { _wcfTcpBindingReceiveTimeoutMin = value; }
        }

        public int WcfTcpBindingSendTimeoutMin
        {
            set { _wcfTcpBindingSendTimeoutMin = value; }
        }

        public int WsCommPingTimeoutMsec
        {
            set { _wsCommPingTimeoutMsec = value; }
        }


        #endregion Public Members


        #region Constructors

        public CaseSources(Type contractType, string wsId, SourceType sourceType, int WcfDiscoveryProbeTimeoutPeriodSec) :
            base()
        {
            m_Shutdown = false;
            m_PingThreads = new SynchronizedCollection<Thread>();
            m_ContractType = contractType;
            m_WSId = wsId;
            m_SourceType = sourceType;
            _wcfDiscoveryProbeTimeoutPeriodSec = WcfDiscoveryProbeTimeoutPeriodSec;
            _enableDiscoveryManagedMode = HostDiscovery.EnableDiscoveryManagedModeEnum.FALSE;
            _discoveryProxyUri = string.Empty;
        }

        public CaseSources(Type contractType, string wsId, SourceType sourceType, int WcfDiscoveryProbeTimeoutPeriodSec,
            HostDiscovery.EnableDiscoveryManagedModeEnum enableManagedMode, string discoveryProxyUri) :
            base()
        {
            m_Shutdown = false;
            m_PingThreads = new SynchronizedCollection<Thread>();
            m_ContractType = contractType;
            m_WSId = wsId;
            m_SourceType = sourceType;
            _wcfDiscoveryProbeTimeoutPeriodSec = WcfDiscoveryProbeTimeoutPeriodSec;
            _enableDiscoveryManagedMode = enableManagedMode;
            _discoveryProxyUri = discoveryProxyUri;
        }

        #endregion Constructors


        #region Private Methods

        private void ListenForSources()
        {
            HostDiscovery sourceDiscovery = new HostDiscovery(m_ContractType, new TimeSpan(0, 0, 0, _wcfDiscoveryProbeTimeoutPeriodSec, 0),
                _enableDiscoveryManagedMode, _discoveryProxyUri);

            while (!m_Shutdown)
            {
                Collection<EndpointDiscoveryMetadata> sourceEndpoints = sourceDiscovery.GetAvailableConnections();

                if (sourceEndpoints.Count > 0)
                {
                    for (int index = 0; index < sourceEndpoints.Count; index++)
                    {
                        string alias = GetAlias(sourceEndpoints[index].Extensions);
                        if (!String.IsNullOrWhiteSpace(alias))
                        {
                            alias = m_SourceType.ToString() + "-" + alias;

                            if (!ContainsEndPoints(alias))
                            {
                                bool isLoginRequired = GetLoginRequired(sourceEndpoints[index].Extensions);

                                CaseSource<T> caseSourceT = new CaseSource<T>(alias, isLoginRequired);

                                InstanceContext sourceCallback = new InstanceContext(caseSourceT);

                                ServiceEndpoint HostEndPoint =
                                    new ServiceEndpoint(ContractDescription.GetContract(m_ContractType),
                                                        new TCPBinding(),
                                                        sourceEndpoints[index].Address);
                                HostEndPoint.Binding.SendTimeout = new TimeSpan(0, _wcfTcpBindingSendTimeoutMin, 0);
                                HostEndPoint.Binding.ReceiveTimeout = new TimeSpan(0, _wcfTcpBindingReceiveTimeoutMin, 0);

                                T t = (T)Activator.CreateInstance(typeof(T), new object[] { sourceCallback, HostEndPoint });

                                caseSourceT.EndPoint = t;

                                AddSource(caseSourceT);

                                Thread caseListThread = new Thread(new ParameterizedThreadStart(delegate { try { GetCaseList(caseSourceT, alias); } catch { } }));
                                caseListThread.Start();

                                Thread pingThread = new Thread(new ParameterizedThreadStart(delegate { PingSource(caseSourceT); }));
                                pingThread.Start();

                                m_PingThreads.Add(pingThread);
                            }
                        }
                    }
                }
            }
        }

        private void GetCaseList(CaseSource<T> caseSourceT, string alias)
        {
            try
            {
                caseSourceT.CaseList = new CaseListDataSet();
                CaseListDataSet tempCaseList = caseSourceT.EndPoint.RequestCaseList(m_WSId);
                lock (caseSourceT.CaseListLock)
                {
                    caseSourceT.CaseList.Merge(tempCaseList);
                }
            }
            catch (Exception ex)
            {
                RemoveSource(caseSourceT);
                throw;
            }
        }

        private void PingSource(CaseSource<T> caseSourceT)
        {
            while (!m_Shutdown)
            {
                try
                {
                    caseSourceT.EndPoint.Ping(m_WSId);

                    if (typeof(T) == typeof(WSCommEndpoint))
                        Thread.Sleep(_wsCommPingTimeoutMsec);
                    else
                        Thread.Sleep(1000);
                }
                catch (Exception)
                {
                    break;
                }
            }

            m_PingThreads.Remove(Thread.CurrentThread);

            RemoveSource(caseSourceT);
        }

        private Boolean ContainsEndPoints(string alias)
        {
            foreach (CaseSource<T> caseSourceT in this)
            {
                if (string.Equals(caseSourceT.Alias, alias))
                {
                    return true;
                }
            }

            return false;
        }

        private string GetAlias(Collection<XElement> extensions)
        {
            string alias = string.Empty;
            foreach (XElement xElement in extensions)
            {
                try
                {
                    if (xElement.Element(m_AliasElementTag) != null)
                    {
                        alias = xElement.Element(m_AliasElementTag).Value;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return alias;
        }

        private bool GetLoginRequired(Collection<XElement> extensions)
        {
            bool LoginRequired = false;

            foreach (XElement xElement in extensions)
            {
                try
                {
                    if (xElement.Element(m_LoginRequiredElementTag) != null)
                    {
                        LoginRequired = Boolean.Parse(xElement.Element(m_LoginRequiredElementTag).Value);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return LoginRequired;
        }

        #endregion Private Methods


        #region Public Methods

        public void AddSource(CaseSource<T> caseSourceT)
        {
            lock (_lockUpdateEventObject)
            {
                Add(caseSourceT);
                SourceListUpdateEvent(caseSourceT.Alias, true, caseSourceT.IsLoginRequired);
            }
        }

        public void RemoveSource(CaseSource<T> caseSourceT)
        {
            lock (_lockUpdateEventObject)
            {
                if (Remove(caseSourceT))
                {
                    SourceListUpdateEvent(caseSourceT.Alias, false, false);
                }
            }
        }

        public void StartUp()
        {
            if (m_ListenerThread == null)
            {
                m_Shutdown = false;
                m_ListenerThread = new Thread(new ThreadStart(ListenForSources));
                m_ListenerThread.Start();
            }
        }

        public void ShutDown()
        {
            if (m_ListenerThread != null)
            {
                m_Shutdown = true;
                m_ListenerThread.Abort();
                m_ListenerThread.Join();
                m_ListenerThread = null;


                for (int index = m_PingThreads.Count - 1; index > 0; index--)
                {
                    try
                    {
                        m_PingThreads[index].Abort();
                        m_PingThreads[index].Join();
                        m_PingThreads.RemoveAt(index);
                    }
                    catch { }
                }
            }
        }

        #endregion Public Methods
    }
}
