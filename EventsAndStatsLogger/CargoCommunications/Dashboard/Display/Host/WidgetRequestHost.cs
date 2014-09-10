using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using L3.Cargo.Communications.Dashboard.Display.Interfaces;

namespace L3.Cargo.Communications.Dashboard.Display.Host
{
    public delegate void ControlUpdateHandler (string name, int value);

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class WidgetRequestHost : DisplayHost, IWidgetRequest
    {
        public event ControlUpdateHandler ControlUpdateEvent;

        public void Request (string name, int value)
        {
            if (ControlUpdateEvent != null)
            {
                ControlUpdateEvent(name, value);
            }
        }
    }
}
