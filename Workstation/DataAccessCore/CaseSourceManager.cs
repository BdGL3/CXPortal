using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.ServiceModel;
using L3.Cargo.Common;
using L3.Cargo.Common.Xml.Profile_1_0;
using L3.Cargo.Communications.Client;
using L3.Cargo.Communications.Interfaces;
using L3.Cargo.Translators;
using L3.Cargo.Workstation.SystemConfigurationCore;
using L3.Cargo.Workstation.Common;

namespace L3.Cargo.Workstation.DataSourceCore
{
    public class CaseSourceManager
    {
        #region Private Members

        private string m_WorkstationId;

        private SysConfigMgrAccess m_SysConfigMgrAccess;

        private CaseSources<WSCommEndpoint> m_WSCommSources;

        private CaseSources<CaseRequestManagerEndpoint> m_ACSSources;

        private Dictionary<SourceType, CaseSourcesList> m_CaseSourceLists;

        private string _workstationMode;

        #endregion Private Members


        #region Public Members

        #endregion Public Members


        #region Constructors

        public CaseSourceManager(SysConfigMgrAccess sysConfigMgrAccess)
        {
            m_SysConfigMgrAccess = sysConfigMgrAccess;
            m_WorkstationId = sysConfigMgrAccess.GetDefaultConfig().WorkstationAlias;
            _workstationMode = m_SysConfigMgrAccess.GetDefaultConfig().WorkstationMode;

            m_CaseSourceLists = new Dictionary<SourceType, CaseSourcesList>();

            foreach (SourceType sourceType in Enum.GetValues(typeof(SourceType)))
            {
                m_CaseSourceLists.Add(sourceType, new CaseSourcesList());
            }

            m_WSCommSources =
                new CaseSources<WSCommEndpoint>(typeof(IWSComm), m_WorkstationId, SourceType.WSComm,
                    sysConfigMgrAccess.GetDefaultConfig().WcfDiscoveryProbeTimeoutPeriodSec,
                    sysConfigMgrAccess.GetDefaultConfig().EnableDiscoveryManagedModeIWSComm,
                    sysConfigMgrAccess.GetDefaultConfig().WcfDiscoveryProxyConnectionUri);

            m_WSCommSources.WcfTcpBindingReceiveTimeoutMin = sysConfigMgrAccess.GetDefaultConfig().WcfTcpBindingReceiveTimeoutMin;
            m_WSCommSources.WcfTcpBindingSendTimeoutMin = sysConfigMgrAccess.GetDefaultConfig().WcfTcpBindingSendTimeoutMin;
            m_WSCommSources.WsCommPingTimeoutMsec = sysConfigMgrAccess.GetDefaultConfig().WsCommPingTimeoutMsec;

            m_ACSSources =
                new CaseSources<CaseRequestManagerEndpoint>
                               (typeof(ICaseRequestManager), m_WorkstationId, SourceType.ArchiveCase,
                               sysConfigMgrAccess.GetDefaultConfig().WcfDiscoveryProbeTimeoutPeriodSec);

            m_ACSSources.WcfTcpBindingReceiveTimeoutMin = sysConfigMgrAccess.GetDefaultConfig().WcfTcpBindingReceiveTimeoutMin;
            m_ACSSources.WcfTcpBindingSendTimeoutMin = sysConfigMgrAccess.GetDefaultConfig().WcfTcpBindingSendTimeoutMin;



            m_WSCommSources.SourceListUpdateEvent +=
                new CaseSources<WSCommEndpoint>.SourceListUpdateHandler(SourceUpdated<WSCommEndpoint>);

            m_ACSSources.SourceListUpdateEvent +=
                new CaseSources<CaseRequestManagerEndpoint>.SourceListUpdateHandler(SourceUpdated<CaseRequestManagerEndpoint>);
        }

        #endregion Constructors


        #region Private Methods

