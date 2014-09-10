using System;
using System.Data;
using L3.Cargo.Common;
using L3.Cargo.Communications.Interfaces;
using L3.Cargo.Workstation.Common;

namespace L3.Cargo.Workstation.SystemManagerCore
{
    public class SystemManagerAccess
    {
        #region Private Members

        private SysModeManager m_SysModeMgr;

        #endregion


        #region Constructors

        public SystemManagerAccess (SysModeManager sysModeMgr)
        {
            try
            {
                m_SysModeMgr = sysModeMgr;
            }
            catch
            {
                throw;
            }
        }
        #endregion 


        #region Public Methods

        public void WaitForAvailableCase(int timeoutInMsec)
        {
            m_SysModeMgr.CaseAvailableEvent.WaitOne(timeoutInMsec);
        }

        public void Login(string sourceAlias, string userName, string password)
        {
            try
            {
                m_SysModeMgr.Login(sourceAlias, userName, password);
            }
            catch
            {
                throw;
            }
        }

        public void Logout()
        {
            try
            {
                m_SysModeMgr.Logout();
            }
            catch
            {
                throw;
            }
        }

        public void CloseCase(string caseID, CaseUpdateEnum updateType)
        {
            try
            {
                m_SysModeMgr.CloseCase(caseID, updateType);
            }
            catch
            {
                throw;
            }
        }

        public void AutoSelectCase (out CaseObject caseObj)
        {
            try
            {
                m_SysModeMgr.AutoSelectCase(out caseObj);
            }
            catch
            {
                throw;
            }
        }

        public void GetCase(string source, string caseID, out CaseObject caseObj, bool IsCaseEditable)
        {
            try
            {
                m_SysModeMgr.GetCase(source, caseID, out caseObj, IsCaseEditable);
            }
            catch
            {
                throw;
            }
        }

        public void GetCaseList(string source, out DataSet list)
        {
            try
            {
                m_SysModeMgr.GetCaseList(source, out list);
            }
            catch
            {
                throw;
            }
        }

        public void RequestSources(SourceType srcType, out CaseSourcesList list)
        {
            try
            {
                m_SysModeMgr.RequestSources(srcType, out list);
            }
            catch
            {
                throw;
            }
        }

        public void Shutdown()
        {
            m_SysModeMgr.Shutdown();
        }

        public void AutoSelectEnabled(bool enabled)
        {
            m_SysModeMgr.AutoSelectEnabled(enabled);
        }

        #endregion Public Methods
    }
}
