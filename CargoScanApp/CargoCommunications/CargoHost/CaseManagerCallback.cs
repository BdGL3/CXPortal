using System;
using l3.cargo.corba;

namespace L3.Cargo.Communications.CargoHost
{
    public class CaseManagerCallback : MarshalByRefObject, CaseChangeListener
    {
        #region Public Methods

        public override Object InitializeLifetimeService()
        {
            return null;
        }

        public virtual void onCaseAdded(XCase c)
        {
        }

        public virtual void onCaseDeleted (XCase c)
        {
        }

        public virtual void onCaseUpdated (XCase c)
        {
        }

        #endregion Public Methods
    }
}
