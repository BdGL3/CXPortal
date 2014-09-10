using System.ServiceModel;

namespace L3.Cargo.Communications.Dashboard.Display.Interfaces
{
    [ServiceContract]
    public interface IStatus
    {
        [OperationContract]
        void UpdateErrorMessage(string[] messages);

        [OperationContract]
        void UpdateWarningMessage(string[] messages);

        [OperationContract]
        void UpdateIndicator(string color);
    }
}
