using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using L3.Cargo.Communications.Dashboard.Display.Interfaces;
using System.ServiceModel.Channels;

namespace L3.Cargo.Communications.Dashboard.Display.Client
{
    public class WidgetStatusEndpoint : ClientBase<IWidgetStatus>, IWidgetStatus, IDisposable
    {
        #region Constructors

        public WidgetStatusEndpoint()
        {
        }

        public WidgetStatusEndpoint(string endpointConfigurationName) :
            base(endpointConfigurationName)
        {
        }

        public WidgetStatusEndpoint(string endpointConfigurationName, string remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public WidgetStatusEndpoint(string endpointConfigurationName, EndpointAddress remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public WidgetStatusEndpoint (Binding binding, EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
        {
        }

        #endregion Constructors


        #region Methods

        public void UpdateErrorMessage(string[] messages)
        {
            base.Channel.UpdateErrorMessage(messages);
        }

        public void UpdateWarningMessage(string[] messages)
        {
            base.Channel.UpdateWarningMessage(messages);
        }

        public void UpdateIndicator(string color)
        {
            base.Channel.UpdateIndicator(color);
        }

        public void Update (string name, int value)
        {
            base.Channel.Update(name, value);
        }

        public void Dispose ()
        {
            try
            {
                if (this.State == CommunicationState.Opened)
                {
                    this.Close();
                }
            }
            catch { }
        }

        #endregion Methods
    }
}
