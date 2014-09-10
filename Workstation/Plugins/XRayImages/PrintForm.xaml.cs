using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using L3.Cargo.Common;
using L3.Cargo.Workstation.Plugins.Common;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using L3.Cargo.Workstation.Plugins.XRayImageBase;
using System.Windows.Data;
using L3.Cargo.Workstation.Plugins.XRayImageBase.Adorners;

namespace L3.Cargo.Workstation.Plugins.XRayImages
{
    /// <summary>
    /// Interaction logic for PrintForm.xaml
    /// </summary>
    public partial class PrintForm : UserControl, IPrintForm
    {
        #region Private Members

        private List<LayoutInfo> m_Layouts;

        private List<RenderedImage> m_RenderedImages;

        private CaseObject m_CaseObject;
        private struct RenderedImage
        {
            public BitmapSource Image;
            public bool IsAnnotationsShown;
            public List<string> AnnotationComments;
        }

        #endregion Private Members


        #region Public Members

        public event PageUpdatedEventHandler PageUpdated;

        #endregion Public Members

        public PrintForm(CaseObject caseObj, List<LayoutInfo> Layouts)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            m_Layouts = Layouts;
            m_CaseObject = caseObj;
            m_RenderedImages = new List<RenderedImage>();
        }

        private void GetDisplayAreas()
        {
            m_RenderedImages.Clear();

            foreach (LayoutInfo layoutInfo in m_Layouts)
            {
                UserControl1 PanelDisplay = layoutInfo.Display as UserControl1;

                if (PanelDisplay != null && PanelDisplay.View0Panel.Children.Count > 0)
                {
                    try
                    {
                        foreach (XRayImage xrayImage in PanelDisplay.View0Panel.Children)
                        {
                            RenderedImage ri;
                            ri.Image = xrayImage.RenderedImage;
                            ri.IsAnnotationsShown = xrayImage.IsAnnotationsShown;

                            List<string> annotations = new List<string>();
                            foreach (Annotation annotation in xrayImage.GetAnnotations())
                            {
                                annotations.Add(annotation.CommentText);
                            }

                            ri.AnnotationComments = annotations;
                            m_RenderedImages.Add(ri);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }

                if (PanelDisplay != null && PanelDisplay.View1Panel.Children.Count > 0)
                {
                    try
                    {
                        foreach (XRayImage xrayImage in PanelDisplay.View1Panel.Children)
                        {
                            RenderedImage ri;
                            ri.Image = xrayImage.RenderedImage;
                            ri.IsAnnotationsShown = xrayImage.IsAnnotationsShown;

                            List<string> annotations = new List<string>();
                            foreach (Annotation annotation in xrayImage.GetAnnotations())
                            {
                                annotations.Add(annotation.CommentText);
                            }

                            ri.AnnotationComments = annotations;
                            m_RenderedImages.Add(ri);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }

        public void InitializePages()
        {
            GetDisplayAreas();
        }


        public List<PageContent> PrintPage()
        {
            List<PageContent> Pages = new List<PageContent>();

            InitializePages();

            PrintDialog printDialog = new PrintDialog();
   
            int ViewNumber = 1;

            foreach (RenderedImage renderedImage in m_RenderedImages)
            {
                PageContent pageContent = new PageContent();
                FixedPage fixedPage = new FixedPage();

	            bool rotate = false;

	            double width = printDialog.PrintableAreaWidth;
	            double height = printDialog.PrintableAreaHeight;

                if (renderedImage.Image.Width > renderedImage.Image.Height)
	            {
	                width = printDialog.PrintableAreaHeight;
	                height = printDialog.PrintableAreaWidth;
	                rotate = true;
	            }

	            PrintPreview printPreview = new PrintPreview(m_CaseObject, width, height, ViewNumber);
                printPreview.SetImage(renderedImage.Image);

	            fixedPage.Children.Add((UIElement)printPreview);

                double pageWidth = printDialog.PrintableAreaWidth;
                double pageHeight = printDialog.PrintableAreaHeight;

	            if (rotate)
	            {
                    TranslateTransform tt = new TranslateTransform((pageWidth - pageHeight) / 2, (pageHeight - pageWidth) / 2);
                    printPreview.RenderTransform = tt;

                    RotateTransform rotateTransform = new RotateTransform(-90D, pageWidth / 2D, pageHeight / 2D);
                    fixedPage.RenderTransform = rotateTransform;
	            }

                ((IAddChild)pageContent).AddChild(fixedPage);

                Pages.Add(pageContent);

                if (renderedImage.IsAnnotationsShown && renderedImage.AnnotationComments.Count > 0)
                {
                    PageContent annotationPageContent = new PageContent();
                    FixedPage annotationFixedPage = new FixedPage();

                    //UserControl annotationPage = new UserControl();
                    AnnotCommentPrintPreview annotationPage;

                    if (m_RenderedImages.Count > 0)
                        annotationPage = new AnnotCommentPrintPreview(m_CaseObject, ViewNumber);
                    else
                        annotationPage = new AnnotCommentPrintPreview(m_CaseObject);

                    //annotationPage.MaxWidth = pageWidth - 48;
                    //annotationPage.MaxHeight = pageHeight - 48;                

                    //WrapPanel annotationPanel = new WrapPanel();
                    //annotationPage.Content = annotationPanel;

                    FixedPage.SetLeft(annotationPage, 48);
                    FixedPage.SetTop(annotationPage, 48);

                    annotationPage.annotationPanel.Width = pageWidth;
                    annotationPage.annotationPanel.Height = pageHeight;

                    annotationFixedPage.Children.Add((UIElement)annotationPage);

                    char[] letters = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

                    int count = 0;

                    foreach (string comment in renderedImage.AnnotationComments)
                    {
                        AnnotationPage ap = new AnnotationPage();
                        string index = string.Empty;

                        int div = (count / 26) - 1;
                        int rem = count % 26;

                        if (div >= 0)
                        {
                            index += letters[div];
                        }

                        index += letters[rem];

                        ap.AnnotationIndex.Text = index;

                        ap.AnnotationComment.Text = comment;

                        annotationPage.annotationPanel.Children.Add(ap);

                        count++;
                    }

                    ((IAddChild)annotationPageContent).AddChild(annotationFixedPage);

                    Pages.Add(annotationPageContent);
                }

                ViewNumber++;
            }

            return Pages;
        }    
    }
}