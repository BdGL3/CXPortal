﻿using System;
using System.Collections.Generic;
using L3.Cargo.Common;
using L3.Cargo.Workstation.Plugins.Common;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using L3.Cargo.Workstation.SystemConfigurationCore;

namespace L3.Cargo.Workstation.Plugins.ManifestImages
{
    public class ManifestHandler : IContent
    {
        #region Private Members

        private string m_Name = "Manifests";

        private string m_Version = "1.0.0";

        private List<LayoutInfo> m_UserControlDisplays;

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
                return null;
            }
        }

        #endregion Public Members


        #region Constructors

        public ManifestHandler ()
        {
            m_UserControlDisplays = new List<LayoutInfo>();
        }

        #endregion Constructors


        #region Public Methods

        public void Initialize (Object passedObj)
        {
            ContentParameter parameters = passedObj as ContentParameter;
            CaseObject CaseObj = parameters.caseObject;
            SysConfiguration SysConfig = parameters.SysConfig;

            if (CaseObj.attachments.CountofType(AttachmentType.Manifest) > 0)
            {
                LayoutInfo layoutInfo = new LayoutInfo();
                layoutInfo.Name = m_Name;
                layoutInfo.Panel = PanelAssignment.SecondaryPanel;
                layoutInfo.Display = new UserControl1(CaseObj);
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
