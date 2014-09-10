using System;
using System.IO;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Collections;
using System.Configuration;
using System.Windows.Forms;
using System.ComponentModel;
using System.Windows;
using System.IO.Compression;

// Custom Imports
using l3.cargo.corba;
using omg.org.CosNaming;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Services;
using System.Runtime.Remoting.Channels;
using L3.Cargo.OCR.Interfaces;
using L3.Cargo.OCR.Messages;
using System.Net;

using Ionic.Zip;

using L3.Cargo.Communications.EventsLogger.Client;


namespace L3.Cargo.OCR.UI
{
    /// <summary>
    /// formOcrMonitor. An instance of this class implements the processing
    /// of OCR data. It integrates the OCR data set with the CargoHost module.
    /// </summary>
    public class formOcrMonitor : System.Windows.Forms.Form
    {
        #region Private Members

        private const string CLASS_NAME="formOcrMonitor";
        private const int MAX_PTS=600;
        private double [] m_X=new double[MAX_PTS];
        private double [,] m_Y = new double[2, MAX_PTS];
        private OCRMonitor m_OCRMonitor = null;
        private CargoHostInterface m_cargoHostIF = null;

        private System.Windows.Forms.StatusBar sbApp;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        public System.Windows.Forms.Label lblOCRMessage;
        public System.Windows.Forms.Label lblOCRConnectedStatus;
        public System.Windows.Forms.Label lblOCRRegisteredStatus;
        private System.Data.DataColumn DataCol_Time;
        private System.Data.DataColumn DataCol_Guid;
        private System.Data.DataColumn DataCol_Message;
        public System.Windows.Forms.ListView listView_Log;
        public System.Windows.Forms.ColumnHeader colTime;
        private ColumnHeader colMessage;
        private System.Windows.Forms.Button btnProcessOCRMessage;
        private Button TestUnzip;

        private EventLoggerAccess _logger;

        #endregion


        #region Constructor

        public formOcrMonitor()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            // Get runtime settings from our .config file.
            IPAddress OCR_Host = IPAddress.Parse((string)ConfigurationManager.AppSettings["OCR_host"]);
            Int32 OCR_Port = Int32.Parse(ConfigurationManager.AppSettings["OCR_port"]);
            string OCR_MessagePrefix = ConfigurationManager.AppSettings["OCR_MessagePrefix"];
            string CargoSenderName = ConfigurationManager.AppSettings["CargoSenderName"];
            bool ConnectAsListener = Boolean.Parse(ConfigurationManager.AppSettings["ConnectAsListener"]);

            m_OCRMonitor = new OCRMonitor(OCR_Host,
                                          OCR_Port,
                                          ConnectAsListener,
                                          OCR_MessagePrefix,
                                          CargoSenderName
                                         );
            // Register for OCR connection status updates.
            m_OCRMonitor.UpdateConnectionStatus +=
                         new OCRMonitor.OCRConnectionStatusNotifier(ConnectionStatusUpdate);
            // Register for notification of receipt of OCR messages.
            m_OCRMonitor.NotifyRxACK +=
                         new OCRMonitor.OCRMessageHandler(ReceivedOcrMessage);
            m_OCRMonitor.NotifyRxNACK +=
                         new OCRMonitor.OCRMessageHandler(ReceivedOcrMessage);
            m_OCRMonitor.NotifyRxOCR_MASTER +=
                         new OCRMonitor.OCRMessageHandler(ReceivedOCR_MASTER);
            m_OCRMonitor.NotifyRxOCR_NEW_EVENT +=
                         new OCRMonitor.OCRMessageHandler(ReceivedOCR_NEW_EVENT);
            m_OCRMonitor.NotifyRxOCR_ULD +=
                         new OCRMonitor.OCRMessageHandler(ReceivedOcrMessage);
            m_OCRMonitor.NotifyRxPING +=
                         new OCRMonitor.OCRMessageHandler(ReceivedOcrMessage);
            m_OCRMonitor.NotifyRxREGISTER +=
                         new OCRMonitor.OCRMessageHandler(ReceivedREGISTER);
            m_OCRMonitor.NotifyRxUNREGISTER +=
                         new OCRMonitor.OCRMessageHandler(ReceivedUNREGISTER);

            m_OCRMonitor.Start();

            _logger = new EventLoggerAccess();

