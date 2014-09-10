using System;
using L3.Cargo.Common;
using L3.Cargo.Communications.Interfaces;

namespace L3.Cargo.Communications.Client
{
    public class TipManagerCallback : ITipManagerCallback
    {
        public virtual void InjectTip(TIPInjectFileMessage tipInjectFileMessage)
        {
            throw new NotImplementedException(ErrorMessages.INVALID_FUNCTION);
        }
    }
}
