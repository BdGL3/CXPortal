using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using L3.Cargo.Common;
using System.Windows.Data;

namespace L3.Cargo.Workstation.Plugins.OCRImages
{
    /// <summary>
    /// Interaction logic for PrintPreview.xaml
    /// </summary>
    public partial class PrintPreview : UserControl
    {
        public PrintPreview (CaseObject caseObj, double width, double height)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            MainDisplay.DataContext = caseObj;
            this.Width = width;
            this.Height = height;

            PrintDate.Text = " " + CultureResources.ConvertDateTimeToStringForDisplay(DateTime.Now);
            CaseCreateTime.Text = " " + CultureResources.ConvertDateTimeToStringForDisplay(caseObj.createTime);

            CultureResources.getDataProvider().DataChanged += new EventHandler(CultureResources_DataChanged);
            this.Unloaded += new System.Windows.RoutedEventHandler(PrintPreview_Unloaded);
        }

        public void SetImage (BitmapImage bitmapSource)
        {
            MainImage.Source = bitmapSource;
            MainImage.MaxHeight = this.Height - (Header.Height.Value + Footer.Height.Value + Title.Height +
                                                 ImageBorder.BorderThickness.Top + ImageBorder.BorderThickness.Bottom +
                                                 MainDisplay.Margin.Top + MainDisplay.Margin.Bottom);
        }

        void PrintPreview_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            CultureResources.getDataProvider().DataChanged -= CultureResources_DataChanged;
        }

        void CultureResources_DataChanged(object sender, EventArgs e)
        {
            PrintDate.Text = " " + CultureResources.ConvertDateTimeToStringForDisplay(DateTime.Now);
            CaseCreateTime.Text = " " + CultureResources.ConvertDateTimeToStringForDisplay(((CaseObject)MainDisplay.DataContext).createTime);
            this.InvalidateVisual();
        }
    }
}