        private void SourceUpdated<T>(string alias, bool IsAdding, bool isLoginRequired)
        {
            if (typeof(T).Equals(typeof(WSCommEndpoint)))
            {
                if (IsAdding)
                {
                    m_CaseSourceLists[SourceType.WSComm].Add(alias, isLoginRequired);
                }
                else
                {
                    m_CaseSourceLists[SourceType.WSComm].Remove(alias);
                }
            }
            else if (typeof(T).Equals(typeof(CaseRequestManagerEndpoint)))
            {
                if (IsAdding)
                {
                    m_CaseSourceLists[SourceType.ArchiveCase].Add(alias, isLoginRequired);
                }
                else
                {
                    m_CaseSourceLists[SourceType.ArchiveCase].Remove(alias);
                }
            }
        }

        private CaseSource<T> FindSource<T>(string sourceAlias)
            where T : ICaseRequestManager
        {
            try
            {
                IEnumerable<CaseSource<T>> sources;

                if (typeof(T).Equals(typeof(WSCommEndpoint)))
                {
                    sources = (IEnumerable<CaseSource<T>>)m_WSCommSources.Where(source => source.Alias.Equals(sourceAlias));
                }
                else if (typeof(T).Equals(typeof(CaseRequestManagerEndpoint)))
                {
                    sources = (IEnumerable<CaseSource<T>>)m_ACSSources.Where(source => source.Alias.Equals(sourceAlias));
                }
                else
                {
                    throw new Exception(ErrorMessages.SOURCE_TYPE_UNKNOWN);
                }

                return (CaseSource<T>)sources.FirstOrDefault();
            }
            catch (Exception)
            {
                return default(CaseSource<T>);
            }
        }

        #endregion Private Methods


        #region Public Methods

        public void StartUp()
        {
            m_WSCommSources.StartUp();
            m_ACSSources.StartUp();
        }

        public void Shutdown()
        {
            m_WSCommSources.ShutDown();
            m_ACSSources.ShutDown();
        }

        public void RequestSources(SourceType sourceType, out CaseSourcesList sourceList)
        {
            switch (sourceType)
            {
                case SourceType.ArchiveCase:
                    sourceList = m_CaseSourceLists[SourceType.ArchiveCase];
                    break;
                case SourceType.WSComm:
                    sourceList = m_CaseSourceLists[SourceType.WSComm];
                    break;
                default:
                    throw new Exception(ErrorMessages.SOURCE_TYPE_UNKNOWN);
            }
        }

        public LoginResponse Login(string sourceAlias, WorkstationInfo workstationInfo)
        {
            CaseSource<WSCommEndpoint> WSCommSource = FindSource<WSCommEndpoint>(sourceAlias);
            CaseSource<CaseRequestManagerEndpoint> ArchiveCaseSource = FindSource<CaseRequestManagerEndpoint>(sourceAlias);

            if (default(CaseSource<WSCommEndpoint>) != WSCommSource)
            {
                workstationInfo.WorkstationId = m_WorkstationId;

                try
                {
                    return WSCommSource.EndPoint.Login(workstationInfo);
                }
                catch (FaultException ex)
                {
                    throw;
                }
                catch (Exception)
                {
                    WSCommSource.EndPoint.Abort();
                    if (m_SysConfigMgrAccess.Contains(WSCommSource.Alias))
                    {
                        m_SysConfigMgrAccess.Delete(WSCommSource.Alias);
                    }
                    m_WSCommSources.RemoveSource(WSCommSource);
                    throw;
                }
            }
            else if (default(CaseSource<CaseRequestManagerEndpoint>) != ArchiveCaseSource)
            {
                workstationInfo.WorkstationId = m_WorkstationId;

                try
                {
                    return ArchiveCaseSource.EndPoint.Login(workstationInfo);
                }
                catch (FaultException ex)
                {
                    throw;
                }
                catch (Exception)
                {
                    ArchiveCaseSource.EndPoint.Abort();
                    if (m_SysConfigMgrAccess.Contains(ArchiveCaseSource.Alias))
                    {
                        m_SysConfigMgrAccess.Delete(ArchiveCaseSource.Alias);
                    }
                    m_ACSSources.RemoveSource(ArchiveCaseSource);
                    throw;
                }
            }

            throw new Exception(ErrorMessages.SOURCE_NOT_AVAILABLE);
        }

