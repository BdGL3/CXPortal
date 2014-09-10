using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.ServiceModel;
using System.Xml.Serialization;
using L3.Cargo.Common;
using L3.Cargo.Common.Xml.XCase_2_0;
using L3.Cargo.Communications.Client;
using L3.Cargo.Communications.Interfaces;

namespace L3.Cargo.WSCommunications
{
    public class TIPClient : TipManagerCallback, IDisposable
    {
        #region Private Members

        private TipManagerEndpoint m_EndPoint;

        private string m_CTIDirectory;

        private string m_FTIDirectory;

        private string m_Alias;

        private string m_CTITemplateDir;

        private WSServer m_WSServer;

        private Dictionary<string, string> m_FTIFiles;

        private Dictionary<string, string> m_CTICases;

        #endregion Private Members


        #region Public Members

        public TipManagerEndpoint EndPoint
        {
            set
            {
                m_EndPoint = value;
            }
        }

        public string Alias
        {
            get
            {
                return m_Alias;
            }
        }

        #endregion Public Members


        #region Constructors

        public TIPClient(string alias, string ctiCaseDir, string ftiImageDir, string ctiTemplateDir, WSServer wsServer)
        {
            m_Alias = alias;
            m_WSServer = wsServer;

            m_CTIDirectory = ctiCaseDir;
            m_FTIDirectory = ftiImageDir;
            m_CTITemplateDir = ctiTemplateDir;

            m_FTIFiles = new Dictionary<string, string>();
            m_CTICases = new Dictionary<string, string>();
        }

        #endregion Constructors


        #region Private Methods

        private string CreateCTICase(string ctiFilename)
        {
            Random random = new Random();

            string[] directories = Directory.GetDirectories(m_CTITemplateDir);
            string templateDir = directories[random.Next(directories.Length)];

            string caseId = CreateNewCaseId();

            DirectoryInfo source = new DirectoryInfo(templateDir);
            DirectoryInfo destination = new DirectoryInfo(m_CTIDirectory + caseId);

            CopyCaseDirectory(source, destination);
            ModifyCase(caseId, new FileInfo(Path.Combine(destination.FullName, "case.xml")), ctiFilename);

            m_WSServer.AddToCaseList(caseId, destination.FullName, DateTime.Now.ToString(), "Analyst", true);//  create a new case in the case list

            return caseId;
        }

        private void CopyCaseDirectory(DirectoryInfo source, DirectoryInfo destination)
        {
            if (Directory.Exists(destination.FullName) == false)
            {
                Directory.CreateDirectory(destination.FullName);
            }

            foreach (FileInfo fi in source.GetFiles())
            {
                if (string.Compare(fi.Extension, ".pxe", true) != 0)
                {
                    fi.CopyTo(Path.Combine(destination.ToString(), fi.Name), true);
                }
            }
        }

        private void ModifyCase(string caseId, FileInfo caseXmlFile, string xRayImage)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(XCase));
            XCase xCase;
            using (Stream reader = new FileStream(caseXmlFile.FullName, FileMode.Open))
            {
                xCase = (XCase)serializer.Deserialize(reader);
            }
            xCase.createTime = DateTime.Now;
            xCase.id = caseId;

            List<Attachment> attachments = new List<Attachment>();

