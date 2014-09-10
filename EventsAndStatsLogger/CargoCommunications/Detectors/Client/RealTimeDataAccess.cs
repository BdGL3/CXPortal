using System;
using System.Net;
using System.Net.Sockets;

namespace L3.Cargo.Communications.Detectors.Client
{
    public class RealTimeDataAccess: IDisposable
    {
        #region Private Members

        private UdpClient _udpClient;

        private IPEndPoint _udpIpEndPoint;

        private string _multicastAddr;

        #endregion


        #region Constructor

        public RealTimeDataAccess(string multicastAddress, int dataPort)
        {
            _multicastAddr = multicastAddress;

            _udpClient = new UdpClient();
            _udpIpEndPoint = new IPEndPoint(IPAddress.Any, dataPort);
            _udpClient.Client.Bind(_udpIpEndPoint);
            _udpClient.JoinMulticastGroup(IPAddress.Parse(multicastAddress));
        }

        #endregion


        #region Private Methods


        #endregion


        #region Public Methods

        public byte[] ReceiveDataLines()
        {
            byte[] receiveBytes;

            try
            {
                receiveBytes = _udpClient.Receive(ref _udpIpEndPoint);
            }
            catch
            {
                receiveBytes = null;
            }

            return receiveBytes;
        }

        #endregion


        #region IDisposable

        public void Dispose()
        {
            if (_udpClient != null)
            {
                _udpClient.DropMulticastGroup(IPAddress.Parse(_multicastAddr));
                _udpClient.Close();
                _udpClient = null;
            }
        }

        #endregion
    }
}
