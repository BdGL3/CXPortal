using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using L3.Cargo.Communications.Dashboard.Display.Interfaces;

namespace L3.Cargo.Communications.Dashboard.Display.Host
{
    public delegate void SendUpdateHandler ();

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class DisplayHost : IDisplay
    {
        public event SendUpdateHandler SendUpdateEvent;

        public void SendUpdate ()
        {
            if (SendUpdateEvent != null)
            {
                SendUpdateEvent();
            }
        }
    }
}
