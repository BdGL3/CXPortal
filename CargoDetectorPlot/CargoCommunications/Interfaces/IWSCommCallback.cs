using System.ServiceModel;

namespace L3.Cargo.Communications.Interfaces
{
    public interface IWSCommCallback : ICaseRequestManagerCallback
    {
        // TODO: Description of the current Interface Call
        [OperationContract(IsOneWay = true)]
        void UpdatedManifestList(ManifestListUpdate listupdate);
    }
}
