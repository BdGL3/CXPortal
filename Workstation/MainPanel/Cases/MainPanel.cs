using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using System.Windows.Controls;
using L3.Cargo.Workstation.Plugins.Common;
using L3.Cargo.Workstation.PresentationCore;

namespace L3.Cargo.Workstation.MainPanel.Cases
{
    public class Cases : IMainPanel
    {
        #region Private Members

        private string m_Name = "Cases";

        private string m_Version = "1.0.0";

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

        public Cases()
        {
        }

        #endregion

        #region IMainPanel

        public void Initialize(Object passedObj)
        {
            MainPanelParameter parameters = passedObj as MainPanelParameter;

            m_UserControlDisplay = new UserControl1(parameters.SysMgr, parameters.SysConfig, parameters.caseObject, (Framework) parameters.MainFrameworkWindow);
        }

        public void Dispose()
        {
            if (m_UserControlDisplay != null)
                m_UserControlDisplay.Dispose();
        }

        public void SetOpenAndCloseCaseCallback(OpenCaseHandler openCase, CloseCaseHandler closeCase)
        {
            m_UserControlDisplay.CloseCase = closeCase;
            m_UserControlDisplay.OpenCase = openCase;
        }

        #endregion


    }
}
