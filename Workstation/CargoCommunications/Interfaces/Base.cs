using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using L3.Cargo.Common.Xml.Profile_1_0;
using L3.Cargo.Communications.Common;

namespace L3.Cargo.Communications.Interfaces
{
    [DataContract]
    public enum TIPImageType
    {
        [EnumMember]
        Unknown,
        [EnumMember]
        CTI,
        [EnumMember]
        FTI
    }


    [DataContract]
    public enum CaseListUpdateState
    {
        [EnumMember]
        None,
        [EnumMember]
        Add,
        [EnumMember]
        Delete,
        [EnumMember]
        Modify
    }


    [DataContract]
    public enum CaseUpdateEnum
    {
        [EnumMember]
        AttachFile,
        [EnumMember]
        SetAsReference,
        [EnumMember]
        Result,
        [EnumMember]
        ObjectID,
        [EnumMember]
        CloseCase,
        [EnumMember]
        ReleaseCase
    }


    [DataContract]
    public enum AttachFileTypeEnum
    {
        [EnumMember]
        SNM,
        [EnumMember]
        OCR,
        [EnumMember]
        NUC,
        [EnumMember]
        History,
        [EnumMember]
        Annotations,
        [EnumMember]
        EVENT_HISTORY,
        [EnumMember]
        Manifest,
        [EnumMember]
        TDSResultFile,
        [EnumMember]
        XRayImage,
        [EnumMember]
        Unknown
    }


    [DataContract]
    public enum CaseType
    {
        [EnumMember]
        LiveCase,
        [EnumMember]
        ArchiveCase,
        [EnumMember]
        FTICase,
        [EnumMember]
        CTICase,
        [EnumMember]
        TrainingCase
    }


    [DataContract]
    public enum FileType
    {
        [EnumMember]
        None,
        [EnumMember]
        FTIFile,
        [EnumMember]
        ManifestFile,
        [EnumMember]
        AnalysisHistory
    }


    [DataContract]
    public enum ManifestListUpdateState
    {
        [EnumMember]
        Add,
        [EnumMember]
        Delete
    }


    [DataContract]
    public enum LoginResult
    {
        [EnumMember]
        Success,
        [EnumMember]
        Failure
    }


    [DataContract]
    public enum AuthenticationLevel
    {
        [EnumMember]
        Operator,
        [EnumMember]
        Supervisor,
        [EnumMember]
        Maintenance,
        [EnumMember]
        Engineer,
        [EnumMember]
        None
    }


    [DataContract]
    public enum WorkstationMode
    {
        [EnumMember]
        Analyst,
        [EnumMember]
        Supervisor,
        [EnumMember]
        Inspector,
        [EnumMember]
        EWS,
        [EnumMember]
        ManualCoding
    }


    [DataContract]
    public enum WorkstationDecision
    {
        [EnumMember]
        Unknown = 0,
        [EnumMember]
        Clear = 1,
        [EnumMember]
        Reject = 2,
        [EnumMember]
        Caution = 3
    }


    [DataContract]
    public enum WorkstationReason
    {
        [EnumMember]
        NotApplicable = 0,
        [EnumMember]
        TooComplex = 1,
        [EnumMember]
        TooDense = 2,
        [EnumMember]
        AnomalyIdentified = 3,
        [EnumMember]
        NoImage = 4,
        [EnumMember]
        PhysicalDeviation = 5
    }


    //-------------------------------------------------------------
    //
    // TODO: Description of the current Structure / Class
    //
    //-------------------------------------------------------------
    [DataContract]
    public class CaseListUpdate
    {
        private CaseListDataSet m_dsCaseList;
        private CaseListUpdateState m_state;

        [DataMember]
        public CaseListUpdateState state
        {
            get
            {
                return m_state;
            }
            set
            {
                m_state = value;
            }
        }

        [DataMember]
        public CaseListDataSet dsCaseList
        {
            get
            {
                return m_dsCaseList;
            }
            set
            {
                m_dsCaseList = value;
            }
        }

