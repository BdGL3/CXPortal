using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using L3.Cargo.Workstation.ProfileManagerCore;
using System.Threading;
using L3.Cargo.Common;
using L3.Cargo.Communications.Client;

namespace L3.Cargo.Workstation.SystemConfigurationCore
{
    
    public class SysConfiguration
    {
        #region Private Members

        private uint m_MacroPlayBackStepDelay;

        private string m_AlgServerAbsolutePath;

        private string m_WorkstationAlias;

        private bool m_DisplayAlgThreatResults;

        private uint m_MaxCompareCases;

        private ProfileObject m_ProfileObject;

        private string m_VersionNumber;

        private string m_BuildNumber;

        private string m_BuildDate;

        private String m_WorkstationMode;

        private Boolean m_AutoSelectPendingCasesEnabled;

        private Boolean m_SelectedArchiveDuringAutoSelect;

        private string m_ContainerDBConnectionString;

        private int m_ContainerRefreshPeriodmsecs;

        private bool m_ForceAutoSelect;

        private bool _FlipView1XAxis;

        private bool _FlipView1YAxis;

        private bool _FlipView2XAxis;

        private bool _FlipView2YAxis;

        private int _wcfDiscoveryProbeTimeoutPeriodSec;

        private HostDiscovery.EnableDiscoveryManagedModeEnum _enableDiscoveryManagedModeIWSComm;

        private string _wcfDiscoveryProxyConnectionUri;

        private int _wsCommPingTimeoutMsec;

        private int _wcfTcpBindingReceiveTimeoutMin;

        private int _wcfTcpBindingSendTimeoutMin;

        private bool _densityAlarmSetOnCaseOpen;

        private double _densityAlarmDefaultValue;

        private string _caseFilterAnalystComment;

        private string _caseFilterCaseID;

        private int _caseFilterUpdateTime_DaysOld;

        private string _caseFilterAnalyst;

        private string _caseFilterFlightNumber;

        private string _caseFilterObjectID;

        private string _caseFilterArea;

        private string _caseFilterResult;

        #endregion Private Members


        #region Internal Members

        internal string m_SysConfigID;

        #endregion Internal Members


        #region Public Members

        public delegate void AutoSelectPendingCasesChangedEventHandler(Boolean AutoSelectPendingEnabled);

        public event AutoSelectPendingCasesChangedEventHandler AutoSelectPendingCasesChanged;

        public delegate void SelectedArchiveDuringAutoSelectChangedEventHandler (Boolean SelectedArchiveDuringAutoSelect);

        public event SelectedArchiveDuringAutoSelectChangedEventHandler SelectedArchiveDuringAutoSelectChanged;

        public bool ForceAutoSelect
        {
            get
            {
                return m_ForceAutoSelect;
            }
            set
            {
                m_ForceAutoSelect = value;
            }
        }

        public Boolean AutoSelectPendingCasesEnabled
        {
            get
            {
                return m_AutoSelectPendingCasesEnabled;
            }
            set
            {
                m_AutoSelectPendingCasesEnabled = value;
                AutoSelectPendingCasesChanged(value);                
            }
        }

        public Boolean SelectedArchiveDuringAutoSelect
        {
            get
            {
                return m_SelectedArchiveDuringAutoSelect;
            }
            set
            {
                m_SelectedArchiveDuringAutoSelect = value;
                if (SelectedArchiveDuringAutoSelectChanged != null)
                    SelectedArchiveDuringAutoSelectChanged(value);
            }
        }
		
		public string ContainerDBConnectionString
        {
            get
            {
                return m_ContainerDBConnectionString;
            }
            set
            {
                m_ContainerDBConnectionString = value;
            }
        }

        public int ContainerRefreshPeriodmsecs
        {
            get
            {
                return m_ContainerRefreshPeriodmsecs;
            }
            set
            {
                m_ContainerRefreshPeriodmsecs = value;
            }
        }

        public uint MacroPlayBackStepDelay
        {
            get
            {
                return m_MacroPlayBackStepDelay;
            }
        }

        public string AlgServerAbsolutePath
        {
            get
            {
                return m_AlgServerAbsolutePath;
            }
        }

        public string WorkstationAlias
        {
            get
            {
                return m_WorkstationAlias;
            }
        }

        public bool DisplayAlgThreatResults
        {
            get
            {
                return m_DisplayAlgThreatResults;
            }
        }

        public uint MaxCompareCases
        {
            get
            {
                return m_MaxCompareCases;
            }
        }

        public string VersionNumber
        {
            get
            {
                return m_VersionNumber;
            }
        }

        public string BuildNumber
        {
            get
            {
                return m_BuildNumber;
            }
        }

        public string BuildDate
        {
            get
            {
                return m_BuildDate;
            }
        }

        public string ID
        {
            get
            {
                return m_SysConfigID;
            }

            set
            {
                m_SysConfigID = value;
            }
        }

        public ProfileObject Profile
        {
            get
            {
                return m_ProfileObject;
            }

            set
            {
                m_ProfileObject = value;
            }
        }

