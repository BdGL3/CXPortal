using System.Windows;
using L3.Cargo.Common;
using L3.Cargo.Workstation.SystemConfigurationCore;
using L3.Cargo.Workstation.SystemManagerCore;

namespace L3.Cargo.Workstation.Plugins.Common
{
    public class MainPanelParameter
    {
        public CaseObject caseObject { get; set; }
        public SysConfigMgrAccess SysConfig { get; set; }
        public SystemManagerAccess SysMgr { get; set; }
        public PrinterObjects printerObjects { get; set; }
        public Window MainFrameworkWindow { get; set; }

        public MainPanelParameter(CaseObject caseObj, SysConfigMgrAccess sysConfig, SystemManagerAccess sysMgrAccess, PrinterObjects objectsPrinter, Window frameworkWindow)
        {
            caseObject = caseObj;
            SysConfig = sysConfig;
            SysMgr = sysMgrAccess;
            printerObjects = objectsPrinter;
            MainFrameworkWindow = frameworkWindow;
        }
    }
}
