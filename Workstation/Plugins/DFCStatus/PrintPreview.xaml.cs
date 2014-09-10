using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using L3.Cargo.Workstation.Common;
using L3.Cargo.Common;

namespace L3.Cargo.Workstation.Plugins.DFCStatus
{
    /// <summary>
    /// Interaction logic for PrintPreview.xaml
    /// </summary>
    public partial class PrintPreview : UserControl
    {
        private CaseObject m_caseObj;
        public PrintPreview (CaseObject caseObj, BitmapImage bitmapSource)
        {
            m_caseObj = caseObj;
            InitializeComponent();
            CultureResources.registerDataProvider(this);
            MainImage.Source = bitmapSource;

            CaseId.Text = caseObj.CaseId;
            CreateTime.Text = " " + CultureResources.ConvertDateTimeToStringForDisplay(caseObj.createTime);
            SiteId.Text = caseObj.systemInfo.SystemType + " " + caseObj.systemInfo.BaseLocation;

            CultureResources.getDataProvider().DataChanged += new EventHandler(CultureResources_DataChanged);
            this.Unloaded += new System.Windows.RoutedEventHandler(PrintPreview_Unloaded);
        }

        void PrintPreview_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            CultureResources.getDataProvider().DataChanged -= CultureResources_DataChanged;
        }

        void CultureResources_DataChanged(object sender, EventArgs e)
        {
            CreateTime.Text = " " + CultureResources.ConvertDateTimeToStringForDisplay(m_caseObj.createTime);
            this.InvalidateVisual();
        }
    }
}
