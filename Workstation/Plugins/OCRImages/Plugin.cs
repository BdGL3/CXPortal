using System;
using System.Collections.Generic;
using L3.Cargo.Common;
using L3.Cargo.Workstation.Plugins.Common;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using L3.Cargo.Workstation.SystemConfigurationCore;
using System.Windows;

namespace L3.Cargo.Workstation.Plugins.OCRImages
{
    public class OCRImageListHandler : IContent
    {
        #region Private Members

        private string m_Name = "OCRImages";

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

        public OCRImageListHandler ()
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

            if (CaseObj.attachments.CountofType(AttachmentType.OCR) > 0)
            {
                LayoutInfo layoutInfo = new LayoutInfo();
                layoutInfo.Name = m_Name;

                if (SysConfig.WorkstationMode == "ManualCoding")
                    layoutInfo.Panel = PanelAssignment.MainPanel;
                else
                    layoutInfo.Panel = PanelAssignment.SecondaryPanel;

                layoutInfo.Display = new UserControl1(CaseObj);
                layoutInfo.StatusItems = null;
                m_UserControlDisplays.Add(layoutInfo);
                m_PrintForm = new PrintForm(CaseObj);
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
        }

        #endregion Public Methods
    }
}
