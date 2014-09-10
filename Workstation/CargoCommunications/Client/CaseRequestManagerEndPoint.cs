using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using L3.Cargo.Common.Xml.Profile_1_0;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Interfaces;

namespace L3.Cargo.Communications.Client
{
    public class CaseRequestManagerEndpoint : DuplexClientBase<ICaseRequestManager>, ICaseRequestManager
    {
        #region Constructors

        public CaseRequestManagerEndpoint(InstanceContext callbackInstance) :
            base(callbackInstance)
        {
        }

        public CaseRequestManagerEndpoint(Object callbackInstance) :
            base(callbackInstance)
        {
        }

        public CaseRequestManagerEndpoint(InstanceContext callbackInstance, ServiceEndpoint endpoint) :
            base(callbackInstance, endpoint)
        {
        }

        public CaseRequestManagerEndpoint(InstanceContext callbackInstance, String endpointConfigurationName) :
            base(callbackInstance, endpointConfigurationName)
        {
        }

        public CaseRequestManagerEndpoint(Object callbackInstance, ServiceEndpoint endpoint) :
            base(callbackInstance, endpoint)
        {
        }

        public CaseRequestManagerEndpoint(Object callbackInstance, String endpointConfigurationName) :
            base(callbackInstance, endpointConfigurationName)
        {
        }

        public CaseRequestManagerEndpoint(InstanceContext callbackInstance, Binding binding, EndpointAddress remoteAddress) :
            base(callbackInstance, binding, remoteAddress)
        {
        }

        public CaseRequestManagerEndpoint(InstanceContext callbackInstance, String endpointConfigurationName, EndpointAddress remoteAddress) :
            base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }

        public CaseRequestManagerEndpoint(InstanceContext callbackInstance, String endpointConfigurationName, String remoteAddress) :
            base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }

        public CaseRequestManagerEndpoint(Object callbackInstance, Binding binding, EndpointAddress remoteAddress) :
            base(callbackInstance, binding, remoteAddress)
        {
        }

        public CaseRequestManagerEndpoint(Object callbackInstance, String endpointConfigurationName, EndpointAddress remoteAddress) :
            base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }

        public CaseRequestManagerEndpoint(Object callbackInstance, String endpointConfigurationName, String remoteAddress) :
            base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }

        #endregion Constructors

        #region Methods

        public CaseListDataSet RequestCaseList(String wsID)
        {
            return base.Channel.RequestCaseList(wsID);
        }

        public CaseRequestMessageResponse RequestCase(CaseMessage message)
        {
            return base.Channel.RequestCase(message);
        }

        public Stream RequestCaseData (CaseDataInfo caseDataInfo)
        {
            return base.Channel.RequestCaseData(caseDataInfo);
        }

        public void UpdateCase(UpdateCaseMessage message)
        {
            base.Channel.UpdateCase(message);
        }

        public void Ping(String awsId)
        {
            base.Channel.Ping(awsId);
        }

        public LoginResponse Login(WorkstationInfo awsInfo)
        {
            return base.Channel.Login(awsInfo);
        }

        public void Logout(LogOutInfo logOutInfo)
        {
            base.Channel.Logout(logOutInfo);
        }

        public void UpdateProfile(string usersName, Profile profile)
        {
            base.Channel.UpdateProfile(usersName, profile);
        }

        public void AutoSelectEnabled(bool enabled, string workstationId)
        {
            base.Channel.AutoSelectEnabled(enabled, workstationId);
        }

        #endregion Methods
    }
}
