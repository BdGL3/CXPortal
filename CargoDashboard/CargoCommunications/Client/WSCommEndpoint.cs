using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using L3.Cargo.Common;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Interfaces;
using L3.Cargo.Common.Xml.Profile_1_0;


namespace L3.Cargo.Communications.Client
{
    public class WSCommEndpoint : DuplexClientBase<IWSComm>, IWSComm
    {
        #region Constructors

        public WSCommEndpoint(InstanceContext callbackInstance) :
            base(callbackInstance)
        {
        }

        public WSCommEndpoint(Object callbackInstance) :
            base(callbackInstance)
        {
        }

        public WSCommEndpoint(InstanceContext callbackInstance, ServiceEndpoint endpoint) :
            base(callbackInstance, endpoint)
        {
        }

        public WSCommEndpoint(InstanceContext callbackInstance, String endpointConfigurationName) :
            base(callbackInstance, endpointConfigurationName)
        {
        }

        public WSCommEndpoint(Object callbackInstance, ServiceEndpoint endpoint) :
            base(callbackInstance, endpoint)
        {
        }

        public WSCommEndpoint(Object callbackInstance, String endpointConfigurationName) :
            base(callbackInstance, endpointConfigurationName)
        {
        }

        public WSCommEndpoint(InstanceContext callbackInstance, Binding binding, EndpointAddress remoteAddress) :
            base(callbackInstance, binding, remoteAddress)
        {
        }

        public WSCommEndpoint(InstanceContext callbackInstance, String endpointConfigurationName, EndpointAddress remoteAddress) :
            base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }

        public WSCommEndpoint(InstanceContext callbackInstance, String endpointConfigurationName, String remoteAddress) :
            base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }

        public WSCommEndpoint(Object callbackInstance, Binding binding, EndpointAddress remoteAddress) :
            base(callbackInstance, binding, remoteAddress)
        {
        }

        public WSCommEndpoint(Object callbackInstance, String endpointConfigurationName, EndpointAddress remoteAddress) :
            base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }

        public WSCommEndpoint(Object callbackInstance, String endpointConfigurationName, String remoteAddress) :
            base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }

        #endregion Constructors

        #region Methods

        public LoginResponse Login(WorkstationInfo awsInfo)
        {
            return base.Channel.Login(awsInfo);
        }

        public void Logout(LogOutInfo logOutInfo)
        {
            base.Channel.Logout(logOutInfo);
        }

        public void UpdateProfile (string usersName, Profile profile)
        {
            base.Channel.UpdateProfile(usersName, profile);
        }

        public CaseListDataSet RequestCaseList (String AwsID)
        {
            return base.Channel.RequestCaseList(AwsID);
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

        public void AutoSelectEnabled(bool enabled, string workstationId)
        {
            base.Channel.AutoSelectEnabled(enabled, workstationId);
        }

        #endregion Methods
    }
}
