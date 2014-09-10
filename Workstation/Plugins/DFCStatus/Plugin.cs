using System;
using System.Collections.Generic;
using L3.Cargo.Common;
using L3.Cargo.Workstation.Plugins.Common;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using L3.Cargo.Workstation.SystemConfigurationCore;

namespace L3.Cargo.Workstation.Plugins.DFCStatus
{
    public class DBProcessStatusHandler : IContent
    {
        #region Private Members

        private string m_Name = "DFCStatus";

        private string m_Version = "1.0.0";

        private List<LayoutInfo> m_UserControlDisplays;

        private PrintForm m_PrintForm;

        #endregion Private Members


        #region Public Members

        public string Name
        {
            get
            {
                return m_Name;
            }
        }

        public string Version
        {
            get
            {
                return m_Version;
            }
        }

        public List<LayoutInfo> UserControlDisplays
        {
            get
            {
                return m_UserControlDisplays;
            }
        }

        public PrinterObject PrinterObject
        {
            get
            {
                return new PrinterObject(m_Name, m_PrintForm);
            }
        }

        #endregion Public Members


        #region Constructors

        public DBProcessStatusHandler()
        {
            m_UserControlDisplays = new List<LayoutInfo>();
        }

        #endregion Constructors


        #region Public Methods

        public void Initialize (Object passedObj)
        {
            ContentParameter parameters = passedObj as ContentParameter;

            if (parameters != null)
            {
                SysConfiguration SysConfig = parameters.SysConfig;

                if (String.IsNullOrWhiteSpace(SysConfig.ContainerDBConnectionString))
                {
                    throw new NotSupportedException();
                }

                CaseObject CaseObj = parameters.caseObject;

                try
                {
                    LayoutInfo layoutInfo = new LayoutInfo();
                    layoutInfo.Name = m_Name;
                    layoutInfo.Panel = PanelAssignment.InfoPanel;
                    layoutInfo.Display = new UserControl1(CaseObj, SysConfig);
                    layoutInfo.StatusItems = null;
                    m_UserControlDisplays.Add(layoutInfo);
                    m_PrintForm = new PrintForm(CaseObj);
                }
                catch (Exception ex)
                {
                    throw new NotSupportedException();
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void Dispose ()
        {
            for (int index = m_UserControlDisplays.Count - 1; index >= 0; index--)
            {
                UserControl1 userControl1 = m_UserControlDisplays[index].Display as UserControl1;

                if (userControl1 != null)
                {
                    userControl1.Dispose();
                }

                m_UserControlDisplays.RemoveAt(index);
            }

            m_UserControlDisplays.Clear();

            m_PrintForm.Dispose();

            m_UserControlDisplays = null;
            m_PrintForm = null;
        }

        #endregion Public Methods
    }
}
