using L3.Cargo.Communications.Client;

namespace L3.Cargo.WSCommunications
{
    public class AnalystCallback_Impl : WSManagerCallback
    {
        #region Private Members

        private WSServer m_WSServer;

        #endregion Private Members


        #region Constructors

        public AnalystCallback_Impl(WSServer wsServer) :
            base()
        {
            m_WSServer = wsServer;
        }

        #endregion Constructors


        #region Public Methods

        public override void onCaseAdded(l3.cargo.corba.XCase xc)
        {
            if (m_WSServer != null)
            {
                m_WSServer.ModifyCaseList(xc);
            }

            base.onCaseAdded(xc);
        }

        #endregion Public Methods
    }
}
