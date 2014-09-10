using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;
using L3.Cargo.Common;
using L3.Cargo.Translators;
using System.Linq;
using System.Timers;
using System.Globalization;

namespace L3.Cargo.Communications.Common
{
    public class CaseList : CaseListTracker<CaseList>, IDisposable
    {
        #region Private Members

        private bool m_StartUpdate;

        private bool m_IsReferenceMode;

        private CaseListDataSet m_CaseListDataSet;

        private Logger m_Logger;

        private FileSystemWatcher m_FileSystemWatcher;

        private UInt16 m_FullSyncPeriodicityDays;

        private DateTime m_lastFullSyncDatetime;

        private bool m_StopSync;

        private bool m_FullSyncStarted;

        private System.Timers.Timer fullSyncTimer;

        private bool _generateThumbnail;

        #endregion Private Members


        #region Protected Members

        protected string m_FileSystemLocation;

        protected string m_CaselistFilename;

        protected Thread m_CheckCaselistThread;
		
	    protected float CaseListLastUpdateTimeOffsetHours;

        protected Database m_DB;

        protected bool m_isDFCMatch;

        protected enum UpdateType
        {
            Add,
            Modify,
            Delete
        }

        #endregion Protected Members


        #region Public Members

        public object CaseListLock;

        public CaseListDataSet List
        {
            get
            {
                return m_CaseListDataSet;
            }
        }

        public bool StopSync
        {
            set 
            { 
                m_StopSync = value;
                m_FullSyncStarted = false;
            }
        }

        public bool FullSyncStarted
        {
            get { return m_FullSyncStarted; }
        }

        public bool StartUpdate
        {
            get
            {
                return m_StartUpdate;
            }
            set
            {
                m_StartUpdate = value;
            }
        }

        public bool GenerateThumbnail
        {
            set { _generateThumbnail = value; }
        }

        #endregion Public Members


        #region Constructors

        public CaseList()
        {
            m_Logger = null;
            m_StartUpdate = false;
            CaseListLock = new object();
            this.GenerateCaseList();
        }

        public CaseList(string path, bool isReferenceMode)
        {
            m_IsReferenceMode = isReferenceMode;
            m_FileSystemLocation = path;
            m_CaselistFilename = Path.Combine(path, "caselist.xml");
            m_Logger = null;
            m_StartUpdate = false;
            CaseListLock = new object();
            this.GenerateCaseList();
        }

        public CaseList(Logger logger, string path, bool isReferenceMode)
        {
            m_IsReferenceMode = isReferenceMode;
            m_FileSystemLocation = path;
            m_CaselistFilename = Path.Combine(path, "caselist.xml");
            m_Logger = logger;
            m_StartUpdate = false;
            CaseListLock = new object();
            this.GenerateCaseList();
        }

        #endregion Constructors


        #region Private Methods

