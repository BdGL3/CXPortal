using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using L3.Cargo.Common;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using System.Windows.Data;

namespace L3.Cargo.Workstation.Plugins.OCRImages
{
    /// <summary>
    /// Interaction logic for PrintForm.xaml
    /// </summary>
    public partial class PrintForm : UserControl, IPrintForm, IDisposable
    {
        #region Private Members

        private CaseObject m_CaseObject;

        private List<ImageInfo> imageFiles;

        #endregion Private Members


        #region Public Members

        public event PageUpdatedEventHandler PageUpdated;

        #endregion Public Members


        public PrintForm (CaseObject caseObj)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
            m_CaseObject = caseObj;
            imageFiles = new List<ImageInfo>();
            this.DataContext = imageFiles;
        }

        public void InitializePages ()
        {
            foreach (DataAttachment attachment in m_CaseObject.attachments.GetOCRAttachments())
            {
                ImageInfo imgInfo = new ImageInfo();
                MemoryStream ms = new MemoryStream(attachment.attachmentData.GetBuffer());
                BitmapImage imageSource = new BitmapImage();

                imageSource.BeginInit();
                imageSource.StreamSource = ms;
                imageSource.EndInit();

                imgInfo.imageSource = imageSource;
                imgInfo.FullName = attachment.attachmentId;

                imageFiles.Add(imgInfo);
            }
        }

        public List<PageContent> PrintPage()
        {
            List<PageContent> Pages = new List<PageContent>();

            PrintDialog printDialog = new PrintDialog();

            foreach (ImageInfo imageInfo in OCRImages_ListBox.SelectedItems)
            {
                PageContent pageContent = new PageContent();
                FixedPage fixedPage = new FixedPage();

                bool rotate = false;

                double width = printDialog.PrintableAreaWidth;
                double height = printDialog.PrintableAreaHeight;

                if (imageInfo.imageSource.Width > imageInfo.imageSource.Height)
                {
                    width = printDialog.PrintableAreaHeight;
                    height = printDialog.PrintableAreaWidth;
                    rotate = true;
                }

                PrintPreview printPreview = new PrintPreview(m_CaseObject, width, height);
                printPreview.SetImage(imageInfo.imageSource);

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
            }

            return Pages;
        }

        public BitmapSource RenderVisaulToBitmap(Visual vsual, int width, int height)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap
                (width, height, 96, 96, PixelFormats.Default);
            rtb.Render(vsual);

            BitmapSource bsource = rtb;

            return bsource;
        }

        private void OCRImagesListBox_SelectionChanged (object sender, RoutedEventArgs e)
        {
            if (this.PageUpdated != null)
                this.PageUpdated();
        }

        public void Dispose ()
        {
            imageFiles.Clear();
            imageFiles = null;
        }
    }
}
