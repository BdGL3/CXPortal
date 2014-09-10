using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.Xml.Linq;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Interfaces;
using System.Timers;

namespace L3.Cargo.Communications.Host
{
    public class NetworkHost
    {
        #region Private Members

        private const string _metaDataElement = "AcceptedIPAddresses";

        private const string _metaDataAlias = "HostAlias";

        private const string _metaDataLogIn = "LoginRequired";

        private ServiceHost _serviceHost;

        private ServiceEndpoint _netTcpEndpoint;

        private string _metaDataElementValue;

        private string _metaDataAliasValue;

        private string _metaDataLogInValue;

        private Timer _announcementTimer;

        private AnnouncementClient _announcementClient;

        private EndpointDiscoveryMetadata _endpointDiscoveryMetadata;

        private AnnouncementEndpoint _announcementEndpoint;

        #endregion Private Members


        #region Public Members

        public bool IsRunning { get; set; }

        public int SendTimeoutMin
        {
            set { _netTcpEndpoint.Binding.SendTimeout = new TimeSpan(0, value, 0); }
        }

        public int ReceiveTimeoutMin
        {
            set { _netTcpEndpoint.Binding.ReceiveTimeout = new TimeSpan(0, value, 0); }
        }

        #endregion Public Members


        #region Constructors

        public NetworkHost(IWSComm awsComm, Uri baseAddress, String alias)
        {
            _serviceHost = new ServiceHost(awsComm, baseAddress);

            //list max buffer size limit
            TCPBinding tcpbinding = new TCPBinding();

            _netTcpEndpoint = _serviceHost.AddServiceEndpoint(typeof(IWSComm), tcpbinding, string.Empty);
            _metaDataElementValue = "*";
            _metaDataAliasValue = alias;
            _metaDataLogInValue = "True";
            IsRunning = false;
        }

        public NetworkHost(ICaseRequestManager caseRequestManager, Uri baseAddress, string alias)
        {
            _serviceHost = new ServiceHost(caseRequestManager, baseAddress);

            //list max buffer size limit
            TCPBinding tcpbinding = new TCPBinding();

            _netTcpEndpoint = _serviceHost.AddServiceEndpoint(typeof(ICaseRequestManager), tcpbinding, String.Empty);
            _metaDataElementValue = "*";
            _metaDataAliasValue = alias;
            _metaDataLogInValue = "False";
            IsRunning = false;
        }

        public NetworkHost (IWSComm awsComm, Uri baseAddress, String alias, string ipAddressRange)
        {
            _serviceHost = new ServiceHost(awsComm, baseAddress);

            //list max buffer size limit
            TCPBinding tcpbinding = new TCPBinding();

            _netTcpEndpoint = _serviceHost.AddServiceEndpoint(typeof(IWSComm), tcpbinding, string.Empty);
            _metaDataElementValue = ipAddressRange;
            _metaDataAliasValue = alias;
            _metaDataLogInValue = "True";
            IsRunning = false;
        }

        public NetworkHost (ICaseRequestManager caseRequestManager, Uri baseAddress, String alias, string ipAddressRange)
        {
            _serviceHost = new ServiceHost(caseRequestManager, baseAddress);

            //list max buffer size limit
            TCPBinding tcpbinding = new TCPBinding();

            _netTcpEndpoint = _serviceHost.AddServiceEndpoint(typeof(ICaseRequestManager), tcpbinding, string.Empty);
            _metaDataElementValue = ipAddressRange;
            _metaDataAliasValue = alias;
            _metaDataLogInValue = "False";
            IsRunning = false;
        }

        public NetworkHost (IWSComm awsComm, Uri baseAddress, string alias, bool isLogginRequired)
        {
            _serviceHost = new ServiceHost(awsComm, baseAddress);

            //list max buffer size limit
            TCPBinding tcpbinding = new TCPBinding();

            _netTcpEndpoint = _serviceHost.AddServiceEndpoint(typeof(IWSComm), tcpbinding, string.Empty);
            _metaDataElementValue = "*";
            _metaDataAliasValue = alias;
            _metaDataLogInValue = isLogginRequired.ToString();
            IsRunning = false;
        }

        public NetworkHost (ICaseRequestManager caseRequestManager, Uri baseAddress, string alias, bool isLogginRequired)
        {
            _serviceHost = new ServiceHost(caseRequestManager, baseAddress);

            //list max buffer size limit
            TCPBinding tcpbinding = new TCPBinding();

            _netTcpEndpoint = _serviceHost.AddServiceEndpoint(typeof(ICaseRequestManager), tcpbinding, string.Empty);
            _metaDataElementValue = "*";
            _metaDataAliasValue = alias;
            _metaDataLogInValue = isLogginRequired.ToString();
            IsRunning = false;
        }

        public NetworkHost (IWSComm awsComm, Uri baseAddress, String alias, string ipAddressRange, bool isLogginRequired)
        {
            _serviceHost = new ServiceHost(awsComm, baseAddress);

            //list max buffer size limit
            TCPBinding tcpbinding = new TCPBinding();

            _netTcpEndpoint = _serviceHost.AddServiceEndpoint(typeof(IWSComm), tcpbinding, string.Empty);
            _metaDataElementValue = ipAddressRange;
            _metaDataAliasValue = alias;
            _metaDataLogInValue = isLogginRequired.ToString();
            IsRunning = false;
        }