        public String WorkstationMode
        {
            get { return m_WorkstationMode; }
        }

        public bool FlipView1XAxis
        {
            get { return _FlipView1XAxis; }
        }

        public bool FlipView1YAxis
        {
            get { return _FlipView1YAxis; }
        }

        public bool FlipView2XAxis
        {
            get { return _FlipView2XAxis; }
        }

        public bool FlipView2YAxis
        {
            get { return _FlipView2YAxis; }
        }

        public int WcfDiscoveryProbeTimeoutPeriodSec
        {
            get { return _wcfDiscoveryProbeTimeoutPeriodSec; }
        }

        public HostDiscovery.EnableDiscoveryManagedModeEnum EnableDiscoveryManagedModeIWSComm
        {
            get { return _enableDiscoveryManagedModeIWSComm; }
        }

        public string WcfDiscoveryProxyConnectionUri
        {
            get { return _wcfDiscoveryProxyConnectionUri; }
        }

        public int WsCommPingTimeoutMsec
        {
            get { return _wsCommPingTimeoutMsec; }
        }

        public int WcfTcpBindingReceiveTimeoutMin
        {
            get { return _wcfTcpBindingReceiveTimeoutMin; }
        }

        public int WcfTcpBindingSendTimeoutMin
        {
            get { return _wcfTcpBindingSendTimeoutMin; }
        }

        public bool DensityAlarmSetOnCaseOpen
        {
            get { return _densityAlarmSetOnCaseOpen; }
        }

        public double DensityAlarmDefaultValue
        {
            get { return _densityAlarmDefaultValue; }
        }

        public string CaseFilterAnalystComment
        {
            get { return _caseFilterAnalystComment; }
        }

        public string CaseFilterCaseID
        {
            get { return _caseFilterCaseID; }
        }

        public string CaseFilterAnalyst
        {
            get { return _caseFilterAnalyst; }
        }

        public string CaseFilterFlightNumber
        {
            get { return _caseFilterFlightNumber; }
        }

        public string CaseFilterObjectID
        {
            get { return _caseFilterObjectID; }
        }

        public string CaseFilterArea
        {
            get { return _caseFilterArea; }
        }

        public string CaseFilterResult
        {
            get { return _caseFilterResult; }
        }

        public int CaseFilterUpdateTime_DaysOld
        {
            get { return _caseFilterUpdateTime_DaysOld; }
        }

        #endregion Public Members


        #region Constructors

        public SysConfiguration()
        {
            ReadAppConfig();
        }

        #endregion Constructors


        #region Private Methods

