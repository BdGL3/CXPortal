using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.ServiceModel.Discovery;
using System.Xml.Linq;
using L3.Cargo.Communications.Common;
using System.ServiceModel;

namespace L3.Cargo.Communications.Client
{
    public class HostDiscovery
    {
        #region Private Members

        private DiscoveryClient m_DiscoveryClient;

        private Type m_ContractType;

        private TimeSpan m_FindDuration;

        private const String MetaDataElement = "AcceptedIPAddresses";

        private bool _discoveryManagedMode;

        private UdpDiscoveryEndpoint _udpDiscoveryEndpoint;

        private object _lockObject;

        private EnableDiscoveryManagedModeEnum _managedMode;

        #endregion Private Members

        #region Public Members

        public enum EnableDiscoveryManagedModeEnum
        {
            TRUE,
            FALSE,
            AUTO
        }

        public TimeSpan FindDuration
        {
            get
            {
                return m_FindDuration;
            }
            set
            {
                m_FindDuration = value;
            }
        }

        public Type ContractType
        {
            get
            {
                return m_ContractType;
            }
            set
            {
                m_ContractType = value;
            }
        }

        #endregion Public Members

        #region Constructors

        public HostDiscovery(Type type)
        {
            m_ContractType = type;

            _udpDiscoveryEndpoint = new UdpDiscoveryEndpoint();

            _lockObject = new object();

            _managedMode = EnableDiscoveryManagedModeEnum.FALSE;

            DiscoveryAdHoc();

            // Change the Default Duration from 20 seconds to 5 Seconds
            m_FindDuration = new TimeSpan(0, 0, 0, 5, 0);
        }

        public HostDiscovery(Type type, TimeSpan Duration)
        {
            m_ContractType = type;

            _udpDiscoveryEndpoint = new UdpDiscoveryEndpoint();

            _lockObject = new object();

            _managedMode = EnableDiscoveryManagedModeEnum.FALSE;

            DiscoveryAdHoc();

            m_FindDuration = Duration;
        }

        public HostDiscovery(Type type, TimeSpan Duration, EnableDiscoveryManagedModeEnum ManagedMode, string DiscoveryProxyUri)
        {
            m_ContractType = type;

            _lockObject = new object();

            _managedMode = ManagedMode;

            if (ManagedMode == EnableDiscoveryManagedModeEnum.TRUE)
            {
                DiscoveryManaged(DiscoveryProxyUri);
            }
            else
            {
                _udpDiscoveryEndpoint = new UdpDiscoveryEndpoint();
                DiscoveryAdHoc();

                //if (ManagedMode == EnableDiscoveryManagedModeEnum.AUTO)
                //{
                //    m_DiscoveryClient.ProxyAvailable += new EventHandler<AnnouncementEventArgs>(m_DiscoveryClient_ProxyAvailable);
                //}
            }
            m_FindDuration = Duration;
        }

        #endregion Constructors

        #region Public Methods

        public Collection<EndpointDiscoveryMetadata> GetAvailableConnections()
        {
            FindCriteria findCriteria = new FindCriteria(m_ContractType);
            findCriteria.Duration = m_FindDuration;
            Collection<EndpointDiscoveryMetadata> endpoints = new Collection<EndpointDiscoveryMetadata>();

            try
            {
                FindResponse findResponse;

                lock (_lockObject)
                {
                    findResponse = m_DiscoveryClient.Find(findCriteria);
                }

                if (findResponse.Endpoints.Count > 0)
                {
                    for (Int32 index = 0; index < findResponse.Endpoints.Count; index++)
                    {
                        String[] AllowedClients = GetAllowedClients(findResponse.Endpoints[index]);

                        if (IsConnectionAllowed(AllowedClients))
                        {
                            endpoints.Add(findResponse.Endpoints[index]);
                        }
                    }
                }
                else
                {
                    //if in managed mode and DiscoveryProxy has not replied to Probe message then switch to Ad-hoc mode
                    if (_discoveryManagedMode)
                    {
                        DiscoveryAdHoc();
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO: Log Error
                DiscoveryAdHoc();
            }

            return endpoints;
        }

        #endregion Public Methods

        #region Private Methods

        private void DiscoveryAdHoc()
        {
            try
            {
                lock (_lockObject)
                {
                    m_DiscoveryClient = new DiscoveryClient(_udpDiscoveryEndpoint);
                    _discoveryManagedMode = false;

                    if(_managedMode == EnableDiscoveryManagedModeEnum.AUTO)
                        m_DiscoveryClient.ProxyAvailable +=new EventHandler<AnnouncementEventArgs>(m_DiscoveryClient_ProxyAvailable);
                }
            }
            catch (Exception exp)
            {
            }
        }

        private void DiscoveryManaged(string uri)
        {
            try
            {
                Uri probeEndpointAddress = new Uri(uri);
                NetTcpBinding binding = new NetTcpBinding();
                binding.Security.Mode = SecurityMode.None;
                DiscoveryEndpoint endpoint = new DiscoveryEndpoint(binding,
                    new EndpointAddress(probeEndpointAddress));

                lock (_lockObject)
                {
                    m_DiscoveryClient = new DiscoveryClient(endpoint);
                    _discoveryManagedMode = true;
                }                
            }
            catch(Exception exp)
            {
            }
        }

        void m_DiscoveryClient_ProxyAvailable(object sender, AnnouncementEventArgs e)
        {            
            DiscoveryManaged(e.EndpointDiscoveryMetadata.Address.Uri.AbsoluteUri);
        }

        private String[] GetAllowedClients(EndpointDiscoveryMetadata EndPoint)
        {
            String AllowedClients = null;

            foreach (XElement xElement in EndPoint.Extensions)
            {
                try
                {
                    AllowedClients = xElement.Element(MetaDataElement).Value;
                    break;
                }
                catch
                {
                    continue;
                }
            }

            return AllowedClients.Split(new String[] { ";" },
                                        StringSplitOptions.RemoveEmptyEntries);
        }

        private Boolean IsConnectionAllowed(String[] AllowedClients)
        {
            Boolean bRet = false;

            if (AllowedClients.Contains("*"))
            {
                bRet = true;
            }
            else
            {
                IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());

                foreach (IPAddress ip in ips)
                {
                    if (AllowedClients.Contains(ip.ToString()))
                    {
                        bRet = true;
                        break;
                    }
                }
            }

            return bRet;
        }

        #endregion Private Methods
    }
}