        #region Constructors

        public CaseListUpdate()
        {
            m_state = CaseListUpdateState.None;
            m_dsCaseList = null;
        }

        public CaseListUpdate(CaseListDataSet list, CaseListUpdateState state)
        {
            m_state = state;
            m_dsCaseList = list;
        }

        #endregion Constructors

    }


    //-------------------------------------------------------------
    //
    // TODO: Description of the current Structure / Class
    //
    //-------------------------------------------------------------
    [DataContract]
    public class CaseDataInfo
    {
        private String m_CaseId;
        private String m_FileName;
        private FileType m_FileType;

        [DataMember]
        public String CaseId
        {
            get
            {
                return m_CaseId;
            }
            set
            {
                m_CaseId = value;
            }
        }

        [DataMember]
        public String FileName
        {
            get
            {
                return m_FileName;
            }
            set
            {
                m_FileName = value;
            }
        }

        [DataMember]
        public FileType fileType
        {
            get
            {
                return m_FileType;
            }
            set
            {
                m_FileType = value;
            }
        }

        #region Constructors

        public CaseDataInfo()
        {
            m_CaseId = String.Empty;
            m_FileName = String.Empty;
            m_FileType = FileType.None;
        }

        public CaseDataInfo(String caseId, String fileName)
        {
            m_CaseId = caseId;
            m_FileName = fileName;
            m_FileType = FileType.None;
        }

        public CaseDataInfo(String fileName, FileType filetype)
        {
            m_CaseId = String.Empty;
            m_FileName = fileName;
            m_FileType = filetype;
        }

        public CaseDataInfo(String caseId, String fileName, FileType filetype)
        {
            m_CaseId = caseId;
            m_FileName = fileName;
            m_FileType = filetype;
        }

        #endregion Constructors

    }


    //-------------------------------------------------------------
    //
    // TODO: Description of the current Structure / Class
    //
    //-------------------------------------------------------------
    [DataContract]
    public class ManifestListUpdate
    {
        private List<String> m_List;
        private ManifestListUpdateState m_State;

        [DataMember]
        public ManifestListUpdateState State
        {
            get
            {
                return m_State;
            }
            set
            {
                m_State = value;
            }
        }

        [DataMember]
        public List<String> List
        {
            get
            {
                return m_List;
            }
            set
            {
                m_List = value;
            }
        }

        #region Constructors

        public ManifestListUpdate()
        {
            m_List = new List<String>();
            m_State = ManifestListUpdateState.Add;
        }

        public ManifestListUpdate(List<String> list, ManifestListUpdateState state)
        {
            m_List = list;
            m_State = state;
        }

        #endregion Constructors

    }


    //-------------------------------------------------------------
    //
    // TODO: Description of the current Structure / Class
    //
    //-------------------------------------------------------------
    [DataContract]
    public class UserInfo
    {
        private String m_UserName;
        private String m_Password;

        [DataMember]
        public String UserName
        {
            get
            {
                return m_UserName;
            }
            set
            {
                m_UserName = value;
            }
        }

        [DataMember]
        public String Password
        {
            get
            {
                return m_Password;
            }
            set
            {
                m_Password = value;
            }
        }

        #region Constructors

        public UserInfo()
        {
            m_UserName = String.Empty;
            m_Password = String.Empty;
        }

        public UserInfo(String username, String password)
        {
            m_UserName = username;
            m_Password = password;
        }

        #endregion Constructors
    }


    /// <Summary>
    /// TODO: Description of the current Structure / Class
    /// </Summary>
    /// <para>
    /// TODO: Return Value
    /// </para>
    /// <remarks>
    /// TODO: Description of the Remarks
    /// </remarks>
    /// <returns>
    /// </returns>
    /// <exception cref=""/>
    /// <seealso cref=""/>
    [DataContract]
    public class WorkstationInfo
    {
        private String m_WorkstationId;
        private UserInfo m_UserInfo;

        [DataMember]
        public String WorkstationId
        {
            get
            {
                return m_WorkstationId;
            }
            set
            {
                m_WorkstationId = value;
            }
        }

