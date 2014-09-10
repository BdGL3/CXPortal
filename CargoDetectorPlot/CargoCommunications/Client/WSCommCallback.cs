using System;
using L3.Cargo.Common;
using L3.Cargo.Communications.Interfaces;

namespace L3.Cargo.Communications.Client
{
    //------------------------------------------------------------------------------------
    // TODO: Put custom code here to process the Client side of the WCF connection.
    //------------------------------------------------------------------------------------
    public class WSCommCallback : CaseRequestManagerCallback, IWSCommCallback
    {
        public virtual void UpdatedManifestList(ManifestListUpdate listupdate)
        {
            throw new NotImplementedException(ErrorMessages.INVALID_FUNCTION);
        }
    }
}
