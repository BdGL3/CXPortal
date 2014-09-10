using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using L3.Cargo.Communications.Dashboard.Display.Interfaces;

namespace L3.Cargo.Communications.Dashboard.Display.Host
{
    public delegate void WidgetUpdateHandler (string name, int value);

    [ServiceBehavior(InstanceContextMode= InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class WidgetStatusHost : StatusHost, IWidgetStatus
    {
        public event WidgetUpdateHandler WidgetUpdateEvent;

        public void Update (string name, int value)
        {
            if (WidgetUpdateEvent != null)
            {
                WidgetUpdateEvent(name, value);
            }
        }
    }
}
