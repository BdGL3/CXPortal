using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using L3.Cargo.Communications.Detectors.Common;
using L3.Cargo.Communications.EventsLogger.Client;

namespace L3.Cargo.Communications.Detectors.Host
{
    public class RealTimeViewerHost: IDisposable
    {
        #region Private Members
        private EventLoggerAccess _logger;
        private UdpClient _udpClient;
        #endregion Private Members

        #region Constructors
        public RealTimeViewerHost(string address, int dataPort, int udpClientPort, EventLoggerAccess logger)
        {
            _logger = logger;
            IPAddress multicastAddress = IPAddress.Parse(address);

            _udpClient = new UdpClient(udpClientPort);
            _udpClient.JoinMulticastGroup(multicastAddress);
            _udpClient.Ttl = 255;
            _udpClient.Connect(new IPEndPoint(multicastAddress, dataPort));
            _logger.LogInfo(MethodBase.GetCurrentMethod().Name + ": joined multicast group " +
                    multicastAddress.ToString() + ":" + udpClientPort.ToString());
        }
        #endregion Constructors

        #region Public Methods

        public void SendData(byte[] data)
        {
            _udpClient.Send(data, data.Length);
        }

        public void Dispose()
        {
            if (_udpClient != null)
            {
                _logger.LogInfo(MethodBase.GetCurrentMethod().Name + ": exiting multicast group");
                _udpClient.Close();
            }
        }

        #endregion Public Methods
    }
}