        public void GetCaseList(string sourceAlias, out DataSet caseList)
        {
            caseList = null;

            CaseSource<WSCommEndpoint> WSCommSource = FindSource<WSCommEndpoint>(sourceAlias);
            CaseSource<CaseRequestManagerEndpoint> ACSSource = FindSource<CaseRequestManagerEndpoint>(sourceAlias);

            if (default(CaseSource<WSCommEndpoint>) != WSCommSource)
            {
                caseList = WSCommSource.CaseList;
            }
            else if (default(CaseSource<CaseRequestManagerEndpoint>) != ACSSource)
            {
                caseList = ACSSource.CaseList;
            }
            else
            {
                throw new Exception(ErrorMessages.CASE_LIST_NOT_AVAILABLE);
            }
        }

        public void UpdateProfile(string sourceAlias, string userName, Profile profile)
        {
            CaseSource<WSCommEndpoint> WSCommSource = FindSource<WSCommEndpoint>(sourceAlias);
            CaseSource<CaseRequestManagerEndpoint> ACSSource = FindSource<CaseRequestManagerEndpoint>(sourceAlias);

            if (default(CaseSource<WSCommEndpoint>) != WSCommSource)
            {
                try
                {
                    WSCommSource.EndPoint.UpdateProfile(userName, profile);
                }
                catch (FaultException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    if (WSCommSource != null)
                    {
                        WSCommSource.EndPoint.Abort();
                        if (m_SysConfigMgrAccess.Contains(WSCommSource.Alias))
                        {
                            m_SysConfigMgrAccess.Delete(WSCommSource.Alias);
                        }
                        m_WSCommSources.RemoveSource(WSCommSource);
                    }
                    throw;
                }
            }
            else if (default(CaseSource<CaseRequestManagerEndpoint>) != ACSSource)
            {
                try
                {
                    ACSSource.EndPoint.UpdateProfile(userName, profile);
                }
                catch (FaultException ex)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    if (ACSSource != null)
                    {
                        ACSSource.EndPoint.Abort();
                        if (m_SysConfigMgrAccess.Contains(ACSSource.Alias))
                        {
                            m_SysConfigMgrAccess.Delete(ACSSource.Alias);
                        }
                        m_ACSSources.RemoveSource(ACSSource);
                    }
                    throw;
                }
            }
            else
            {
                throw new Exception(ErrorMessages.SOURCE_NOT_AVAILABLE);
            }
        }

        public void Logout(LogOutInfo logOutInfo)
        {
            foreach (CaseSource<WSCommEndpoint> caseSource in m_WSCommSources)
            {
                int index = m_WSCommSources.IndexOf(caseSource);

                try
                {
                    caseSource.EndPoint.Logout(logOutInfo);
                }
                catch (Exception)
                {
                    caseSource.EndPoint.Abort();
                    if (m_SysConfigMgrAccess.Contains(caseSource.Alias))
                    {
                        m_SysConfigMgrAccess.Delete(caseSource.Alias);
                    }
                    m_WSCommSources.RemoveSource(caseSource);
                    throw;
                }
            }
        }

