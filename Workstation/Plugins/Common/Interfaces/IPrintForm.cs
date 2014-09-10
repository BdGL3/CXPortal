using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows;

namespace L3.Cargo.Workstation.Plugins.Common.Interfaces
{
    public delegate void PageUpdatedEventHandler ();

    public interface IPrintForm
    {
        event PageUpdatedEventHandler PageUpdated;

        List<PageContent> PrintPage();

        void InitializePages ();
    }
}