        [DataMember]
        public UserInfo userInfo
        {
            get
            {
                return m_UserInfo;
            }
            set
            {
                m_UserInfo = value;
            }
        }

        #region Constructors

        public WorkstationInfo()
        {
            m_WorkstationId = String.Empty;
            m_UserInfo = new UserInfo();
        }

        public WorkstationInfo(String awsId)
        {
            m_WorkstationId = awsId;
            m_UserInfo = new UserInfo();
        }

        public WorkstationInfo(UserInfo userInfo)
        {
            m_WorkstationId = String.Empty;
            m_UserInfo = userInfo;
        }

        public WorkstationInfo(String awsId, UserInfo userInfo)
        {
            m_WorkstationId = awsId;
            m_UserInfo = userInfo;
        }

        #endregion Constructors
    }


    //-------------------------------------------------------------
    //
    // TODO: Description of the current Structure / Class
    //
    //-------------------------------------------------------------
    [DataContract]
    public class LogOutInfo
    {
        private String m_WorkstationId;

        [DataMember]
        public String WorkstationId
        {
            get
            {
                return m_WorkstationId;
            }
            set
            {
                m_WorkstationId = value;
            }
        }

        #region Constructors

        public LogOutInfo()
        {
            m_WorkstationId = String.Empty;
        }

        public LogOutInfo(String awsId)
        {
            m_WorkstationId = awsId;
        }

        #endregion Constructors
    }


    //-------------------------------------------------------------
    //
    // TODO: Description of the current Structure / Class
    //
    //-------------------------------------------------------------
    [MessageContract]
    public class CaseMessage
    {
        #region Private Members

        private String m_CaseId;

        private String m_WorkstationId;

        private String m_Value;

        private CaseUpdateEnum m_Type;

        private Boolean m_IsCaseEditable;

        private string m_WorkstationMode;

        #endregion Private Members


        #region Public Members

        [MessageHeader]
        public String CaseId
        {
            get
            {
                return m_CaseId;
            }

            set
            {
                m_CaseId = value;
            }
        }

        [MessageHeader]
        public String WorkstationId
        {
            get
            {
                return m_WorkstationId;
            }

            set
            {
                m_WorkstationId = value;
            }
        }

        [MessageHeader]
        public String Value
        {
            get
            {
                return m_Value;
            }

            set
            {
                m_Value = value;
            }
        }

        [MessageHeader]
        public CaseUpdateEnum Type
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

        [MessageHeader]
        public Boolean IsCaseEditable
        {
            get
            {
                return m_IsCaseEditable;
            }

            set
            {
                m_IsCaseEditable = value;
            }
        }

        [MessageHeader]
        public string WorkstationMode
        {
            get
            {
                return m_WorkstationMode;
            }

            set
            {
                m_WorkstationMode = value;
            }
        }

        #endregion Public Members


        #region Constructors

        public CaseMessage()
        {
            m_CaseId = String.Empty;
            m_WorkstationId = String.Empty;
            m_Value = String.Empty;
        }

        public CaseMessage(String caseId)
        {
            m_CaseId = caseId;
            m_WorkstationId = String.Empty;
            m_Value = String.Empty;
            m_IsCaseEditable = true;
        }

        public CaseMessage(String caseId, String awsId)
        {
            m_CaseId = caseId;
            m_WorkstationId = awsId;
            m_Value = String.Empty;
            m_IsCaseEditable = true;
        }

        public CaseMessage(String caseId, String awsId, CaseUpdateEnum type)
        {
            m_CaseId = caseId;
            m_WorkstationId = awsId;
            m_Value = String.Empty;
            m_Type = type;
            m_IsCaseEditable = true;
        }

        public CaseMessage(String caseId, String awsId, CaseUpdateEnum type, String value)
        {
            m_CaseId = caseId;
            m_WorkstationId = awsId;
            m_Value = value;
            m_Type = type;
            m_IsCaseEditable = true;
        }