        public NetworkHost (ICaseRequestManager caseRequestManager, Uri baseAddress, string alias, string ipAddressRange, bool isLogginRequired)
        {
            _serviceHost = new ServiceHost(caseRequestManager, baseAddress);

            //list max buffer size limit
            TCPBinding tcpbinding = new TCPBinding();

            _netTcpEndpoint = _serviceHost.AddServiceEndpoint(typeof(ICaseRequestManager), tcpbinding, string.Empty);
            _metaDataElementValue = ipAddressRange;
            _metaDataAliasValue = alias;
            _metaDataLogInValue = isLogginRequired.ToString();
            IsRunning = false;
        }

        public NetworkHost(ITipManager tipManager, Uri baseAddress, String alias)
        {
            _serviceHost = new ServiceHost(tipManager, baseAddress);

            //list max buffer size limit
            TCPBinding tcpbinding = new TCPBinding();

            _netTcpEndpoint = _serviceHost.AddServiceEndpoint(typeof(ITipManager), tcpbinding, String.Empty);
            _metaDataElementValue = "*";
            _metaDataAliasValue = alias;
            IsRunning = false;
        }

        public NetworkHost(ITipManager tipManager, Uri baseAddress, String alias, String ipAddressRange)
        {
            _serviceHost = new ServiceHost(tipManager, baseAddress);

            //list max buffer size limit
            TCPBinding tcpbinding = new TCPBinding();

            _netTcpEndpoint = _serviceHost.AddServiceEndpoint(typeof(ITipManager), tcpbinding, String.Empty);
            _metaDataElementValue = ipAddressRange;
            _metaDataAliasValue = alias;
            IsRunning = false;
        }

        #endregion Constructors


        #region Public Methods

        public void UpdateIPAddressList(String ipAddressRange)
        {
            if (!String.IsNullOrWhiteSpace(ipAddressRange))
            {
                _metaDataElementValue = ipAddressRange;

                EndpointDiscoveryBehavior endpointDiscoveryBehavior = _netTcpEndpoint.Behaviors.Find<EndpointDiscoveryBehavior>();

                foreach (XElement endpointMetadata in endpointDiscoveryBehavior.Extensions.Elements<XElement>())
                {
                    if (_metaDataElement.Equals(endpointMetadata.Name.ToString()))
                    {
                        endpointMetadata.SetValue(_metaDataElementValue);
                    }
                }
            }
        }

        public void Open(string announcementConnectionUri, int announcementTimerSec, bool enableAnnouncement)
        {
            try
            {
                
                ServiceDiscoveryBehavior serviceDiscoveryBehavior = new ServiceDiscoveryBehavior();

                EndpointDiscoveryBehavior endpointDiscoveryBehavior = new EndpointDiscoveryBehavior();

                XElement endpointMetadata = new XElement("Root", new XElement(_metaDataElement, _metaDataElementValue));
                XElement aliasMetadata = new XElement("Root", new XElement(_metaDataAlias, _metaDataAliasValue));
                XElement LoginMetadata = new XElement("Root", new XElement(_metaDataLogIn, _metaDataLogInValue));

                endpointDiscoveryBehavior.Extensions.Add(endpointMetadata);
                endpointDiscoveryBehavior.Extensions.Add(aliasMetadata);
                endpointDiscoveryBehavior.Extensions.Add(LoginMetadata);

                _netTcpEndpoint.Behaviors.Add(endpointDiscoveryBehavior);

                _serviceHost.Description.Behaviors.Add(serviceDiscoveryBehavior);
                _serviceHost.AddServiceEndpoint(new UdpDiscoveryEndpoint());

                _serviceHost.Open();

                if (enableAnnouncement)
                {
                    Uri announcementEndpointUri = new Uri(announcementConnectionUri);
                    EndpointAddress announcementEndpointAddress = new EndpointAddress(announcementEndpointUri);
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.Security.Mode = SecurityMode.None;
                    _announcementEndpoint = new AnnouncementEndpoint(binding, announcementEndpointAddress);

                    _announcementClient = new AnnouncementClient(_announcementEndpoint);
                    _endpointDiscoveryMetadata = EndpointDiscoveryMetadata.FromServiceEndpoint(_netTcpEndpoint);

                    //Start a timer that send announcement message
                    _announcementTimer = new Timer(announcementTimerSec * 1000);
                    _announcementTimer.AutoReset = true;
                    _announcementTimer.Elapsed += new ElapsedEventHandler(_announcementTimer_Elapsed);
                    _announcementTimer.Start();

                    _announcementClient.Open();
                }

                IsRunning = true;
               
            }
            catch (EndpointNotFoundException ex)
            {
                //this error occurs when announcement endpoint is not on the network
            }
            catch (CommunicationException ex)
            {
                _serviceHost.Abort();
                // customize this exception to be more specific
                throw;
            }
        }
        
        public void Close()
        {
            try
            {
                _announcementClient.AnnounceOffline(_endpointDiscoveryMetadata);

                if (_announcementTimer != null)
                    _announcementTimer.Close();

                _announcementClient.Close();
                _serviceHost.Close();
            }
            catch (CommunicationException ex)
            {
                _serviceHost.Abort();
                // customize this exception to be more specific
                throw;
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void _announcementTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _announcementClient.AnnounceOnline(_endpointDiscoveryMetadata);
            }
            catch (EndpointNotFoundException ex)
            {
                //this error occurs when announcement endpoint is not on the network
            }
            catch (CommunicationException ex)
            {
                _announcementClient.InnerChannel.Abort();
                _announcementClient = new AnnouncementClient(_announcementEndpoint);
            }
            catch (Exception exp)
            {
            }
        }

        #endregion
    }
}
