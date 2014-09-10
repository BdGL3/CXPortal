using System;
using System.Windows;
using System.Windows.Controls;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Data;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.ServiceModel.Description;
using System.Threading;
using System.IO;
using System.Xml.Serialization;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Client;
using L3.Cargo.Communications.Interfaces;
using L3.Cargo.Common;
using System.ComponentModel;
using System.Windows.Data;

namespace ArchiveCaseClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static CaseList caseList = new CaseList();
        private CaseRequestManagerEndpoint ACSEndPoint;
        private Guid m_AWSId;        

        class CACSCaseRequestManagerCallback : CaseRequestManagerCallback
        {
            MainWindow logger;
            public CACSCaseRequestManagerCallback(MainWindow log)
            {
                logger = log;
            }
            public override void UpdatedCaseList(CaseListUpdate updatelist)
            {
                if (updatelist.state == CaseListUpdateState.Add)
                {
                    //caseList.Merge(updatelist.dsCaseList);
                    
                    foreach (CaseListDataSet.CaseListTableRow row in updatelist.dsCaseList.CaseListTable.Rows)
                    {
                        try
                        {
                            lock (caseList.CaseListLock)
                            {
                                caseList.List.CaseListTable.AddCaseListTableRow(row.CaseId, row.AnalystComment, row.ObjectId, row.FlightNumber,
                                    row.Analyst, row.CaseDirectory, row.ReferenceImage, row.Result,
                                    row.UpdateTime, row.Archived, row.AnalysisTime, row.CreateTime, row.Area, row.Image, row.CTI, row.AssignedId);
                            }
                        }
                        catch (ConstraintException exp)
                        {                            
                            logger.listBox1.Dispatcher.Invoke(DispatcherPriority.Normal,
                                new Action(
                                    delegate()
                                    {
                                        logger.listBox1.Items.Add("Exp: " + exp.ToString());
                                    }));
                        }
                    }
                    
                }
                else if (updatelist.state == CaseListUpdateState.Delete)
                {
                    foreach (DataRow row in updatelist.dsCaseList.CaseListTable.Rows)
                    {
                        string caseId = row[updatelist.dsCaseList.CaseListTable.CaseIdColumn, DataRowVersion.Original].ToString();
                        bool refImg = (bool) row[updatelist.dsCaseList.CaseListTable.ReferenceImageColumn, DataRowVersion.Original];
                        DataRow foundRow = caseList.List.CaseListTable.FindByCaseIdReferenceImage(caseId, refImg);
                        if (foundRow != null)
                        {
                            foundRow.Delete();
                        }
                    }
                }
                else if (updatelist.state == CaseListUpdateState.Modify)
                {
                    foreach (DataRow row in updatelist.dsCaseList.CaseListTable.Rows)
                    {
                        string caseId = row[updatelist.dsCaseList.CaseListTable.CaseIdColumn, DataRowVersion.Original].ToString();
                        bool refImg = (bool)row[updatelist.dsCaseList.CaseListTable.ReferenceImageColumn, DataRowVersion.Original];
                        DataRow foundRow = caseList.List.CaseListTable.FindByCaseIdReferenceImage(caseId, refImg);
                        if (foundRow != null)
                        {
                            foundRow.ItemArray = row.ItemArray;
                        }
                    }
                }

                //caseList.AcceptChanges();                
            }
        }

        private void PingHost()
        {
            while (ACSEndPoint != null)
            {
                try
                {
                    ACSEndPoint.Ping(m_AWSId.ToString());
                }
                catch
                {
                    ACSEndPoint = null;                    
                }

                Thread.Sleep(1000);
            }
        }

        private void ListenForACSComm()
        {
            Boolean createdPanel = false;
            HostDiscovery awsCommDiscovery = new HostDiscovery(typeof(ICaseRequestManager), new TimeSpan(0, 0, 0, 1, 0));

            while (!createdPanel)
            {

                Collection<EndpointDiscoveryMetadata> ACSCommEndpoints = awsCommDiscovery.GetAvailableConnections();

                if (ACSCommEndpoints.Count > 0)
                {
                    for (Int32 index = 0; index < ACSCommEndpoints.Count; index++)
                    {
                        InstanceContext acsCommCallback = new InstanceContext(new CACSCaseRequestManagerCallback(this));

                        TCPBinding tcpbinding = new TCPBinding();                        

                        ServiceEndpoint HostEndPoint = new ServiceEndpoint(ContractDescription.GetContract(typeof(ICaseRequestManager)), tcpbinding, ACSCommEndpoints[index].Address);

                        if (index == 0)
                        {
                            // Connect and setup our endpoint
                            ACSEndPoint = new CaseRequestManagerEndpoint(acsCommCallback, HostEndPoint);
                            //tabItem1.Visibility = System.Windows.Visibility.Visible;
                            //Tab1Grid.Visibility = System.Windows.Visibility.Visible;

                            Thread Conn1Ping = new Thread(new ThreadStart(PingHost));
                            Conn1Ping.Start();
                        }
                          
                        else if (index == 1)
                        {
                            //m_AWSCommEndPoint2 = new AWSCommEndpoint(awsCommCallback, HostEndPoint);
                            //tabItem2.Visibility = System.Windows.Visibility.Visible;
                            //Tab2Grid.Visibility = System.Windows.Visibility.Visible;
                        }
                        else if (index == 2)
                        {
                           // m_AWSCommEndPoint3 = new AWSCommEndpoint(awsCommCallback, HostEndPoint);
                          //  tabItem3.Visibility = System.Windows.Visibility.Visible;
                           // Tab3Grid.Visibility = System.Windows.Visibility.Visible;
                        }
                        else if (index == 3)
                        {
                           // m_AWSCommEndPoint4 = new AWSCommEndpoint(awsCommCallback, HostEndPoint);
                           // tabItem4.Visibility = System.Windows.Visibility.Visible;
                            //Tab4Grid.Visibility = System.Windows.Visibility.Visible;
                        }
                        
                        //Show the Tab Page
                    }
                    createdPanel = true;
                }
                Thread.Sleep(10000);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            m_AWSId = Guid.NewGuid();
            ListenForACSComm();

            dataGrid1.DataContext = caseList.List.CaseListTable;            
            dataGrid1.AddHandler(Control.MouseDoubleClickEvent, new RoutedEventHandler(DataGrid1_OnMouseDoubleClickEvent));           
        }

        protected void DataGrid1_OnMouseDoubleClickEvent(object sender, RoutedEventArgs e)
        {
            if (e.Source != null)
            {               
                DataGrid grid = e.Source as DataGrid;

                if (grid.CurrentItem != null)
                {
                    DataRowView selectedRow = (DataRowView)grid.CurrentItem;
                    int caseIdColumnNum = caseList.List.CaseListTable.CaseIdColumn.Ordinal;
                    String caseid = (String)selectedRow.Row.ItemArray[caseIdColumnNum];
                    listBox1.Items.Clear();
                    listBox1.Items.Add("Getting case files for caseid: " + caseid);
                    //GetCase(caseid);
                }
            }
            return;
        }
        /*
        private void GetCase(String caseid)
        {
            CaseMessage message = new CaseMessage();
            message.CaseId = caseid;
            message.WorkstationId = m_AWSId.ToString();

            //make case directory
            if (!Directory.Exists("C:\\Reena"))
                Directory.CreateDirectory("C:\\Reena");

            String DirName = "C:\\Reena\\" + caseid;
            if (!Directory.Exists(DirName))
                Directory.CreateDirectory(DirName);

            try
            {
                //get case xml file
                CaseRequestMessageResponse response = ACSEndPoint.RequestCase(message);

                FileStream casexml = new FileStream(DirName + "\\case.xml", FileMode.Create);
                response.file.CopyTo(casexml);
                listBox1.Items.Add("case.xml");
                casexml.Close();
                GetCaseFiles(DirName, caseid);
            }
            catch (FaultException exp)
            {
                listBox1.Items.Add(exp.Message);
            }
        }

        private void GetCaseFiles(String dir, String caseid)
        {
            String xmlfile = dir + "\\case.xml";
            FileStream casexml = new FileStream(xmlfile, FileMode.Open);
            XmlSerializer serializer = new XmlSerializer(typeof(XCase));
            XCase xc = (XCase) serializer.Deserialize(casexml);

            if (xc != null)
            {
                if (xc.xRayImage != null)
                {
                    String[] xrayImage = xc.xRayImage;

                    foreach (String imagename in xrayImage)
                    {
                        try
                        {
                            Stream image = ACSEndPoint.RequestCaseData(new CaseDataInfo(caseid, imagename));
                            FileStream img = new FileStream(dir + "\\" + imagename, FileMode.Create);
                            image.CopyTo(img);
                            img.Close();
                            listBox1.Items.Add(imagename);
                        }
                        catch (Exception exp)
                        {
                            listBox1.Items.Add(exp.Message);
                        }
                    }
                }
                
                if (xc.attachment != null)
                {
                    Attachment[] attch = xc.attachment;

                    foreach (Attachment file in attch)
                    {
                        try
                        {
                            Stream f = ACSEndPoint.RequestCaseData(new CaseDataInfo(caseid, file.File));
                            FileStream attachFile = new FileStream(dir + "\\" + file, FileMode.Create);
                            f.CopyTo(attachFile);
                            attachFile.Close();
                            listBox1.Items.Add(file.File);
                        }
                        catch (Exception exp)
                        {
                            listBox1.Items.Add(exp.Message);
                        }
                    }
                }

                if (xc.tdsResultFile != null && xc.tdsResultFile.Length > 1)
                {
                    try
                    {
                        Stream f = ACSEndPoint.RequestCaseData(new CaseDataInfo(caseid, xc.tdsResultFile));
                        FileStream attachFile = new FileStream(dir + "\\" + xc.tdsResultFile, FileMode.Create);
                        f.CopyTo(attachFile);
                        attachFile.Close();
                        listBox1.Items.Add(xc.tdsResultFile);
                    }
                    catch (Exception exp)
                    {
                        listBox1.Items.Add(exp.Message);
                    }                 
                }

                if (xc.vehicle != null)
                {
                    if (xc.vehicle.manifest != null)
                    {
                        Manifest[] files = xc.vehicle.manifest;
                        
                       foreach (Manifest file in files)
                        {
                            try
                            {
                                Stream f = ACSEndPoint.RequestCaseData(new CaseDataInfo(caseid, file.image));
                                FileStream attachFile = new FileStream(dir + "\\" + file.image, FileMode.Create);
                                f.CopyTo(attachFile);
                                attachFile.Close();
                                listBox1.Items.Add(file.image);
                            }
                            catch (Exception exp)
                            {
                                listBox1.Items.Add(exp.Message);
                            }
                        }
                    }
                }
            }

            casexml.Close();

            return;
          
        }
        */
        private void GetCaseListBtn_Click(object sender, RoutedEventArgs e)
        {
            DateTime StartTime = DateTime.Now;
            listBox1.Items.Add(StartTime + ":" + StartTime.Millisecond + ": " + "Requesting Case list from ACS" + "\r");
            //caseList.List = ACSEndPoint.RequestCaseList(m_AWSId.ToString());

            var CaseListview = (ExtendedCollectionViewSource)FindResource("cvs");
            lock (caseList.CaseListLock)
            {
                CaseListview.Source = caseList.List.Tables[0].DefaultView;
            }
            dataGrid1.DataContext = CaseListview;
            dataGrid1.SelectedIndex = -1;

            foreach (DataGridColumn column in dataGrid1.Columns)
            {
                if ((String)column.Header == "Update Time")
                {
                    ListSortDirection direction = ListSortDirection.Descending;
                    column.SortDirection = direction;
                    SortCaseList(direction, column);
                    break;
                }
            }

            DateTime EndTime = DateTime.Now;
            listBox1.Items.Add(EndTime + ":" + EndTime.Millisecond + ": " + "GetCaseList " + caseList.List.CaseListTable.Count + " entries. Elapsed time: " + (EndTime - StartTime));     
            caseList.List.AcceptChanges();
            //dataGrid1.DataContext = caseList.CaseListTable;
        }

        public void RefreshCaseList()
        {
            try
            {
                if (!dataGrid1.CheckAccess())
                {
                    try
                    {
                        dataGrid1.Dispatcher.Invoke(DispatcherPriority.Normal,
                            new Action(
                                delegate()
                                {
                                    dataGrid1.Items.Refresh();
                                }));
                    }
                    catch (Exception exp)
                    {
                        throw exp;
                    }
                }
                else
                {
                    dataGrid1.Items.Refresh();
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        private void GetCaseListCountBtn_Click(object sender, RoutedEventArgs e)
        {
            listBox1.Items.Add("Case list count: " + caseList.List.CaseListTable.Count);
        }

        private void SetCaseRefBtn_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selectedRow = (DataRowView)dataGrid1.SelectedItem;
            int caseIdColumnNum = caseList.List.CaseListTable.CaseIdColumn.Ordinal;
            String caseid = (String)selectedRow.Row.ItemArray[caseIdColumnNum];
            listBox1.Items.Clear();
            listBox1.Items.Add("Setting case as reference: " + caseid);
            
            UpdateCaseMessage updateMsg = new UpdateCaseMessage();
            updateMsg.CaseId = caseid;
            updateMsg.File = new MemoryStream();
            updateMsg.Type = CaseUpdateEnum.SetAsReference;//UpdatedCaseFile;
            //updateMsg.SetAsReference = true;
            ACSEndPoint.UpdateCase(updateMsg);
        }

        private void Source_dataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;

            ListSortDirection direction = (e.Column.SortDirection != ListSortDirection.Ascending) ?
                ListSortDirection.Ascending : ListSortDirection.Descending;
            e.Column.SortDirection = direction;

            SortCaseList(direction, e.Column);            
        }

        private void SortCaseList(ListSortDirection direction, DataGridColumn column)
        {
            CaseListSort sort = new CaseListSort(direction, column);
            var collectionview = (ExtendedCollectionViewSource)FindResource("cvs");
            ListCollectionView view = (ListCollectionView)collectionview.View;

            using (collectionview.DeferRefresh())
            {
                view.CustomSort = sort;
            }
        }
        
    }
}
