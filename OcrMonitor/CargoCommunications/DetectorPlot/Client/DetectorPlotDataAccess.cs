using System;
using System.Net;
using System.Net.Sockets;

namespace L3.Cargo.Communications.DetectorPlot.Client
{
    public class DetectorPlotDataAccess : IDisposable
    {
        #region Private Members

        private UdpClient _udpClient;

        private IPEndPoint _ipEndPoint;

        private string _multicastAddress;

        #endregion Private Members


        #region Public Members

        public bool IsConnected
        {
            get { return _udpClient.Client.Connected; }
        }

        #endregion


        #region Constructor

        public DetectorPlotDataAccess(string multicastAddress, int dataPort)
        {
            _multicastAddress = multicastAddress;

            _ipEndPoint = new IPEndPoint(IPAddress.Any, dataPort);

            _udpClient = new UdpClient();
            _udpClient.Client.Bind(_ipEndPoint);
            _udpClient.JoinMulticastGroup(IPAddress.Parse(multicastAddress));
        }

        #endregion


        #region Public Methods

        public byte[] ReceiveDataLines()
        {
            return _udpClient.Receive(ref _ipEndPoint);
        }

        public void Dispose()
        {
            if (_udpClient != null)
            {
                _udpClient.DropMulticastGroup(IPAddress.Parse(_multicastAddress));
                _udpClient.Close();
                _udpClient = null;
            }
        }

        #endregion Public Methods
    }
}