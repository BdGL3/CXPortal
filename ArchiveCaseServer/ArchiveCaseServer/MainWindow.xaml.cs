using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using L3.Cargo.Common;
using L3.Cargo.Communications.Common;
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;

namespace L3.Cargo.ArchiveCaseServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Logger log;
        CaseList caseList;
        ExtendedCollectionViewSource CaseListview;
        //bool IsCaseListSearchEnabled = false;
        //CaseListSearch m_caseListSearch;
        Boolean closeApplication = false;

        private CaseWSCollection caseWSCollection;
        private ACSServer acsServer = null;
        private HostComm hostComm = null;

        private string _localCaseListPath = null;

        //use to disable window close button
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                //modify title to include ACS alias
                this.Title += " - " + ConfigurationManager.AppSettings["ServerName"];

                ReportdatePickerStart.SelectedDate = DateTime.Now.AddMonths(-1);

                log = new ACSLogger(this);

                caseWSCollection = (CaseWSCollection)FindResource("casewscollection");

                bool m_EnableSelectLocalCaseFolder = (bool)bool.Parse(ConfigurationManager.AppSettings["EnableSelectLocalCaseFolder"]);

                if (!m_EnableSelectLocalCaseFolder)
                {
                    SelectLocalCaseFolderMenuItem.Visibility = System.Windows.Visibility.Collapsed;
                }

                bool m_EnableFullSync = (bool)bool.Parse(ConfigurationManager.AppSettings["EnableFullSync"]);

                if (!m_EnableFullSync)
                {
                    FullSyncMenuItem.Visibility = System.Windows.Visibility.Collapsed;

                    if (!m_EnableSelectLocalCaseFolder)
                    {
                        SystemMenuItem.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
            }
            catch (Exception exp)
            {
                log.PrintInfoLine("MainWindow exp: " + exp);
            }
        }

        public void m_HostComm_Impl_ConnectedToHostEvent(Boolean Connected)
        {
            ConnectStatus.Dispatcher.Invoke(DispatcherPriority.Normal,
                                            new Action(delegate()
            {
                if (!Connected)
                {
                    ConnectStatus.Fill = Brushes.Red;
                    ConnectStatus.ToolTip = "Host Disconnected";
                    HostConnectStatusLabel.Content = "Host Disconnected";
                }
                else
                {
                    ConnectStatus.Fill = Brushes.Green;
                    ConnectStatus.ToolTip = "Host Connected";
                    HostConnectStatusLabel.Content = "Host Connected";
                }
            }));
        }

        public void UpdateCaseList(String caseId, String awsId, Boolean additem)
        {
            Dispatcher.BeginInvoke(new Action<String, String, Boolean>(UpdateItem), caseId, awsId, additem);
        }

        private void UpdateItem(String caseId, String wsId, Boolean additem)
        {
            Boolean caseFound = false;

            foreach (CaseWS caseWS in caseWSCollection)
            {
                if (caseWS.Case.Equals(caseId))
                {
                    if (additem)
                    {
                        Int32 index = caseWSCollection.IndexOf(caseWS);
                        caseWSCollection.RemoveAt(index);
                        caseWS.WS = wsId;
                        caseWSCollection.Insert(index, caseWS);
                    }
                    else
                    {
                        caseWSCollection.Remove(caseWS);
                    }
                    caseFound = true;

                    break;
                }
            }

            if (!caseFound && additem)
            {
                CaseWS caseWS = new CaseWS();
                caseWS.Case = caseId;
                caseWS.WS = wsId;
                caseWSCollection.Add(caseWS);
            }
        }

        public void LogMessage(String message)
        {
            if (!richTextBox1.CheckAccess())
            {
                richTextBox1.Dispatcher.Invoke(DispatcherPriority.Normal,
                    new Action(
                        delegate()
                        {
                            richTextBox1.AppendText(message);
                            richTextBox1.LineDown();
                            File.AppendAllText("log.txt", message + "\n");
                        }));
            }
            else
            {
                richTextBox1.AppendText(message);
                richTextBox1.LineDown();
                File.AppendAllText("log.txt", message + "\n");
            }
        }

        public void RefreshCaseList()
        {
            using (CaseListview.DeferRefresh()) { CaseListview.View.Refresh(); }

        }

        private void StartACSServer()
        {
            Thread AppStart = new Thread(new ThreadStart(StartApplicationThreadMethod));
            AppStart.Start();
        }

        private void StartApplicationThreadMethod()
        {
            try
            {
                if (hostComm == null)
                {
                    hostComm = new HostComm();
                    hostComm.ConnectedToHostEvent += new HostComm.ConnectedToHostHandler(m_HostComm_Impl_ConnectedToHostEvent);
                    hostComm.StartUp();
                }

                acsServer = new ACSServer(hostComm, log, _localCaseListPath, out caseList);
                acsServer.CaseListUpdateEvent += new ACSServer.CaseListUpdateHandler(UpdateCaseList);
                acsServer.StartUp();

                log.PrintLine("Network service (re)started.");

                if (caseList.FullSyncStarted)
                {
                    FullSyncMenuItem.Dispatcher.Invoke(DispatcherPriority.Normal,
                                                       new Action(delegate()
                    {
                        FullSyncMenuItem.Header = "Stop Sync";
                    }));
                }
            }
            catch (Exception exp)
            {
                log.PrintInfoLine("StartApplicationThreadMethod exp: " + exp);
            }
        }

        private void GetCaseListCountBtn_Click(object sender, RoutedEventArgs e)
        {
            if (caseList != null && caseList.List != null)
            {
                label1.Content = caseList.List.CaseListTable.Count.ToString();
            }
        }

        private void GenerateReport(IEnumerable data, String path, String ExportFormat)
        {
            try
            {
                LocalReport report = new LocalReport();
                ReportDataSource ds = new ReportDataSource("CaseListDataSet", data);
                report.DataSources.Add(ds);

                report.ReportPath = path;
                report.SetBasePermissionsForSandboxAppDomain(new System.Security.PermissionSet(System.Security.Permissions.PermissionState.Unrestricted));

                Warning[] warnings;
                string[] streamids;
                string mimeType;
                string encoding;
                string filenameExtention;

                if (ExportFormat == "PDF")
                {
                    //export report to PDF
                    byte[] bytes = report.Render(
                        "PDF", null, out mimeType, out encoding, out filenameExtention, out streamids, out warnings);

                    using (FileStream fs = new FileStream("Report.pdf", FileMode.Create))
                    {
                        fs.Write(bytes, 0, bytes.Length);
                    }

                    PDFHolder pf = new PDFHolder();
                    System.Windows.Forms.Integration.WindowsFormsHost wh = new System.Windows.Forms.Integration.WindowsFormsHost();
                    wh.Child = pf;

                    Window pdfWin = new Window();
                    pdfWin.Content = wh;
                    pdfWin.Show();
                    pf.LoadPDF("Report.pdf");
                }
                else if (ExportFormat == "Export")
                {
                    SaveFileDialog saveFileDlg = new SaveFileDialog();
                    saveFileDlg.FileName = "Report";
                    saveFileDlg.Filter = "Excel Worksheets|*.xls";
                    saveFileDlg.DefaultExt = ".xls";

                    Nullable<bool> result = saveFileDlg.ShowDialog();

                    if (result == true)
                    {
                        //export report to excel worksheet
                        byte[] bytes = report.Render(
                            "Excel", null, out mimeType, out encoding, out filenameExtention, out streamids, out warnings);

                        using (FileStream fs = new FileStream(saveFileDlg.FileName, FileMode.Create))
                        {
                            fs.Write(bytes, 0, bytes.Length);
                        }
                    }
                }

                report.Dispose();
            }
            catch (Exception exp)
            {
                log.PrintInfoLine("GenerateReport Exception: " + exp);
            }
        }

        private void GenerateReportBtn_Click(object sender, RoutedEventArgs e)
        {
            String currDirectory = AppDomain.CurrentDomain.BaseDirectory;

            String ReportPath = currDirectory + "\\Reports\\";
            if ((Boolean)ResultReport_RadioButton.IsChecked)
            {
                //Generate report based on Result
                ReportPath += "ResultReport.rdlc";
            }
            else if ((Boolean)UserReport_RadioButton.IsChecked)
            {
                //Generate report based on User/Analyst
                ReportPath += "AnalystReport.rdlc";
            }
            else if ((Boolean)DailyReport_RadioButton.IsChecked)
            {
                //Generate report based on case update Time
                ReportPath += "DailyReport.rdlc";
            }
            else if ((Boolean)AnalysisTimeReport_RadioButton.IsChecked)
            {
                ReportPath += "AnalysisTimeReport.rdlc";
            }
            else //((Boolean)AllReport_RadioButton.IsChecked)
            {
                ReportPath += "AllReport.rdlc";
            }

            String format;
            if ((Boolean)PDFExport_RadioButton.IsChecked)
            {
                //export report to PDF
                format = "PDF";
            }
            else //((Boolean)Export_RadioButton.IsChecked)
            {
                format = "Export";
            }

            if (ReportPath != "AllReport.rdlc")
            {
                // filter list based on selected dates
                var query =
                   from o in caseList.List.CaseListTable.AsEnumerable()
                   where o.UpdateTime >= ReportdatePickerStart.SelectedDate &&
                         o.UpdateTime <= ReportdatePickerEnd.SelectedDate
                   orderby o.UpdateTime descending
                   select new
                   {
                       CaseId = o.CaseId,
                       AnalystComment = o.AnalystComment,
                       ObjectId = o.ObjectId,
                       FlightNumber = o.FlightNumber,
                       Analyst = o.Analyst,
                       CaseDirectory = o.CaseDirectory,
                       ReferenceImage = o.ReferenceImage,
                       Result = o.Result,
                       UpdateTime = o.UpdateTime.Date.ToString("d"),
                       AnalysisTime = o.AnalysisTime
                   };
                GenerateReport(query, ReportPath, format);
            }
            else
            {
                // filter list based on selected dates
                var query =
                   from o in caseList.List.CaseListTable.AsEnumerable()
                   where o.UpdateTime >= ReportdatePickerStart.SelectedDate &&
                         o.UpdateTime <= ReportdatePickerEnd.SelectedDate
                   orderby o.UpdateTime descending
                   select new
                   {
                       CaseId = o.CaseId,
                       AnalystComment = o.AnalystComment,
                       ObjectId = o.ObjectId,
                       FlightNumber = o.FlightNumber,
                       Analyst = o.Analyst,
                       CaseDirectory = o.CaseDirectory,
                       ReferenceImage = o.ReferenceImage,
                       Result = o.Result,
                       UpdateTime = o.UpdateTime,
                       AnalysisTime = o.AnalysisTime
                   };

                GenerateReport(query, ReportPath, format);
            }

        }

        private void GenerateCustomReportBtn_Click(object sender, RoutedEventArgs e)
        {
            GenerateReport(CaseListview.View, ".\\Reports\\AllReport.rdlc", "PDF");
        }

        private void ShutdownACSServer()
        {
            if (acsServer != null)
            {
                acsServer.ShutDown();
                acsServer = null;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!closeApplication)
            {
                if(hostComm != null)
                {
                    hostComm.ShutDown();
                    hostComm = null;
                }

                ShutdownACSServer();

                var hwnd = new WindowInteropHelper(this).Handle;
                SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);

                e.Cancel = true;

                Thread SaveCaseListThread = new Thread(new ParameterizedThreadStart(delegate
                {
                    SaveCaseListThreadMethod(e);
                }));
                SaveCaseListThread.Start();
            }
            else
            {
                e.Cancel = false;
            }
        }

        private void SaveCaseListThreadMethod(CancelEventArgs e)
        {
            //if (caseList != null)
            //{
            //    log.PrintInfoLine("Saving case list to caselist.xml...");
            //    caseList.SaveCaseListToXMLFile();
            //    log.PrintInfoLine("Saving case list to caselist.xml...Done");
            //}

            closeApplication = true;

            Thread.Sleep(1000);

            this.Dispatcher.Invoke(DispatcherPriority.Normal,
                                   new Action(delegate()
                                   {
                                       this.Close();
                                   }));
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            About aboutdlg = new About();
            aboutdlg.ShowDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartACSServer();
        }

        private void FullSyncMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (caseList.FullSyncStarted == true)
            {
                caseList.StopSync = true;
                FullSyncMenuItem.Header = "Start Full Sync";
            }
            else
            {
                caseList.StartFullSync();
                FullSyncMenuItem.Header = "Stop Sync";
            }

        }

        private void SelectLocalCaseFolderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var selLocalCaseFldrDlg = new L3.Cargo.ArchiveCaseServer.FolderBrowserDialogEx
            {
                Description = "Select archived case folder:",
                NewStyle = true,
                ShowNewFolderButton = false,
                ShowEditBox = true,
                ShowFullPathInEditBox = true,
                RootFolder = System.Environment.SpecialFolder.MyComputer,
                SelectedPath = ""
            };

            var result = selLocalCaseFldrDlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ShutdownACSServer();

                _localCaseListPath = selLocalCaseFldrDlg.SelectedPath;
                StartACSServer();
            }
        }
    }

    public class CaseWSCollection : ObservableCollection<CaseWS>
    {
    }

    public class CaseWS
    {
        public String Case { get; set; }
        public String WS { get; set; }
    }

}
