using System;
using System.Collections.Generic;
using System.IO;
using L3.Cargo.Common;
using L3.Cargo.Communications.Interfaces;
using L3.Cargo.Translators;
using L3.Cargo.Workstation.DataSourceCore;
using L3.Cargo.Workstation.SystemConfigurationCore;

namespace L3.Cargo.Workstation.CaseHandlerCore
{
    public class CaseHandler
    {
        #region Private Members

        private SysConfigMgrAccess m_SysConfigMgrAccess;

        private DataSourceAccess m_dataSourceAccess;

        private CaseCollection m_caseCollection;

        #endregion Private Members


        #region Constructors

        public CaseHandler (SysConfigMgrAccess sysConfigMgrAccess, DataSourceAccess dataAccessLayer)
        {
            m_SysConfigMgrAccess = sysConfigMgrAccess;
            m_dataSourceAccess = dataAccessLayer;
            m_caseCollection = new CaseCollection();
        }

        #endregion Constructors


        #region Public Members

        public void GetCase(string source, string caseID, out CaseObject caseObj, bool IsCaseEditable)
        {
            //request DataSourceAccess to return translated CaseObj.
            caseObj = m_dataSourceAccess.RequestCase(source, caseID, IsCaseEditable);

            //send RequestFile message to DataSourceAccess to retrieve additional information
            //that is part of the CaseObj           
            if (caseObj.attachments.Count > 0)
            {
                foreach (DataAttachment xCaseAttachment in caseObj.attachments)
                {
                    FileType fileType = FileType.None;
                    if (xCaseAttachment.attachmentType == AttachmentType.FTIImage)
                    {
                        fileType = FileType.FTIFile;
                    }

                    Stream FileData = m_dataSourceAccess.RequestFile(source, caseObj.CaseId, xCaseAttachment.attachmentId, fileType);
                    xCaseAttachment.attachmentData = new MemoryStream();
                    FileData.CopyTo(xCaseAttachment.attachmentData);
                    xCaseAttachment.attachmentData.Seek(0, SeekOrigin.Begin);
                }
            }

            foreach (DataAttachment attach in caseObj.attachments.GetEventHistoryAttachments())
            {
                caseObj.EventRecords = new List<CaseObject.CaseEventRecord>();
                caseObj.EventRecords = EventHistoryTranslator.Translate(attach.attachmentData);
            }

            foreach (DataAttachment analysisHistory in caseObj.attachments.GetHistoryAttachments())
            {
                caseObj.CaseHistories = HistoryTranslator.Translate(analysisHistory);
            }

            caseObj.SourceAlias = source;

            //add case to the case collection
            m_caseCollection.Add(caseObj);
        }

        public void CloseCase (string caseID, CaseUpdateEnum updateType)
        {
            CaseObject caseObject = m_caseCollection.Find(caseID);

            if (caseObject != null)
            {
                if (updateType == CaseUpdateEnum.Result)
                {
                    UpdateCaseResult(caseObject);
                }
                else if (updateType == CaseUpdateEnum.ReleaseCase)
                {
                    //Update modified Addattachment, SetAsReference, ObjectID and result and then Release the Case
                    //works with live case only
                    UpdateCaseHistory(caseObject);
                    UpdateEventRecords(caseObject);
                    UpdateCaseAttachments(caseObject);
                    UpdateCaseContainerId(caseObject);
                    UpdateCaseSetAsReference(caseObject);
                    ReleaseCase(caseObject);
                }
                else if (updateType == CaseUpdateEnum.CloseCase)
                {
                    //Update modified Addattachment, SetAsReference, and result and then Close the Case
                    //works with live and archive case
                    CloseCase(caseObject);
                }
            }
        }

        private void UpdateCaseContainerId (CaseObject caseObject)
        {
            if (caseObject.ScanContainerIdModified)
            {
                m_dataSourceAccess.UpdateCase(caseObject.SourceAlias, caseObject.CaseId, CaseUpdateEnum.ObjectID, null, null, AttachFileTypeEnum.Unknown, null,
                    caseObject.scanInfo.container.Id, null, CultureResources.ConvertDateTimeToStringForData(DateTime.Now), (Communications.Interfaces.CaseType)caseObject.caseType);

                caseObject.ScanContainerIdModified = false;
            }
        }

        private void UpdateCaseSetAsReference (CaseObject caseObject)
        {
            if (caseObject.SetAsReferenceModified)
            {
                m_dataSourceAccess.UpdateCase(caseObject.SourceAlias, caseObject.CaseId, CaseUpdateEnum.SetAsReference, null, null, AttachFileTypeEnum.Unknown, 
                    null, null, null, null, (Communications.Interfaces.CaseType)caseObject.caseType);

                caseObject.SetAsReferenceModified = false;
            }
        }

        private void UpdateCaseHistory (CaseObject caseObject)
        {
            DataAttachment dataAttachment = null;

            foreach (DataAttachment attachment in caseObject.attachments.GetHistoryAttachments())
            {
                if (attachment.attachmentType == AttachmentType.History)
                {
                    dataAttachment = attachment;
                    break;
                }
            }

            if (dataAttachment == null)
            {
                dataAttachment = new DataAttachment();
                dataAttachment.attachmentId = "History.xml";
                dataAttachment.attachmentType = AttachmentType.History;
                dataAttachment.CreateTime = CultureResources.ConvertDateTimeToStringForData(DateTime.Now);
            }

            dataAttachment.attachmentData = (MemoryStream)HistoryTranslator.Translate(caseObject.CaseHistories);
            dataAttachment.IsNew = true;
            caseObject.NewAttachments.Add(dataAttachment);
        }

