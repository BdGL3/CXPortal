using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace L3.Cargo.Communications.Interfaces
{
    public interface ITipManagerCallback
    {
        // TODO: Description of the current Interface Call
        [OperationContract(IsOneWay = true)]
        void InjectTip(TIPInjectFileMessage tipInjectFileMessage);
    }
}