        public CaseObject RequestCase(string sourceAlias, string caseId, bool isEditable)
        {
            CaseMessage caseMessage = new CaseMessage(caseId, m_WorkstationId);
            caseMessage.IsCaseEditable = isEditable;
            caseMessage.WorkstationMode = _workstationMode;
            CaseRequestMessageResponse response = new CaseRequestMessageResponse();

            CaseSource<WSCommEndpoint> WSCommSource = FindSource<WSCommEndpoint>(sourceAlias);
            CaseSource<CaseRequestManagerEndpoint> ACSSource = FindSource<CaseRequestManagerEndpoint>(sourceAlias);

            if (default(CaseSource<WSCommEndpoint>) != WSCommSource)
            {
                try
                {
                    response = WSCommSource.EndPoint.RequestCase(caseMessage);
                }
                catch (FaultException ex)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    WSCommSource.EndPoint.Abort();
                    if (m_SysConfigMgrAccess.Contains(WSCommSource.Alias))
                    {
                        m_SysConfigMgrAccess.Delete(WSCommSource.Alias);
                    }
                    m_WSCommSources.RemoveSource(WSCommSource);

                    throw;
                }
            }
            else if (default(CaseSource<CaseRequestManagerEndpoint>) != ACSSource)
            {
                try
                {
                    response = ACSSource.EndPoint.RequestCase(caseMessage);
                }
                catch (FaultException ex)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    ACSSource.EndPoint.Abort();
                    if (m_SysConfigMgrAccess.Contains(ACSSource.Alias))
                    {
                        m_SysConfigMgrAccess.Delete(ACSSource.Alias);
                    }
                    m_ACSSources.RemoveSource(ACSSource);
                    throw;
                }
            }
            else
            {
                throw new Exception(ErrorMessages.SOURCE_NOT_AVAILABLE);
            }

            CaseObject tmpCaseObj = CaseTranslator.Translate(response.file);

            if (response.AdditionalFiles != null)
            {
                foreach (KeyValuePair<FileType, String> file in response.AdditionalFiles)
                {
                    DataAttachment attach = new DataAttachment();
                    attach.attachmentId = file.Value;
                    switch (file.Key)
                    {
                        case FileType.AnalysisHistory:
                            attach.attachmentType = AttachmentType.AnalysisHistory;
                            break;
                        case FileType.FTIFile:
                            attach.attachmentType = AttachmentType.FTIImage;
                            break;
                        default:
                            attach.attachmentType = AttachmentType.Unknown;
                            break;
                    }
                    tmpCaseObj.attachments.Add(attach);
                }
            }

            tmpCaseObj.caseType = (L3.Cargo.Common.CaseType)response.caseType;

            //Case is editable if it is a primary case and case source specidies result (Decision) is enabled.
            tmpCaseObj.IsCaseEditable = (isEditable && response.IsResultEnabled);

            return tmpCaseObj;
        }

        public Stream RequestFile(string sourceAlias, string caseId, string filename, FileType filetype)
        {
            CaseDataInfo caseDataInfo = new CaseDataInfo(caseId, filename, filetype);

            CaseSource<WSCommEndpoint> WSCommSource = FindSource<WSCommEndpoint>(sourceAlias);
            CaseSource<CaseRequestManagerEndpoint> ACSSource = FindSource<CaseRequestManagerEndpoint>(sourceAlias);

            if (default(CaseSource<WSCommEndpoint>) != WSCommSource)
            {
                try
                {
                    return WSCommSource.EndPoint.RequestCaseData(caseDataInfo);
                }
                catch (FaultException ex)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    WSCommSource.EndPoint.Abort();
                    if (m_SysConfigMgrAccess.Contains(WSCommSource.Alias))
                    {
                        m_SysConfigMgrAccess.Delete(WSCommSource.Alias);
                    }
                    m_WSCommSources.RemoveSource(WSCommSource);

                    throw;
                }
            }
            else if (default(CaseSource<CaseRequestManagerEndpoint>) != ACSSource)
            {
                try
                {
                    return ACSSource.EndPoint.RequestCaseData(caseDataInfo);
                }
                catch (FaultException ex)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    ACSSource.EndPoint.Abort();
                    if (m_SysConfigMgrAccess.Contains(ACSSource.Alias))
                    {
                        m_SysConfigMgrAccess.Delete(ACSSource.Alias);
                    }
                    m_ACSSources.RemoveSource(ACSSource);

                    throw;
                }
            }
            else
            {
                throw new Exception(ErrorMessages.SOURCE_NOT_AVAILABLE);
            }
        }

        public void GetManifestList(string sourceAlias, out ObservableCollectionEx<string> manifestList)
        {
            CaseSource<WSCommEndpoint> WSCommSource = FindSource<WSCommEndpoint>(sourceAlias);

            if (default(CaseSource<WSCommEndpoint>) != WSCommSource)
            {
                try
                {
                    WSCommSource.GetManifestList(out manifestList);
                }
                catch (FaultException ex)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    WSCommSource.EndPoint.Abort();
                    if (m_SysConfigMgrAccess.Contains(WSCommSource.Alias))
                    {
                        m_SysConfigMgrAccess.Delete(WSCommSource.Alias);
                    }
                    m_WSCommSources.RemoveSource(WSCommSource);

                    throw;
                }
            }
            else
            {
                throw new Exception(ErrorMessages.SOURCE_NOT_AVAILABLE);
            }
        }

        public void UpdateCase(string sourceAlias, string caseId, CaseUpdateEnum type, string filename, Stream file, AttachFileTypeEnum attachFileType,
            WorkstationResult result, string ContainerNum, string UserName, string CreateTime, L3.Cargo.Communications.Interfaces.CaseType caseType)
        {
            UpdateCaseMessage updateCaseMessage = new UpdateCaseMessage(caseId, type, caseType);

            if (type == CaseUpdateEnum.AttachFile)
            {
                updateCaseMessage.AttachFileType = attachFileType;
                updateCaseMessage.File = file;
                updateCaseMessage.Filename = filename;
                updateCaseMessage.UserName = UserName;
            }

            updateCaseMessage.WorkstationId = m_SysConfigMgrAccess.GetDefaultConfig().WorkstationAlias;
            updateCaseMessage.CaseId = caseId;
            updateCaseMessage.ObjectId = ContainerNum;
            updateCaseMessage.workstationResult = result;
            updateCaseMessage.Type = type;
            updateCaseMessage.CreateTime = CreateTime;
            updateCaseMessage.UserName = UserName;

            CaseSource<WSCommEndpoint> WSCommSource = FindSource<WSCommEndpoint>(sourceAlias);
            CaseSource<CaseRequestManagerEndpoint> ACSSource = FindSource<CaseRequestManagerEndpoint>(sourceAlias);

            if (default(CaseSource<WSCommEndpoint>) != WSCommSource)
            {
                try
                {
                    WSCommSource.EndPoint.UpdateCase(updateCaseMessage);
                }
                catch (FaultException ex)
                {
                    throw;
                }
                catch (Exception)
                {
                    WSCommSource.EndPoint.Abort();
                    if (m_SysConfigMgrAccess.Contains(WSCommSource.Alias))
                    {
                        m_SysConfigMgrAccess.Delete(WSCommSource.Alias);
                    }
                    m_WSCommSources.RemoveSource(WSCommSource);
                    throw;
                }
            }
            else if (default(CaseSource<CaseRequestManagerEndpoint>) != ACSSource)
            {
                try
                {
                    ACSSource.EndPoint.UpdateCase(updateCaseMessage);
                }
                catch (FaultException ex)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    ACSSource.EndPoint.Abort();
                    if (m_SysConfigMgrAccess.Contains(ACSSource.Alias))
                    {
                        m_SysConfigMgrAccess.Delete(ACSSource.Alias);
                    }
                    m_ACSSources.RemoveSource(ACSSource);
                    throw;
                }
            }
            else
            {
                throw new Exception(ErrorMessages.SOURCE_NOT_AVAILABLE);
            }

        }

        public void AutoSelectEnabled(bool enabled)
        {
            foreach (CaseSource<WSCommEndpoint> caseSource in m_WSCommSources)
            {
                try
                {
                    caseSource.EndPoint.AutoSelectEnabled(enabled, m_WorkstationId);
                }
                catch (Exception)
                {
                    caseSource.EndPoint.Abort();
                    if (m_SysConfigMgrAccess.Contains(caseSource.Alias))
                    {
                        m_SysConfigMgrAccess.Delete(caseSource.Alias);
                    }
                    m_WSCommSources.RemoveSource(caseSource);
                    throw;
                }
            }
        }

        #endregion Public Methods
    }
}