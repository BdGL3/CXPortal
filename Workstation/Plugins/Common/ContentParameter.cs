using System;
using L3.Cargo.Common;
using L3.Cargo.Workstation.SystemConfigurationCore;

namespace L3.Cargo.Workstation.Plugins.Common
{
    public class ContentParameter
    {
        public CaseObject caseObject { get; set; }
        public SysConfiguration SysConfig { get; set; }

        public ContentParameter (CaseObject caseObj, SysConfiguration sysConfig)
        {
            caseObject = caseObj;
            SysConfig = sysConfig;
        }
    }
}
