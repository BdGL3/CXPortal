using System.ServiceModel;

namespace L3.Cargo.Communications.Dashboard.Display.Interfaces
{
    [ServiceContract]
    public interface IDisplay
    {
        [OperationContract]
        void SendUpdate ();
    }
}