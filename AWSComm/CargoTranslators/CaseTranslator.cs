using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using L3.Cargo.Common;

namespace L3.Cargo.Translators
{
    public enum WorkstationDecision
    {
        Unknown = 0,
        Clear = 1,
        Reject = 2,
        Caution = 3
    }

    public class CaseTranslator
    {
        public static CaseObject Translate(Stream CaseXML, String Directory = null)
        {
            CaseObject caseObj = null;

            try
            {
                Translate2_0(CaseXML, ref caseObj);
            }
            catch (Exception exp)
            {
                try
                {
                    if (exp.Message == ErrorMessages.CASE_VERSION_MISMATCH)
                    {
                        CaseXML.Seek(0, SeekOrigin.Begin);

                        if (Directory != null)
                        {
                            if (!File.Exists(Path.Combine(Directory, "case1_0.xml")))
                            {
                                //Preserve Case1.0 file for now
                                FileStream f = new FileStream(Path.Combine(Directory, "case.xml"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                                FileStream f2 = File.Open(Directory + "\\case1_0.xml", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                                f.CopyTo(f2);
                                f.Close();
                                f2.Close();
                            }
                        }

                        CaseXML.Seek(0, SeekOrigin.Begin);

                        caseObj = Translate1_0(CaseXML);
                    }
                    else
                        throw;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return caseObj;
        }

        public static void Translate(CaseObject caseObj, ref Stream caseXML, String FilePath)
        {
            Translate2_0(caseObj, ref caseXML, FilePath);
        }

        public static CaseObject AddResult(Stream CaseXML, result res, String Directory = null)
        {
            try
            {
                CaseObject caseObj = null;
                caseObj = Translate(CaseXML, Directory);

                caseObj.ResultsList.Add(res);

                return caseObj;
            }
            catch (Exception exp)
            {
                throw;
            }
        }

        public static CaseObject AddAttachment(Stream CaseXML, DataAttachment attach, String Directory = null)
        {
            try
            {
                CaseObject caseObj = null;
                caseObj = Translate(CaseXML, Directory);
                DataAttachment RemoveAttachmentFirst = null;

                if (caseObj.attachments != null)
                {
                    foreach (DataAttachment att in caseObj.attachments)
                    {
                        if ((att.attachmentType == attach.attachmentType) &&
                            ((attach.attachmentType == AttachmentType.History) || (attach.attachmentType == AttachmentType.EVENT_HISTORY) || 
                            (attach.attachmentType == AttachmentType.Annotations)))
                        {
                            RemoveAttachmentFirst = att;
                            break;
                        }
                    }
                }

                if (RemoveAttachmentFirst != null)
                    caseObj.attachments.Remove(RemoveAttachmentFirst);

                caseObj.attachments.Add(attach);

                return caseObj;
            }
            catch (Exception exp)
            {
                throw;
            }
        }

        public static void TranslateToCase2_0(Stream CaseXML, ref Stream outCaseXML2_0, String caseDirectory)
        {
            CaseObject caseObj = null;

            try
            {
                Translate2_0(CaseXML, ref caseObj);
                outCaseXML2_0 = CaseXML;
                outCaseXML2_0.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception exp)
            {
                //case is 1.0
                CaseXML.Seek(0, SeekOrigin.Begin);
                caseObj = Translate1_0(CaseXML);
                //convert to 2.0
                CaseXML.Seek(0, SeekOrigin.Begin);
                Translate2_0(caseObj, ref outCaseXML2_0, caseDirectory);
            }

        }

        private static void Translate2_0(CaseObject caseObj, ref Stream caseXML, String caseDirectory)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(L3.Cargo.Common.Xml.XCase_2_0.XCase));
            L3.Cargo.Common.Xml.XCase_2_0.XCase xcase = new L3.Cargo.Common.Xml.XCase_2_0.XCase();

            if (caseObj != null)
            {
                xcase.version = "2.0";

                if (caseObj.EventRecords != null && caseObj.attachments.CountofType(AttachmentType.EVENT_HISTORY) <= 0)
                {
                    DataAttachment dataAttachmentEventHistory = null;
                    List<L3.Cargo.Common.Xml.XCase_2_0.EventRecord> recordList = new List<L3.Cargo.Common.Xml.XCase_2_0.EventRecord>();

                    foreach (CaseObject.CaseEventRecord eventrecord in caseObj.EventRecords)
                    {
                        L3.Cargo.Common.Xml.XCase_2_0.EventRecord rc = new L3.Cargo.Common.Xml.XCase_2_0.EventRecord();
                        rc.createTime = eventrecord.createTime;
                        rc.description = eventrecord.description;
                        recordList.Add(rc);
                    }

                    dataAttachmentEventHistory = new DataAttachment();
                    dataAttachmentEventHistory.attachmentId = "events.xml";
                    dataAttachmentEventHistory.attachmentType = AttachmentType.EVENT_HISTORY;
                    caseObj.attachments.Add(dataAttachmentEventHistory);

                    XmlSerializer eventSerializer = new XmlSerializer(typeof(L3.Cargo.Common.Xml.XCase_2_0.EventHistory));
                    L3.Cargo.Common.Xml.XCase_2_0.EventHistory eventHistory = new L3.Cargo.Common.Xml.XCase_2_0.EventHistory();
                    eventHistory.EventRecord = recordList.ToArray();

                    FileStream fs = new FileStream(caseDirectory + "\\events.xml", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    eventSerializer.Serialize(fs, eventHistory);
                    fs.Close();
                }

                xcase.abortedby = caseObj.AbortedBy;

                if (caseObj.attachments != null)
                {
                    List<L3.Cargo.Common.Xml.XCase_2_0.Attachment> attachList = new List<L3.Cargo.Common.Xml.XCase_2_0.Attachment>();

                    foreach (DataAttachment attach in caseObj.attachments)
                    {
                        L3.Cargo.Common.Xml.XCase_2_0.Attachment at = new L3.Cargo.Common.Xml.XCase_2_0.Attachment();
                        at.createTime = attach.CreateTime;
                        at.filename = attach.attachmentId;
                        at.type = attach.attachmentType.ToString();
                        at.user = attach.User;
                        attachList.Add(at);
                    }

                    xcase.attachments = attachList.ToArray();
                }

                xcase.createTime = caseObj.createTime;
                xcase.currentArea = caseObj.CurrentArea;
                xcase.id = caseObj.CaseId;
                xcase.linkedCase = caseObj.LinkedCaseId;

                if (caseObj.ResultsList != null)
                {
                    List<L3.Cargo.Common.Xml.XCase_2_0.Result> resultList = new List<L3.Cargo.Common.Xml.XCase_2_0.Result>();

                    try
                    {
                        foreach (result res in caseObj.ResultsList)
                        {
                            L3.Cargo.Common.Xml.XCase_2_0.Result rest = new L3.Cargo.Common.Xml.XCase_2_0.Result();
                            rest.analysisTime = res.AnalysisTime;
                            rest.comment = res.Comment;
                            rest.createTime = res.CreateTime;

                            if (res.Decision == WorkstationDecision.Caution.ToString())
                                rest.decision = (int)WorkstationDecision.Caution;
                            else if (res.Decision == WorkstationDecision.Clear.ToString())
                                rest.decision = (int)WorkstationDecision.Clear;
                            else if (res.Decision == WorkstationDecision.Reject.ToString())
                                rest.decision = (int)WorkstationDecision.Reject;
                            else
                                rest.decision = (int)WorkstationDecision.Unknown;

                            rest.reason = res.Reason;
                            rest.stationType = res.StationType;
                            rest.user = res.User;

                            resultList.Add(rest);
                        }
                    }
                    catch (Exception exp)
                    {
                        throw;
                    }

                    xcase.results = resultList.ToArray();
                }

                if (caseObj.scanInfo != null)
                {
                    xcase.scanInfo = new L3.Cargo.Common.Xml.XCase_2_0.ScanInfo();

                    if (caseObj.scanInfo.container != null)
                    {
                        xcase.scanInfo.container = new L3.Cargo.Common.Xml.XCase_2_0.Container();
                        xcase.scanInfo.container.code = caseObj.scanInfo.container.Code;
                        xcase.scanInfo.container.id = caseObj.scanInfo.container.Id;
                        xcase.scanInfo.container.sequenceNum = caseObj.scanInfo.container.SequenceNum;
                        xcase.scanInfo.container.weight = caseObj.scanInfo.container.Weight;
                    }

                    if (caseObj.scanInfo.conveyance != null)
                    {
                        xcase.scanInfo.conveyance = new L3.Cargo.Common.Xml.XCase_2_0.Conveyance();
                        xcase.scanInfo.conveyance.batchNum = caseObj.scanInfo.conveyance.BatchNum;
                        xcase.scanInfo.conveyance.id = caseObj.scanInfo.conveyance.Id;
                        xcase.scanInfo.conveyance.totalWeight = caseObj.scanInfo.conveyance.TotalWeight;
                    }

                    if (caseObj.scanInfo.location != null)
                    {
                        xcase.scanInfo.location = new L3.Cargo.Common.Xml.XCase_2_0.Location();
                        xcase.scanInfo.location.latitude = caseObj.scanInfo.location.Latitude;
                        xcase.scanInfo.location.longitude = caseObj.scanInfo.location.Longitude;
                    }

                    if (caseObj.scanInfo.ScanType != null)
                        xcase.scanInfo.type = caseObj.scanInfo.ScanType;
                }

                if (caseObj.systemInfo != null)
                {
                    xcase.systemInfo = new L3.Cargo.Common.Xml.XCase_2_0.SystemInfo();
                    xcase.systemInfo.baseLocation = caseObj.systemInfo.BaseLocation;
                    xcase.systemInfo.systemType = caseObj.systemInfo.SystemType;
                }

                MemoryStream ms = new MemoryStream();
                serializer.Serialize(ms, xcase);
                ms.Seek(0, SeekOrigin.Begin);
                caseXML.Flush();
                ms.CopyTo(caseXML);
                caseXML.Seek(0, SeekOrigin.Begin);
            }
        }


        private static CaseObject Translate1_0(Stream CaseXML)
        {
            CaseObject caseObj = new CaseObject();

            XmlSerializer serializer = new XmlSerializer(typeof(L3.Cargo.Common.Xml.XCase_1_0.XCase));
            L3.Cargo.Common.Xml.XCase_1_0.XCase xcase = (L3.Cargo.Common.Xml.XCase_1_0.XCase)serializer.Deserialize(CaseXML);

            caseObj.CaseId = xcase.id;
            caseObj.createTime = xcase.createTime;
            caseObj.LinkedCaseId = xcase.linkedCase;
            caseObj.AbortedBy = xcase.abortedBy;
            caseObj.CurrentArea = xcase.currentArea;

            Location location = null;

            Conveyance convey = null;

            Container cont = null;

            caseObj.attachments = new DataAttachments();

            if (xcase.vehicle != null)
            {
                cont = new Container(String.Empty, String.Empty, String.Empty, String.Empty);
                cont.Id = xcase.vehicle.registrationNum;
                cont.Weight = xcase.vehicle.weight.ToString();

                if (xcase.vehicle.manifest != null)
                {
                    foreach (L3.Cargo.Common.Xml.XCase_1_0.Manifest manifest in xcase.vehicle.manifest)
                    {
                        DataAttachment attach = new DataAttachment();
                        attach.attachmentType = AttachmentType.Manifest;
                        attach.attachmentId = manifest.image;
                        attach.User = String.Empty;
                        attach.CreateTime = String.Empty;
                        caseObj.attachments.Add(attach);
                    }
                }
            }

            caseObj.scanInfo = new ScanInfo(String.Empty, location, convey, cont);

            caseObj.systemInfo = new SystemInfo(String.Empty, xcase.siteId);

            if (xcase.xRayImage != null)
            {
                foreach (String str in xcase.xRayImage)
                {
                    DataAttachment attach = new DataAttachment();
                    attach.attachmentType = AttachmentType.XRayImage;
                    attach.attachmentId = str;
                    attach.User = String.Empty;
                    attach.CreateTime = String.Empty;
                    caseObj.attachments.Add(attach);
                }
            }

            if (xcase.attachment != null)
            {
                foreach (L3.Cargo.Common.Xml.XCase_1_0.Attachment atch in xcase.attachment)
                {
                    if (atch.File != String.Empty)
                    {
                        DataAttachment attach = new DataAttachment();
                        if (atch.type.ToLower() == "unknown" || atch.type == String.Empty)
                            attach.attachmentType = AttachmentType.Unknown;
                        else
                            attach.attachmentType = (AttachmentType)Enum.Parse(typeof(AttachmentType), atch.type);

                        attach.attachmentId = atch.File;
                        attach.User = String.Empty;
                        attach.CreateTime = String.Empty;
                        caseObj.attachments.Add(attach);
                    }
                }
            }

            if (xcase.tdsResultFile != null && xcase.tdsResultFile != String.Empty)
            {
                DataAttachment attach = new DataAttachment();
                attach.attachmentType = AttachmentType.TDSResultFile;
                attach.attachmentId = xcase.tdsResultFile;
                attach.User = String.Empty;
                attach.CreateTime = String.Empty;
                caseObj.attachments.Add(attach);
            }

            if (xcase.eventRecord != null)
            {
                caseObj.EventRecords = new List<CaseObject.CaseEventRecord>();

                foreach (L3.Cargo.Common.Xml.XCase_1_0.EventRecord record in xcase.eventRecord)
                {
                    CaseObject.CaseEventRecord eventRecord = new CaseObject.CaseEventRecord(record.createTime, record.description, false);
                    caseObj.EventRecords.Add(eventRecord);
                }
            }

            caseObj.ResultsList = new List<result>();

            if (xcase.awsResult != null)
            {
                String decision;

                switch (xcase.awsResult.decision)
                {
                    case L3.Cargo.Common.Xml.XCase_1_0.AWSDecision.AWS_CAUTION:
                        decision = WorkstationDecision.Caution.ToString();
                        break;
                    case L3.Cargo.Common.Xml.XCase_1_0.AWSDecision.AWS_CLEAR:
                        decision = WorkstationDecision.Clear.ToString();
                        break;
                    case L3.Cargo.Common.Xml.XCase_1_0.AWSDecision.AWS_REJECT:
                        decision = WorkstationDecision.Reject.ToString();
                        break;
                    case L3.Cargo.Common.Xml.XCase_1_0.AWSDecision.AWS_UNKNOWN:
                        decision = WorkstationDecision.Unknown.ToString();
                        break;
                    default:
                        decision = WorkstationDecision.Unknown.ToString();
                        break;
                }

                result res = new result(decision, xcase.awsResult.reason.ToString(),
                    String.Empty, xcase.awsResult.awsUserId, xcase.awsResult.comment, "Analyst", String.Empty);

                caseObj.ResultsList.Add(res);
            }

            if (xcase.ewsResult != null)
            {
                String decision;

                switch (xcase.ewsResult.decision)
                {
                    case L3.Cargo.Common.Xml.XCase_1_0.EWSDecision.EWS_RELEASE:
                    case L3.Cargo.Common.Xml.XCase_1_0.EWSDecision.EWS_CLEAR:
                        decision = WorkstationDecision.Clear.ToString();
                        break;
                    case L3.Cargo.Common.Xml.XCase_1_0.EWSDecision.EWS_REJECT:
                        decision = WorkstationDecision.Reject.ToString();
                        break;
                    case L3.Cargo.Common.Xml.XCase_1_0.EWSDecision.EWS_UNKNOWN:
                        decision = WorkstationDecision.Unknown.ToString();
                        break;
                    default:
                        decision = WorkstationDecision.Unknown.ToString();
                        break;
                }

                result res = new result(decision, String.Empty,
                    String.Empty, xcase.ewsResult.ewsUserId, xcase.ewsResult.comment, "EWS", String.Empty);

                caseObj.ResultsList.Add(res);
            }

            if (xcase.insResult != null)
            {
                String decision;

                switch (xcase.insResult.decision)
                {
                    case L3.Cargo.Common.Xml.XCase_1_0.INSDecision.INS_CLEAR:
                        decision = WorkstationDecision.Clear.ToString();
                        break;
                    case L3.Cargo.Common.Xml.XCase_1_0.INSDecision.INS_REJECT:
                        decision = WorkstationDecision.Reject.ToString();
                        break;
                    default:
                        decision = WorkstationDecision.Unknown.ToString();
                        break;
                }

                result res = new result(decision, String.Empty,
                    String.Empty, xcase.insResult.insUserId, xcase.insResult.comment, "Inspector", String.Empty);

                caseObj.ResultsList.Add(res);
            }

            if (xcase.supResult != null)
            {
                String decision;

                switch (xcase.supResult.decision)
                {
                    case L3.Cargo.Common.Xml.XCase_1_0.SUPDecision.SUP_CLEAR:
                        decision = WorkstationDecision.Clear.ToString();
                        break;
                    case L3.Cargo.Common.Xml.XCase_1_0.SUPDecision.SUP_REJECT:
                        decision = WorkstationDecision.Reject.ToString();
                        break;
                    default:
                        decision = WorkstationDecision.Unknown.ToString();
                        break;
                }

                result res = new result(decision, String.Empty,
                    String.Empty, xcase.supResult.supUserId, xcase.supResult.comment, "Supervisor", String.Empty);

                caseObj.ResultsList.Add(res);
            }

            return caseObj;
        }

        private static void Translate2_0(Stream CaseXML, ref CaseObject CaseObj)
        {
            CaseObject caseObj = new CaseObject();

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(L3.Cargo.Common.Xml.XCase_2_0.XCase));
                L3.Cargo.Common.Xml.XCase_2_0.XCase xcase = (L3.Cargo.Common.Xml.XCase_2_0.XCase)serializer.Deserialize(CaseXML);

                if (xcase.version == "2.0")
                {
                    caseObj.CaseId = xcase.id;
                    caseObj.createTime = xcase.createTime;
                    caseObj.LinkedCaseId = xcase.linkedCase;
                    caseObj.AbortedBy = xcase.abortedby;
                    caseObj.CurrentArea = xcase.currentArea;

                    Location location = null;
                    if (xcase.scanInfo != null && xcase.scanInfo.location != null)
                    {
                        location = new Location(String.Empty, String.Empty);
                        location.Latitude = xcase.scanInfo.location.latitude;
                        location.Longitude = xcase.scanInfo.location.longitude;
                    }

                    Conveyance convey = null;
                    if (xcase.scanInfo != null && xcase.scanInfo.conveyance != null)
                    {
                        convey = new Conveyance(String.Empty, String.Empty, String.Empty);
                        convey.BatchNum = xcase.scanInfo.conveyance.batchNum;
                        convey.Id = xcase.scanInfo.conveyance.id;
                        convey.TotalWeight = xcase.scanInfo.conveyance.totalWeight;
                    }

                    Container cont = null;
                    if (xcase.scanInfo != null && xcase.scanInfo.container != null)
                    {
                        cont = new Container(String.Empty, String.Empty, String.Empty, String.Empty);
                        cont.Code = xcase.scanInfo.container.code;
                        cont.Id = xcase.scanInfo.container.id;
                        cont.SequenceNum = xcase.scanInfo.container.sequenceNum;
                        cont.Weight = xcase.scanInfo.container.weight;
                    }

                    if (xcase.scanInfo != null)
                        caseObj.scanInfo = new ScanInfo(xcase.scanInfo.type, location, convey, cont);

                    if (xcase.systemInfo != null)
                    {
                        caseObj.systemInfo = new SystemInfo(xcase.systemInfo.systemType, xcase.systemInfo.baseLocation);
                    }


                    caseObj.ResultsList = new List<result>();

                    if (xcase.results != null)
                    {
                        foreach (L3.Cargo.Common.Xml.XCase_2_0.Result result in xcase.results)
                        {
                            WorkstationDecision decision = (WorkstationDecision)Enum.ToObject(typeof(WorkstationDecision), result.decision);
                            caseObj.ResultsList.Add(new result(decision.ToString(), result.reason, result.createTime, result.user, result.comment,
                                result.stationType, result.analysisTime));
                        }
                    }

                    if (xcase.attachments != null)
                    {
                        foreach (L3.Cargo.Common.Xml.XCase_2_0.Attachment attach in xcase.attachments)
                        {
                            DataAttachment attachment = new DataAttachment();
                            attachment.attachmentType = (AttachmentType)Enum.Parse(typeof(AttachmentType), attach.type);
                            attachment.attachmentId = attach.filename;
                            attachment.CreateTime = attach.createTime;
                            attachment.User = attach.user;
                            caseObj.attachments.Add(attachment);
                        }
                    }

                    CaseObj = caseObj;
                }
                else
                {
                    throw new Exception(ErrorMessages.CASE_VERSION_MISMATCH);
                }
            }
            catch (Exception exp)
            {
                throw;
            }
        }
    }
}