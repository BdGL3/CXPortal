using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.ServiceModel.Discovery;
using System.Xml.Linq;

namespace L3.Cargo.Communications.Common
{
    public class HostDiscovery
    {
        #region Private Members

        private DiscoveryClient m_DiscoveryClient;

        private Type m_ContractType;

        private TimeSpan m_FindDuration;

        #endregion Private Members


        #region Public Members

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
            m_DiscoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint());
            // Change the Default Duration from 20 seconds to 5 Seconds
            m_FindDuration = new TimeSpan(0, 0, 0, 5, 0);
        }

        public HostDiscovery(Type type, TimeSpan Duration)
        {
            m_ContractType = type;
            m_DiscoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint());
            m_FindDuration = Duration;
        }

        #endregion Constructors


        #region Public Methods

        public Collection<EndpointDiscoveryMetadata> GetAvailableConnections()
        {
            FindCriteria findCriteria = new FindCriteria(m_ContractType);
            findCriteria.Duration = m_FindDuration;
            Collection<EndpointDiscoveryMetadata> endpoints = new Collection<EndpointDiscoveryMetadata>();

            FindResponse findResponse = m_DiscoveryClient.Find(findCriteria);

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

            return endpoints;
        }

        #endregion Public Methods


        #region Private Methods

        private String[] GetAllowedClients(EndpointDiscoveryMetadata EndPoint)
        {
            String AllowedClients = null;

            foreach (XElement xElement in EndPoint.Extensions)
            {
                try
                {
                    AllowedClients = xElement.Element(CommuncationInfo.MetaDataIPAddresses).Value;
                    break;
                }
                catch
                {
                    continue;
                }
            }

            return AllowedClients.ToUpper().Split(new String[] { ";" },
                                        StringSplitOptions.RemoveEmptyEntries);
        }

        private Boolean IsConnectionAllowed(String[] AllowedClients)
        {
            Boolean bRet = false;

            if (AllowedClients.Contains("*"))
            {
                bRet = true;
            }
            else if (AllowedClients.Contains(Dns.GetHostName().ToUpper()))
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
