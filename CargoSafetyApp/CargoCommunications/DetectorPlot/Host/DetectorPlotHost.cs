using System;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.DetectorPlot.Common;
using L3.Cargo.Communications.DetectorPlot.Interface;

namespace L3.Cargo.Communications.DetectorPlot.Host
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class DetectorPlotHost : IDetectorPlot
    {
        #region Private Members

        private UdpClient _udpClient;

        private IPEndPoint _remoteEp;

        private NetworkHost<IDetectorPlot> _networkHost;

        #endregion Private Members


        #region Constructors

        public DetectorPlotHost(string connectionString, string address, int dataPort)
        {
            _networkHost = new NetworkHost<IDetectorPlot>(this, new Uri(connectionString));
            _udpClient = new UdpClient(dataPort-100);
            _remoteEp = new IPEndPoint(IPAddress.Parse(address), dataPort);
        }

        #endregion Constructors


        #region Public Methods

        public void Open()
        {
            _networkHost.Open();
            //_udpClient.Connect(_remoteEp);
        }

        public void Close()
        {
            _networkHost.Close();
            _udpClient.Close();
        }

        public virtual DetectorConfig GetConfig()
        {
            throw new NotImplementedException();
        }

        public virtual void GetHighEnergyData()
        {
            throw new NotImplementedException();
        }

        public virtual void GetLowEnergyData()
        {
            throw new NotImplementedException();
        }

        public virtual bool IsDataSourceConnected()
        {
            throw new NotImplementedException();
        }

        public void SendLineData(byte[] data)
        {
            _udpClient.Send(data, data.Length, _remoteEp);
        }

        #endregion Public Methods
    }
}