            //if (m_OCRMonitor.IsRunning)
            {
                m_cargoHostIF = new CargoHostInterface(_logger);
            }
        }

        #endregion

        #region Deconstructor

        protected override void Dispose( bool disposing )
        {
            StopOCRMonitor();
            base.Dispose( disposing );
        }

        #endregion

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formOcrMonitor));
            this.sbApp = new System.Windows.Forms.StatusBar();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblOCRMessage = new System.Windows.Forms.Label();
            this.lblOCRConnectedStatus = new System.Windows.Forms.Label();
            this.lblOCRRegisteredStatus = new System.Windows.Forms.Label();
            this.btnProcessOCRMessage = new System.Windows.Forms.Button();
            this.DataCol_Time = new System.Data.DataColumn();
            this.DataCol_Guid = new System.Data.DataColumn();
            this.DataCol_Message = new System.Data.DataColumn();
            this.listView_Log = new System.Windows.Forms.ListView();
            this.colTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colMessage = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TestUnzip = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // sbApp
            // 
            this.sbApp.Location = new System.Drawing.Point(0, 707);
            this.sbApp.Name = "sbApp";
            this.sbApp.Size = new System.Drawing.Size(488, 22);
            this.sbApp.TabIndex = 10;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.lblOCRMessage);
            this.groupBox1.Controls.Add(this.lblOCRConnectedStatus);
            this.groupBox1.Controls.Add(this.lblOCRRegisteredStatus);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(8, 32);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(472, 88);
            this.groupBox1.TabIndex = 44;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "OCR SYSTEM";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(16, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(320, 20);
            this.label3.TabIndex = 52;
            this.label3.Text = "Current Message";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(352, 62);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(90, 20);
            this.label4.TabIndex = 51;
            this.label4.Text = "Connected";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(452, 62);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(100, 20);
            this.label5.TabIndex = 50;
            this.label5.Text = "Registered";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label5.Visible = false;
            // 
            // lblOCRMessage
            // 
            this.lblOCRMessage.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblOCRMessage.Location = new System.Drawing.Point(16, 24);
            this.lblOCRMessage.Name = "lblOCRMessage";
            this.lblOCRMessage.Size = new System.Drawing.Size(320, 32);
            this.lblOCRMessage.TabIndex = 2;
            this.lblOCRMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblOCRConnectedStatus
            // 
            this.lblOCRConnectedStatus.BackColor = System.Drawing.Color.Red;
            this.lblOCRConnectedStatus.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblOCRConnectedStatus.Location = new System.Drawing.Point(384, 24);
            this.lblOCRConnectedStatus.Name = "lblOCRConnectedStatus";
            this.lblOCRConnectedStatus.Size = new System.Drawing.Size(32, 32);
            this.lblOCRConnectedStatus.TabIndex = 0;
            // 
            // lblOCRRegisteredStatus
            // 
            this.lblOCRRegisteredStatus.BackColor = System.Drawing.Color.Red;
            this.lblOCRRegisteredStatus.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblOCRRegisteredStatus.Location = new System.Drawing.Point(484, 24);
            this.lblOCRRegisteredStatus.Name = "lblOCRRegisteredStatus";
            this.lblOCRRegisteredStatus.Size = new System.Drawing.Size(32, 32);
            this.lblOCRRegisteredStatus.TabIndex = 0;
            this.lblOCRRegisteredStatus.Visible = false;
            // 
            // btnProcessOCRMessage
            // 
            this.btnProcessOCRMessage.Location = new System.Drawing.Point(330, 3);
            this.btnProcessOCRMessage.Name = "btnProcessOCRMessage";
            this.btnProcessOCRMessage.Size = new System.Drawing.Size(150, 33);
            this.btnProcessOCRMessage.TabIndex = 55;
            this.btnProcessOCRMessage.Text = "Test OCR Message";
            this.btnProcessOCRMessage.Click += new System.EventHandler(this.btnProcessOCRMessage_Click);
            // 
            // DataCol_Time
            // 
            this.DataCol_Time.ColumnName = "Time";
            // 
            // DataCol_Guid
            // 
            this.DataCol_Guid.ColumnName = "GUID";
            // 
            // DataCol_Message
            // 
            this.DataCol_Message.ColumnName = "Message";
            // 
            // listView_Log
            // 
            this.listView_Log.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colTime,
            this.colMessage});
            this.listView_Log.FullRowSelect = true;
            this.listView_Log.Location = new System.Drawing.Point(8, 126);
            this.listView_Log.Name = "listView_Log";
            this.listView_Log.Size = new System.Drawing.Size(472, 595);
            this.listView_Log.TabIndex = 57;
            this.listView_Log.UseCompatibleStateImageBehavior = false;
            this.listView_Log.View = System.Windows.Forms.View.Details;
            // 
            // colTime
            // 
            this.colTime.Text = "Time";
            this.colTime.Width = 161;
            // 
            // colMessage
            // 
            this.colMessage.Text = "Message";
            this.colMessage.Width = 306;
            // 
            // TestUnzip
            // 
            this.TestUnzip.Location = new System.Drawing.Point(164, 3);
            this.TestUnzip.Name = "TestUnzip";
            this.TestUnzip.Size = new System.Drawing.Size(150, 33);
            this.TestUnzip.TabIndex = 58;
            this.TestUnzip.Text = "Test Unzip";
            this.TestUnzip.Click += new System.EventHandler(this.TestUnzip_Click);
            // 
            // formOcrMonitor
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(488, 729);
            this.Controls.Add(this.TestUnzip);
            this.Controls.Add(this.listView_Log);
            this.Controls.Add(this.btnProcessOCRMessage);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.sbApp);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "formOcrMonitor";
            this.Text = "OCR Monitor";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.frmMonitor_Closing);
            this.Load += new System.EventHandler(this.frmMonitor_Load);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        #region Methods

        [STAThread]
        static void Main()
        {
            formOcrMonitor thisForm = new formOcrMonitor();
            Application.Run(thisForm);
        }

        private void frmMonitor_Load(object sender, System.EventArgs e)
        {
            System.Threading.Thread.CurrentThread.Name = "MainApp";
        }

        private void StopOCRMonitor()
        {
            //stop the ocr thread and wait for
            //it to gracefully die
            m_OCRMonitor.Shutdown();
            m_OCRMonitor = null;
        }

        private void frmMonitor_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void WriteLogMessage(string message)
        {
            // Verify that the log isn't too large.
            while ((this.listView_Log.Items.Count - 1) >= 100)
            {
                // Log is too large, let's reduce the log list to a more
                // managable size.
                this.listView_Log.Items.RemoveAt(this.listView_Log.Items.Count - 1);
            }

            // Create a new ListViewItem and add all the necessary
            // information to it.
            ListViewItem item = new ListViewItem(DateTime.Now.ToString(), 0);
            item.SubItems.Add(message);

            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate()
                {
                    // Insert the ListViewItem into the ListView.
                    this.listView_Log.Items.Insert(0, item);

                    // Update Current Message Location
                    this.lblOCRMessage.Text = message;
                }));
            }
            else
            {
                // Insert the ListViewItem into the ListView.
                this.listView_Log.Items.Insert(0, item);

                // Update Current Message Location
                this.lblOCRMessage.Text = message;
            }
        }

        private void ConnectionStatusUpdate(bool isConnected)
        {
            this.lblOCRConnectedStatus.BackColor = (isConnected
                                                      ? System.Drawing.Color.FromArgb(255, 64, 224, 64)
                                                      : System.Drawing.Color.Red);
        }

        private void ReceivedOcrMessage(object messageObj, string msgName)
        {
            try
            {
                WriteLogMessage(msgName);
            }
            catch (Exception ex)
            {
                // bury any exceptions
            }
        }

        private void ReceivedOCR_MASTER(object messageObj, string msgName)
        {
            L3.Cargo.OCR.Messages.ocr_master.message master_msg = (Messages.ocr_master.message)messageObj;

            string[] rootCaseID = new string[10];
            int itemCount = 0;

            try
            {
                WriteLogMessage(msgName);

                if (string.IsNullOrEmpty(master_msg.body.payload.CaseNumber))
                {   // Message contains no case ID, create a new case for
                    // the message.

                    int i = 0;

                    for (; i < master_msg.body.payload.Cont.Length; i++)
                    {
                        rootCaseID[i] = m_cargoHostIF.CreateCase(master_msg.body.payload.Cont[i].Code);
                        itemCount++;
                    }

                    if (String.IsNullOrEmpty(rootCaseID[0]))
                    {
                        for (; i < master_msg.body.payload.Trailer.Length; i++)
                        {
                            rootCaseID[i] = m_cargoHostIF.CreateCase(master_msg.body.payload.Trailer[i].Code);
                            itemCount++;
                        }
                    }

                    if (String.IsNullOrEmpty(rootCaseID[0]))
                    {
                        rootCaseID[i] = m_cargoHostIF.CreateCase(master_msg.body.payload.Vehicle.Code);
                        itemCount++;
                    }
                }
                else
                {   // Extract the case ID from the message.
                    m_cargoHostIF.SetContainerNumber(rootCaseID[0], master_msg.body.payload.Cont[0].Code);

                    string [] caseList = m_cargoHostIF.GetScanCaseList();

                    if (caseList != null && caseList.Length > 0)
                    {
                        int j = 1;

                        for( int i = 0; i < caseList.Length; i++) 
                        //foreach (string caseid in caseList)
                        {
                            if (caseList[i].CompareTo(rootCaseID) == 0)
                            {
                                if (i <= caseList.Length)
                                {
                                    rootCaseID[j] = caseList[i];
                                    m_cargoHostIF.SetContainerNumber(rootCaseID[j], master_msg.body.payload.Cont[j].Code);
                                    j++;
                                }
                                else
                                {
                                    for (; j < master_msg.body.payload.Cont.Length; j++)
                                    {
                                        rootCaseID[j] = m_cargoHostIF.CreateCase(master_msg.body.payload.Cont[j].Code);
                                    }
                                }
                            }

                            break;
                        }
                    }
                }

                try
                {   // ==> Add Manifest(s) to Case
                    for (int id = 0; id < master_msg.body.payload.Cont.Length; id++)
                    {
                        try
                        {
                            if (!String.IsNullOrEmpty(master_msg.body.payload.Cont[id].Code))
                            {
                                m_cargoHostIF.AddManifest(master_msg.body.payload.Cont[id].Code, rootCaseID[id]);
                            }
                        }
                        catch (System.NullReferenceException)
                        {
                            // Container Object was not created after
                            // deserializing object. Don't worry about
                            // it, press on.
                        }
                        catch (CargoException)
                        {
                            // Error with logger
                        }
                    }
                }   // <== Add Manifest(s) to Case
                catch (System.NullReferenceException)
                {
                    // Container Object was not created after
                    // deserializing object. Don't worry about it,
                    // press on.
                }

                try
                {   // ==> Add per-container OCR file(s) to case
                    for (int id = 0; id < master_msg.body.payload.Cont.Length; id++)
                    {
                        try
                        {
                            if (File.Exists(master_msg.body.payload.Cont[id].FileName))
                            {
                                string basePath = @"C:\Temp\";
                                if (master_msg.body.payload.Cont[id].FileName.EndsWith(".zip", false, null))
                                {
                                    ZipFile zf = new ZipFile(master_msg.body.payload.Cont[id].FileName);

                                    foreach(ZipEntry ze in zf)
                                    {
                                        if (ze.FileName.EndsWith(".jpg", false, null))
                                        {
                                            ze.Extract(basePath);
                                            m_cargoHostIF.AddOCRFile(rootCaseID[id], basePath + ze.FileName);
                                        }
                                    }

                                    zf.Dispose();
                                }
                                else
                                {
                                    m_cargoHostIF.AddOCRFile(rootCaseID[id], master_msg.body.payload.Cont[id].FileName);
                                }
                            }
                        }
                        catch (System.NullReferenceException)
                        {
                            // Container Object was not created after
                            // deserializing object. Don't worry about
                            // it, press on.
                        }
                        catch (CargoException)
                        {
                            // Error with logger
                        }
                    }
                }   // <== Add per-container OCR file(s) to case
                catch (System.NullReferenceException)
                {
                    // Container Object was not created after
                    // deserializing object. Don't worry about it, press
                    // on.
                }

                try
                {   // ==> Add per-trailer OCR file(s) to case
                    for (int id = 0; id < master_msg.body.payload.Trailer.Length; id++)
                    {
                        try
                        {
                            if (File.Exists(master_msg.body.payload.Trailer[id].FileName))
                            {
                                string basePath = @"C:\Temp\";

                                if (master_msg.body.payload.Trailer[id].FileName.EndsWith(".zip", false, null))
                                {
                                    ZipFile zf = new ZipFile(master_msg.body.payload.Trailer[id].FileName);

                                    foreach (ZipEntry ze in zf)
                                    {
                                        if (ze.FileName.EndsWith(".jpg", false, null))
                                        {
                                            ze.Extract(basePath);
                                            m_cargoHostIF.AddOCRFile(rootCaseID[id], basePath + ze.FileName);
                                        }
                                    }

                                    zf.Dispose();
                                }
                                else
                                {
                                    m_cargoHostIF.AddOCRFile(rootCaseID[id], master_msg.body.payload.Trailer[id].FileName);
                                }
                            }
                        }
                        catch (System.NullReferenceException)
                        {
                            // Container Object was not created after
                            // deserializing object. Don't worry about
                            // it, press on.
                        }
                        catch (CargoException)
                        {
                            // Error with logger
                        }
                    }
                }   // <== Add per-trailer OCR file(s) to case
                catch (System.NullReferenceException)
                {
                    // Container Object was not created after
                    // deserializing object. Don't worry about it, press
                    // on.
                }

                try
                {   // ==> Add vehicle OCR file to case
                    if (File.Exists(master_msg.body.payload.Vehicle.FileName))
                    {
                        string basePath = @"C:\Temp\";

                        if (master_msg.body.payload.Vehicle.FileName.EndsWith(".zip", false, null))
                        {
                            ZipFile zf = new ZipFile(master_msg.body.payload.Vehicle.FileName);

                            foreach (ZipEntry ze in zf)
                            {
                                if (ze.FileName.EndsWith(".jpg", false, null))
                                {
                                    ze.Extract(basePath);
                                    m_cargoHostIF.AddOCRFile(rootCaseID[0], basePath + ze.FileName);
                                }
                            }

                            zf.Dispose();
                        }
                        else
                        {
                            m_cargoHostIF.AddOCRFile(rootCaseID[0], master_msg.body.payload.Vehicle.FileName);
                        }
                    }
                }   // <== Add vehicle OCR file to case
                catch (System.NullReferenceException)
                {
                    // Container Object was not created after
                    // deserializing object. Don't worry about it, press
                    // on.
                }
                catch (CargoException)
                {
                    // Error with logger
                }
            }
            catch (Exception ex)
            {
                try
                {
                    _logger.LogError(  "OCRMonitor ReceivedOCR_MASTER - "+ ex.Message);
                }
                catch (CargoException)
                {
                    // Error with logger
                }
            }
        }

        private void ReceivedOCR_NEW_EVENT(object messageObj, string msgName)
        {
            try
            {
                WriteLogMessage(msgName);

                string rootCaseID = m_cargoHostIF.CreateCase();
                if (rootCaseID != null)
                {
                    m_OCRMonitor.SendNewEventResponse(messageObj, rootCaseID);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(  "OCRMonitor - ReceivedOCR_NEW_EVENT"+ ex.Message);
            }
        }

        private void ReceivedREGISTER(object messageObj, string msgName)
        {
            try
            {
                WriteLogMessage(msgName);

                this.lblOCRRegisteredStatus.BackColor = System.Drawing.Color.FromArgb( 255, 64, 224, 64);
            }
            catch (Exception)
            {
                // Per the ICD, if something is unrecognized or corrupt,
                // just ignore it .
            }
        }

        private void ReceivedUNREGISTER(object messageObj, string msgName)
        {
            try
            {
                WriteLogMessage(msgName);

                this.lblOCRRegisteredStatus.BackColor = System.Drawing.Color.Red;
            }
            catch (Exception)
            {
                // Per the ICD, if something is unrecognized or corrupt,
                // just ignore it .
            }
        }

        private void btnProcessOCRMessage_Click(object sender, System.EventArgs ea)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "xml files (*.xml)|*.xml";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Multiselect = false;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamReader sr = new StreamReader(openFileDialog1.FileName))
                    {
                        String s = String.Empty;
                        String line;

                        // Read and display lines from the file until the end of
                        // the file is reached.
                        while ((line = sr.ReadLine()) != null)
                        {
                            s += line;
                        }

                        m_OCRMonitor.ProcessOCRMessage(s);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        #endregion

        private void TestUnzip_Click(object sender, EventArgs e)
        {
        }

    }
}