        private void UpdateEventRecords(CaseObject caseObject)
        {
            if (caseObject.EventRecordsModified)
            {
                DataAttachment dataAttachment = null;
                foreach (DataAttachment attachment in caseObject.attachments.GetEventHistoryAttachments())
                {
                    if (attachment.attachmentType == AttachmentType.EVENT_HISTORY)
                    {
                        dataAttachment = attachment;
                        break;
                    }
                }

                if (dataAttachment == null)
                {
                    dataAttachment = new DataAttachment();
                    dataAttachment.attachmentId = "events.xml";
                    dataAttachment.attachmentType = AttachmentType.EVENT_HISTORY;
                    dataAttachment.CreateTime = CultureResources.ConvertDateTimeToStringForData(DateTime.Now);
                }

                dataAttachment.attachmentData = (MemoryStream)EventHistoryTranslator.Translate(caseObject.EventRecords, dataAttachment.attachmentData);
                dataAttachment.IsNew = true;
                caseObject.NewAttachments.Add(dataAttachment);

                caseObject.EventRecordsModified = false;
            }
        }

        private void UpdateCaseAttachments (CaseObject caseObject)
        {
            foreach (DataAttachment attachment in caseObject.NewAttachments)
            {
                AttachFileTypeEnum fileEnum;

                switch (attachment.attachmentType)
                {
                    case AttachmentType.Annotations:
                        fileEnum = AttachFileTypeEnum.Annotations;
                        break;
                    case AttachmentType.EVENT_HISTORY:
                        fileEnum = AttachFileTypeEnum.EVENT_HISTORY;
                        break;
                    case AttachmentType.History:
                        fileEnum = AttachFileTypeEnum.History;
                        break;
                    case AttachmentType.Manifest:
                        fileEnum = AttachFileTypeEnum.Manifest;
                        break;
                    case AttachmentType.NUC:
                        fileEnum = AttachFileTypeEnum.NUC;
                        break;
                    case AttachmentType.OCR:
                        fileEnum = AttachFileTypeEnum.OCR;
                        break;
                    case AttachmentType.SNM:
                        fileEnum = AttachFileTypeEnum.SNM;
                        break;
                    case AttachmentType.TDSResultFile:
                        fileEnum = AttachFileTypeEnum.TDSResultFile;
                        break;
                    case AttachmentType.XRayImage:
                        fileEnum = AttachFileTypeEnum.XRayImage;
                        break;
                    default:
                        fileEnum = AttachFileTypeEnum.Unknown;
                        break;
                }

                SysConfiguration sysConfig = m_SysConfigMgrAccess.GetConfig(caseObject.SourceAlias);

                if (sysConfig == null)
                {
                    sysConfig = m_SysConfigMgrAccess.GetDefaultConfig();
                }

                attachment.attachmentData.Seek(0, SeekOrigin.Begin);

                string userName = "Unknown";

                if (m_SysConfigMgrAccess.GetDefaultConfig().Profile != null &&
                    !String.IsNullOrWhiteSpace(m_SysConfigMgrAccess.GetDefaultConfig().Profile.UserName))
                {
                    userName = m_SysConfigMgrAccess.GetDefaultConfig().Profile.UserName;
                }

                m_dataSourceAccess.UpdateCase(caseObject.SourceAlias, caseObject.CaseId,
                    CaseUpdateEnum.AttachFile, attachment.attachmentId, attachment.attachmentData, fileEnum, null, string.Empty, userName, CultureResources.ConvertDateTimeToStringForData(DateTime.Now), (Communications.Interfaces.CaseType)caseObject.caseType);
            }

            caseObject.NewAttachments.Clear();
        }

        private void UpdateCaseResult (CaseObject caseObject)
        {
            if (caseObject.WorkstationResult != null)
            {
                m_dataSourceAccess.UpdateCase(caseObject.SourceAlias, caseObject.CaseId,
                    CaseUpdateEnum.Result, null, null, AttachFileTypeEnum.Unknown, new WorkstationResult(caseObject.WorkstationResult), null, null, null, (Communications.Interfaces.CaseType)caseObject.caseType);
            }
        }

        private void ReleaseCase (CaseObject caseObject)
        {
            if (caseObject.caseType != Common.CaseType.CTICase && caseObject.caseType != Common.CaseType.FTICase)
            {
                m_dataSourceAccess.UpdateCase(caseObject.SourceAlias, caseObject.CaseId,
                    CaseUpdateEnum.ReleaseCase, null, null, AttachFileTypeEnum.Unknown, null, null, null, null, (Communications.Interfaces.CaseType)caseObject.caseType);
            }

            m_caseCollection.Remove(caseObject);
        }

        private void CloseCase (CaseObject caseObject)
        {
            m_dataSourceAccess.UpdateCase(caseObject.SourceAlias, caseObject.CaseId, CaseUpdateEnum.CloseCase,
                null, null, AttachFileTypeEnum.Unknown, null, null, null, CultureResources.ConvertDateTimeToStringForData(caseObject.AnalysisStartTime), (Communications.Interfaces.CaseType)caseObject.caseType);

            m_caseCollection.Remove(caseObject);
        }

        #endregion Public Members
    }
}