        public CaseMessage (String caseId, String awsId, CaseUpdateEnum type, String value, Boolean caseEditable)
        {
            m_CaseId = caseId;
            m_WorkstationId = awsId;
            m_Value = value;
            m_Type = type;
            m_IsCaseEditable = caseEditable;
        }

        public CaseMessage(String caseId, String awsId, CaseUpdateEnum type, String value, Boolean caseEditable, string workstationMode)
        {
            m_CaseId = caseId;
            m_WorkstationId = awsId;
            m_Value = value;
            m_Type = type;
            m_IsCaseEditable = caseEditable;
            m_WorkstationMode = workstationMode;
        }

        #endregion Constructors
    }


    //-------------------------------------------------------------
    //
    // TODO: Description of the current Structure / Class
    //
    //-------------------------------------------------------------
    [MessageContract]
    public class UpdateCaseMessage
    {
        #region Private Members

        private CaseUpdateEnum m_Type;

        private CaseType m_CaseType;

        private AttachFileTypeEnum m_AttachFileType;

        private String m_CaseId;

        private String m_Filename;

        private Stream m_File;

        private String m_ObjectId;

        private WorkstationResult m_WorkstationResult;

        private String m_UserName;

        private String m_CreateTime;

        private string m_WorkstationId;

        #endregion Private Members


        #region Public Members

        [MessageHeader]
        public String CreateTime
        {
            get { return m_CreateTime; }
            set { m_CreateTime = value; }
        }

        [MessageHeader]
        public String ObjectId
        {
            get { return m_ObjectId; }
            set { m_ObjectId = value; }
        }

        [MessageHeader]
        public WorkstationResult workstationResult
        {
            get { return m_WorkstationResult; }
            set { m_WorkstationResult = value; }
        }

        [MessageHeader]
        public CaseUpdateEnum Type
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

        [MessageHeader]
        public CaseType CaseType
        {
            get
            {
                return m_CaseType;
            }

            set
            {
                m_CaseType = value;
            }
        }

        [MessageHeader]
        public String CaseId
        {
            get
            {
                return m_CaseId;
            }

            set
            {
                m_CaseId = value;
            }
        }
		
		[MessageHeader]
        public String UserName
        {
            get { return m_UserName; }
            set { m_UserName = value; }
        }

        [MessageHeader]
        public String Filename
        {
            get
            {
                return m_Filename;
            }

            set
            {
                m_Filename = value;
            }
        }

        [MessageHeader]
        public AttachFileTypeEnum AttachFileType
        {
            get
            {
                return m_AttachFileType;
            }

            set
            {
                m_AttachFileType = value;
            }
        }

        [MessageBodyMember]
        public Stream File
        {
            get
            {
                return m_File;
            }

            set
            {
                m_File = value;
            }
        }

        
        [MessageHeader]
        public string WorkstationId
        {
            get
            {
                return m_WorkstationId;
            }

            set
            {
                m_WorkstationId = value;
            }
        }

        #endregion Public Members


        #region Constructors

        public UpdateCaseMessage()
        {
        }

        public UpdateCaseMessage(String caseId, CaseUpdateEnum type, CaseType caseType)
        {
            m_CaseId = caseId;
            m_Type = type;
            m_CaseType = caseType;
            m_Filename = String.Empty;
            m_File = new MemoryStream();
            m_AttachFileType = AttachFileTypeEnum.Unknown;
            m_ObjectId = String.Empty;
            m_WorkstationResult = null;
            m_UserName = String.Empty;           
            m_CreateTime = CreateTime;
        }

        #endregion Constructors
    }


    //-------------------------------------------------------------
    //
    // TODO: Description of the current Structure / Class
    //
    //-------------------------------------------------------------
    [MessageContract]
    public class CaseAttachFileMessage
    {
        [MessageHeader]
        public String caseId;

        [MessageHeader]
        public String filename;

        [MessageBodyMember]
        public Stream file;
    }


