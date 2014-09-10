using System;
using System.Collections.Generic;
using L3.Cargo.Common;
using L3.Cargo.Workstation.Plugins.Common;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using L3.Cargo.Workstation.SystemConfigurationCore;

namespace L3.Cargo.Workstation.Plugins.RadiationDetectorData
{
    public class RadiationDetectorDataHandler : IContent
    {

        #region Private Members

        private string m_Name = "RadiationDetectorData";

        private string m_Version = "1.0.0";

        private List<LayoutInfo> m_UserControlDisplays;

        private CaseObject m_CaseObj;

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
                return new PrinterObject(m_Name, new PrintForm(m_CaseObj, m_UserControlDisplays));
            }
        }

        #endregion Public Members


        #region Constructors

        public RadiationDetectorDataHandler()
        {
            m_UserControlDisplays = new List<LayoutInfo>();
        }

        #endregion Constructors


        #region Public Methods

        public void Initialize (Object passedObj)
        {
            ContentParameter parameters = passedObj as ContentParameter;
            m_CaseObj = parameters.caseObject;
            SysConfiguration SysConfig = parameters.SysConfig;

            if (m_CaseObj.attachments.CountofType(AttachmentType.SNM) > 0)
            {
                LayoutInfo layoutInfo = new LayoutInfo();
                layoutInfo.Name = m_Name;
                layoutInfo.Panel = PanelAssignment.SubPanel;
                layoutInfo.Display = new UserControl1(m_CaseObj);
                layoutInfo.StatusItems = null;
                m_UserControlDisplays.Add(layoutInfo);
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
                ((UserControl1)m_UserControlDisplays[index].Display).Dispose();
                m_UserControlDisplays.RemoveAt(index);
            }
        }

        #endregion Public Methods
    }
}
