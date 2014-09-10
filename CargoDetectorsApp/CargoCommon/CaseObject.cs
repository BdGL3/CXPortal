using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using L3.Cargo.Common;
using System.ComponentModel;
using L3.Cargo.Common.Xml.History_1_0;

namespace L3.Cargo.Common
{
    public enum AttachmentType
    {
        XRayImage,
        TDSResultFile,
        Manifest,
        OCR,
        SNM,
        NUC,
        AnalysisHistory,
        History,
        Annotations,
        EVENT_HISTORY,
        FTIImage,
        Unknown
    }


    public enum CaseType
    {
        LiveCase,
        ArchiveCase,
        FTICase,
        CTICase,
        TrainingCase
    }

    public class userInformation
    {
        public String Decision { get; set; }
        public String Reason { get; set; }
        public String Comment { get; set; }
        public String UserID { get; set; }

        public userInformation(String decision, String reason, String comment, String userID)
        {
            Decision = decision;
            Reason = reason;
            Comment = comment;
            UserID = userID;
        }
    }

    public struct AnnotationInfo
    {
        public Point TopLeft;
        public double Width;
        public double Height;
        public double RadiusX;
        public double RadiuxY;
        public String Comment;

        public AnnotationInfo(Point topLeft, double width, double height, double radiusX, double radiusY, String comment)
        {
            TopLeft = topLeft;
            Width = width;
            Height = height;
            RadiusX = radiusX;
            RadiuxY = radiusY;
            Comment = comment;
        }
    }

    public class SystemInfo : INotifyPropertyChanged
    {
        #region private members

        private String m_SystemType;
        private String m_BaseLocation;

        #endregion

        public String SystemType
        {
            get { return m_SystemType; }
            set
            {
                m_SystemType = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("SystemType"));
            }
        }