    //-------------------------------------------------------------
    //
    // TODO: Description of the current Structure / Class
    //
    //-------------------------------------------------------------
    [MessageContract]
    public class CaseRequestMessageResponse
    {
        [MessageHeader]
        public CaseType caseType;

        [MessageBodyMember]
        public Stream file;

        [MessageHeader]
        public bool IsResultEnabled;

        [MessageHeader]
        public Dictionary<FileType, String> AdditionalFiles;

        #region Constructors

        public CaseRequestMessageResponse()
        {
            caseType = CaseType.LiveCase;
            AdditionalFiles = new Dictionary<FileType, String>();
            file = null;
            IsResultEnabled = true;
        }

        public CaseRequestMessageResponse(CaseType ct, Stream f)
        {
            caseType = ct;
            AdditionalFiles = new Dictionary<FileType, String>();
            file = f;
            IsResultEnabled = true;
        }

        public CaseRequestMessageResponse(CaseType ct, Stream f, Dictionary<FileType, String> additionFiles)
        {
            caseType = ct;
            AdditionalFiles = additionFiles;
            file = f;
            IsResultEnabled = true;
        }

        public CaseRequestMessageResponse(CaseType ct, Stream f, Dictionary<FileType, String> additionFiles, bool resultEnabled)
        {
            caseType = ct;
            AdditionalFiles = additionFiles;
            file = f;
            IsResultEnabled = resultEnabled;
        }

        #endregion
    }


    //-------------------------------------------------------------
    //
    // TODO: Description of the current Structure / Class
    //
    //-------------------------------------------------------------
    [MessageContract]
    public class TIPInjectFileMessage
    {
        [MessageHeader]
        public TIPImageType imageType;

        [MessageHeader]
        public String filename;

        [MessageBodyMember]
        public Stream file;
    }


    //-------------------------------------------------------------
    //
    // TODO: Description of the current Structure / Class
    //
    //-------------------------------------------------------------
    [DataContract]
    public class LoginResponse
    {
        private AuthenticationLevel m_UserAuthenticationLevel;

        private SystemConfiguration m_SystemConfiguration;

        private Profile m_Profile;

        [DataMember]
        public AuthenticationLevel UserAuthenticationLevel
        {
            get
            {
                return m_UserAuthenticationLevel;
            }
            set
            {
                m_UserAuthenticationLevel = value;
            }
        }

        [DataMember]
        public SystemConfiguration systemConfiguration
        {
            get
            {
                return m_SystemConfiguration;
            }
            set
            {
                m_SystemConfiguration = value;
            }
        }

        [DataMember]
        public Profile UserProfile
        {
            get
            {
                return m_Profile;
            }
            set
            {
                m_Profile = value;
            }
        }

        #region Constructors

        public LoginResponse()
        {
            m_UserAuthenticationLevel = AuthenticationLevel.None;
        }

        public LoginResponse(AuthenticationLevel authenticationLevel)
        {
            m_UserAuthenticationLevel = authenticationLevel;
        }

        public LoginResponse(SystemConfiguration systemconfiguration)
        {
            m_UserAuthenticationLevel = AuthenticationLevel.None;
            m_SystemConfiguration = systemconfiguration;
        }

        public LoginResponse(AuthenticationLevel authenticationLevel, SystemConfiguration systemconfiguration, Profile profile)
        {
            m_UserAuthenticationLevel = authenticationLevel;
            m_SystemConfiguration = systemconfiguration;
            m_Profile = profile;
        }

        #endregion Constructors
    }


    //-------------------------------------------------------------
    //
    // TODO: Description of the current Structure / Class
    //
    //-------------------------------------------------------------
    [DataContract]
    public class WorkstationResult
    {
        private String m_CaseId;

        private String m_Comment;

        private WorkstationDecision m_Decision;

        private WorkstationReason m_Reason;

        private String m_UserName;

        private String m_WorkstationType;

        private String m_WorkstationId;

        private CaseType m_CaseType;

        private uint m_AnalysisTime;

        private String m_CreateTime;

