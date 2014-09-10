using System;
using System.Collections.Generic;
using L3.Cargo.Common;
using L3.Cargo.Workstation.Plugins.Common;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using L3.Cargo.Workstation.SystemConfigurationCore;
using System.Diagnostics;
using L3.Cargo.Workstation.Plugins.XRayImageBase;
using System.IO;

namespace L3.Cargo.Workstation.Plugins.XRayImages
{
    public class XRayDisplay : IContent
    {
        #region Private Members

        private string m_Name = "Xray Image";

        private string m_Version = "1.0.0";

        private List<LayoutInfo> m_UserControlDisplays;

        private XRayDisplays m_XrayDisplays;

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

        public XRayDisplay()
        {
        }

        #endregion Constructors


        #region Public Methods

        public void Initialize(Object passedObj)
        {
            ContentParameter parameters = passedObj as ContentParameter;
            m_CaseObj = parameters.caseObject;
            SysConfiguration SysConfig = parameters.SysConfig;

            if (m_CaseObj.attachments.CountofType(AttachmentType.XRayImage) > 0)
            {
                m_XrayDisplays = new XRayDisplays(m_CaseObj, SysConfig);
                m_UserControlDisplays = m_XrayDisplays.Displays;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void Dispose()
        {
            m_XrayDisplays.Dispose();
        }

        #endregion Public Methods
    }
}
