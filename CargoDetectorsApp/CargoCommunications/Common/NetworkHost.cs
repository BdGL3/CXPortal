using System;
using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.ServiceModel;
using System.Xml.Linq;

namespace L3.Cargo.Communications.Common
{
    public class NetworkHost<T>
    {
        public void Close(TimeSpan timeout = /*TimeSpan.Zero*/ default(TimeSpan))
        {
            try
            {
                if (HostService != null)
                {
                    if (/*default/omitted?*/ timeout == /*TimeSpan.Zero*/ default(TimeSpan))
                        timeout = CloseTimeout;
                    HostService.Close(timeout);
                }
            }
            catch (Exception ex)
            {
                try { HostService.Abort(); } catch { }
                Debug.WriteLine(ex.ToString());
                throw ex;
            }
            finally
            {
                HostService = null;
                Debug.Assert(!IsRunning);
            }
        }

        public static TimeSpan CloseTimeout { get { return TimeSpan.FromSeconds(3); } } 

        [DefaultValue(null)]
        public ServiceEndpoint HostEndpoint { get; private set; }

        [DefaultValue(null)]
        public List<DiscoveryMetadata> HostMetadata { get; private set; }

        [DefaultValue(null)]
        public ServiceHost HostService { get; private set; }

        [DefaultValue(false)]
        public bool IsRunning { get { return (HostService == null)? false : CommunicationState.Opened == (CommunicationState)HostService.State; } }

        public void MetadataSet(DiscoveryMetadata metadata)
        {
            if (HostMetadata == null)
                HostMetadata = new List<DiscoveryMetadata>();
            HostMetadata.Add(metadata);
        }

        public void MetadataUpdate(DiscoveryMetadata metadata)
        {
            if (!String.IsNullOrWhiteSpace(metadata.Value))
                if (HostMetadata != null)
                    if (HostMetadata.Contains(metadata))
                        try
                        {
                            Predicate<DiscoveryMetadata> finder = (DiscoveryMetadata m) => { return m.Name == metadata.Name; };
                            DiscoveryMetadata data = HostMetadata.Find(finder);
                            data.Value = metadata.Value;
                            EndpointDiscoveryBehavior behavior = HostEndpoint.Behaviors.Find<EndpointDiscoveryBehavior>();
                            foreach (XElement element in behavior.Extensions.Elements<XElement>())
                                if (element.Name.Equals(metadata.Name.ToString()))
                                    element.SetValue(metadata.Value);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                            throw ex;
                        }
        }

        public NetworkHost(T service, Uri address)
        {
            TCPBinding binding = new TCPBinding();
            HostService = new ServiceHost(service, address);
            HostEndpoint = HostService.AddServiceEndpoint(typeof(T), binding, string.Empty);
            Debug.Assert(!IsRunning);
        }

        public NetworkHost (T service, Uri address, List<DiscoveryMetadata> metaData)
        {
            HostMetadata = metaData;
            TCPBinding binding = new TCPBinding();
            HostService = new ServiceHost(service, address);
            HostEndpoint = HostService.AddServiceEndpoint(typeof(T), binding, string.Empty);
            Debug.Assert(!IsRunning);
        }

        public NetworkHost(T service, Uri address, List<DiscoveryMetadata> metaData, int timeoutReceive, int timeoutSend)
        {
            HostMetadata = metaData;
            TCPBinding binding = new TCPBinding();
            binding.ReceiveTimeout = new TimeSpan(0, timeoutReceive, 0);
            binding.SendTimeout = new TimeSpan(0, timeoutSend, 0);
            HostService = new ServiceHost(service, address);
            HostEndpoint = HostService.AddServiceEndpoint(typeof(T), binding, string.Empty);
            Debug.Assert(!IsRunning);
        }

        public NetworkHost(T service, Uri address, string msmqAddress, List<DiscoveryMetadata> metaData, string nameSpace)
        {
            HostMetadata = metaData;
            NetMsmqBinding binding = new NetMsmqBinding(NetMsmqSecurityMode.None);
            binding.Namespace = nameSpace;
            HostService = new ServiceHost(service);
            HostEndpoint = HostService.AddServiceEndpoint(typeof(T), binding, new Uri(msmqAddress));
            Debug.Assert(!IsRunning);
        }

        public void Open()
        {
            try
            {
                if (HostMetadata != null)
                {
                    EndpointDiscoveryBehavior behavior = new EndpointDiscoveryBehavior();
                    foreach (DiscoveryMetadata metadata in HostMetadata)
                    {
                        XElement element = new XElement("Root", new XElement(metadata.Name, metadata.Value));
                        behavior.Extensions.Add(element);
                    }
                    HostEndpoint.Behaviors.Add(behavior);
                }
                HostService.Description.Behaviors.Add(new ServiceDiscoveryBehavior());
                HostService.AddServiceEndpoint(new UdpDiscoveryEndpoint());
                HostService.Open();
                Debug.Assert(IsRunning);
            }
            catch (Exception ex)
            {
                try { Close(); } catch { }
                Debug.WriteLine(ex.ToString());
                Debug.Assert(!IsRunning);
                throw ex;
            }
        }
    }
}