        private void ReadAppConfig()
        {
            try
            {
                string str = string.Empty;

                str = ConfigurationManager.AppSettings["MacroPlayBackStepDelay"].ToString();
                if(str != null)
                {
                    m_MacroPlayBackStepDelay = uint.Parse(str);
                }
                else
                {
                    m_MacroPlayBackStepDelay = 0;
                }

                str = ConfigurationManager.AppSettings["AlgServerAbsolutePath"].ToString();
                if(str != null)
                {
                    m_AlgServerAbsolutePath = str;
                }
                else
                {
                    m_AlgServerAbsolutePath = @"C:\";
                }

                str = ConfigurationManager.AppSettings["WorkstationAlias"].ToString();
                if(str != null)
                {
                    m_WorkstationAlias = System.Environment.MachineName + "_" + str;
                }
                else
                {
                    m_WorkstationAlias = @"Unknown";
                }

                str = ConfigurationManager.AppSettings["DisplayAlgThreatResults"].ToString();
                if (str != null)
                {
                    m_DisplayAlgThreatResults = Boolean.Parse(str);
                }
                else
                {
                    m_DisplayAlgThreatResults = true;
                }


                str = ConfigurationManager.AppSettings["MaxCompareCases"].ToString();
                if (str != null)
                {
                    m_MaxCompareCases = uint.Parse(str);
                }
                else
                {
                    m_MaxCompareCases = 0;
                }



                str = ConfigurationManager.AppSettings["VersionNumber"].ToString();
                if (str != null)
                {
                    m_VersionNumber = str;
                }
                else
                {
                    m_VersionNumber = @"Unknown";
                }

                str = ConfigurationManager.AppSettings["BuildNumber"].ToString();
                if (str != null)
                {
                    m_BuildNumber = str;
                }
                else
                {
                    m_BuildNumber = @"Unknown";
                }

                str = ConfigurationManager.AppSettings["BuildDate"].ToString();
                if (str != null)
                {
                    m_BuildDate = str;
                }
                else
                {
                    m_BuildDate = @"Unknown";
                }

                str = ConfigurationManager.AppSettings["WorkstationMode"].ToString();
                if (str != null)
                {
                    m_WorkstationMode = str;
                }
                else
                {
                    m_WorkstationMode = @"Analyst";
                }

                str = ConfigurationManager.AppSettings["FlipView1XAxis"].ToString();
                if (str != null)
                {
                    _FlipView1XAxis = Boolean.Parse(str);
                }
                else
                {
                    _FlipView1XAxis = false;
                }

                str = ConfigurationManager.AppSettings["FlipView1YAxis"].ToString();
                if (str != null)
                {
                    _FlipView1YAxis = Boolean.Parse(str);
                }
                else
                {
                    _FlipView1YAxis = false;
                }

                str = ConfigurationManager.AppSettings["FlipView2XAxis"].ToString();
                if (str != null)
                {
                    _FlipView2XAxis = Boolean.Parse(str);
                }
                else
                {
                    _FlipView2XAxis = false;
                }

                str = ConfigurationManager.AppSettings["FlipView2YAxis"].ToString();
                if (str != null)
                {
                    _FlipView2YAxis = Boolean.Parse(str);
                }
                else
                {
                    _FlipView2YAxis = false;
                }

                str = ConfigurationManager.AppSettings["ForceAutoSelect"].ToString();
                if (str != null)
                {
                    ForceAutoSelect = Boolean.Parse(str);
                }
                else
                {
                    ForceAutoSelect = false;
                }

                m_SelectedArchiveDuringAutoSelect = false;

                str = ConfigurationManager.AppSettings["WCFDiscoveryProbeTimeoutPeriodSec"];
                if (str != null)
                {
                    _wcfDiscoveryProbeTimeoutPeriodSec = int.Parse(str);
                }
                else
                {
                    _wcfDiscoveryProbeTimeoutPeriodSec = 5;
                }

                str = ConfigurationManager.AppSettings["EnableDiscoveryManagedModeIWSComm"];
                if (str != null)
                {
                    str = str.ToUpper();
                    _enableDiscoveryManagedModeIWSComm = (HostDiscovery.EnableDiscoveryManagedModeEnum)
                        Enum.Parse(typeof(HostDiscovery.EnableDiscoveryManagedModeEnum), str);
                }
                else
                {
                    _enableDiscoveryManagedModeIWSComm = HostDiscovery.EnableDiscoveryManagedModeEnum.FALSE;
                }

                str = ConfigurationManager.AppSettings["WcfDiscoveryProxyConnectionUri"];
                if (str != null)
                {
                    _wcfDiscoveryProxyConnectionUri = str;
                }
                else
                {
                    _wcfDiscoveryProxyConnectionUri = string.Empty;
                }

                str = ConfigurationManager.AppSettings["WsCommPingTimeoutMsec"];
                if (str != null)
                {
                    _wsCommPingTimeoutMsec = int.Parse(str);
                }
                else
                {
                    _wsCommPingTimeoutMsec = 1000;
                }

                str = ConfigurationManager.AppSettings["WcfTcpBindingReceiveTimeoutMin"];
                if (str != null)
                {
                    _wcfTcpBindingReceiveTimeoutMin = int.Parse(str);
                }
                else
                {
                    _wcfTcpBindingReceiveTimeoutMin = 1;
                }

                str = ConfigurationManager.AppSettings["WcfTcpBindingSendTimeoutMin"];
                if (str != null)
                {
                    _wcfTcpBindingSendTimeoutMin = int.Parse(str);
                }
                else
                {
                    _wcfTcpBindingSendTimeoutMin = 1;
                }

                str = ConfigurationManager.AppSettings["DensityAlarmSetOnCaseOpen"];
                if (str != null)
                {
                    _densityAlarmSetOnCaseOpen = bool.Parse(str);
                }
                else
                {
                    _densityAlarmSetOnCaseOpen = false;
                }

                str = ConfigurationManager.AppSettings["DensityAlarmDefaultValue"];
                if (str != null)
                {
                    _densityAlarmDefaultValue = double.Parse(str);
                }
                else
                {
                    _densityAlarmDefaultValue = 0.0;
                }

                _caseFilterAnalystComment = ConfigurationManager.AppSettings["CaseFilterAnalystComment"];
                _caseFilterCaseID = ConfigurationManager.AppSettings["CaseFilterCaseID"];
                _caseFilterAnalyst = ConfigurationManager.AppSettings["CaseFilterAnalyst"];
                _caseFilterFlightNumber = ConfigurationManager.AppSettings["CaseFilterFlightNumber"];
                _caseFilterObjectID = ConfigurationManager.AppSettings["CaseFilterObjectID"];
                _caseFilterArea = ConfigurationManager.AppSettings["CaseFilterArea"];
                _caseFilterResult = ConfigurationManager.AppSettings["CaseFilterResult"];

                str = ConfigurationManager.AppSettings["CaseFilterUpdateTime_DaysOld"];
                if (str != null && str != "")
                {
                    _caseFilterUpdateTime_DaysOld = int.Parse(str);
                }
                else
                {
                    _caseFilterUpdateTime_DaysOld = 0;
                }

            }
            catch (Exception ex)
            {
                //TODO: Log exception
            }
        }

        #endregion Private Methods
    }
}
