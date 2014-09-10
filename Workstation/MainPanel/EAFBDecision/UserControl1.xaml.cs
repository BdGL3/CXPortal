using System;
using System.Windows;
using System.Windows.Controls;
using L3.Cargo.Common;
using L3.Cargo.Communications.Interfaces;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using L3.Cargo.Workstation.SystemConfigurationCore;
using System.Windows.Data;

namespace L3.Cargo.Workstation.MainPanel.Decision
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl, IDisposable
    {
        public OpenCaseHandler OpenCase;
        public CloseCaseHandler CloseCase;

        private SysConfigMgrAccess m_SysConfigMgr;

        private CaseObject m_CaseObject;

        public UserControl1(CaseObject caseObj, SysConfigMgrAccess  sysConfigMgr)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            m_CaseObject = caseObj;

            if (sysConfigMgr.GetDefaultConfig().WorkstationMode == "ManualCoding")
                DecisionButton.Content = "Verified";

            m_SysConfigMgr = sysConfigMgr;

            this.DataContext = caseObj;
        }

        #region IDisposable

        public void Dispose()
        {
            DecisionButton.Visibility = Visibility.Collapsed;
            m_CaseObject = null;
        }

        #endregion

        private void Decision_Button_Click(object sender, RoutedEventArgs e)
        {
            if (m_SysConfigMgr.GetDefaultConfig().WorkstationMode == "ManualCoding")
            {
                CloseCase(String.Empty, String.Empty, CaseUpdateEnum.ReleaseCase);
            }
            else
                DecisionSelection_Popup.IsOpen = true;
        }

        private void Decision_Btn_Click(object sender, RoutedEventArgs e)
        {
            DecisionSelection_Popup.IsOpen = false;

            try
            {
                WorkstationDecision decision;

                switch (DecisionComboBox.SelectedIndex)
                {
                    case 0:
                        decision = WorkstationDecision.Clear;
                        break;
                    case 1:
                        decision = WorkstationDecision.Reject;
                        break;
                    case 2:
                        decision = WorkstationDecision.Caution;
                        break;
                    default:
                        decision = WorkstationDecision.Unknown;
                        break;
                }

                TimeSpan analysisTime = DateTime.Now.Subtract(m_CaseObject.AnalysisStartTime);

                m_CaseObject.WorkstationResult = new result(decision.ToString(),
                                                            ReasonComboBox.SelectedIndex.ToString(),
                                                            DateTime.Now.ToString(CultureResources.getDefaultDisplayCulture()),
                                                            m_SysConfigMgr.GetDefaultConfig().Profile.UserName,
                                                            CommentTextBox.Text,
                                                            m_SysConfigMgr.GetDefaultConfig().WorkstationMode,
                                                            analysisTime.Seconds.ToString(CultureResources.getDefaultDisplayCulture()),
                                                            m_CaseObject.CaseId,
                                                            m_CaseObject.caseType,
                                                            m_SysConfigMgr.GetDefaultConfig().WorkstationAlias);

                CloseCase(m_CaseObject.SourceAlias, m_CaseObject.CaseId, CaseUpdateEnum.Result);

                if (m_CaseObject.caseType == Common.CaseType.CTICase ||
                    m_CaseObject.caseType == Common.CaseType.FTICase)
                {
                    m_CaseObject.DisplayTIP();
                }

                CloseCase(m_CaseObject.SourceAlias, m_CaseObject.CaseId, CaseUpdateEnum.ReleaseCase);

                CommentTextBox.Text = String.Empty;
                DecisionComboBox.SelectedItem = Clear;
                ReasonComboBox.SelectedItem = NotApplicable;
                DecisionButton.Visibility = Visibility.Collapsed;
                m_CaseObject = null;
            }
            catch (Exception ex)
            {
                //TODO: Log Error Message here
                MessageBox.Show(ex.Message);
            }            
                       
        }
    }
}
