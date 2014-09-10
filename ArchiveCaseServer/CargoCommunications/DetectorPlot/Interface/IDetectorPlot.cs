using System.ServiceModel;
using L3.Cargo.Communications.DetectorPlot.Common;

namespace L3.Cargo.Communications.DetectorPlot.Interface
{
    [ServiceContract]
    public interface IDetectorPlot
    {        
        [OperationContract]
        DetectorConfig GetConfig();

        [OperationContract]
        void GetHighEnergyData();

        [OperationContract]
        void GetLowEnergyData();

        [OperationContract]
        bool IsDataSourceConnected();
    }
}
