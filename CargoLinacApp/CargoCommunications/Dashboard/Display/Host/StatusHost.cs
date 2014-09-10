using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using L3.Cargo.Communications.Dashboard.Display.Interfaces;

namespace L3.Cargo.Communications.Dashboard.Display.Host
{
    public delegate void UpdateErrorMessageHandler(string[] messages);

    public delegate void UpdateWarningMessageHandler(string[] messages);

    public delegate void UpdateIndicatorHandler(string color);

    [ServiceBehavior(InstanceContextMode= InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class StatusHost : IStatus
    {
        public event UpdateErrorMessageHandler ErrorMessageUpdate;

        public event UpdateWarningMessageHandler WarningMessageUpdate;

        public event UpdateIndicatorHandler IndicatorUpdate;


        public void UpdateErrorMessage(string[] messages)
        {
            if (ErrorMessageUpdate != null)
            {
                ErrorMessageUpdate(messages);
            }
        }

        public void UpdateWarningMessage(string[] messages)
        {
            if (WarningMessageUpdate != null)
            {
                WarningMessageUpdate(messages);
            }
        }

        public void UpdateIndicator(string color)
        {
            if (IndicatorUpdate != null)
            {
                IndicatorUpdate(color);
            }
        }
    }
}