            if (xCase.attachments != null)
            {
                foreach (Attachment att in xCase.attachments)
                {
                    if (!att.type.Equals(AttachFileTypeEnum.XRayImage.ToString(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        Attachment attachment = new Attachment();
                        attachment.createTime = att.createTime;
                        attachment.filename = att.filename;
                        attachment.type = att.type;
                        attachment.user = att.user;
                        attachments.Add(attachment);
                    }
                }
            }

            Attachment xrayAtt = new Attachment();
            xrayAtt.createTime = DateTime.Now.ToString();
            xrayAtt.filename = xRayImage;
            xrayAtt.type = AttachFileTypeEnum.XRayImage.ToString();
            xrayAtt.user = "SYSTEM";
            attachments.Add(xrayAtt);

            xCase.attachments = attachments.ToArray();

            using (Stream writer = new FileStream(caseXmlFile.FullName, FileMode.Create))
            {
                serializer.Serialize(writer, xCase);
            }
        }

        private string CreateNewCaseId()
        {
            string caseId = string.Empty;
            int serialnum = 1;
            int lastIndex = m_WSServer.caseList.List.Tables[0].Rows.Count - 1;
            
            if (lastIndex >= 0)
            {
                string filterExpression = "";
                string sort = "CaseId DESC";

                DataRow[] rows = m_WSServer.caseList.List.CaseListTable.Select(filterExpression, sort);

                if (rows.Length > 0)
                {
                    caseId = rows[0]["CaseId"] as string;

                    if (!String.IsNullOrWhiteSpace(caseId))
                    {
                        serialnum = Convert.ToInt32(caseId.Substring(caseId.Length - 4));
                        serialnum++;
                    }
                }
            }

            caseId = DateTime.Now.ToString("yyMMddHHmmss");
            caseId += serialnum.ToString("0000");

            return caseId;
        }

        private string FindFileByAssignment(string assignment)
        {
            string ret = null;

            foreach (string key in m_FTIFiles.Keys)
            {
                if (string.Compare(m_FTIFiles[key], assignment, true) == 0)
                {
                    ret = key;
                    break;
                }
            }

            return ret;
        }

        private string FindCaseByAssignment (string assignment)
        {
            string ret = null;

            foreach (string key in m_CTICases.Keys)
            {
                if (string.Compare(m_CTICases[key], assignment, true) == 0)
                {
                    ret = key;
                    break;
                }
            }

            return ret;
        }

        private string GetPXEFromCase (string caseId)
        {
            string ret = null;
            DirectoryInfo dirInfo = new DirectoryInfo(m_CTIDirectory + caseId);
            FileInfo[] pxeFiles = dirInfo.GetFiles("*.pxe", SearchOption.TopDirectoryOnly);

            if (pxeFiles.Length > 0)
            {
                ret = pxeFiles[0].Name;
            }

            return ret;
        }

        private void DeleteFTIFile (string filename)
        {
            File.Delete(m_FTIDirectory + filename);
            m_FTIFiles.Remove(filename);
        }

        private void DeleteCTICase (string caseId)
        {
            Directory.Delete(m_CTIDirectory + caseId, true);
            m_CTICases.Remove(caseId);
        }

        #endregion Private Methods


        #region Public Methods

        public void Dispose ()
        {
            foreach(string filename in  m_FTIFiles.Keys)
            {
                DeleteFTIFile(filename);
            }

            foreach (string caseId in m_CTICases.Keys)
            {
                DeleteCTICase(caseId);
            }
        }

        public void AssignCTICase(string caseId, string workstationId)
        {
            if (m_CTICases.ContainsKey(caseId) && 
                String.IsNullOrWhiteSpace(m_CTICases[caseId]))
            {
                m_CTICases[caseId] = workstationId;
            }
            else
            {
                throw new FaultException(new FaultReason(ErrorMessages.CASE_CURRENTLY_IN_USE));
            }
        }

        public void RemoveCTICaseAssignment(string caseId)
        {
            if (m_CTICases.ContainsKey(caseId))
            {
                m_CTICases[caseId] = string.Empty;
            }
            else
            {
                throw new FaultException(new FaultReason(ErrorMessages.CASE_CURRENTLY_IN_USE));
            }
        }
        
        public string RequestFTIFile(string workstationId)
        {
            string ret = null;

            foreach (string key in m_FTIFiles.Keys)
            {
                if (String.IsNullOrWhiteSpace(m_FTIFiles[key]))
                {
                    ret = key;
                }
            }

            if (!String.IsNullOrWhiteSpace(ret))
            {
                m_FTIFiles[ret] = workstationId;
            }

            return ret;
        }

        public void ClearAssignments (string workstationId)
        {
            foreach (string key in m_FTIFiles.Keys)
            {
                if (m_FTIFiles[key] == workstationId)
                {
                    m_FTIFiles[key] = string.Empty;
                }
            }

            foreach (string key in m_CTICases.Keys)
            {
                if (m_CTICases[key] == workstationId)
                {
                    m_CTICases[key] = string.Empty;
                }
            }
        }

        public bool ContainsFTIFile (string fileName)
        {
            return m_FTIFiles.ContainsKey(fileName);
        }

        public bool ContainsCTICase (string caseId)
        {
            return m_CTICases.ContainsKey(caseId);
        }

        public bool ContainsWorkstation (string workstationId)
        {
            return (m_CTICases.ContainsValue(workstationId) || m_FTIFiles.ContainsValue(workstationId));
        }

        public void ProcessedCase (string caseId)
        {
            m_EndPoint.ProcessedCase(m_Alias, caseId);
        }

        public void TipResult(WorkstationResult workstationResult)
        {
            if (workstationResult.CaseType == L3.Cargo.Communications.Interfaces.CaseType.FTICase)
            {
                string fileName = FindFileByAssignment(workstationResult.WorkstationId);

                if (!String.IsNullOrWhiteSpace(fileName))
                {
                    m_EndPoint.TipResult(fileName.Remove(fileName.Length - 4), workstationResult);
                    DeleteFTIFile(fileName);
                }
            }
            else if (workstationResult.CaseType == L3.Cargo.Communications.Interfaces.CaseType.CTICase)
            {
                string caseId = FindCaseByAssignment(workstationResult.WorkstationId);

                if (!String.IsNullOrWhiteSpace(caseId))
                {
                    string fileName = GetPXEFromCase(caseId);

                    if (!String.IsNullOrWhiteSpace(fileName))
                    {
                        m_EndPoint.TipResult(fileName.Remove(fileName.Length - 4), workstationResult);
                    }

                    DeleteCTICase(caseId);
                }
            }
        }

        public override void InjectTip (TIPInjectFileMessage message)
        {
            using (message.file)
            {
                string fileCopyDir = string.Empty;

                if (message.imageType == TIPImageType.CTI)
                {
                    string caseId = CreateCTICase(message.filename);
                    m_CTICases.Add(caseId, string.Empty);
                    fileCopyDir = m_CTIDirectory + caseId;
                }
                else if (message.imageType == TIPImageType.FTI)
                {
                    if (!m_FTIFiles.ContainsKey(message.filename))
                    {
                        m_FTIFiles.Add(message.filename, string.Empty);
                    }
                    fileCopyDir = m_FTIDirectory;
                }

                using (FileStream stream = new FileStream(fileCopyDir + @"\" + message.filename, FileMode.OpenOrCreate))
                {
                    message.file.CopyTo(stream);
                }
            }
        }

        #endregion Public Methods
    }
}
