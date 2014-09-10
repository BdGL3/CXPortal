using L3.Cargo.Communications.Interfaces;

namespace L3.Cargo.Communications.Client
{
    //------------------------------------------------------------------------------------
    // TODO: Put custom code here to process the Client side of the WCF connection.
    //------------------------------------------------------------------------------------
    public class CaseRequestManagerCallback: ICaseRequestManagerCallback
    {
        public virtual void UpdatedCaseList(CaseListUpdate listupdate)
        {
        }
    }
}
