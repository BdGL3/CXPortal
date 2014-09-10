using System;
using System.Data;
using System.Threading;
using L3.Cargo.Common;
using L3.Cargo.Common.Xml.Profile_1_0;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Interfaces;
using L3.Cargo.Translators;
using L3.Cargo.Workstation.CaseHandlerCore;
using L3.Cargo.Workstation.DataSourceCore;
using L3.Cargo.Workstation.SystemConfigurationCore;
using L3.Cargo.Workstation.Common;

namespace L3.Cargo.Workstation.SystemManagerCore
{
    public class SysModeManager
    {
        #region Private Members

        private SysConfigMgrAccess m_SysConfig;

        private DataSourceAccess m_DataSourceAccess;

        private CaseHandler m_caseHandler;

        //private string m_PreviousCaseId;

        private string m_LinkedCaseID;

        private string m_LinkedCaseSource;

        #endregion Private Members


        #region Public Members

        public delegate void CaseAvailableEventHandler();

        public EventWaitHandle CaseAvailableEvent = new AutoResetEvent(false);

        #endregion Public Members


        #region Constructors

        internal SysModeManager (SysConfigMgrAccess sysConfig, DataSourceAccess dataSourceAccess, CaseHandler handler)
        {
            m_SysConfig = sysConfig;
            m_DataSourceAccess = dataSourceAccess;
            m_caseHandler = handler;
            m_LinkedCaseID = string.Empty;
            m_LinkedCaseSource = string.Empty;
            //m_PreviousCaseId = string.Empty;

            //register an event handler with System Configuration Manager to get notified
            //when configuration is modified.
        }

        #endregion Constructors


        #region Private Methods

