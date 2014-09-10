using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel.Configuration;

namespace L3.Cargo.Communications.Common
{
    public class NetworkHost<T>
    {
        #region Private Members

        private ServiceHost m_ServiceHost;

        private ServiceEndpoint m_NetEndpoint;

        private EndpointDiscoveryBehavior m_endpointDiscoveryBehavior;

        private List<DiscoveryMetadata> m_MetadataList;

        #endregion Private Members


        #region Public Members

        public bool IsRunning { get; set; }

        #endregion Public Members


        #region Constructors

        public NetworkHost(T service, Uri baseAddress)
        {
            m_ServiceHost = new ServiceHost(service, baseAddress);

            //list max buffer size limit
            TCPBinding tcpbinding = new TCPBinding();

            m_NetEndpoint = m_ServiceHost.AddServiceEndpoint(typeof(T), tcpbinding, string.Empty);
            IsRunning = false;
        }

        public NetworkHost (T service, Uri baseAddress, List<DiscoveryMetadata> list)
        {
            m_ServiceHost = new ServiceHost(service, baseAddress);

            //list max buffer size limit
            TCPBinding tcpbinding = new TCPBinding();

            m_NetEndpoint = m_ServiceHost.AddServiceEndpoint(typeof(T), tcpbinding, string.Empty);
            IsRunning = false;
            m_MetadataList = list;
        }

        public NetworkHost(T service, Uri baseAddress, List<DiscoveryMetadata> list, int receiveTimeout, int sendTimeout)
        {
            m_ServiceHost = new ServiceHost(service, baseAddress);

            //list max buffer size limit
            TCPBinding tcpbinding = new TCPBinding();
            tcpbinding.SendTimeout = new TimeSpan(0, sendTimeout, 0);
            tcpbinding.ReceiveTimeout = new TimeSpan(0, receiveTimeout, 0);

            m_NetEndpoint = m_ServiceHost.AddServiceEndpoint(typeof(T), tcpbinding, string.Empty);
            IsRunning = false;
            m_MetadataList = list;
        }

        public NetworkHost(T service, Uri baseAddress, string MSMQAddress, List<DiscoveryMetadata> list, string namespaceName)
        {
            m_ServiceHost = new ServiceHost(service);
           
            NetMsmqBinding msmqBinding = new NetMsmqBinding(NetMsmqSecurityMode.None);
            msmqBinding.Namespace = namespaceName;

            m_NetEndpoint = m_ServiceHost.AddServiceEndpoint(typeof(T), msmqBinding, new Uri(MSMQAddress));
            // Expose the service metadata on the metadataAddress

            IsRunning = false;
            m_MetadataList = list;
        }

        #endregion Constructors


        #region Public Methods

        public void SetMetadata (DiscoveryMetadata metadata)
        {
            if (m_MetadataList == null)
                m_MetadataList = new List<DiscoveryMetadata>();

            m_MetadataList.Add(metadata);
        }

        public void UpdateMetadata (DiscoveryMetadata metadata)
        {
            if (!String.IsNullOrWhiteSpace(metadata.Value) && m_MetadataList.Contains(metadata))
            {
                Predicate<DiscoveryMetadata> NameFinder = (DiscoveryMetadata m) => { return m.Name == metadata.Name; };
                DiscoveryMetadata data = m_MetadataList.Find(NameFinder);
                data.Value = metadata.Value;

                EndpointDiscoveryBehavior endpointDiscoveryBehavior = m_NetEndpoint.Behaviors.Find<EndpointDiscoveryBehavior>();

                foreach (XElement endpointMetadata in endpointDiscoveryBehavior.Extensions.Elements<XElement>())
                {
                    if (endpointMetadata.Name.Equals(metadata.Name.ToString()))
                    {
                        endpointMetadata.SetValue(metadata.Value);
                    }
                }
            }
        }

        public void Open()
        {
            try
            {
                m_endpointDiscoveryBehavior = new EndpointDiscoveryBehavior();

                if (m_MetadataList != null)
                {
                    foreach (DiscoveryMetadata metadata in m_MetadataList)
                    {
                        XElement endpointMetadata = new XElement("Root", new XElement(metadata.Name, metadata.Value));

                        m_endpointDiscoveryBehavior.Extensions.Add(endpointMetadata);
                    }

                    m_NetEndpoint.Behaviors.Add(m_endpointDiscoveryBehavior);
                }

                m_ServiceHost.Description.Behaviors.Add(new ServiceDiscoveryBehavior());
                m_ServiceHost.AddServiceEndpoint(new UdpDiscoveryEndpoint());

                m_ServiceHost.Open();

                IsRunning = true;
            }
            catch (CommunicationException ex)
            {
                m_ServiceHost.Abort();
                // customize this exception to be more specific
                throw ex;
            }
        }

        public void Close()
        {
            try
            {
                m_ServiceHost.Close();
            }
            catch (CommunicationException ex)
            {
                m_ServiceHost.Abort();
                // customize this exception to be more specific
                throw ex;
            }
        }

        #endregion Public Methods
    }
}
