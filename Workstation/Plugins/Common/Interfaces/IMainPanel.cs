using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using L3.Cargo.Communications.Interfaces;
using System.Windows;

namespace L3.Cargo.Workstation.Plugins.Common.Interfaces
{
    public delegate void OpenCaseHandler(String SourceAlias, String CaseId, Boolean CompareCase);
    public delegate void CloseCaseHandler(String SourceAlias, String CaseId, CaseUpdateEnum updateType);    

    public interface IMainPanel: IPlugin
    {
        UserControl UserControlDisplay { get; }        

        void SetOpenAndCloseCaseCallback(OpenCaseHandler openCase, CloseCaseHandler closeCase);
    }    
}