        [DataMember]
        public String CaseId
        {
            get
            {
                return m_CaseId;
            }
            set
            {
                m_CaseId = value;
            }
        }

        [DataMember]
        public String Comment
        {
            get
            {
                return m_Comment;
            }
            set
            {
                m_Comment = value;
            }
        }

        [DataMember]
        public String CreateTime
        {
            get
            {
                return m_CreateTime;
            }
            set
            {
                m_CreateTime = value;
            }
        }

        [DataMember]
        public WorkstationDecision Decision
        {
            get
            {
                return m_Decision;
            }
            set
            {
                m_Decision = value;
            }
        }

        [DataMember]
        public uint AnalysisTime
        {
            get
            {
                return m_AnalysisTime;
            }
            set
            {
                m_AnalysisTime = value;
            }
        }

        [DataMember]
        public WorkstationReason Reason
        {
            get
            {
                return m_Reason;
            }
            set
            {
                m_Reason = value;
            }
        }

        [DataMember]
        public String UserName
        {
            get
            {
                return m_UserName;
            }
            set
            {
                m_UserName = value;
            }
        }

        [DataMember]
        public String WorkstationType
        {
            get
            {
                return m_WorkstationType;
            }
            set
            {
                m_WorkstationType = value;
            }
        }
		
        [DataMember]
        public String WorkstationId
        {
            get
            {
                return m_WorkstationId;
            }
            set
            {
                m_WorkstationId = value;
            }
        }

        [DataMember]
        public CaseType CaseType
        {
            get
            {
                return m_CaseType;
            }
            set
            {
                m_CaseType = value;
            }
        }

        public WorkstationResult()
        {
        }

        public WorkstationResult (L3.Cargo.Common.result result)
        {
            m_AnalysisTime = Convert.ToUInt32(result.AnalysisTime);
            m_Comment = result.Comment;
            m_CreateTime = result.CreateTime;
            m_Decision = (WorkstationDecision) Enum.Parse(typeof(WorkstationDecision), result.Decision, true);
            m_Reason = (WorkstationReason) Enum.Parse(typeof(WorkstationReason), result.Reason, true);
            m_UserName = result.User;
            m_WorkstationType = result.StationType;
            m_CaseId = result.CaseId;
            m_CaseType = (CaseType)Enum.Parse(typeof(CaseType), result.CaseType.ToString(), true);
            m_WorkstationId = result.WorkstationId;
        }
    }


    //-------------------------------------------------------------
    //
    // TODO: Description of the current Structure / Class
    //
    //-------------------------------------------------------------
    [DataContract]
    public class SystemConfiguration
    {
        private String m_SystemConfiguration;

        private int m_MaxManifestPerVehicle = 0;

        private string m_ContainerDBConnect;

        private int m_ContainerRefreshPeriodSeconds;

        [DataMember]
        public int MaxManifestPerVehicle
        {
            get
            {
                return m_MaxManifestPerVehicle;
            }
            set
            {
                m_MaxManifestPerVehicle = value;
            }
        }

        [DataMember]
        public int ContainerRefreshPeriodSeconds
        {
            get
            {
                return m_ContainerRefreshPeriodSeconds;
            }
            set
            {
                m_ContainerRefreshPeriodSeconds = value;
            }
        }

        [DataMember]
        public string ContainerDBConnectString
        {
            get
            {
                return m_ContainerDBConnect;
            }
            set
            {
                m_ContainerDBConnect = value;
            }
        }

        [DataMember]
        public String systemConfigurationID
        {
            get
            {
                return m_SystemConfiguration;
            }
            set
            {
                m_SystemConfiguration = value;
            }
        }

        #region Constructors

        public SystemConfiguration()
        {
        }

        public SystemConfiguration(String sc)
        {
            m_SystemConfiguration = sc;
        }

        public SystemConfiguration(String sc, int maxManifestPerVehicle)
        {
            m_SystemConfiguration = sc;
            m_MaxManifestPerVehicle = maxManifestPerVehicle;
        }

        #endregion Constructors
    }
}