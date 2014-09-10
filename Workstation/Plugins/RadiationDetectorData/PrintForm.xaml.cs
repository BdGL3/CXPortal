using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using L3.Cargo.Common;
using L3.Cargo.Workstation.Plugins.Common;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using System.Windows.Data;

namespace L3.Cargo.Workstation.Plugins.RadiationDetectorData
{
    /// <summary>
    /// Interaction logic for PrintForm.xaml
    /// </summary>
    public partial class PrintForm : UserControl, IPrintForm
    {
        #region Private Members

        private List<LayoutInfo> m_Layouts;

        private WindowsFormsHost m_FormHost;

        private CaseObject m_CaseObject;

        #endregion Private Members


        #region Public Members

        public event PageUpdatedEventHandler PageUpdated;

        #endregion Public Members


        public PrintForm (CaseObject caseObj, List<LayoutInfo> Layouts)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
            m_Layouts = Layouts;
            m_CaseObject = caseObj;
        }

        private void GetChart()
        {
            foreach (LayoutInfo layoutInfo in m_Layouts)
            {
                UserControl1 PanelDisplay = layoutInfo.Display as UserControl1;

                if (PanelDisplay != null && PanelDisplay.ChartHost != null)
                {
                    try
                    {
                        m_FormHost = PanelDisplay.ChartHost;
                    }
                    catch
                    {
                    }
                }
            }
        }

        public void InitializePages ()
        {
            GetChart();
        }

        public List<PageContent> PrintPage()
        {
            List<PageContent> Pages = new List<PageContent>();

            PrintDialog printDialog = new PrintDialog();

            PageContent pageContent = new PageContent();
            FixedPage fixedPage = new FixedPage();

            BitmapSource displayAreaSource = RenderVisaulToBitmap(m_FormHost, (int)m_FormHost.ActualWidth, (int)m_FormHost.ActualHeight);

            bool rotate = false;

            double width = printDialog.PrintableAreaWidth;
            double height = printDialog.PrintableAreaHeight;

            if (displayAreaSource.Width > displayAreaSource.Height)
            {
                width = printDialog.PrintableAreaHeight;
                height = printDialog.PrintableAreaWidth;
                rotate = true;
            }

            PrintPreview printPreview = new PrintPreview(m_CaseObject, width, height);
            printPreview.SetImage(displayAreaSource);

            fixedPage.Children.Add((UIElement)printPreview);

            if (rotate)
            {
                double pageWidth = printDialog.PrintableAreaWidth;
                double pageHeight = printDialog.PrintableAreaHeight;

                TranslateTransform tt = new TranslateTransform((pageWidth - pageHeight) / 2, (pageHeight - pageWidth) / 2);
                printPreview.RenderTransform = tt;

                RotateTransform rotateTransform = new RotateTransform(-90D, pageWidth / 2D, pageHeight / 2D);
                fixedPage.RenderTransform = rotateTransform;
            }

            ((IAddChild)pageContent).AddChild(fixedPage);

            Pages.Add(pageContent);

            return Pages;
        }

        public BitmapSource RenderVisaulToBitmap(Visual vsual, int width, int height)
        {
            BitmapSource bsource;

            using (var bmp = new System.Drawing.Bitmap(m_FormHost.Child.Width, m_FormHost.Child.Height))
            {
                m_FormHost.Child.DrawToBitmap(bmp, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height));

                IntPtr hBitmap = bmp.GetHbitmap();
                BitmapSizeOptions sizeOptions = BitmapSizeOptions.FromEmptyOptions();
                bsource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, sizeOptions);
                bsource.Freeze();
            }

            return bsource;
        }
    }
}
