using System.ServiceModel;
using L3.Cargo.Common;

namespace L3.Cargo.Communications.Interfaces
{
    [ServiceContract(Namespace = "CargoCommInterfacesV1.0", CallbackContract = typeof(IWSCommCallback))]
    public interface IWSComm : ICaseRequestManager
    {
    }
}
