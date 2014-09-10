using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using L3.Cargo.Common;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using System.Windows.Data;

namespace L3.Cargo.Workstation.Plugins.CaseInformationDisplay
{
    /// <summary>
    /// Interaction logic for PrintForm.xaml
    /// </summary>
    public partial class PrintForm : UserControl, IPrintForm
    {
        #region Private Members

        private List<BitmapSource> m_RenderedImages;

        private CaseObject m_CaseObject;

        #endregion Private Members


        #region Public Members

        public event PageUpdatedEventHandler PageUpdated;

        #endregion Public Members

        public PrintForm(CaseObject caseObj)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
            m_CaseObject = caseObj;
            m_RenderedImages = new List<BitmapSource>();
        }


        public void InitializePages()
        {
        }


        public List<PageContent> PrintPage()
        {
            List<PageContent> Pages = new List<PageContent>();

            InitializePages();

            PageContent pageContent = new PageContent();
            FixedPage fixedPage = new FixedPage();

            PrintDialog printDlg = new PrintDialog();

            PrintPreview printPreview = new PrintPreview(m_CaseObject);
            printPreview.Width = printDlg.PrintableAreaWidth;
            printPreview.Height = printDlg.PrintableAreaHeight;

            fixedPage.Children.Add((UIElement)printPreview);
                
            ((IAddChild)pageContent).AddChild(fixedPage);

            Pages.Add(pageContent);

            return Pages;
        }
    }
}