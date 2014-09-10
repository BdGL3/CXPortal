using System;
using System.IO;
using System.ServiceModel;
using L3.Cargo.Communications.Common;
using L3.Cargo.Common;
using L3.Cargo.Common.Xml.Profile_1_0;

namespace L3.Cargo.Communications.Interfaces
{
    [ServiceContract(Namespace = "CargoCommInterfacesV1.0", CallbackContract = typeof(ICaseRequestManagerCallback))]
    public interface ICaseRequestManager
    {
        // TODO: Description of the current Interface Call
        [OperationContract]
        CaseListDataSet RequestCaseList(String AwsID);

        // TODO: Description of the current Interface Call
        [OperationContract]
        CaseRequestMessageResponse RequestCase(CaseMessage message);

        // TODO: Description of the current Interface Call
        [OperationContract]
        Stream RequestCaseData (CaseDataInfo caseDataInfo);

        // TODO: Description of the current Interface Call
        //[OperationContract(Name = "UpdateCaseProperties")]
        [OperationContract]
        void UpdateCase(UpdateCaseMessage message);

        // TODO: Description of the current Interface Call
        [OperationContract]
        void Ping(String awsId);

        // TODO: Description of the current Interface Call
        [OperationContract]
        LoginResponse Login(WorkstationInfo awsInfo);

        // TODO: Description of the current Interface Call
        [OperationContract]
        void Logout(LogOutInfo logOutInfo);

        // TODO: Description of the current Interface Call
        [OperationContract]
        void UpdateProfile(string usersName, Profile profile);

        [OperationContract]
        void AutoSelectEnabled(bool enabled, string workstationId);

    }
}
