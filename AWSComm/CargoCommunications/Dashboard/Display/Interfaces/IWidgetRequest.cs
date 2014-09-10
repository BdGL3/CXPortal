using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace L3.Cargo.Communications.Dashboard.Display.Interfaces
{
    [ServiceContract]
    public interface IWidgetRequest : IDisplay
    {
        [OperationContract]
        void Request (string name, int value);
    }
}
