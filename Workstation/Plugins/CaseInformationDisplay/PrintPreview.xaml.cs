using System;
using System.Windows.Controls;
using L3.Cargo.Common;
using L3.Cargo.Workstation.Plugins.Common;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.Threading;
using System.Globalization;

namespace L3.Cargo.Workstation.Plugins.CaseInformationDisplay
{
    /// <summary>
    /// Interaction logic for PrintPreview.xaml
    /// </summary>
    public partial class PrintPreview : UserControl
    {
        public PrintPreview(CaseObject CaseObj/*, BitmapSource bitmapSource*/) :
            base()
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            MainDisplay.DataContext = CaseObj;

            PrintDate.Text = " " + CultureResources.ConvertDateTimeToStringForDisplay(DateTime.Now);

            foreach (result result in CaseObj.ResultsList)
            {
                ResultItem resultItem = new ResultItem(result);

                ResultsView.Children.Add(resultItem);
            }

            CultureResources.getDataProvider().DataChanged += new EventHandler(CultureResources_DataChanged);
            this.Unloaded += new System.Windows.RoutedEventHandler(PrintPreview_Unloaded);
        }

        void PrintPreview_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            CultureResources.getDataProvider().DataChanged -= CultureResources_DataChanged;
        }

        void CultureResources_DataChanged(object sender, EventArgs e)
        {
            // reset the attachment list data context so that the strings get updated
            var data = AttachmentView.DataContext;
            AttachmentView.DataContext = null;
            AttachmentView.DataContext = data;

            PrintDate.Text = " " + CultureResources.ConvertDateTimeToStringForDisplay(DateTime.Now);
            this.InvalidateVisual();
        }
    }
}