        private static void Copy(String sourceDirectory, String targetDirectory)
        {
            CopyAll(new DirectoryInfo(sourceDirectory), new DirectoryInfo(targetDirectory));
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into it's new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                //Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        private void GenerateCaseList()
        {
            m_CaseListDataSet = new CaseListDataSet();
            m_CaseListDataSet.EnforceConstraints = true;
            m_CaseListDataSet.SchemaSerializationMode = SchemaSerializationMode.ExcludeSchema;
            m_CaseListDataSet.RemotingFormat = SerializationFormat.Binary;
            

            try
            {
                CaseListLastUpdateTimeOffsetHours = float.Parse(ConfigurationManager.AppSettings["CaseListLastUpdateTimeOffsetHours"]);
            }
            catch (ArgumentNullException exp)
            {
                CaseListLastUpdateTimeOffsetHours = 0;
            }
            catch
            {
                throw;
            }
        }

        private void fullSyncTimer_Elapsed(Object sender, EventArgs e)
        {
            StartFullSync();
        }

        private void InformOfupdate()
        {
            try
            {
                lock (CaseListLock)
                {
                    Notify(this);
                    if(m_DB != null)
                        m_DB.update(List.CaseListTable);
                    SaveCaseList();
                }
            }
            catch (Exception ex)
            {
                if (m_Logger != null)
                {
                    m_Logger.PrintLine(ex.Message);
                }

                throw;
            }
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            try
            {
                if (e.ChangeType == WatcherChangeTypes.Changed)
                {

                    if (e.FullPath.EndsWith("\\case.xml"))
                    {
                        // Specify what is done when a file is changed.                       
                        UpdateCaseList(true, e.FullPath);
                    }
                }
                else if (e.ChangeType == WatcherChangeTypes.Deleted)
                {
                    //extract caseid from the filepath
                    int SubStringStartIdx = e.Name.LastIndexOf("_") + 1;
                    String caseid;

                    if (e.Name.LastIndexOf("\\") < 0)
                        caseid = e.Name.Substring(SubStringStartIdx);
                    else
                        caseid = e.Name.Substring(SubStringStartIdx, e.Name.LastIndexOf("\\") - SubStringStartIdx);

                    // Specify what is done when a file is deleted.
                    Delete(caseid);
                }
            }
            catch (Exception exp)
            {
                string message = "OnChanged " + e.FullPath + " exp: " + exp.Message;
                if (exp.InnerException != null)
                    message += " " + exp.InnerException.Message;
                if (m_Logger != null)
                    m_Logger.PrintInfoLine(message);                
            }
        }

        private void FSWatcher_Error(object source, ErrorEventArgs e)
        {
            //  Show that an error has been detected.
            if (m_Logger != null)
                m_Logger.PrintInfoLine("The FileSystemWatcher has detected an error: " + e.GetException().Message);
            //  Give more information if the error is due to an internal buffer overflow.
            if (e.GetException().GetType() == typeof(InternalBufferOverflowException))
            {
                //  This can happen if Windows is reporting many file system events quickly 
                //  and internal buffer of the  FileSystemWatcher is not large enough to handle this
                //  rate of events. The InternalBufferOverflowException error informs the application
                //  that some of the file system events are being lost.
                if (m_Logger != null)
                    m_Logger.PrintInfoLine(("The file system watcher experienced an internal buffer overflow: " + e.GetException().Message));
                try
                {
                    if (m_CheckCaselistThread != null)
                    {
                        m_CheckCaselistThread.Abort();
                        m_CheckCaselistThread.Join();
                    }
                   
                    m_CheckCaselistThread = new Thread(new ParameterizedThreadStart(delegate { CheckCaseListThreadMethod(false); }));
                    m_CheckCaselistThread.Start();

                }
                catch (Exception exp)
                {
                    if (m_Logger != null)
                        m_Logger.PrintInfoLine("FSWatcherError exp: " + exp.Message);
                }
            }
        }

        #endregion Private Methods


        #region Protected Methods

        protected void SaveCaseList()
        {
            try
            {
                lock (CaseListLock)
                {
                    m_CaseListDataSet.AcceptChanges();
                }
            }
            catch (Exception ex)
            {
                if (m_Logger != null)
                {
                    m_Logger.PrintLine(ex.Message);
                }

                throw;
            }
        }

        protected void CheckCaseListThreadMethod(bool FullSync)
        {
            DirectoryInfo[] directoryInfoArray=null;
            string[] Dirs = Directory.GetDirectories(m_FileSystemLocation);
            var directory = new DirectoryInfo(m_FileSystemLocation);
            var directories = (from d in directory.GetDirectories()
                               orderby d.LastAccessTime ascending
                               select d);

            if (!FullSync)
            {
                try
                {
                    string caselistupdateTime = ConfigurationManager.AppSettings["CaseListLastUpdateTime"];

                    if(caselistupdateTime == null && m_Logger != null)
                        m_Logger.PrintInfoLine("CheckCaseListThreadMethod: CaseList UpateTime not defined in the config file.");

                    DateTime lastUpdateTime = DateTime.Parse(caselistupdateTime, CultureResources.getDefaultDisplayCulture());
                    DateTime datetime = lastUpdateTime.AddHours(-CaseListLastUpdateTimeOffsetHours);

                    var myDirs = (from d in directories
                                  orderby d.LastAccessTime ascending
                                  where d.LastAccessTime >= datetime
                                  select d);

                    directoryInfoArray = myDirs.ToArray();

                    if (m_Logger != null)
                        m_Logger.PrintInfoLine("CheckCaseListThreadMethod: Starting incremental sync..." + directoryInfoArray[0].FullName + " " + directoryInfoArray[0].LastAccessTime + " " + directoryInfoArray.Count());
                }
                catch
                {                    
                    FullSync = true;

                    if (m_Logger != null)
                        m_Logger.PrintInfoLine("CheckCaseListThreadMethod: Exception occurred while starting incremental sync...");
                }
            }

            if(FullSync)
            {
                //record this update time in the config. file
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (config.AppSettings.Settings["LastFullSyncDateTime"] == null)
                    config.AppSettings.Settings.Add("LastFullSyncDateTime", CultureResources.ConvertDateTimeToStringForData(m_lastFullSyncDatetime));
                else
                    config.AppSettings.Settings["LastFullSyncDateTime"].Value = CultureResources.ConvertDateTimeToStringForData(m_lastFullSyncDatetime);
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                m_FullSyncStarted = true;

                directoryInfoArray = directories.ToArray();

                if (m_Logger != null)
                    m_Logger.PrintInfoLine("CheckCaseListThreadMethod: Starting full sync...");
            }

            List<String> list = new List<String>();

            try
            {
                if (directoryInfoArray.Count() > 0)
                {
                    if (FullSync)
                    {
                        foreach (CaseListDataSet.CaseListTableRow row in this.List.CaseListTable.Rows)
                        {
                            list.Add(row.CaseId);
                        }
                    }

                    foreach (DirectoryInfo dir in directoryInfoArray)
                    {
                        string Dir = dir.FullName;

                        try
                        {
                            string path = Path.Combine(Dir, "case.xml");
                            if (File.Exists(path))
                            {
                                CaseObject caseObj = GetCaseObj(Dir, "case.xml");

                                if (CaseExists(caseObj.CaseId))
                                {
                                    String decision = GetDecision(caseObj.CaseId);
                                    String comment = GetComment(caseObj.CaseId);

                                    if (caseObj.ResultsList.Count > 0)
                                    {
                                        result tmpResult = caseObj.ResultsList[0];

                                        foreach (result result in caseObj.ResultsList)
                                        {
                                            if (result.CreateTime.CompareTo(tmpResult.CreateTime) > 0)
                                                tmpResult = result;
                                        }

                                        if (tmpResult != null)
                                        {
                                            String caseDecision = tmpResult.Decision;
                                            //if comment has changed update comment section
                                            if (((tmpResult.Comment != null) && (comment != tmpResult.Comment)) || (caseDecision != decision))
                                            {
                                                ModifyCommentResult(caseObj.CaseId, tmpResult.Comment, (Int32)Enum.Parse(typeof(L3.Cargo.Communications.Interfaces.WorkstationDecision), tmpResult.Decision));
                                            }
                                        }
                                    }

                                    if(FullSync)
                                        list.Remove(caseObj.CaseId);
                                }
                                else //add case to the list
                                {
                                    UpdateCaseList(false, Path.Combine(Dir, "case.xml"));
                                }
                            }
                            else
                            {
                                if (m_Logger != null)
                                    m_Logger.PrintInfoLine("CheckCaseListThreadMethod: " + Dir + "\\case.xml not found.");
                                string caseid = Dir.Substring(Dir.LastIndexOf("_") + 1);
                                if (CaseExists(caseid))
                                {
                                    if (m_Logger != null)
                                        m_Logger.PrintInfoLine("CheckCaseListThreadMethod: not found case.xml exists in the list, deleting this entry.");

                                    Delete(caseid);

                                    if(FullSync)
                                        list.Remove(caseid);
                                }
                            }
                        }
                        catch (Exception exp)
                        {
                            String message = "CheckCaseListThreadMethod " + Dir + " exp: " + exp.Message;
                            if (exp.InnerException != null)
                                message += " " + exp.InnerException.Message;

                            if (m_Logger != null)
                                m_Logger.PrintInfoLine(message);
                        }

                        if (m_StopSync)
                        {
                            if (m_Logger != null)
                                m_Logger.PrintInfoLine("CheckCaseListThreadMethod: Stopping Full sync.");

                            break;
                        }

                    }

                    if (!m_StopSync && FullSync)
                    {
                        foreach (String caseid in list)
                        {
                            Delete(caseid);
                        }
                    }

                    if (m_FileSystemWatcher == null)
                    {
                        if (m_Logger != null)
                            m_Logger.PrintInfoLine("Starting File System " + m_FileSystemLocation + " Monitor service.");
                        StartMonitoringFileSystem(m_FileSystemLocation);
                    }

                    if (!FullSync)
                    {
                        if (m_Logger != null)
                            m_Logger.PrintInfoLine("CheckCaseListThreadMethod: Starting incremental sync...Done.");
                    }
                    else
                    {
                        if (m_Logger != null)
                            m_Logger.PrintInfoLine("CheckCaseListThreadMethod: Starting full sync...Done.");
                    }
                }
                else
                {
                    if (m_Logger != null)
                        m_Logger.PrintInfoLine("CheckCaseListThreadMethod: File System is in Sync with Database.");
                }
            }
            catch (Exception exp)
            {
                if (m_Logger != null)
                    m_Logger.PrintInfoLine("CheckCaseListThreadMethod exp: " + exp.Message);
            }
        }

        protected Byte[] GetImageThumbnail(string path)
        {
            if (File.Exists(Path.Combine(path, "Image0.jpg")))
            {
                if (!File.Exists(Path.Combine(path, "Thumb.jpg")))
                {
                    BitmapImage CargoBitmapImage = new BitmapImage();
                    CargoBitmapImage.BeginInit();
                    CargoBitmapImage.UriSource = new Uri(Path.Combine(path, "Image0.jpg"));
                    CargoBitmapImage.DecodePixelHeight = 50;
                    CargoBitmapImage.EndInit();

                    JpegBitmapEncoder enc = new JpegBitmapEncoder();
                    BitmapFrame frame = BitmapFrame.Create(CargoBitmapImage);
                    enc.Frames.Add(frame);

                    if (File.Exists(Path.Combine(path, "Image1.jpg")))
                    {
                        BitmapImage CargoBitmapImage1 = new BitmapImage();
                        CargoBitmapImage1.BeginInit();
                        CargoBitmapImage1.UriSource = new Uri(Path.Combine(path, "Image1.jpg"));
                        CargoBitmapImage1.DecodePixelHeight = 50;
                        CargoBitmapImage1.EndInit();

                        BitmapFrame frame1 = BitmapFrame.Create(CargoBitmapImage1);
                        enc.Frames.Add(frame1);
                    }

                    using (Stream strm = File.Create(Path.Combine(path, "Thumb.jpg")))
                    {
                        enc.Save(strm);
                    }

                }
                return File.ReadAllBytes(Path.Combine(path, "Thumb.jpg"));
            }
            else if (File.Exists(Path.Combine(path, "Thumb.jpg")))
            {
                return File.ReadAllBytes(Path.Combine(path, "Thumb.jpg"));
            }
            return null;
        }

        protected CaseObject GetCaseObj(String Dir, String casefile)
        {
            long fileLengthinByte = 0;

            try
            {
                CaseObject caseObj = null;                

                using (FileStream fs = new FileStream(Dir + "\\" + casefile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fileLengthinByte = fs.Length;   
                    caseObj = CaseTranslator.Translate(fs);

                    foreach (DataAttachment attach in caseObj.attachments)
                    {
                        if (attach.attachmentType == AttachmentType.EVENT_HISTORY)
                        {
                            using (FileStream fs1 = new FileStream(Dir + "\\" + attach.attachmentId, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                caseObj.EventRecords = EventHistoryTranslator.Translate(fs1);
                            }
                        }
                    }
                }

                return caseObj;

            }
            catch (Exception ex)
            {
                if (m_Logger != null)
                {
                    m_Logger.PrintLine(ex.Message);
                }

                throw;
            }
        }

        protected void CaseListUpdate(string caseId, string objectId, string flightNumber, string user, string comment, int analysisTime, string caseDirectory, bool isReferenceImage,
            Int32 result, DateTime updateTime, bool isArchived, byte[] image, DateTime createTime, string area, bool isCTI, string assignedId, bool DFCMatch, UpdateType updateType)
        {
            lock (CaseListLock)
            {
                if (updateType.Equals(UpdateType.Add))
                {
                    CaseListDataSet.CaseListTableRow caseRow = m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(caseId, isReferenceImage);

                    if (m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(caseId, false) == null)
                    {
                        AddtoCaseList(caseId, objectId, flightNumber, user, comment, analysisTime, caseDirectory, isReferenceImage, result, updateTime,
                            isArchived, image, createTime, area, isCTI, assignedId, DFCMatch);
                    }
                    else
                    {
                        ModifyCaseList(caseId, objectId, flightNumber, user, comment, analysisTime, caseDirectory, isReferenceImage,
                            result, updateTime, isArchived, image, createTime, area, isCTI, assignedId, DFCMatch);
                    }
                }
                else if (updateType.Equals(UpdateType.Modify))
                {
                    CaseListDataSet.CaseListTableRow foundRow = (CaseListDataSet.CaseListTableRow)m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(caseId, isReferenceImage);

                    if (foundRow != null)
                    {
                        ModifyCaseList(caseId, objectId, flightNumber, user, comment, analysisTime, caseDirectory, isReferenceImage,
                            result, updateTime, isArchived, image, createTime, area, isCTI, assignedId, DFCMatch);
                    }
                    else
                    {
                        AddtoCaseList(caseId, objectId, flightNumber, user, comment, analysisTime, caseDirectory, isReferenceImage, result, updateTime, isArchived, image, createTime, area, isCTI, assignedId, DFCMatch);
                    }
                }
                else if (updateType.Equals(UpdateType.Delete))
                {
                    DeleteFromCaseList(caseId);
                }
            }
        }

        protected void AddtoCaseList(string caseId, string objectId, string flightNumber, string user, string comment, int analysisTime, string caseDirectory,
            bool isReferenceImage, Int32 result, DateTime updateTime, bool isArchived, byte[] image, DateTime createTime, string area, bool isCTI, string assignedId, bool DFCMatch)
        {
            try
            {
                CaseListDataSet.CaseListTableRow caseRow = m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(caseId, isReferenceImage);

                if (m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(caseId, false) == null)
                {
                    if (image == null && _generateThumbnail)
                    {
                        try
                        {
                            DirectoryInfo dirInfo = new DirectoryInfo(caseDirectory);
                            FileInfo[] pxeFiles = dirInfo.GetFiles("*.pxe", SearchOption.TopDirectoryOnly);

                            if (pxeFiles.Length > 0)
                            {
                                Thumbnail tn = new Thumbnail();
                                tn.CreateJPEGFromFile(pxeFiles[0]);
                                image = GetImageThumbnail(caseDirectory);
                            }

                            //create the image here
                        }
                        catch
                        {
                            if (m_Logger != null)
                            {
                                m_Logger.PrintLine(ErrorMessages.THUMBNAIL_CREATE_FAIL);
                            }
                        }
                    }


                    caseRow = m_CaseListDataSet.CaseListTable.NewCaseListTableRow();

                    caseRow[m_CaseListDataSet.CaseListTable.CaseIdColumn] = caseId;
                    caseRow[m_CaseListDataSet.CaseListTable.ObjectIdColumn] = objectId;
                    caseRow[m_CaseListDataSet.CaseListTable.FlightNumberColumn] = flightNumber;
                    caseRow[m_CaseListDataSet.CaseListTable.AnalystColumn] = user;
                    caseRow[m_CaseListDataSet.CaseListTable.AnalystCommentColumn] = comment;
                    caseRow[m_CaseListDataSet.CaseListTable.AnalysisTimeColumn] = analysisTime;
                    caseRow[m_CaseListDataSet.CaseListTable.CaseDirectoryColumn] = caseDirectory;
                    caseRow[m_CaseListDataSet.CaseListTable.ReferenceImageColumn] = isReferenceImage;
                    caseRow[m_CaseListDataSet.CaseListTable.ResultColumn] = result;
                    caseRow[m_CaseListDataSet.CaseListTable.UpdateTimeColumn] = updateTime;
                    caseRow[m_CaseListDataSet.CaseListTable.ImageColumn] = image;
                    caseRow[m_CaseListDataSet.CaseListTable.CreateTimeColumn] = createTime;
                    caseRow[m_CaseListDataSet.CaseListTable.AreaColumn] = area;
                    caseRow[m_CaseListDataSet.CaseListTable.CTIColumn] = isCTI;
                    caseRow[m_CaseListDataSet.CaseListTable.AssignedIdColumn] = assignedId;
					caseRow[m_CaseListDataSet.CaseListTable.DFCMatchColumn] = DFCMatch;

                    if (m_CaseListDataSet.CaseListTable.Count == 0)
                    {
                        m_CaseListDataSet.CaseListTable.AddCaseListTableRow(caseRow);
                    }
                    else
                    {
                        int pos = m_CaseListDataSet.CaseListTable.Count;

                        foreach (CaseListDataSet.CaseListTableRow row in m_CaseListDataSet.CaseListTable)
                        {
                            int CompareResult = caseRow.UpdateTime.CompareTo(row.UpdateTime) * -1;

                            //This instance is earlier than value. 
                            if (CompareResult < 0)
                            {
                                pos = m_CaseListDataSet.CaseListTable.Rows.IndexOf(row);
                                break;
                            }
                        }
                        m_CaseListDataSet.CaseListTable.Rows.InsertAt(caseRow, pos);
                    }

                    if (m_StartUpdate)
                    {
                        InformOfupdate();
                    }

                }
            }
            catch (Exception ex)
            {
                if (m_Logger != null)
                {
                    m_Logger.PrintLine(ex.Message);
                }

                throw;
            }
        }

        protected void ModifyCaseList(string caseId, string objectId, string flightNumber, string user, string comment, int analysisTime, string caseDirectory,
            bool isReferenceImage, Int32 result, DateTime updateTime, bool isArchived, byte[] image, DateTime createTime, string area, bool isCTI, string assignedId, bool DFCMatch)
        {
            try
            {
                CaseListDataSet.CaseListTableRow foundRow = (CaseListDataSet.CaseListTableRow)m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(caseId, isReferenceImage);

                if (foundRow != null)
                {
                    if (image == null && foundRow.Image == null)
                    {
                        try
                        {
                            DirectoryInfo dirInfo = new DirectoryInfo(caseDirectory);
                            FileInfo[] pxeFiles = dirInfo.GetFiles("*.pxe", SearchOption.TopDirectoryOnly);

                            if (pxeFiles.Length > 0)
                            {
                                Thumbnail tn = new Thumbnail();
                                tn.CreateJPEGFromFile(pxeFiles[0]);
                                image = GetImageThumbnail(caseDirectory);
                            }
                        }
                        catch
                        {
                            if (m_Logger != null)
                            {
                                m_Logger.PrintLine(ErrorMessages.THUMBNAIL_CREATE_FAIL);
                            }
                        }
                    }


                    foundRow.BeginEdit();

                    if (foundRow.ObjectId != objectId && !String.IsNullOrWhiteSpace(objectId))
                        foundRow.ObjectId = objectId;

                    if (foundRow.FlightNumber != flightNumber && !String.IsNullOrWhiteSpace(flightNumber))
                        foundRow.FlightNumber = flightNumber;

                    if (foundRow.Analyst != user && !String.IsNullOrWhiteSpace(user))
                        foundRow.Analyst = user;

                    if (foundRow.AnalystComment != comment && !String.IsNullOrWhiteSpace(comment))
                        foundRow.AnalystComment = comment;

                    if (foundRow.AnalysisTime != analysisTime)
                        foundRow.AnalysisTime = analysisTime;

                    if (foundRow.CaseDirectory != caseDirectory && !String.IsNullOrWhiteSpace(caseDirectory))
                        foundRow.CaseDirectory = caseDirectory;

                    if (foundRow.Result != result)
                        foundRow.Result = result;

                    if (foundRow.UpdateTime != updateTime && updateTime != null)
                        foundRow.UpdateTime = updateTime;

                    if (foundRow.Archived != isArchived)
                        foundRow.Archived = isArchived;

                    if (foundRow.Image != image && image != null)
                        foundRow.Image = image;

                    if (foundRow.CreateTime != createTime)
                        foundRow.CreateTime = createTime;

                    if (foundRow.Area != area && !String.IsNullOrWhiteSpace(area))
                        foundRow.Area = area;

                    if (foundRow.CTI != isCTI)
                        foundRow.CTI = isCTI;

                    if (foundRow.AssignedId != assignedId && assignedId != null)
                        foundRow.AssignedId = assignedId;

                    if (foundRow.DFCMatch != DFCMatch)
                		foundRow.DFCMatch = DFCMatch;

                        foundRow.EndEdit();

                    if (m_StartUpdate)
                        InformOfupdate();
                }
            }
            catch (Exception ex)
            {
                if (m_Logger != null)
                {
                    m_Logger.PrintLine(ex.Message);
                }

                throw;
            }
        }

        protected void DeleteFromCaseList(string caseId)
        {
            try
            {
                CaseListDataSet.CaseListTableRow foundRow = (CaseListDataSet.CaseListTableRow)m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(caseId, m_IsReferenceMode);

                if (foundRow != null)
                {
                    foundRow.Delete();

                    if (m_StartUpdate)
                        InformOfupdate();
                }
            }
            catch (Exception ex)
            {
                if (m_Logger != null)
                {
                    m_Logger.PrintLine(ex.Message);
                }

                throw;
            }
        }


        #endregion Protected Methods


        #region Public Methods
		
		public void configFullSync()
        {            
            m_FullSyncPeriodicityDays = (UInt16)UInt16.Parse(ConfigurationManager.AppSettings["FullSyncPeriodicityDays"]);

            try
            {
                m_lastFullSyncDatetime = DateTime.Parse(ConfigurationManager.AppSettings["LastFullSyncDateTime"], CultureResources.getDefaultDisplayCulture());
            }
            catch
            {
                m_lastFullSyncDatetime = DateTime.Now;
            }

            DateTime NextFullSyncDateTime = m_lastFullSyncDatetime.AddDays(m_FullSyncPeriodicityDays);           

            TimeSpan delayIntervalTime = NextFullSyncDateTime.Subtract(m_lastFullSyncDatetime);

            fullSyncTimer = new System.Timers.Timer();
            fullSyncTimer.Interval = delayIntervalTime.TotalMilliseconds;
            fullSyncTimer.Elapsed += new ElapsedEventHandler(fullSyncTimer_Elapsed);
            fullSyncTimer.Start();

            m_StopSync = false;
        }

        public void StartFullSync()
        {           
            if (m_CheckCaselistThread != null)
            {
                m_CheckCaselistThread.Abort();
                m_CheckCaselistThread.Join();
            }

            m_CheckCaselistThread = new Thread(new ParameterizedThreadStart(delegate { CheckCaseListThreadMethod(true); }));
            m_CheckCaselistThread.Start();

            m_StopSync = false;            
        }       

        virtual public void Dispose()
        {
            if (m_CaseListDataSet != null)
                m_CaseListDataSet.Dispose();

            if (m_CheckCaselistThread != null)
                m_CheckCaselistThread.Abort();

            fullSyncTimer.Stop();
            fullSyncTimer.Dispose();
            fullSyncTimer = null;
        }

        public void Add(string caseId, string objectId, string flightNumber, string user, string comment, int analysisTime, string caseDirectory,
            bool isReferenceImage, string result, DateTime updateTime, bool isArchived, byte[] image, DateTime createTime, string area, bool isCTI, string assignedId, bool DFCMatch)
        {
            Int32 decision = 0;

            if (!String.IsNullOrWhiteSpace(result))
            {
                decision = (Int32)Enum.Parse(typeof(L3.Cargo.Communications.Interfaces.WorkstationDecision), result);
            }

            CaseListUpdate(caseId, objectId, flightNumber, user, comment, analysisTime, caseDirectory, isReferenceImage,
                        decision, updateTime, isArchived, image, createTime, area, isCTI, assignedId, DFCMatch, UpdateType.Add);
        }

        public void Add(string caseId, string caseDir, DateTime createTime, string area, bool isCTI)
        {
            Add(caseId, string.Empty, string.Empty, string.Empty, string.Empty, 0, caseDir, false,
                string.Empty, DateTime.Now, false, null, createTime, area, isCTI, string.Empty, false);
        }

        public void Add(string caseId, string caseDir, DateTime createTime, string area)
        {
            Add(caseId, string.Empty, string.Empty, string.Empty, string.Empty, 0, caseDir, false,
                 string.Empty, DateTime.Now, false, null, createTime, area, false, string.Empty, false);
        }

        public void Add(CaseListDataSet.CaseListTableRow newRow)
        {
            try
            {
                if (m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(newRow.CaseId, newRow.ReferenceImage) == null)
                {
                    lock (CaseListLock)
                    {
                        m_CaseListDataSet.CaseListTable.AddCaseListTableRow(newRow);
                    }
                }
                else
                {
                    Modify(newRow);
                }

                if (m_StartUpdate)
                    InformOfupdate();
            }
            catch (Exception ex)
            {
                if (m_Logger != null)
                {
                    m_Logger.PrintLine(ex.Message);
                }

                throw;
            }
        }

        public void Modify(string caseId, string objectId, string flightNumber, string user, string comment, int analysisTime, string caseDirectory,
                    bool isReferenceImage, Int32 result, DateTime updateTime, bool isArchived, byte[] image, DateTime createTime, string area, bool isCTI, string assignedId, bool DFCMatch)
        {
            CaseListUpdate(caseId, objectId, flightNumber, user, comment, analysisTime, caseDirectory, isReferenceImage,
            result, updateTime, isArchived, image, createTime, area, isCTI, assignedId, DFCMatch, UpdateType.Modify);
        }

        public void Modify(string caseId, string objectId, string flightNumber, string user, string comment, int analysisTime, string caseDirectory,
            bool isReferenceImage, string result, DateTime updateTime, byte[] image, DateTime createTime, string area, bool isCTI)
        {
            CaseListDataSet.CaseListTableRow row = m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(caseId, isReferenceImage);

            if (row != null)
            {
                Int32 decision = (Int32) Enum.Parse(typeof(WorkstationDecision), result);
                //DateTime createtime = DateTime.Parse(createTime);

                Modify(caseId, objectId, flightNumber, user, comment, analysisTime, caseDirectory, isReferenceImage,
                    decision, updateTime, row.Archived, image, createTime, area, isCTI, row.AssignedId, row.DFCMatch);
            }
            else
            {
                throw new Exception(ErrorMessages.CASE_DOES_NOT_EXIST);
            }
        }

        public void Modify(string caseId, string objectId, string flightNumber, string user, string comment, int analysisTime, string caseDirectory,
            bool isReferenceImage, string result, DateTime updateTime, byte[] image, DateTime createTime, string area)
        {
            CaseListDataSet.CaseListTableRow row = m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(caseId, isReferenceImage);

            if (row != null)
            {
                Int32 decision = (Int32)Enum.Parse(typeof(WorkstationDecision), result);
                //DateTime createtime = DateTime.Parse(createTime);

                Modify(caseId, objectId, flightNumber, user, comment, analysisTime, caseDirectory, isReferenceImage,
                    decision, updateTime, row.Archived, image, createTime, area, row.CTI, row.AssignedId, row.DFCMatch);
            }
            else
            {
                throw new Exception(ErrorMessages.CASE_DOES_NOT_EXIST);
            }
        }

        public void Modify(CaseListDataSet.CaseListTableRow newRow)
        {
            CaseListDataSet.CaseListTableRow row = m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(newRow.CaseId, newRow.ReferenceImage);

            if (row != null)
            {
                Modify(newRow.CaseId, newRow.ObjectId, newRow.FlightNumber, newRow.Analyst, newRow.AnalystComment, newRow.AnalysisTime, newRow.CaseDirectory,
                    newRow.ReferenceImage, newRow.Result, newRow.UpdateTime, newRow.Archived, newRow.Image, newRow.CreateTime, newRow.Area, newRow.CTI, newRow.AssignedId, newRow.DFCMatch);
            }
            else
            {
                throw new Exception(ErrorMessages.CASE_DOES_NOT_EXIST);
            }
        }

        public void ModifyArchived(string caseId, bool isArchived)
        {
            CaseListDataSet.CaseListTableRow row = m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(caseId, m_IsReferenceMode);

            if (row != null)
            {
                Modify(row.CaseId, row.ObjectId, row.FlightNumber, row.Analyst, row.AnalystComment, row.AnalysisTime, row.CaseDirectory,
                    row.ReferenceImage, row.Result, row.UpdateTime, isArchived, row.Image, row.CreateTime, row.Area, row.CTI, row.AssignedId, row.DFCMatch);
            }
            else
            {
                throw new Exception(ErrorMessages.CASE_DOES_NOT_EXIST);
            }
        }

        public void ModifyCommentResult(string caseId, string comment, Int32 result)
        {
            CaseListDataSet.CaseListTableRow row = m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(caseId, m_IsReferenceMode);

            if (row != null)
            {
                Modify(row.CaseId, row.ObjectId, row.FlightNumber, row.Analyst, comment, row.AnalysisTime, row.CaseDirectory,
                    row.ReferenceImage, result, row.UpdateTime, row.Archived, row.Image, row.CreateTime, row.Area, row.CTI, row.AssignedId, row.DFCMatch);
            }
            else
            {
                throw new Exception(ErrorMessages.CASE_DOES_NOT_EXIST);
            }
        }

        public void ModifyComment(string caseId, string comment)
        {
            CaseListDataSet.CaseListTableRow row = m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(caseId, m_IsReferenceMode);

            if (row != null)
            {
                Modify(row.CaseId, row.ObjectId, row.FlightNumber, row.Analyst, comment, row.AnalysisTime, row.CaseDirectory,
                    row.ReferenceImage, row.Result, row.UpdateTime, row.Archived, row.Image, row.CreateTime, row.Area, row.CTI, row.AssignedId, row.DFCMatch);
            }
            else
            {
                throw new Exception(ErrorMessages.CASE_DOES_NOT_EXIST);
            }
        }

        public void ModifyAssignId(string caseId, string assignedId)
        {
            try
            {
                CaseListDataSet.CaseListTableRow row = m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(caseId, m_IsReferenceMode);

                if (row != null)
                {
                    Modify(row.CaseId, row.ObjectId, row.FlightNumber, row.Analyst, row.AnalystComment, row.AnalysisTime, row.CaseDirectory,
                        row.ReferenceImage, row.Result, row.UpdateTime, row.Archived, row.Image, row.CreateTime, row.Area, row.CTI, assignedId, row.DFCMatch);
                }
                else
                {
                    throw new Exception(ErrorMessages.CASE_DOES_NOT_EXIST);
                }
            }
            catch
            {
                throw;
            }
        }

        public void ModifyDFCMatch(string caseId, bool match)
        {
            CaseListDataSet.CaseListTableRow row = m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(caseId, m_IsReferenceMode);

            if (row != null)
            {
                Modify(row.CaseId, row.ObjectId, row.FlightNumber, row.Analyst, row.AnalystComment, row.AnalysisTime, row.CaseDirectory,
                    row.ReferenceImage, row.Result, row.UpdateTime, row.Archived, row.Image, row.CreateTime, row.Area, row.CTI, row.AssignedId, match);
            }
            else
            {
                throw new Exception(ErrorMessages.CASE_DOES_NOT_EXIST);
            }
        }
        public virtual void Delete(string caseId)
        {
            CaseListUpdate(caseId, string.Empty, string.Empty, string.Empty, string.Empty, 0, string.Empty, m_IsReferenceMode, 0, DateTime.Now,
                false, null, DateTime.Now, string.Empty, false, string.Empty, false, UpdateType.Delete);
        }

        public virtual void UpdateCaseAsReference(string caseId)
        {
            try
            {
                string caseDirectory = GetCaseDirectory(caseId);

                string ReferenceDirectory = (String)ConfigurationManager.AppSettings["ReferenceFileSystemPath"];
                ReferenceDirectory = caseDirectory.Replace((String)ConfigurationManager.AppSettings["CaseListFileSystemPath"], ReferenceDirectory);

                if (caseDirectory != null && caseDirectory.Length > 0)
                {
                    Copy(caseDirectory, ReferenceDirectory);
                }
            }
            catch (Exception ex)
            {
                if (m_Logger != null)
                {
                    m_Logger.PrintLine(ex.Message);
                }

                throw;
            }
        }

        public virtual void UpdateCaseAddAttachment(string caseid, Stream file, string filename, string user, string CreateTime, string type)
        {
            try
            {
                if (!m_IsReferenceMode)
                {
                    //Case is updated with Result, update the case list dataset.
                    CaseListDataSet.CaseListTableRow foundRow = List.CaseListTable.FindByCaseIdReferenceImage(caseid, m_IsReferenceMode);

                    if (foundRow != null)
                    {
						foundRow.BeginEdit();
                    	foundRow[List.CaseListTable.UpdateTimeColumn] = DateTime.Now;
                        foundRow.EndEdit();
                        string caseDirectory = GetCaseDirectory(caseid);

                        if (filename.Length > 0)
                        {
                            using (FileStream stream = new FileStream(Path.Combine(caseDirectory, filename), FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }

                            CaseObject caseObj = null;

                            using (FileStream fs = File.Open(Path.Combine(caseDirectory, "case.xml"), FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                            {
                                DataAttachment attach = new DataAttachment();
                                attach.attachmentType = (AttachmentType)Enum.Parse(typeof(AttachmentType), type);
                                attach.CreateTime = CreateTime;
                                attach.attachmentId = filename;
                                attach.User = user;
                                caseObj = CaseTranslator.AddAttachment(fs, attach, caseDirectory);
                            }

                            FileStream f = File.Open(Path.Combine(caseDirectory, "case.xml"), FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                            Stream f1 = f;
                            CaseTranslator.Translate(caseObj, ref f1, caseDirectory);
                            f.Close();

                            Modify(foundRow.CaseId, foundRow.ObjectId, foundRow.FlightNumber, foundRow.Analyst, foundRow.AnalystComment, foundRow.AnalysisTime, foundRow.CaseDirectory,
                                foundRow.ReferenceImage, foundRow.Result, DateTime.Parse(CreateTime, CultureResources.getDefaultDisplayCulture()), foundRow.Archived, foundRow.Image, foundRow.CreateTime, foundRow.Area, foundRow.CTI, foundRow.AssignedId, foundRow.DFCMatch);
                        }
                    }
                    else
                    {
                        throw new Exception(ErrorMessages.FILENAME_EMPTY);
                    }
                }
            }
            catch (Exception ex)
            {
                if (m_Logger != null)
                {
                    m_Logger.PrintLine(ex.Message);
                }

                throw;
            }
        }

        public virtual void UpdateCaseAddResult(string caseid, string AnalysisTime, string Comment, string user, string CreateTime, string Reason, string StationType, string Decision)
        {
            try
            {
                if (!m_IsReferenceMode)
                {
                    //Case is updated with Result, update the case list dataset.
                    CaseListDataSet.CaseListTableRow foundRow = List.CaseListTable.FindByCaseIdReferenceImage(caseid, m_IsReferenceMode);

                    if (foundRow != null)
                    {
						foundRow.BeginEdit();
                        foundRow[List.CaseListTable.UpdateTimeColumn] = DateTime.Now;
                        foundRow.EndEdit();
                        string caseDirectory = GetCaseDirectory(caseid);

                        CaseObject caseObj = null;

                        using (FileStream fs = File.Open(Path.Combine(caseDirectory, "case.xml"), FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            result res = new result(Decision, Reason, CreateTime, user, Comment, StationType, AnalysisTime);
                            caseObj = CaseTranslator.AddResult(fs, res, caseDirectory);
                        }

                        FileStream f = File.Open(Path.Combine(caseDirectory, "case.xml"), FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                        Stream f1 = f;
                        CaseTranslator.Translate(caseObj, ref f1, caseDirectory);
                        f.Close();

                        Int32 decision = (Int32)Enum.Parse(typeof(L3.Cargo.Communications.Interfaces.WorkstationDecision), Decision);

                        Modify(foundRow.CaseId, foundRow.ObjectId, foundRow.FlightNumber, user, Comment, Int32.Parse(AnalysisTime), foundRow.CaseDirectory,
                            foundRow.ReferenceImage, decision, DateTime.Parse(CreateTime, CultureResources.getDefaultDisplayCulture()), foundRow.Archived, foundRow.Image, foundRow.CreateTime, foundRow.Area, foundRow.CTI, foundRow.AssignedId, foundRow.DFCMatch);
                    }
                }
            }
            catch (Exception ex)
            {
                if (m_Logger != null)
                {
                    m_Logger.PrintLine(ex.Message);
                }

                throw;
            }
        }

        public void updateObjectID(String caseID, String ContainerID, String CreateTime)
        {
            try
            {
                if (!m_IsReferenceMode)
                {
                    CaseListDataSet.CaseListTableRow foundRow = List.CaseListTable.FindByCaseIdReferenceImage(caseID, m_IsReferenceMode);

                    if (foundRow != null)
                    {
						foundRow.BeginEdit();
                        foundRow[List.CaseListTable.ObjectIdColumn] = ContainerID;
                        foundRow[List.CaseListTable.UpdateTimeColumn] = DateTime.Now;
                        foundRow.EndEdit();
                        String caseDirectory = GetCaseDirectory(caseID);

                        CaseObject caseObj;
                        using (FileStream fs = File.Open(caseDirectory + "\\case.xml", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            caseObj = CaseTranslator.Translate(fs, caseDirectory);
                            caseObj.scanInfo.container.Id = ContainerID;
                        }

                        FileStream f = File.Open(caseDirectory + "\\case.xml", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                        Stream f1 = f;
                        CaseTranslator.Translate(caseObj, ref f1, caseDirectory);
                        f.Close();

                        Modify(foundRow.CaseId, ContainerID, foundRow.FlightNumber, foundRow.Analyst, foundRow.AnalystComment, foundRow.AnalysisTime, foundRow.CaseDirectory,
                            foundRow.ReferenceImage, foundRow.Result, DateTime.Parse(CreateTime, CultureResources.getDefaultDisplayCulture()), foundRow.Archived, foundRow.Image, foundRow.CreateTime, foundRow.Area, foundRow.CTI, foundRow.AssignedId, foundRow.DFCMatch);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool CaseExists(string caseId)
        {
            bool ret = false;

            if (m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(caseId, m_IsReferenceMode) != null)
            {
                ret = true;
            }

            return ret;
        }

        public bool IsArchived(string caseId)
        {
            try
            {
                CaseListDataSet.CaseListTableRow foundRow = m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(caseId, m_IsReferenceMode);

                if (foundRow != null)
                {
                    return foundRow.Archived;
                }
                else
                {
                    throw new CaseIdNotFoundException(ErrorMessages.CASE_NOT_LISTED + caseId);
                }
            }
            catch (Exception ex)
            {
                if (m_Logger != null)
                {
                    m_Logger.PrintLine(ex.Message);
                }

                throw;
            }
        }

        public bool IsCTI(string caseId)
        {
            try
            {
                bool ret = false;

                CaseListDataSet.CaseListTableRow foundRow = m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(caseId, m_IsReferenceMode);

                if (foundRow != null)
                {
                    ret = foundRow.CTI;
                }
                else
                {
                    throw new CaseIdNotFoundException(ErrorMessages.CASE_NOT_LISTED + caseId);
                }

                return ret;
            }
            catch (Exception ex)
            {
                if (m_Logger != null)
                {
                    m_Logger.PrintLine(ex.Message);
                }

                throw;
            }
        }

        public bool IsAssigned(string caseId)
        {
            bool ret = false;

            try
            {
                CaseListDataSet.CaseListTableRow foundRow = m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(caseId, m_IsReferenceMode);

                if (foundRow != null)
                {
                    ret = !String.IsNullOrWhiteSpace(foundRow.AssignedId);
                }
            }
            catch (Exception ex)
            {
                if (m_Logger != null)
                {
                    m_Logger.PrintLine(ex.Message);
                }
            }
            return ret;
        }


        public string AssignedId(string caseId)
        {
            try
            {
                CaseListDataSet.CaseListTableRow foundRow = m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(caseId, m_IsReferenceMode);

                if (foundRow != null)
                {
                    return foundRow.AssignedId;
                }
                else
                {
                    throw new CaseIdNotFoundException(ErrorMessages.CASE_NOT_LISTED + caseId);
                }
            }
            catch (Exception ex)
            {
                if (m_Logger != null)
                {
                    m_Logger.PrintLine(ex.Message);
                }

                throw;
            }
        }

        public void ClearAssignments(string workstationId)
        {
            try
            {
                DataRow[] foundRow;

                lock (CaseListLock)
                {
                    foundRow = m_CaseListDataSet.CaseListTable.Select(String.Format("AssignedId = '{0}'", workstationId));
                }

                if (foundRow != null)
                {
                    foreach (DataRow row in foundRow)
                    {
                        ClearAssignment(row[m_CaseListDataSet.CaseListTable.CaseIdColumn].ToString());
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public string[] GetAssignedCaseID(string workstationId)
        {
            try
            {
                DataRow[] foundRow;
                string[] caseidList = null;

                lock (CaseListLock)
                {
                    foundRow = m_CaseListDataSet.CaseListTable.Select(String.Format("AssignedId = '{0}'", workstationId));
                }

                if (foundRow != null)
                {
                    caseidList = new string[foundRow.Length];
                    int index = 0;

                    foreach (DataRow row in foundRow)
                    {
                        caseidList[index++] = row[m_CaseListDataSet.CaseListTable.CaseIdColumn].ToString();
                    }
                }

                return caseidList;
            }
            catch
            {
                throw;
            }
        }

        public void ClearAssignment(string caseId)
        {
            try
            {
                CaseListDataSet.CaseListTableRow foundRow = m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(caseId, m_IsReferenceMode);

                if (foundRow != null)
                {
                    Modify(foundRow.CaseId, foundRow.ObjectId, foundRow.FlightNumber, foundRow.Analyst, foundRow.AnalystComment, foundRow.AnalysisTime, foundRow.CaseDirectory,
                            foundRow.ReferenceImage, foundRow.Result, foundRow.UpdateTime, foundRow.Archived, foundRow.Image, foundRow.CreateTime, foundRow.Area, foundRow.CTI, string.Empty, foundRow.DFCMatch);
                }
            }
            catch
            {
                throw;
            }
        }

        public string GetUnassignedCaseId(string workstationMode)
        {
            string caseId = string.Empty;
            string expression = "Area = '" + workstationMode + "' AND AssignedId = ''";
            string sortOrder = "CreateTime ASC";
            CaseListDataSet.CaseListTableRow[] foundRows = (CaseListDataSet.CaseListTableRow[])m_CaseListDataSet.CaseListTable.Select(expression, sortOrder);

            if(foundRows.Length > 0)
            {
                caseId = foundRows[0].CaseId;                
            }

            return caseId;
        }

        public string GetCaseDirectory(string caseId)
        {
            try
            {
                CaseListDataSet.CaseListTableRow foundRow = m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(caseId, m_IsReferenceMode);

                if (foundRow != null)
                {
                    return foundRow.CaseDirectory;
                }
                else
                {
                    throw new CaseIdNotFoundException(ErrorMessages.CASE_NOT_LISTED + caseId);
                }
            }
            catch (Exception ex)
            {
                if (m_Logger != null)
                {
                    m_Logger.PrintLine(ex.Message);
                }

                throw;
            }
        }

        public string GetComment(string caseId)
        {
            try
            {
                CaseListDataSet.CaseListTableRow foundRow = m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(caseId, m_IsReferenceMode);

                if (foundRow != null)
                {
                    return foundRow.AnalystComment;
                }
                else
                {
                    throw new CaseIdNotFoundException(ErrorMessages.CASE_NOT_LISTED + caseId);
                }
            }
            catch (Exception ex)
            {
                if (m_Logger != null)
                {
                    m_Logger.PrintLine(ex.Message);
                }

                throw;
            }
        }

        public string GetDecision(string caseId)
        {
            try
            {
                CaseListDataSet.CaseListTableRow foundRow = m_CaseListDataSet.CaseListTable.FindByCaseIdReferenceImage(caseId, m_IsReferenceMode);

                if (foundRow != null)
                {
                    return Enum.GetName(typeof(WorkstationDecision), foundRow.Result);
                }
                else
                {
                    throw new CaseIdNotFoundException(ErrorMessages.CASE_NOT_LISTED + caseId);
                }
            }
            catch (Exception ex)
            {
                if (m_Logger != null)
                {
                    m_Logger.PrintLine(ex.Message);
                }

                throw;
            }
        }

        public virtual void PopulateCaseList()
        {
        }

        public void PopulateCaseListFromXMLfile(string filename)
        {
            try
            {
                m_CaseListDataSet.ReadXml(filename);
            }
            catch (Exception ex)
            {
                if (m_Logger != null)
                {
                    m_Logger.PrintLine(ex.Message);
                }

                throw;
            }
        }

        public virtual void StartMonitoringFileSystem(String location)
        {
            m_FileSystemWatcher = new FileSystemWatcher();

            m_FileSystemWatcher.Path = location;
            m_FileSystemWatcher.IncludeSubdirectories = true;

            //DirectoryName is used when case is removed from the directory being monitored.
            m_FileSystemWatcher.NotifyFilter =  NotifyFilters.LastWrite | NotifyFilters.DirectoryName;
            m_FileSystemWatcher.Filter = "*.*";

            //m_FileSystemWatcher.Created += new FileSystemEventHandler(OnChanged);
            m_FileSystemWatcher.Deleted += new FileSystemEventHandler(OnChanged);
            m_FileSystemWatcher.Changed += new FileSystemEventHandler(OnChanged);
            m_FileSystemWatcher.Error += new ErrorEventHandler(FSWatcher_Error);

            /* You can set the buffer to 4 KB or larger, but it must not exceed 64 KB. For best performance, use a multiple of 4 KB on Intel-based computers.*/
            /* The internal buffer size. The default is 8192 (8 KB). */
            m_FileSystemWatcher.InternalBufferSize = 65536;

            m_FileSystemWatcher.EnableRaisingEvents = true;

            m_Logger.PrintInfoLine("Starting File System " + m_FileSystemLocation + " Monitor service.");
        }

        public virtual void UpdateCaseList(bool modifyExistingEntry, string casefile)
        {
            try
            {
                bool WasAbleToAccessFile = false;
                DateTime StartTime = DateTime.Now;
                DateTime EndTime = DateTime.Now;

                while (!WasAbleToAccessFile && ((EndTime.Second - StartTime.Second) <= 10.0))
                {
                    try
                    {
                        FileInfo file = new FileInfo(casefile);
                        CaseObject caseObj = GetCaseObj(file.DirectoryName, file.Name);

                        WasAbleToAccessFile = true;

                        if (caseObj != null)
                        {
                            string userid = string.Empty;
                            string objectId = string.Empty;
                            string awsresult = L3.Cargo.Communications.Interfaces.WorkstationDecision.Unknown.ToString();
                            string comment = string.Empty;
                            string caseid = string.Empty;
                            string casepath = file.DirectoryName;
                            string FlightNumber = string.Empty;
                            DateTime CreateTime = caseObj.createTime;
                            string Area = caseObj.CurrentArea;

                            if (caseObj.CaseId != null)
                            {
                                caseid = caseObj.CaseId;
                            }

                            if (caseObj.scanInfo != null && caseObj.scanInfo.container != null && caseObj.scanInfo.container.Id != null)
                            {
                                objectId = caseObj.scanInfo.container.Id;
                            }

                            if (caseObj.ResultsList != null && caseObj.ResultsList.Count > 0)
                            {

                                result tmpResult = caseObj.ResultsList[0];// xc.results[0];

                                foreach (result result in caseObj.ResultsList)
                                {
                                    if (result.CreateTime.CompareTo(tmpResult.CreateTime) > 0)
                                        tmpResult = result;
                                }

                                if (tmpResult.Comment != null)
                                {
                                    comment = tmpResult.Comment;
                                }

                                if (tmpResult.Decision != null)
                                {
                                    //awsresult = (Int32)Enum.Parse(typeof(L3.Cargo.Communications.Interfaces.WorkstationDecision), tmpResult.Decision);
                                    awsresult = tmpResult.Decision;
                                }

                                userid = tmpResult.User;
                            }

                            CaseObject.CaseEventRecord lastEventRecord = new CaseObject.CaseEventRecord();
                            lastEventRecord.createTime = caseObj.createTime;

                            if (caseObj.EventRecords != null && caseObj.EventRecords.Count > 0)
                            {
                                lastEventRecord = caseObj.EventRecords[0];

                                foreach (CaseObject.CaseEventRecord record in caseObj.EventRecords)
                                {
                                    if (record.createTime > lastEventRecord.createTime)
                                        lastEventRecord = record;
                                }
                            }

                            Byte[] thumbdata = GetImageThumbnail(file.DirectoryName);

                            if (caseObj.CurrentArea == null)
                            {
                                Area = string.Empty;
                            }

                            if (modifyExistingEntry)
                            {
                                try
                                {
                                    Modify(caseid, objectId, FlightNumber, userid, comment, 0, file.DirectoryName, m_IsReferenceMode, awsresult, lastEventRecord.createTime, thumbdata, CreateTime, Area, m_isDFCMatch);
                                }
                                catch
                                {
                                    Add(caseid, objectId, FlightNumber, userid, comment, 0, file.DirectoryName, m_IsReferenceMode, awsresult, lastEventRecord.createTime, false, thumbdata, CreateTime, Area, false, string.Empty, m_isDFCMatch);
                                }
                            }
                            else
                            {
                                Add(caseid, objectId, FlightNumber, userid, comment, 0, file.DirectoryName, m_IsReferenceMode, awsresult, lastEventRecord.createTime, false, thumbdata, CreateTime, Area, false, string.Empty, m_isDFCMatch);
                            }

                        }
                            //record this update time in the config. file
                            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);                           
                            if (config.AppSettings.Settings["CaseListLastUpdateTime"] == null)
                                config.AppSettings.Settings.Add("CaseListLastUpdateTime", CultureResources.ConvertDateTimeToStringForData(file.Directory.LastAccessTime));
                            else
                                config.AppSettings.Settings["CaseListLastUpdateTime"].Value = CultureResources.ConvertDateTimeToStringForData(file.Directory.LastAccessTime);                         
                            config.Save(ConfigurationSaveMode.Modified);
                            ConfigurationManager.RefreshSection("appSettings");  
                    }
                    catch (IOException exp)
                    {
                        Type expType = exp.GetType();

                        if ((expType != typeof(DirectoryNotFoundException)) &&
                            (expType != typeof(EndOfStreamException)) &&
                            (expType != typeof(FileLoadException)) &&
                            (expType != typeof(PathTooLongException)))
                        {
                            WasAbleToAccessFile = false;
                            EndTime = DateTime.Now;

                            Thread.Sleep(500);

                            if ((EndTime.Second - StartTime.Second) > 10.0)
                                throw;
                        }
                        else
                            throw;
                    }
                    catch (InvalidOperationException exp)
                    {
                        WasAbleToAccessFile = false;
                        EndTime = DateTime.Now;

                        Thread.Sleep(500);

                        if ((EndTime.Second - StartTime.Second) > 10.0)
                            throw;
                    }
                    catch (Exception exp)
                    {
                        throw;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SaveCaseListToXMLFile()
        {
            try
            {
                m_FileSystemWatcher.EnableRaisingEvents = false;
                // Create the FileStream to write with.
                using (FileStream stream = new FileStream(m_CaselistFilename, System.IO.FileMode.Create))
                {
                    m_CaseListDataSet.WriteXml(stream);
                }
            }
            catch (Exception exp)
            {
                if (m_Logger != null)
                    m_Logger.PrintInfoLine("Exception occured while saving case list to XML file: " + exp);
            }

        }

        #endregion Public Methods
    }
}