        private AuthenticationLevel LoginToIndividualSource (string sourceAlias, string username, string password)
        {
            AuthenticationLevel ret = AuthenticationLevel.None;

            try
            {
                string wsId = m_SysConfig.GetDefaultConfig().WorkstationAlias;
                UserInfo userInfo = new UserInfo(username, password);
                WorkstationInfo wsInfo = new WorkstationInfo(wsId, userInfo);

                LoginResponse loginResponse = m_DataSourceAccess.Login(sourceAlias, wsInfo);

                ret = loginResponse.UserAuthenticationLevel;

                if (!ret.Equals(AuthenticationLevel.None))
                {
                    SysConfiguration sysConfig = new SysConfiguration();
                    sysConfig.ID = sourceAlias;
                    sysConfig.ContainerDBConnectionString = loginResponse.systemConfiguration.ContainerDBConnectString;
                    sysConfig.ContainerRefreshPeriodmsecs = loginResponse.systemConfiguration.ContainerRefreshPeriodSeconds * 1000;

                    if (m_SysConfig.Contains(sourceAlias))
                    {
                        m_SysConfig.Delete(sourceAlias);
                    }

                    m_SysConfig.Add(sysConfig);

                    if (m_SysConfig.UserProfileManager.Profile == null)
                    {
                        ProfileObject profile = ProfileTranslator.Translate(loginResponse.UserProfile, 4);
                        profile.SourceAlias = sourceAlias;
                        profile.UserName = wsInfo.userInfo.UserName;
                        profile.Password = wsInfo.userInfo.Password;

                        m_SysConfig.UserProfileManager.Profile = profile;
                        m_SysConfig.UserProfileManager.Profile.ProfileUpdatedEvent += new ProfileUpdated(ProfileUpdated);
                    }

                    DataSet caselist = null;
                    m_DataSourceAccess.GetCaseList(sourceAlias, out caselist);

                    if (caselist != null)
                    {
                        CaseListDataSet caseListDataSet = (CaseListDataSet)caselist;
                        caseListDataSet.CaseListTable.CaseListTableRowChanged +=
                            new CaseListDataSet.CaseListTableRowChangeEventHandler(CaseListTable_CaseListTableRowChanged);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return ret;
        }

        private CaseSourcesObject FindSource (string name)
        {
            CaseSourcesObject ret = null;

            CaseSourcesList wsCommSourceList;
            m_DataSourceAccess.RequestSources(SourceType.WSComm, out wsCommSourceList);

            foreach (CaseSourcesObject caseSource in wsCommSourceList)
            {
                if (caseSource.Name.Equals(name))
                {
                    ret = caseSource;
                    break;
                }
            }

            if (ret == null)
            {
                CaseSourcesList acsSourceList;
                m_DataSourceAccess.RequestSources(SourceType.ArchiveCase, out acsSourceList);

                foreach (CaseSourcesObject caseSource in acsSourceList)
                {
                    if (caseSource.Name.Equals(name))
                    {
                        ret = caseSource;
                        break;
                    }
                }
            }

            return ret;
        }


        private void LoginSources (string username, string password)
        {
            CaseSourcesList wsCommSourceList;
            m_DataSourceAccess.RequestSources(SourceType.WSComm, out wsCommSourceList);

            foreach (CaseSourcesObject caseSourceObj in wsCommSourceList)
            {
                if (!caseSourceObj.IsLoggedIn)
                {
                    try
                    {
                        AuthenticationLevel authLevel = LoginToIndividualSource(caseSourceObj.Name, username, password);
                        if (!authLevel.Equals(AuthenticationLevel.None))
                        {
                            caseSourceObj.IsLoggedIn = true;
                        }
                        else
                        {
                            caseSourceObj.IsLoggedIn = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        caseSourceObj.IsLoggedIn = false;
                    }
                }
            }

            CaseSourcesList acsCommSourceList;
            m_DataSourceAccess.RequestSources(SourceType.ArchiveCase, out acsCommSourceList);

            foreach (CaseSourcesObject caseSourceObj in acsCommSourceList)
            {
                if (!caseSourceObj.IsLoggedIn)
                {
                    try
                    {
                        AuthenticationLevel authLevel = LoginToIndividualSource(caseSourceObj.Name, username, password);
                        if (!authLevel.Equals(AuthenticationLevel.None))
                        {
                            caseSourceObj.IsLoggedIn = true;
                        }
                        else
                        {
                            caseSourceObj.IsLoggedIn = false;
                        }
                    }
                    catch
                    {
                        caseSourceObj.IsLoggedIn = false;
                    }
                }
            }
        }

        #endregion Private Methods


        #region Internal Methods

        internal void AutoSelectCase(out CaseObject caseObj)
        {
            CaseSourcesList wsCommSources;
            m_DataSourceAccess.RequestSources(SourceType.WSComm, out wsCommSources);

            string caseId = string.Empty;
            string caseSource = string.Empty;

            caseObj = null;

            try
            {
                //select case from caselist in a FIFO format using updateTime field of case information.   
                if (!String.IsNullOrWhiteSpace(m_LinkedCaseID))
                {
                    caseId = m_LinkedCaseID;
                    caseSource = m_LinkedCaseSource;
                    m_LinkedCaseID = string.Empty;
                    m_LinkedCaseSource = string.Empty;

                    m_caseHandler.GetCase(caseSource, caseId, out caseObj, true);
                }
                else
                {
                    DataSet caseList;

                    if (wsCommSources.Count == 0)
                    {
                        throw new Exception(ErrorMessages.NO_LIVE_SOURCES);
                    }                    

                    foreach (CaseSourcesObject caseSourceObj in wsCommSources)
                    {
                        if (caseSourceObj.IsLoggedIn)
                        {
                            //for auto select let the case source select case to be displayed, for this set caseId to empty string

                            //first find out whether this source has any live cases
                            GetCaseList(caseSourceObj.Name, out caseList);

                            if (caseList.Tables[0].Rows.Count > 0)
                            {
                                caseId = string.Empty;
                                caseSource = caseSourceObj.Name;

                                //found a case source to request a case
                                try
                                {
                                    try
                                    {
                                        m_caseHandler.GetCase(caseSource, caseId, out caseObj, true);                                       
                                    }
                                    catch
                                    {
                                        throw;
                                    }
                                    m_LinkedCaseID = caseObj.LinkedCaseId;
                                    m_LinkedCaseSource = caseSource;
                                }
                                catch (Exception exp)
                                {
                                    if (exp.Message == ErrorMessages.NO_LIVE_CASE)
                                    {
                                        //if this is the last entry then exit the foreach loop
                                        //otherwise continue checking other sources for livecases if the
                                        //current source does not have any live cases.
                                        if (wsCommSources.IndexOf(caseSourceObj) == wsCommSources.Count-1)
                                        {
                                            throw;
                                        }
                                    }
                                    else
                                    {
                                        throw;
                                    }
                                }
                            }                            
                        }
                    }                   
                }                
            }
            catch (Exception ex)
            {
                m_LinkedCaseID = string.Empty;
                m_LinkedCaseSource = string.Empty;
                throw;
            }
        }

        internal void Shutdown()
        {
            m_DataSourceAccess.Shutdown();
        }

        internal void RequestSources (SourceType srcType, out CaseSourcesList list)
        {
            try
            {
                m_DataSourceAccess.RequestSources(srcType, out list);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal void Login (string sourceAlias, string username, string password)
        {
            try
            {
                //See if we can log into the selected source.
                AuthenticationLevel authLevel = LoginToIndividualSource(sourceAlias, username, password);

                if (authLevel != AuthenticationLevel.None)
                {
                    CaseSourcesObject caseSource = FindSource(sourceAlias);

                    if (caseSource != null)
                    {
                        caseSource.IsLoggedIn = true;
                    }

                    //If we are successful in logging in, spin off a thread to try the login with other sources.
                    Thread loginSourcesThread = new Thread(new ParameterizedThreadStart(delegate { LoginSources(username, password); }));
                    loginSourcesThread.Start();
                }
            }
            catch (Exception exp)
            {
                //TODO: Log error into error log whenever we finally do that.
                throw exp;
            }
        }

        internal void ProfileUpdated()
        {
            ProfileObject profileObj = m_SysConfig.UserProfileManager.Profile;

            if (profileObj != null)
            {
                Profile profile = ProfileTranslator.Translate(profileObj);
                m_DataSourceAccess.UpdateProfile(profileObj.SourceAlias, profileObj.UserName, profile);
            }
        }

        internal void Logout ()
        {
        }

        internal void GetCaseList (string source, out DataSet list)
        {
            //obtain requested case list from the Data Access Layer
            m_DataSourceAccess.GetCaseList(source, out list);            
        }

        internal void GetCase(string source, string CaseID, out CaseObject caseObj, bool IsCaseEditable)
        {
            try
            {
                //m_PreviousCaseId = string.Empty;
                //obtain case information from CaseHandler
                m_caseHandler.GetCase(source, CaseID, out caseObj, IsCaseEditable);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal void CloseCase (string caseId, CaseUpdateEnum updateType)
        {
            try
            {
                //CaseHandler close case
                m_caseHandler.CloseCase(caseId, updateType);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal void AutoSelectEnabled(bool enabled)
        {
            try
            {
                m_DataSourceAccess.AutoSelectEnabled(enabled);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Internal Methods


        #region Public Methods

        public void CaseListTable_CaseListTableRowChanged (object sender, L3.Cargo.Communications.Common.CaseListDataSet.CaseListTableRowChangeEvent e)
        {
            //notify presentation layer
            CaseAvailableEvent.Set();
        }

        #endregion Public Methods
    }
}
