using L3.Cargo.Workstation.Plugins.Manager;
using L3.Cargo.Workstation.SystemConfigurationCore;
using L3.Cargo.Workstation.SystemManagerCore;

namespace L3.Cargo.Workstation.PresentationCore
{
    public class Presentation
    {
        #region private members
		
        private LayoutManager m_LayoutMgr;

        private ContentPluginManager m_PluginMgr;

        private MainPanelPluginManager m_MainPanelPluginMgr;
		
        #endregion

        public Presentation(SysConfigMgrAccess sysConfig, SystemManagerAccess sysMgr)
        {
            m_PluginMgr = new ContentPluginManager(sysConfig);
            m_MainPanelPluginMgr = new MainPanelPluginManager(sysConfig);
            m_LayoutMgr = new LayoutManager(m_PluginMgr, sysConfig, sysMgr, m_MainPanelPluginMgr);
        }

        public void Show()
        {
            m_LayoutMgr.Show();
        }
    }
}