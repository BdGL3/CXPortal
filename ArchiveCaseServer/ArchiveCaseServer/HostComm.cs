using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using omg.org.CosNaming;
using Ch.Elca.Iiop;
using l3.cargo.corba;
using System.Runtime.Remoting.Channels;
using Ch.Elca.Iiop.Services;
using omg.org.CORBA;

namespace L3.Cargo.ArchiveCaseServer
{
    public class HostComm
    {
        #region Private Memebers

        private Thread HostConnThread;

        private NameComponent[] m_ncHost = new NameComponent[]{ new NameComponent("cargo", "context"), 
																new NameComponent("host", "object")};

        static IiopChannel m_Channel;

        static Host m_Host;

        static NamingContext m_NameService;

        private bool m_Shutdown = false;

        #endregion

        #region Public Members

        public struct ResultStruct
        {
            public uint AnalysisTime;
            public String Comment;
            public String RenderTime;
            public ResultDecision Decision;
            public String Reason;
            public String UserId;
        };

        public delegate void ConnectedToHostHandler(Boolean connected);
        public event ConnectedToHostHandler ConnectedToHostEvent;

        public delegate void CaseListUpdatedHandler(l3.cargo.corba.XCase xCase, Boolean RemoveCase);
        public event CaseListUpdatedHandler CaseListUpdatedEvent;
        

        #endregion

        #region Constructors

        public HostComm()
        {
            String caseManagerIP = (string)ConfigurationManager.AppSettings["host"];
            Int32 caseManagerPort = Int32.Parse(ConfigurationManager.AppSettings["port"]);            

            //connected with host manager
            m_Channel = new IiopChannel(0);
            ChannelServices.RegisterChannel(m_Channel, false);

            CorbaInit m_Init = CorbaInit.GetInit();
            m_NameService = m_Init.GetNameService(caseManagerIP, (int)caseManagerPort);           

            HostConnThread = new Thread(new ThreadStart(HostConnState));                       
        }

        #endregion

        #region Public Methods


        public void StartUp()
        {
            if (!HostConnThread.IsAlive)
            {
                HostConnThread.Start();
            }            
        }      

        /// <summary>
        /// ShutDown.  This interface function Shutdown CM
        /// 
        ///	Arguments:
        ///		none
        ///	Exceptions:
        ///		none
        ///	Return:
        ///		none
        /// </summary>
        public void ShutDown()
        {            
            try
            {
                HostConnThread.Abort();                
            }
            catch (CargoException)
            {
            }
            catch (TRANSIENT)
            {
            }
        }

        /// <summary>
        /// Description:
        ///     none
        ///	Arguments:
        ///		none
        ///	Exceptions:
        ///		none
        ///	Return:
        ///		none
        /// </summary>
        public Boolean IsHostAvailable()
        {
            Boolean result = false;
            if (GetHost() != null)
            {
                try
                {
                    m_Host.isCorrectVersion(VersionLabel.ConstVal);
                    result = true;
                }
                catch
                {
                }
            }

            return result;
        }        

        /// <summary>
        /// Description:
        ///     none
        ///	Arguments:
        ///		none
        ///	Exceptions:
        ///		none
        ///	Return:
        ///		none
        /// </summary>
        public AuthenticationLevel Login(String Username, String Password)
        {           
            AuthenticationLevel authLevel = m_Host.Login(Username, Password);

            if (authLevel.Equals(AuthenticationLevel.NONE))
            {
                throw new Exception("Invalid Login");
            }

            return authLevel;
        }

        /// <summary>
        /// Description:
        ///     none
        ///	Arguments:
        ///		none
        ///	Exceptions:
        ///		none
        ///	Return:
        ///		none
        /// </summary>
        public void AWSLogOut(String aws)
        {
           
        }

        /// <summary>
        /// Description:
        ///     none
        ///	Arguments:
        ///		none
        ///	Exceptions:
        ///		none
        ///	Return:
        ///		none
        /// </summary>
        public int GetMaxManifestPerCase()
        {
            try
            {
                l3.cargo.corba.CaseManager caseMgr = GetCaseManager();
                return caseMgr.getMaxManifestPerCase();
            }
            catch (NullReferenceException exp)
            {
                // Log the error
                throw exp;
            }
        }

        #endregion

        #region Private Methods

        private void HostConnState()
        {
            Boolean IsConnected = false;

            while (!m_Shutdown)
            {
                if (IsHostAvailable())
                {
                    if (!IsConnected)
                    {
                        m_Host = GetHost();
                        IsConnected = true;
                        ConnectedToHostEvent(true);
                    }
                }
                else
                {
                    IsConnected = false;
                    ConnectedToHostEvent(false);
                }

                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// GetHost.  This helper function gets Host object from CargoHost module
        /// 
        ///	Arguments:
        ///		none
        ///	Exceptions:
        ///		none
        ///	Return:
        ///		Host
        /// </summary>
        protected Host GetHost()
        {
            try
            {
                m_Host = (Host)m_NameService.resolve(m_ncHost);
            }
            catch (CargoException)
            {
                return null;
            }
            catch (omg.org.CORBA.INTERNAL)
            {
                return null;
            }

            catch (System.Reflection.TargetInvocationException)
            {
                return null;
            }
            catch (AbstractCORBASystemException)
            {
                return null;
            }
                       
            return m_Host;
        }

        /// <summary>
        /// Description:
        ///     none
        ///	Arguments:
        ///		none
        ///	Exceptions:
        ///		none
        ///	Return:
        ///		none
        /// </summary>
        protected l3.cargo.corba.CaseManager GetCaseManager()
        {
            m_Host = GetHost();

            l3.cargo.corba.CaseManager m_CaseMgr = m_Host.getCaseManager();

            return m_CaseMgr;
        }
       
        #endregion        
    }
}
