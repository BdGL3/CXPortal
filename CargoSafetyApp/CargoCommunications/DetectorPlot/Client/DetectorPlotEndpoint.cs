using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using L3.Cargo.Communications.DetectorPlot.Common;
using L3.Cargo.Communications.DetectorPlot.Interface;

namespace L3.Cargo.Communications.DetectorPlot.Client
{
    public class DetectorPlotEndpoint : ClientBase<IDetectorPlot>, IDetectorPlot, IDisposable
    {
        #region Constructors

        public DetectorPlotEndpoint()
        {
        }

        public DetectorPlotEndpoint(string endpointConfigurationName) :
            base(endpointConfigurationName)
        {
        }

        public DetectorPlotEndpoint(string endpointConfigurationName, string remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public DetectorPlotEndpoint(string endpointConfigurationName, EndpointAddress remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public DetectorPlotEndpoint(Binding binding, EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
        {
        }

        #endregion Constructors


        #region Methods

        public DetectorConfig GetConfig()
        {
            return base.Channel.GetConfig();
        }

        public void GetHighEnergyData()
        {
            base.Channel.GetHighEnergyData();
        }

        public void GetLowEnergyData()
        {
            base.Channel.GetLowEnergyData();
        }

        public bool IsDataSourceConnected()
        {
            return base.Channel.IsDataSourceConnected();
        }

        public void Dispose()
        {
            try
            {
                if (this.State == CommunicationState.Opened)
                {
                    this.Close();
                }
            }
            catch { }
        }

        #endregion Methods
    }
}
