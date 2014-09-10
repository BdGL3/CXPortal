using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;

namespace L3.Cargo.Communications.Dashboard.Interfaces
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract(Namespace = "SubsystemCommInterfacesV1.0")]
    public interface ISubsystem
    {
        [OperationContract]
        SubsystemAssembly GetAssembly(GetAssemblyParameterMessage message);

        // TODO: Add your service operations here
    }

    [DataContract]
    public enum EnumSystemOperationMode
    {
        [EnumMember]
        Operator,

        [EnumMember]
        Supervisor,

        [EnumMember]
        Engineer,

        [EnumMember]
        Maintenance
    }

    [MessageContract]
    public class GetAssemblyParameterMessage
    {
        [MessageHeader]
        public EnumSystemOperationMode SystemMode;

        public GetAssemblyParameterMessage()
        {
        }

        public GetAssemblyParameterMessage(EnumSystemOperationMode mode)
        {
            SystemMode = mode;
        }

    }

    [MessageContract]
    public class SubsystemAssembly
    {
        [MessageHeader]
        public String filename;

        [MessageBodyMember]
        public Stream file;

        public SubsystemAssembly()
        {
            file = null;
            filename = string.Empty;
        }

        public SubsystemAssembly(Stream f, string name)
        {
            filename = name;
            file = f;
        }
    }
}
