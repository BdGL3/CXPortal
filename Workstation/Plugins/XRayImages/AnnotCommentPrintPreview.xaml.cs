using System;
using System.Windows.Controls;
using L3.Cargo.Common;
using System.Windows.Data;

namespace L3.Cargo.Workstation.Plugins.XRayImages
{
    /// <summary>
    /// Interaction logic for AnnotPrintPreview.xaml
    /// </summary>
    public partial class AnnotCommentPrintPreview : UserControl
    {
        public AnnotCommentPrintPreview(CaseObject CaseObj, int viewNumber)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            CaseId.Text = CaseObj.CaseId;
            CreateTime.Text = CaseObj.createTime.ToString(CultureResources.getDefaultDisplayCulture());
            SiteId.Text = CaseObj.systemInfo.SystemType + " " + CaseObj.systemInfo.BaseLocation;
            ViewNumber.Text = "View" + viewNumber.ToString();
        }

        public AnnotCommentPrintPreview(CaseObject CaseObj)
        {
            InitializeComponent();

            CaseId.Text = CaseObj.CaseId;
            CreateTime.Text = CaseObj.createTime.ToString(CultureResources.getDefaultDisplayCulture());
            SiteId.Text = CaseObj.systemInfo.SystemType + " " + CaseObj.systemInfo.BaseLocation;
            ViewNumber.Text = String.Empty;
        }
    }
}