        public String BaseLocation
        {
            get { return m_BaseLocation; }
            set
            {
                m_BaseLocation = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("BaseLocation"));
            }
        }

        public SystemInfo()
        {
        }

        public SystemInfo(String systemType, String baseLocation)
        {
            SystemType = systemType;
            BaseLocation = baseLocation;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, e);
        }

        #endregion
    }

    public class Container : INotifyPropertyChanged
    {
        #region private members

        private String m_Id;
        private String m_Code;
        private String m_Weight;
        private String m_SequenceNum;

        #endregion

        public String Id
        {
            get { return m_Id; }
            set
            {
                m_Id = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Id"));
            }
        }

        public String Code
        {
            get { return m_Code; }
            set
            {
                m_Code = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Code"));
            }
        }

        public String Weight
        {
            get { return m_Weight; }
            set
            {
                m_Weight = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Weight"));
            }
        }

        public String SequenceNum
        {
            get { return m_SequenceNum; }
            set
            {
                m_SequenceNum = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("SequenceNum"));
            }
        }

        public Container()
        {
        }

        public Container(String id, String code, String weight, String seqNum)
        {
            Id = id;
            Code = code;
            Weight = weight;
            SequenceNum = seqNum;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, e);
        }

        #endregion
    }

    public class Conveyance : INotifyPropertyChanged
    {
        #region private members

        private String m_Id;
        private String m_TotalWeight;
        private String m_BatchNum;

        #endregion


        public String Id
        {
            get { return m_Id; }
            set
            {
                m_Id = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Id"));
            }
        }

        public String TotalWeight
        {
            get { return m_TotalWeight; }
            set
            {
                m_TotalWeight = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("TotalWeight"));
            }
        }

        public String BatchNum
        {
            get { return m_BatchNum; }
            set
            {
                m_BatchNum = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("BatchNum"));
            }
        }

        public Conveyance()
        {
        }

        public Conveyance(String id, String totalWeight, String batchNum)
        {
            Id = id;
            TotalWeight = totalWeight;
            BatchNum = batchNum;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, e);
        }

        #endregion
    }

    public class Location : INotifyPropertyChanged
    {
        #region private members

        private String m_Longitude;
        private String m_Latitude;

        #endregion

        public String Longitude
        {
            get { return m_Longitude; }
            set
            {
                m_Longitude = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Longitude"));
            }
        }

        public String Latitude
        {
            get { return m_Latitude; }
            set
            {
                m_Latitude = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Latitude"));
            }
        }

        public Location(String longitude, String latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, e);
        }

        #endregion
    }

    public class ScanInfo : INotifyPropertyChanged
    {
        #region Private Memebers

        private String m_ScanType;
        private Location m_Location;
        private Conveyance m_Conveyance;
        private Container m_Container;

        #endregion
        public String ScanType
        {
            get { return m_ScanType; }
            set
            {
                m_ScanType = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("ScanType"));
            }
        }

        public Location location
        {
            get { return m_Location; }
            set
            {
                m_Location = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("location"));
            }
        }

        public Conveyance conveyance
        {
            get { return m_Conveyance; }
            set
            {
                m_Conveyance = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("conveyance"));
            }
        }

        public Container container
        {
            get { return m_Container; }
            set
            {
                m_Container = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("container"));
            }
        }

        public ScanInfo()
        {
        }

        public ScanInfo(String scanType, Location loc, Conveyance convey, Container cont)
        {
            ScanType = scanType;
            location = loc;
            conveyance = convey;
            container = cont;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, e);
        }

        #endregion
    }

    public class result
    {
        public string Decision { get; set; }
        public string Reason { get; set; }
        public string CreateTime { get; set; }
        public string User { get; set; }
        public string Comment { get; set; }
        public string StationType { get; set; }
        public string AnalysisTime { get; set; }

        public string CaseId { get; set; }
        public CaseType CaseType { get; set; }
        public string WorkstationId { get; set; }

        public result (string decision, string reason, string createTime, string user, string comment, string stationType, string analysisTime)
        {
            Decision = decision;
            Reason = reason;
            CreateTime = createTime;
            User = user;
            Comment = comment;
            StationType = stationType;
            AnalysisTime = analysisTime;
        }

        public result (string decision, string reason, string createTime, string user, string comment, string stationType, string analysisTime, string caseId, CaseType caseType, string wsId)
        {
            Decision = decision;
            Reason = reason;
            CreateTime = createTime;
            User = user;
            Comment = comment;
            StationType = stationType;
            AnalysisTime = analysisTime;
            CaseId = caseId;
            CaseType = caseType;
            WorkstationId = wsId;
        }
    }

    public class attachment
    {
        public String User { get; set; }
        public String AttachmentType { get; set; }
        public String Filename { get; set; }
        public String CreateTime { get; set; }

        public attachment(String user, String type, String filename, String createTime)
        {
            User = user;
            AttachmentType = type;
            Filename = filename;
            CreateTime = createTime;
        }
    }

    public class CaseObject : INotifyPropertyChanged
    {
        #region Private Members

        private result m_WorkstationResult;

        private string m_SourceAlias;

        #endregion Private Members


        #region Public Members

        public String CaseId {get; set;}

        public String SourceAlias {
            get
            {
                return m_SourceAlias;
            }
            set
            {
                m_SourceAlias = value;
            }
        }

        public SystemInfo systemInfo
        {
            get { return m_systemInfo; }
            set
            {
                m_systemInfo = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("systemInfo"));
            }
        }

        public DateTime createTime { get; set; }

        public ScanInfo scanInfo
        {
            get { return m_ScanInfo; }
            set
            {
                m_ScanInfo = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("scanInfo"));
            }
        }

        public Boolean ScanContainerIdModified { get; set; }

        public List<result> ResultsList { get; set; }

        public DataAttachments attachments { get; set; }

        public Boolean AttachmentsModified;

        public Boolean SetAsReference { get; set; }

        public Boolean SetAsReferenceModified { get; set; }

        public Int32 MaxManifests { get; set; }

        public Boolean IsCaseEditable { get; set; }

        public DateTime AnalysisStartTime;

        public String CurrentArea { get; set; }

        public String LinkedCaseId { get; set; }

        public String AbortedBy { get; set; }
			
        public CaseType caseType { get; set; }

        public Boolean IsTIPResultReturned { get; set; }

        public result WorkstationResult
        {
            get
            {
                return m_WorkstationResult;
            }
            set
            {
                m_WorkstationResult = value;
            }
        }

        public struct CaseEventRecord
        {
            public DateTime createTime;
            public String description;
            public Boolean IsNew;

            public CaseEventRecord(DateTime crtTime, String Desc, Boolean isNew)
            {
                createTime = crtTime;
                description = Desc;
                IsNew = isNew;
            }
        }

        public Histories CaseHistories { get; set; }

        public Boolean EventRecordsModified { get; set; }

        public List<CaseEventRecord> EventRecords { get; set; }

        public delegate void DisplayTIPHandler(CaseType caseType, bool isCorrect);

        public event DisplayTIPHandler DisplayTIPEvent;

        public DataAttachments NewAttachments { get; set; }

        #endregion Public Members


        #region Private Members

        private ScanInfo m_ScanInfo;

        private SystemInfo m_systemInfo;

        #endregion


        #region Constructors

        public CaseObject ()
        {
            NewAttachments = new DataAttachments();
            IsTIPResultReturned = false;
            CaseHistories = new Histories();
            attachments = new DataAttachments();
            m_WorkstationResult = null;
            AnalysisStartTime = DateTime.Now;
        }

        #endregion Constructors


        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, e);
        }

        #endregion 
		

		#region Public Methods

        public void DisplayTIP()
        {
            if (DisplayTIPEvent != null)
            {
                bool isCorrect = (string.Compare(WorkstationResult.Decision, "Clear") != 0) ? true : false;
                DisplayTIPEvent(caseType, isCorrect);
            }
        }

        #endregion Public Methods
    }


    public class DataAttachments : ObservableCollection<DataAttachment>
    {
        #region Constructors

        public DataAttachments()
            : base()
        {
        }

        #endregion Constructors


        #region Public Methods

        public int CountofType (AttachmentType attachmentType)
        {
            int count = 0;

            foreach (DataAttachment dataAttachment in this.Where(attachment => attachment.attachmentType == attachmentType))
            {
                count++;
            }

            return count;
        }

        public IEnumerable<DataAttachment> GetFTIImageAttachments()
        {
            return this.Where(attachment => attachment.attachmentType == AttachmentType.FTIImage);
        }

        public IEnumerable<DataAttachment> GetXrayImageAttachments()
        {
            return this.Where(attachment => attachment.attachmentType == AttachmentType.XRayImage);
        }

        public IEnumerable<DataAttachment> GetManifestAttachments()
        {
            return this.Where(attachment => attachment.attachmentType == AttachmentType.Manifest);
        }

        public IEnumerable<DataAttachment> GetOCRAttachments()
        {
            return this.Where(attachment => attachment.attachmentType == AttachmentType.OCR);
        }

        public IEnumerable<DataAttachment> GetTDSAttachments()
        {
            return this.Where(attachment => attachment.attachmentType == AttachmentType.TDSResultFile);
        }

        public IEnumerable<DataAttachment> GetSNMAttachments()
        {
            return this.Where(attachment => attachment.attachmentType == AttachmentType.SNM);
        }

        public IEnumerable<DataAttachment> GetUnknownAttachments()
        {
            return this.Where(attachment => attachment.attachmentType == AttachmentType.Unknown);
        }

        public IEnumerable<DataAttachment> GetNUCDataAttachments()
        {
            return this.Where(attachment => attachment.attachmentType == AttachmentType.NUC);
        }

        public IEnumerable<DataAttachment> GetAnalysisHistoryAttachments()
        {
            return this.Where(attachment => attachment.attachmentType == AttachmentType.AnalysisHistory);
        }

        public IEnumerable<DataAttachment> GetHistoryAttachments()
        {
            return this.Where(attachment => attachment.attachmentType == AttachmentType.History);
        }

        public IEnumerable<DataAttachment> GetAnnotationsAttachments()
        {
            return this.Where(attachment => attachment.attachmentType == AttachmentType.Annotations);
        }

        public IEnumerable<DataAttachment> GetEventHistoryAttachments()
        {
            return this.Where(attachment => attachment.attachmentType == AttachmentType.EVENT_HISTORY);
        }

        #endregion Public Methods
    }


    public class DataAttachment : IDisposable
    {
        #region Private Members

        private AttachmentType m_Type;

        private String m_Id;

        private MemoryStream m_Data;

        private String m_CreateTime;

        private Boolean m_IsNew = false;

        private String m_User;

        #endregion Private Members


        #region Public Members

        public String User
        {
            get { return m_User; }
            set { m_User = value; }
        }

        public String CreateTime
        {
            get { return m_CreateTime; }
            set { m_CreateTime = value; }
        }

        public AttachmentType attachmentType
        {
            get
            {
                return m_Type;
            }

            set
            {
                m_Type = value;
            }
        }

        public Boolean IsNew
        {
            get
            {
                return m_IsNew;
            }

            set
            {
                m_IsNew = value;
            }
        }

        public String attachmentId
        {
            get
            {
                return m_Id;
            }

            set
            {
                m_Id = value;
            }
        }

        public MemoryStream attachmentData
        {
            get
            {
                return m_Data;
            }

            set
            {
                m_Data = value;
            }
        }

        #endregion Public Members


        #region Constructors

        public DataAttachment()
        {
        }

        public void Dispose()
        {
            m_Data.Dispose();
        }

        #endregion Constructors
    }
}
