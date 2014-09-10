using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.ServiceModel.Discovery;
using System.Xml.Linq;

namespace L3.Cargo.Communications.Common
{
    public class HostDiscovery
    {
        [DefaultValue(null)]
        public DiscoveryClient ClientDiscovery { get; private set; }

        [DefaultValue(default(Type))]
        public Type ContractType { get; private set;}

        public TimeSpan FindDuration { get { return _findDuration; } private set { _findDuration = value; } }
        private TimeSpan _findDuration = /*TimeSpan.Zero*/ default(TimeSpan);

        private String[] GetAllowedClients(EndpointDiscoveryMetadata EndPoint)
        {
            String AllowedClients = null;
            foreach (XElement item in EndPoint.Extensions)
                try
                {
                    AllowedClients = item.Element(CommuncationInfo.MetaDataIPAddresses).Value;
                    break;
                }
                catch { continue; }
            return AllowedClients.ToUpper().Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
        }

        public Collection<EndpointDiscoveryMetadata> GetAvailableConnections()
        {
            FindCriteria criteria = new FindCriteria(ContractType);
            criteria.Duration = FindDuration;
            Collection<EndpointDiscoveryMetadata> endpoints = new Collection<EndpointDiscoveryMetadata>();
            FindResponse response = ClientDiscovery.Find(criteria);
            if (response.Endpoints.Count > 0)
                for (Int32 index = 0; index < response.Endpoints.Count; index++)
                {
                    String[] allowedClients = GetAllowedClients(response.Endpoints[index]);
                    if (IsConnectionAllowed(allowedClients))
                        endpoints.Add(response.Endpoints[index]);
                }
            return endpoints;
        }

        public HostDiscovery(Type type)
        {
            ContractType = type;
            ClientDiscovery = new DiscoveryClient(new UdpDiscoveryEndpoint());
            FindDuration = new TimeSpan(0, 0, 0, 5, 0); /*change default duration from 20 seconds to 5 Seconds*/
        }

        public HostDiscovery(Type type, TimeSpan Duration)
        {
            ContractType = type;
            ClientDiscovery = new DiscoveryClient(new UdpDiscoveryEndpoint());
            FindDuration = Duration;
        }

        private Boolean IsConnectionAllowed(String[] AllowedClients)
        {
            Boolean bRet = false;
            if (AllowedClients.Contains("*"))
                bRet = true;
            else if (AllowedClients.Contains(Dns.GetHostName().ToUpper()))
                bRet = true;
            else
            {
                IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
                foreach (IPAddress ip in ips)
                    if (AllowedClients.Contains(ip.ToString()))
                    {
                        bRet = true;
                        break;
                    }
            }
            return bRet;
        }
    }
}