using System;
using System.ServiceModel;
using L3.Cargo.Common;
using L3.Cargo.Communications.Interfaces;

namespace L3.Cargo.Communications.Host
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class TipManagerComm : ITipManager
    {
        public virtual void ProcessedCase(string alias, string caseId)
        {
            throw new NotImplementedException(ErrorMessages.INVALID_FUNCTION);
        }

        public virtual void TipResult (string tipFile, WorkstationResult workstationResult)
        {
            throw new NotImplementedException(ErrorMessages.INVALID_FUNCTION);
        }
    }

}
