using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using L3.Cargo.Workstation.Common;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using System;
using L3.Cargo.Common;
using System.Windows.Data;

namespace L3.Cargo.Workstation.Plugins.DFCStatus
{
    /// <summary>
    /// Interaction logic for PrintForm.xaml
    /// </summary>
    public partial class PrintForm : UserControl, IPrintForm, IDisposable
    {
        #region Private Members

        private CaseObject m_CaseObject;

        #endregion Private Members


        #region Public Members

        public event PageUpdatedEventHandler PageUpdated;

        #endregion Public Members


        public PrintForm (CaseObject caseObj)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
            m_CaseObject = caseObj;
            this.DataContext = null;
        }

        public void InitializePages ()
        {
        }

        public List<PageContent> PrintPage()
        {
            List<PageContent> Pages = new List<PageContent>();

            PrintDialog tmpPrintDlg = new PrintDialog();

            double PrintPaperWidthInches = tmpPrintDlg.PrintableAreaWidth / 96D;
            double PrintPaperHeightInches = tmpPrintDlg.PrintableAreaHeight / 96D;

            return Pages;
        }

        public BitmapSource RenderVisaulToBitmap(Visual vsual, int width, int height)
        {
            BitmapSource bs = null;
            return bs;
        }

        private void DBProcessStatus_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (this.PageUpdated != null)
                this.PageUpdated();
        }

        public void Dispose ()
        {
        }
    }
}
