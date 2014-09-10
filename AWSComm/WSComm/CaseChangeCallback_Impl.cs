using System;
using L3.Cargo.Communications.Client;

namespace L3.Cargo.WSCommunications
{
    public class CaseChangeCallback_Impl : CaseManagerCallback
    {
        #region Protected Members

        protected WSServer m_WSServer;

        #endregion Protected Members


        #region Constructors

        public CaseChangeCallback_Impl(WSServer wsServer)
        {
            m_WSServer = wsServer;
        }

        #endregion Constructors


        #region Public Methods

        public override void onCaseAdded(l3.cargo.corba.XCase c)
        {
            if (m_WSServer != null)
            {
                m_WSServer.AddToCaseList(c);
            }
        }

        public override void onCaseDeleted (l3.cargo.corba.XCase c)
        {
            if (m_WSServer != null)
            {
                m_WSServer.DeleteFromCaseList(c);
            }
        }


        #endregion Public Methods
    }
}
