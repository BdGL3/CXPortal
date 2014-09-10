using System.ServiceModel;

namespace L3.Cargo.Communications.Interfaces
{
    [ServiceContract(Namespace = "CargoCommInterfacesV1.0", CallbackContract = typeof(ITipManagerCallback))]
    public interface ITipManager
    {
        // TODO: Description of the current Interface Call
        [OperationContract(IsOneWay=true)]
        void ProcessedCase(string alias, string caseId);

        // TODO: Description of the current Interface Call
        [OperationContract(IsOneWay = true)]
        void TipResult (string tipFile, WorkstationResult workstationResult);
    }
}
