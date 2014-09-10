using System.ServiceModel;

namespace L3.Cargo.Communications.Interfaces
{
    public interface ICaseRequestManagerCallback
    {
        // TODO: Description of the current Interface Call
        [OperationContract(IsOneWay = true)]
        void UpdatedCaseList(CaseListUpdate listupdate);
    }
}
