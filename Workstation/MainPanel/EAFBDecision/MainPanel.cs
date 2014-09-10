using System;
using System.Windows.Controls;
using L3.Cargo.Common;
using L3.Cargo.Workstation.Plugins.Common;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using L3.Cargo.Workstation.SystemConfigurationCore;
using L3.Cargo.Workstation.SystemManagerCore;

namespace L3.Cargo.Workstation.MainPanel.Decision
{
    public class Decision: IMainPanel
    {
        #region Private Members

        private string m_Name = "Decision";

        private string m_Version = "1.0.0";

        private CaseObject caseObj;

        private SysConfigMgrAccess sysConfig;

        private SystemManagerAccess sysMgr;

        private UserControl1 m_UserControlDisplay;

        #endregion

        #region Public Members

        public string Name
        {
            get { return m_Name; }
        }

        public string Version
        {
            get { return m_Version; }
        }

        public UserControl UserControlDisplay
        {
            get { return m_UserControlDisplay; }
        }

        #endregion

        #region Constructor

        public Decision()
        {
        }

        #endregion

        #region IMainPanel 

        public void Initialize(Object passedObj)
        {
            MainPanelParameter parameters = passedObj as MainPanelParameter;
            caseObj = parameters.caseObject;
            sysConfig = parameters.SysConfig;
            sysMgr = parameters.SysMgr;

            m_UserControlDisplay = new UserControl1(caseObj, sysConfig);
        }

        public void Dispose()
        {
            if (m_UserControlDisplay != null)
                m_UserControlDisplay.Dispose();
        }

        public void SetOpenAndCloseCaseCallback(OpenCaseHandler openCase, CloseCaseHandler closeCase)
        {
            m_UserControlDisplay.OpenCase = openCase;
            m_UserControlDisplay.CloseCase = closeCase;
        }

        #endregion
    }
}
