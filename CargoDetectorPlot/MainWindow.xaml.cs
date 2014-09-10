using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms.DataVisualization.Charting;
using System.Configuration;
using Microsoft.Win32;
using System.IO;
using System.IO.Pipes;
using System.Windows.Forms;
using System.Threading;
using System.Windows.Threading;
using System.Collections.Specialized;
using System.Reflection;
using System.Diagnostics;
using System.Xml;
using System.Xaml;


namespace L3.Cargo.DetectorPlot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        #region Pipes
        //public string SaveDataPath = "C:\\Pipe Data";
        //String strPipeName1 = "CXMSG1";
        //String strPipeName2 = "CXDATA1";
        //public string strServerName = ".";//same computer;"HOST";//for test only 
        //public NamedPipeServerStream MsgSrvHandle;
        //public NamedPipeServerStream DataSrvHandle;
        #endregion

        public const int LoByteMask = 255;
        public const int MidByteMask = 4080;
        public const int HiByteMask = 65280;
        public Process client;

        DataLines lineArray = new DataLines(); 

        #region Globals
        public double SizeRatio;
        public float[] pxedata = new float[0];
        public int CurrentPXEHeight = 0;
        public int CurrentPXEWidth = 0;
        public int ShowPoints = 0;
        public int BitsTypeShow = 0;
        public PxeAccess pxa;
        public NumericUpDown NumofEmulatorBoardsUpdown;
        public NumericUpDown AnCurrentDetectorSelUpDown;
        public NumericUpDown CurrentSelDetNumUpDown;
        public System.Windows.Forms.ToolTip ChartTT;
        public int SelectedType;
        public int DisplayMode;//0-spatial,1-time
        public int BitsShowMode;//0-full,1-low,2-hi,3-mid
        public int NumberOfRuns;
        public int CurrentTimeModeCounter;
        public byte[,] TimeModeArray;
        public byte[] ArrayFromFile = new byte[0];
        static public PxeAccess pa;
        public int CurDet;
        public int DetPoints;
        public bool EmulatorEnabled;
        public int EmulatorBoardsCount = 1;
        public byte[] EmulatorData;
        public byte[] tempstorage;
        public bool lb1initflag;
        //=============== couple flags for plot
        public bool SetupTimeModeUi = false;
        public bool SetupSpatialModeUi = false;
        //======================================
        public DataPoint[] localbackup = new DataPoint[0];
        public DispatcherTimer dispatcherTimer;
        public string SavedFileName = "";
        public string AnSavedFileName = "";
        public int SaveLocation = 0;//1-chart,2-analisys
        public int timercnt = 0;
        public int TabSelItem;
        //  public string TestComment;
        public string AnFileName;
        public string comments = "";
        // public int SKIP;
        public int LastTipPos = -1;
        public int LastAnTipPos = -1;
        public int LastSelSer = -1;
        //=============== test stuff
        public Thread testupdate = null;
        public int SelTimeSer = -1;
        //===========================
        #endregion
        
        #region Pipevariables
        //============= threads
        public Thread PDatath;
        public Thread pipeData;
        public bool DataPipeConnected;
        public bool DoNotRead;//test
        public bool ShowTooltips;
        public byte[] DataStorage;
        public uint[] LineDataX;
        int[] seldet;
        public UInt32 NumofSavedFiles;
        public bool StopReceivingData;
        public int savefilecnt;
        public struct DAQInfo
        {
            public uint DetNum;
            public uint LineHeaderSize;
            public uint BytesPerPixel;
            public uint Unknown1;
            public uint Unknown2;
        }
        public DAQInfo dqi;
        public uint bState;
        public int SaveType;//0-pxe,1-bin,2-both
        public int AnShowMode;//0-spatial,1-time
        public float[,] Pxe4Emulator = new float[0, 0];
        public int Pxe4EmulatorCount = 0;
        public bool IsTestConnection = false;
        public bool PXEDataType = false;
        #endregion
        //settings
        public int SeriesMarkerSize = 1;
        int SKIP = 0;

        public PipeComm pipeComm = new PipeComm();
        public WCFComm diPlotDetectorAppChannel;

        
        //============================================
        public MainWindow()
        {
            SKIP = Convert.ToInt32(ConfigurationManager.AppSettings["NumofSkipLines"]);
            diPlotDetectorAppChannel = new WCFComm(ref lineArray);

            lineArray.OnLineAdded += new DataLines.NewLineEventHandler(lineArray_OnLineAdded);

            {
                InitializeComponent();
                NumofEmulatorBoardsUpdown = new NumericUpDown();
                NumofEmulatorBoardsUpdown.Minimum = 1;
                NumofEmulatorBoardsUpdown.Maximum = 100;
                NumofEmulatorBoardsUpdown.Increment = 1;
                windowsFormsHost3.Child = NumofEmulatorBoardsUpdown;
                NumofEmulatorBoardsUpdown.ValueChanged += new EventHandler(NumofEmulatorBoardsUpdown_ValueChanged);
                EmulatorEnabledChk.IsChecked = false;
                nofreadtbox.Items.Add("256");
                nofreadtbox.Items.Add("512");
                nofreadtbox.Items.Add("1024");
                nofreadtbox.Items.Add("2048");
                nofreadtbox.SelectedIndex = 0;
                CurrentSelDetNumUpDown = new NumericUpDown();
                CurrentSelDetNumUpDown.Minimum = 1;
                CurrentSelDetNumUpDown.Maximum = 10000;
                CurrentSelDetNumUpDown.Increment = 1;
                windowsFormsHost5.Child = CurrentSelDetNumUpDown;
                AnChart.Tag = "An";
                MyChart.Tag = "Pipe";
                //1
                SetupChart(MyChart, 1);
                //============================== foe estetic only
                AnChart.BorderColor = System.Drawing.Color.Black;
                AnChart.BorderDashStyle = ChartDashStyle.Solid;
                AnChart.BorderWidth = 2;
                //=============================
                ChartSaveImageBtn.IsEnabled = false;
                ChartSaveDataBtn.IsEnabled = false;
                ChartShowToolTipChk.IsEnabled = false;
                ChartAddCommentsBtn.IsEnabled = false;
                ChartSavedFileNameTxt.Visibility = System.Windows.Visibility.Hidden;
                //================ timer
                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Interval = new TimeSpan(0, 0, 3);
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Stop();
                //====================2
                AnFileName = "";
                ChartFileSaveTypeCmb.Items.Add("PXE");
                ChartFileSaveTypeCmb.Items.Add("Binary");
                ChartFileSaveTypeCmb.Items.Add("Both");
                ChartFileSaveTypeCmb.SelectedIndex = 0;
                RunModeCmb.Items.Add("Spatial");
                RunModeCmb.Items.Add("Time");
                RunModeCmb.SelectedIndex = 0;

                MainBitsSowModeCmb.Items.Add("Full");
                MainBitsSowModeCmb.Items.Add("Low");
                MainBitsSowModeCmb.Items.Add("Middle");
                MainBitsSowModeCmb.Items.Add("High");
                MainBitsSowModeCmb.SelectedIndex = 0;


                ChartBitsShowModeCmb.Items.Add("Full");
                ChartBitsShowModeCmb.Items.Add("Low");
                ChartBitsShowModeCmb.Items.Add("Middle");
                ChartBitsShowModeCmb.Items.Add("High");
                ChartBitsShowModeCmb.SelectedIndex = 0;

                AnBitsShowModeCmb.Items.Add("Full");
                AnBitsShowModeCmb.Items.Add("Low");
                AnBitsShowModeCmb.Items.Add("Middle");
                AnBitsShowModeCmb.Items.Add("High");
                AnBitsShowModeCmb.SelectedIndex = 0;

                MarkerSizeCmb.Items.Add("1");
                MarkerSizeCmb.Items.Add("2");
                MarkerSizeCmb.Items.Add("3");
                MarkerSizeCmb.Items.Add("4");
                MarkerSizeCmb.Items.Add("5");
                //===========================
                ChartTT = new System.Windows.Forms.ToolTip();
                ChartTT.UseFading = true;
                ChartTT.UseAnimation = true;
                ChartTT.IsBalloon = true;
                ChartTT.ShowAlways = true;
                ChartTT.AutoPopDelay = 5000;
                ChartTT.InitialDelay = 50;
                ChartTT.ReshowDelay = 50;
                ChartShowToolTipChk.IsChecked = false;

                AnCurrentDetectorSelUpDown = new NumericUpDown();
                AnCurrentDetectorSelUpDown.Minimum = 1;
                AnCurrentDetectorSelUpDown.Maximum = 100;
                AnCurrentDetectorSelUpDown.Increment = 1;
                windowsFormsHost2.Child = AnCurrentDetectorSelUpDown;
                groupBox3.BorderThickness = new System.Windows.Thickness(1, 1, 1, 1);

                CurrPointinfoGB.Visibility = System.Windows.Visibility.Hidden;
                AnCurrPointinfoGB.Visibility = System.Windows.Visibility.Hidden;
                ChartTooltipScroll.Minimum = 0;
                ChartTooltipScroll.SmallChange = 1;
                ChartTooltipScroll.LargeChange = 1;
                AnShowToolScroll.Minimum = 0;
                AnShowToolScroll.SmallChange = 1;
                AnShowToolScroll.LargeChange = 1;
                textBlock36.Visibility = System.Windows.Visibility.Hidden;
                ChartTooltipSelDetCmb.Visibility = System.Windows.Visibility.Hidden;
                EmulatorDataTypeChk.IsChecked = false;
                AddAboutDialog();
                //settings
                SeriesMarkerSize = Convert.ToInt32(ConfigurationManager.AppSettings["MarkerSize"]);
                if (SeriesMarkerSize == 0)
                    SeriesMarkerSize = 1;
                SetMarkerSizeComboindex();
                //================== show internal emulator
                int vis = Convert.ToInt32(ConfigurationManager.AppSettings["EmulatorEnabled"]);
                if (vis == 0)
                    EmulatorGrBox.Visibility = System.Windows.Visibility.Hidden;
                else
                    EmulatorGrBox.Visibility = System.Windows.Visibility.Visible;
                //try
                AnAddCommentBtn.IsEnabled = false;
                AnSaveFileBtn.IsEnabled = false;
                AnShowToolTipChk.IsEnabled = false;
                AnRestoreScaleBtn.IsEnabled = false;
                //AnDetSelList.Visibility = System.Windows.Visibility.Hidden;
                AnDetInfoGB.Visibility = System.Windows.Visibility.Hidden;
                DetectorDisplayGB.Visibility = System.Windows.Visibility.Hidden;
                this.MaxHeight = SystemParameters.PrimaryScreenHeight * 0.99;//????????????????
                this.MaxWidth = SystemParameters.PrimaryScreenWidth * 0.99;//???????????????????
                // MainViewBox.Height = MainViewBox.MaxHeight; 
            }
        }

        public static int temp_count = 0;

        void lineArray_OnLineAdded(object sender, LineDataEventArg fe)
        {
            if (temp_count % SKIP == 0)
                PlotSpatial(16, fe.lineData);
            
            temp_count++;
            //throw new NotImplementedException();
        }

        //=================================================================
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            //keep constant ratio during resising
            if (sizeInfo.PreviousSize.Width == 0 && sizeInfo.PreviousSize.Height == 0)
            {  //define initial ratio which will be used for next calls.occurs at the very first call only
                SizeRatio = sizeInfo.NewSize.Width / sizeInfo.NewSize.Height;
                MainViewBox.MinHeight = sizeInfo.NewSize.Height / 4;
                MainViewBox.MinWidth = sizeInfo.NewSize.Width / 4;
                // this.Height = this.MaxHeight;
                // this.Width  = this.MaxWidth; 
                //MinHeight ????????? maybe set here 
                //this.MinWidth 
                return;
            }
            //  var percentWidthChange = Math.Abs(sizeInfo.NewSize.Width - sizeInfo.PreviousSize.Width) / sizeInfo.PreviousSize.Width;
            //  var percentHeightChange = Math.Abs(sizeInfo.NewSize.Height - sizeInfo.PreviousSize.Height) / sizeInfo.PreviousSize.Height;
            //  if (percentWidthChange > percentHeightChange)
            //      this.Height = sizeInfo.NewSize.Width / SizeRatio;
            //  else
            //      this.Width = sizeInfo.NewSize.Height * SizeRatio;
            //  // debug only
            ////  this.Title = "New width is " + this.Width.ToString() + "  New height is " + this.Height.ToString();
            //  //===================??????????

            //  this.Left = 0;
            //  this.Top = 0;
            //===========================
            base.OnRenderSizeChanged(sizeInfo);
        }
        //====================================================================
        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            string fname = "";//
            if (SaveLocation == 1)
            {
                fname = System.IO.Path.GetFileName(SavedFileName);
            }
            else
                if (SaveLocation == 2)
                {
                    fname = System.IO.Path.GetFileName(AnSavedFileName);
                }
            if (timercnt == 0)
            {
                this.Cursor = System.Windows.Input.Cursors.Wait;
                if (SaveLocation == 1)
                {
                    ChartSavedFileNameTxt.Visibility = System.Windows.Visibility.Visible;
                    if (SavedFileName == "")
                    {
                        ChartSavedFileNameTxt.Foreground = Brushes.Red;
                        ChartSavedFileNameTxt.Text = "File not saved !";
                    }
                    else
                    {
                        ChartSavedFileNameTxt.Foreground = Brushes.Black;
                        ChartSavedFileNameTxt.Text = "File " + fname + " saved";
                    }
                }
                else if (SaveLocation == 2)
                {
                    AnSaveFileTxt.Visibility = System.Windows.Visibility.Visible;
                    if (AnSavedFileName == "")
                    {
                        AnSaveFileTxt.Foreground = Brushes.Red;
                        AnSaveFileTxt.Text = "File not saved !";
                    }
                    else
                    {
                        AnSaveFileTxt.Foreground = Brushes.Black;
                        AnSaveFileTxt.Text = "File " + fname + " saved";
                    }
                }
                timercnt++;
            }
            else
            {
                this.Cursor = System.Windows.Input.Cursors.Arrow;
                if (SaveLocation == 1)
                {
                    ChartSavedFileNameTxt.Visibility = System.Windows.Visibility.Hidden;
                    ChartSaveDataBtn.IsEnabled = true;
                    ChartShowToolTipChk.IsEnabled = true;
                    ChartAddCommentsBtn.IsEnabled = true;
                }
                else if (SaveLocation == 2)
                {
                    AnSaveFileTxt.Visibility = System.Windows.Visibility.Hidden;
                    AnSaveFileBtn.IsEnabled = true;
                    AnShowToolTipChk.IsEnabled = true;
                    AnRestoreScaleBtn.IsEnabled = true;
                    AnAddCommentBtn.IsEnabled = true;
                }
                dispatcherTimer.Stop();
            }

        }
        private void SetupChart(Chart ch, int opt)
        {
            int detpb = Convert.ToInt32(ConfigurationManager.AppSettings["DetPerBoard"]);
            ChartArea cha = new ChartArea();
            cha.BorderColor = System.Drawing.Color.Black;
            cha.BorderDashStyle = ChartDashStyle.Solid;
            cha.BorderWidth = 2;
            ch.ChartAreas.Add(cha);
            Series s = new Series();
            s.ChartType = SeriesChartType.FastPoint;
            ch.Series.Add(s);
            ch.Series[0].Color = System.Drawing.Color.Red;
            ch.ChartAreas[0].AxisX.ScaleView.Position = 0;
            ch.ChartAreas[0].AxisX.ScaleView.Size = 5 * detpb;
            ch.ChartAreas[0].AxisY.ScaleView.Position = 0;
            ch.ChartAreas[0].AxisY.ScaleView.Size = double.NaN;
            //============== zoom enabled
            // Enable range selection and zooming end user interface
            ch.ChartAreas[0].CursorX.IsUserEnabled = true;
            ch.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            ch.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            ch.ChartAreas[0].CursorY.IsUserEnabled = true;
            ch.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            ch.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            // Set Grid lines and tick marks interval
            int dist = Convert.ToInt32(ConfigurationManager.AppSettings["DetPerBoard"]);
            ch.ChartAreas[0].AxisX.MajorGrid.Interval = dist;
            ch.ChartAreas[0].AxisX.MajorTickMark.Interval = dist;
            ch.ChartAreas[0].AxisX.ScaleView.Position = 0;
            ch.ChartAreas[0].AxisX.ScaleView.Size = 5 * dist;
            ch.ChartAreas[0].AxisX.MajorGrid.IntervalOffset = 0;
            ch.ChartAreas[0].AxisX.MajorGrid.Interval = dist;
            ch.ChartAreas[0].AxisX.Interval = dist;
            ch.ChartAreas[0].AxisX.MajorGrid.IntervalType = DateTimeIntervalType.Number;
            ch.ChartAreas[0].AxisY.ScaleView.Position = 0;
            ch.ChartAreas[0].AxisY.ScaleView.Size = Double.NaN;
            // Set Line Color
            ch.ChartAreas[0].AxisX.MajorGrid.LineColor = System.Drawing.Color.Blue;
            // Set Line Width
            ch.ChartAreas[0].AxisX.MajorGrid.LineWidth = 1;
            ch.GetToolTipText += new EventHandler<System.Windows.Forms.DataVisualization.Charting.ToolTipEventArgs>(ch_GetToolTipText);
            if (opt == 0)
            {
                ch.ChartAreas[0].AxisX.ScrollBar.BackColor = System.Drawing.Color.LightBlue;
                ch.ChartAreas[0].AxisX.ScrollBar.ButtonColor = System.Drawing.Color.LightBlue;
                ch.ChartAreas[0].AxisX.ScrollBar.LineColor = System.Drawing.Color.LightCoral;

            }
            else if (opt == 1)
            {
                ch.ChartAreas[0].AxisX.ScrollBar.BackColor = System.Drawing.Color.Linen;
                ch.ChartAreas[0].AxisX.ScrollBar.ButtonColor = System.Drawing.Color.Linen;
                ch.ChartAreas[0].AxisX.ScrollBar.LineColor = System.Drawing.Color.LightCoral;

            }
        }

        void ch_GetToolTipText(object sender, System.Windows.Forms.DataVisualization.Charting.ToolTipEventArgs e)
        {
            Chart ch = (Chart)sender;
            int num, ptnum;
            string msg = "";
            if (ShowTooltips == true)
            {
                try
                {
                    DataPoint p;
                    switch (e.HitTestResult.ChartElementType)
                    {
                        case ChartElementType.DataPoint:
                            num = GetSeriesIndex(e.HitTestResult.Series.Name, ch);
                            p = ch.Series[num].Points.ElementAt(e.HitTestResult.PointIndex);
                            double[] v = p.YValues;
                            if (ch.Series.Count > 1)
                            {
                                msg = "Data Point " + e.HitTestResult.PointIndex.ToString() + " Value " + v[0].ToString() + " in Series " + ch.Series[num].Name;
                                ptnum = e.HitTestResult.PointIndex;
                            }
                            else
                            {
                                msg = "Detector " + (e.HitTestResult.PointIndex + 1).ToString() + " Value " + v[0].ToString();
                                ptnum = e.HitTestResult.PointIndex + 1;
                            }
                            SetMarker(ch, num, ptnum);
                            //=========If you want to show baloon style tooltip,uncomment this
                            // ChartTT.SetToolTip(ch, msg); 
                            //TTip.Text = msg; 
                            //========================================
                            break;

                        default:
                            break;
                    }
                }
                catch { }
            }
            else
            {
                ChartTT.RemoveAll();
            }
        }

        private void SetMarker(Chart ch, int serindex, int ptnum)
        {
            int a = String.Compare(ch.Tag.ToString(), "An");
            int b = String.Compare(ch.Tag.ToString(), "Pipe");
            if (a == 0)
            {
                if (ch.Series.Count == 1)
                {
                    AnShowToolScroll.Value = ptnum - 1;
                }
                else
                {
                    AnDetSelCmb.SelectedIndex = serindex;
                    AnShowToolScroll.Value = ptnum;
                }
            }
            else if (b == 0)
            {
                if (ch.Series.Count == 1)
                {
                    ChartTooltipScroll.Value = ptnum - 1;
                }
                else
                {
                    ChartTooltipSelDetCmb.SelectedIndex = serindex;
                    ChartTooltipScroll.Value = ptnum;
                }
            }
        }

        private int GetSeriesIndex(string name, Chart ch)
        {
            int idx = 0;
            for (int i = 0; i < ch.Series.Count; i++)
            {
                if (ch.Series[i].Name == name)
                {
                    idx = i;
                    break;
                }
            }
            return idx;
        }

        private void NumofEmulatorBoardsUpdown_ValueChanged(object sender, EventArgs e)
        {
            EmulatorBoardsCount = (int)NumofEmulatorBoardsUpdown.Value;
            int detnum = Convert.ToInt32(ConfigurationManager.AppSettings["DetPerBoard"]);
            if (EmulatorEnabled == true)
            {
                AnCurrentDetectorSelUpDown.Maximum = detnum * EmulatorBoardsCount;
                CurrentSelDetNumUpDown.Maximum = detnum * EmulatorBoardsCount;
            }

        }

        private void ChartRestoreScaleBtn_Click(object sender, RoutedEventArgs e)
        {
            MyChart.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
            MyChart.ChartAreas[0].AxisY.ScaleView.ZoomReset(0);
        }

        private void AnOpenFileBtn_Click(object sender, RoutedEventArgs e)
        {
            string ext = "";
            bool? DialogResult;
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "PXE files (*.pxe)|*.pxe|Binary files (*.pbn)|*.pbn|All files (*.*)|*.*";
            ofd.FilterIndex = 1;
            ofd.InitialDirectory = "C:\\Pipe Data";
            pxedata = new float[0];
            DialogResult = ofd.ShowDialog();
            if (DialogResult == true)
            {
                AnFileName = ofd.FileName;
                AnChart.Titles.Clear();
                AnChart.Titles.Add(AnFileName);
                FileSubs fs = new FileSubs();
                ext = System.IO.Path.GetExtension(AnFileName);
                //========================
                AnSaveFileBtn.IsEnabled = true;
                AnShowToolTipChk.IsEnabled = true;
                AnRestoreScaleBtn.IsEnabled = true;
                AnAddCommentBtn.IsEnabled = true;
                //==============================
                if (ext == ".pxe")
                {
                    pxa = new PxeAccess();
                    showPXE(AnFileName);
                    pxa = null;
                }

                else if (ext == ".pbn")
                {

                    ShowBinary(AnFileName);

                }
                else if (ext == ".txt")
                {
                    System.Diagnostics.Process.Start(AnFileName);
                }

                comments = fs.ReadComment(AnFileName + ".txt");
                if (comments != "")
                {
                    Comment cm = new Comment();
                    cm.CommentsText = comments;
                    cm.ShowDialog();
                }
            }
            else
                AnFileName = "";
        }

        private void ShowBinary(string fname)
        {
            int nn;
            byte[] bindata;
            FileSubs fs = new FileSubs();
            int[] prm = fs.GetReadingsCount(AnFileName);
            if (prm[0] == 0 || prm[1] == 0)
                return;
            else
            {
                bindata = fs.ReadBinary(fname);
                //======== add replot array
                pxedata = new float[bindata.Length];
                for (int jj = 0; jj < bindata.Length; jj++)
                {
                    pxedata[jj] = (float)bindata[jj];
                }
                //prepare chart=====================================
                AnChart.Series.Clear();
                AnChart.Legends.Clear();
                AnChart.ChartAreas.Clear();
                SetupChart(AnChart, 0);
                AnChart.ChartAreas[0].AxisX.CustomLabels.Clear();
                AnChart.ChartAreas[0].AxisX.ScaleView.Size = Double.NaN;
                AnChart.ChartAreas[0].AxisX.Minimum = 0;
                AnChart.ChartAreas[0].AxisY.ScaleView.Size = Double.NaN;
                //====================================================
                if (prm[1] > 1)//time mode
                {
                    AnShowMode = 1;
                    DetectorDisplayGB.Visibility = System.Windows.Visibility.Visible;
                    AnDetSelList.Visibility = System.Windows.Visibility.Visible;
                    AnDetInfoGB.Visibility = System.Windows.Visibility.Visible;
                    AnDetSelList.Items.Clear();
                    AnDetSelCmb.Items.Clear();
                    nn = 1;//to start
                    AnDetSelList.Items.Add("1");
                    AnDetSelCmb.Items.Add("1");
                    AnDetSelCmb.SelectedIndex = 0;
                    CreateChartSeriesA();
                    //fill by data
                    CurrentPXEWidth = prm[1];
                    for (int k = 1; k <= nn; k++)
                    {
                        for (int l = 0; l < CurrentPXEWidth; l++)
                        {
                            ushort dt = (ushort)bindata[l + CurrentPXEWidth * (k - 1)];
                            DataPoint pt = new DataPoint(l, GetMaskedData(dt));
                            AnChart.Series[k - 1].Points.Add(pt);
                        }
                    }
                    AnChart.Update();

                }
                else//spatial
                {
                    AnShowMode = 0;
                    DetectorDisplayGB.Visibility = System.Windows.Visibility.Hidden;
                    AnDetSelList.Visibility = System.Windows.Visibility.Hidden;
                    AnDetInfoGB.Visibility = System.Windows.Visibility.Hidden;
                    Series line = new Series();
                    line.MarkerStyle = MarkerStyle.Circle;
                    line.MarkerSize = SeriesMarkerSize; //2;
                    line.ChartType = SeriesChartType.Point;
                    AnChart.Series.Add(line);
                    for (int j = 0; j < bindata.Length; j++)
                    {
                        // 
                        ushort dt = (ushort)bindata[j];
                        DataPoint pt = new DataPoint(j, GetMaskedData(dt));
                        line.Points.Add(pt);
                    }
                    AnChart.Update();
                }
            }
        }

        private void showPXE(string fname)
        {
            float[] normdata;
            pxa.OpenPXEFile(fname);
            CurrentPXEHeight = pxa.m_Img_Ht;
            CurrentPXEWidth = pxa.m_Img_Width;
            if (CurrentPXEHeight * CurrentPXEWidth == 0)
                return;
            normdata = new float[pxa.m_rawData.Length];
            pxa.m_rawData.CopyTo(normdata, 0);
            pxedata = new float[pxa.m_rawData.Length];
            pxa.m_rawData.CopyTo(pxedata, 0);//keep it for possible replot
            pxa.ClosePXE();

            bool res = false;
            int nn;
            float[] lindata = pxa.ReadLine(fname, ref res);
            //prepare chart=====================================
            AnChart.Series.Clear();
            AnChart.Legends.Clear();
            AnChart.ChartAreas.Clear();
            SetupChart(AnChart, 0);
            AnChart.ChartAreas[0].AxisX.CustomLabels.Clear();
            AnChart.ChartAreas[0].AxisX.ScaleView.Size = Double.NaN;
            AnChart.ChartAreas[0].AxisX.Minimum = 0;
            AnChart.ChartAreas[0].AxisY.ScaleView.Size = Double.NaN;
            //====================================================
            if (res == true)//time mode
            {
                if ((int)lindata[0] == 0)//one more test
                {
                    AnShowMode = -1;
                    DetectorDisplayGB.Visibility = System.Windows.Visibility.Hidden;
                    AnDetSelList.Visibility = System.Windows.Visibility.Hidden;
                    AnDetInfoGB.Visibility = System.Windows.Visibility.Hidden;

                    Series line = new Series();
                    line.ChartType = SeriesChartType.FastPoint;
                    AnChart.Series.Add(line);
                    for (int j = 0; j < normdata.Length; j++)
                    {
                        ushort dt = (ushort)normdata[j];
                        DataPoint pt = new DataPoint(j, GetMaskedData(dt));
                        line.Points.Add(pt);
                    }
                    AnChart.Update();
                    return;
                }
                AnShowMode = 1;
                DetectorDisplayGB.Visibility = System.Windows.Visibility.Visible;
                AnDetSelList.Visibility = System.Windows.Visibility.Visible;
                AnDetInfoGB.Visibility = System.Windows.Visibility.Visible;
                AnDetSelList.Items.Clear();
                AnDetSelCmb.Items.Clear();
                nn = (int)lindata[0];
                for (int i = 1; i <= nn; i++)
                {
                    AnDetSelList.Items.Add(lindata[i].ToString());
                    AnDetSelCmb.Items.Add(lindata[i].ToString());
                }
                AnDetSelCmb.SelectedIndex = 0;
                //fill by data
                CreateChartSeriesA();
                for (int k = 1; k <= nn; k++)
                {
                    for (int l = 0; l < CurrentPXEWidth; l++)
                    {
                        ushort dt = (ushort)normdata[l + CurrentPXEWidth * (k - 1)];

                        DataPoint pt = new DataPoint(l, GetMaskedData(dt)); //normdata[l + CurrentPXEWidth * (k - 1)]);

                        AnChart.Series[k - 1].Points.Add(pt);
                    }
                }
                AnChart.Update();

            }
            else//spatial
            {
                AnShowMode = 0;
                DetectorDisplayGB.Visibility = System.Windows.Visibility.Hidden;
                AnDetSelList.Visibility = System.Windows.Visibility.Hidden;
                AnDetInfoGB.Visibility = System.Windows.Visibility.Hidden;
                AnChart.Series.Clear();
                Series line = new Series();
                line.ChartType = SeriesChartType.Point;
                line.MarkerStyle = MarkerStyle.Circle;
                line.MarkerSize = SeriesMarkerSize; //2;
                AnChart.Series.Add(line);
                for (int j = 0; j < normdata.Length; j++)
                {
                    ushort dt = (ushort)normdata[j];
                    DataPoint pt = new DataPoint(j, GetMaskedData(dt));//normdata[j]);
                    line.Points.Add(pt);
                }
                AnChart.Update();
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            diPlotDetectorAppChannel.StopAcq();
            Thread.Sleep(1000);
            this.Close();
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DisplayMode == 1 && NumberOfRuns > 0)
            {
                SetupTimeModeUi = false;
                ChartStopContinueBtn.IsEnabled = false;
                CurrentTimeModeCounter = 0;

            }
            else
            {
                ChartStopContinueBtn.IsEnabled = true;
                SetupSpatialModeUi = false;
            }
            ChartTabControlsSetup(false);
            StopReceivingData = false;
            ClearChart(MyChart);
            StartBtn.IsEnabled = false;
            ChartTooltipSelDetCmb.Items.Clear();

            if (EmulatorEnabled == false)
            {
                //StartDataPipe();
                //Thread MsgPipeThread = new Thread(CreatePipe1);
                //MsgPipeThread.Start();
                try
                {
                    diPlotDetectorAppChannel.StartAcq();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "DetectorPlot");
                }
            }
            else
            {
                if (CheckEmulatorSettings())
                {

                    if (testupdate == null)
                    {
                        testupdate = new Thread(GetData);
                        testupdate.Priority = ThreadPriority.Highest;
                        testupdate.Start();
                    }
                    else
                    {
                        testupdate.Abort();
                        testupdate = null;
                        testupdate = new Thread(GetData);
                        testupdate.Priority = ThreadPriority.Highest;
                        testupdate.Start();
                    }
                }
                else
                    StartBtn.IsEnabled = true;
            }
        }

        private void ChartStopContinueBtn_Click(object sender, RoutedEventArgs e)
        {
            string cnt = (string)ChartStopContinueBtn.Content;
            if (cnt.Contains("Stop"))
            {
                StopReceivingData = true;
                ChartStopContinueBtn.Content = "Continue";
                ChartTabControlsSetup(true);
                StartBtn.IsEnabled = true;
                ChartTooltipScroll.Maximum = MyChart.Series[0].Points.Count;
                if (MyChart.Series[0].ChartType != SeriesChartType.Point)
                    MyChart.Series[0].ChartType = SeriesChartType.Point;//to enable marker
            }
            else
            {
                if (cnt.Contains("Continue"))
                {
                    if (MyChart.Series[0].ChartType != SeriesChartType.FastPoint)
                        MyChart.Series[0].ChartType = SeriesChartType.FastPoint;
                    StopReceivingData = false;
                    ChartStopContinueBtn.Content = "Stop Getting Data";
                    ChartTabControlsSetup(false);
                }
            }
        }

        public void StartDataPipe()
        {
            if (DataPipeConnected == true)
                return;
            DataPipeConnected = false;
            PDatath = new Thread(pipeComm.CreatePipe2);
            PDatath.Priority = ThreadPriority.Highest;
            PDatath.Start();
        }

        private void EmulatorEnabledChk_Checked(object sender, RoutedEventArgs e)
        {
            IsTestConnection = false;
            EmulatorEnabled = true;
        }

        private void EmulatorEnabledChk_Unchecked(object sender, RoutedEventArgs e)
        {
            EmulatorEnabled = false;
            IsTestConnection = false;
        }

        private void MainAddDetBtn_Click(object sender, RoutedEventArgs e)
        {
            string item = CurrentSelDetNumUpDown.Value.ToString();
            if (MainSeldetLBox.Items.Contains(item) == false)
            {
                MainSeldetLBox.Items.Add(item);
            }
            GetSelectedDetectors(MainSeldetLBox);
        }

        private void MainRemoveDetBtn_Click(object sender, RoutedEventArgs e)
        {
            //remove item from list
            if (MainSeldetLBox.Items.Count > 0)
            {
                MainSeldetLBox.Items.RemoveAt(MainSeldetLBox.SelectedIndex);
            }
            GetSelectedDetectors(MainSeldetLBox);
        }

        private void MainClearDetSelBtn_Click(object sender, RoutedEventArgs e)
        {
            //clear
            MainSeldetLBox.Items.Clear();
            GetSelectedDetectors(MainSeldetLBox);
        }
        void GetSelectedDetectors(System.Windows.Controls.ListBox lb)
        {
            int itemscount = lb.Items.Count;
            seldet = new int[0];
            if (itemscount < 1)
                return;
            else
            {
                seldet = new int[itemscount];
                for (int i = 0; i < itemscount; i++)
                {
                    lb.SelectedIndex = i;
                    seldet[i] = Convert.ToInt32(lb.SelectedItem.ToString());
                }
            }
            return;
        }

        private void GetDetInfo(int num)
        {
            try
            {
                MeanLbl.Content = Math.Round(MyChart.DataManipulator.Statistics.Mean("Det " + num.ToString()), 3).ToString();
                MedianLbl.Content = Math.Round(MyChart.DataManipulator.Statistics.Median("Det " + num.ToString()), 3).ToString();
                double variance = MyChart.DataManipulator.Statistics.Variance("Det " + num.ToString(), true);
                Variancelbl.Content = Math.Round(variance, 3).ToString();//round to 3
                StDevlbl.Content = Math.Round(Math.Sqrt(variance), 3).ToString();
                DataPoint dpmax = MyChart.Series["Det " + num.ToString()].Points.FindMaxByValue();
                DataPoint dpmin = MyChart.Series["Det " + num.ToString()].Points.FindMinByValue();
                MaxLbl.Content = dpmax.YValues[0].ToString();
                MinLbl.Content = dpmin.YValues[0].ToString();
                double val = dpmax.YValues[0];
                lbl10p.Content = (0.1 * val).ToString();
                lbl90p.Content = (0.9 * val).ToString();
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }

        private void MainSeldetLBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                int num = Convert.ToInt32(MainSeldetLBox.SelectedItem.ToString());
                GetDetInfo(num);
            }
            catch { }
        }

        public ushort GetMaskedData(ushort dt)
        {
            switch (BitsShowMode)
            {
                case 0://full
                    break;
                case 1://LSB
                    dt = (ushort)(dt & LoByteMask);
                    break;
                case 2://MSB hi
                    dt = (ushort)((dt & HiByteMask) >> 8);
                    break;
                case 3://Middle
                    dt = (ushort)((dt & MidByteMask) >> 4);
                    break;
                default:
                    break;

            }
            return dt;
        }

        public uint GetMaskedData(uint dt)
        {
            switch (BitsShowMode)
            {
                case 0://full
                    break;
                case 1://LSB
                    dt = (ushort)(dt & LoByteMask);
                    break;
                case 2://MSB hi
                    dt = (ushort)((dt & HiByteMask) >> 8);
                    break;
                case 3://Middle
                    dt = (ushort)((dt & MidByteMask) >> 4);
                    break;
                default:
                    break;

            }
            return dt;
        }

        public delegate void PlotChartDelegate();
        public delegate void CreateChartSeriesDelegate();
        public delegate void PlotDataTimeModeDelegate();

        private void UpdateCtl(object ctl, string txt)
        {
            if (ctl is System.Windows.Controls.Label)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                  (ThreadStart)delegate()
                  {
                      System.Windows.Controls.Label lbl = (System.Windows.Controls.Label)ctl;
                      lbl.Content = txt;
                      ctl = (object)lbl;
                  }
                  );
            }
            else if (ctl is TextBlock)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                  (ThreadStart)delegate()
                  {
                      TextBlock lbl = (TextBlock)ctl;
                      lbl.Text = txt;
                      ctl = (object)lbl;
                  }
                  );
            }
            else if (ctl is System.Windows.Controls.TextBox)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                  (ThreadStart)delegate()
                  {
                      System.Windows.Controls.TextBox lbl = (System.Windows.Controls.TextBox)ctl;
                      lbl.Text += "\r\n" + txt;
                      lbl.SelectionLength = lbl.Text.Length;
                      lbl.ScrollToEnd();
                      ctl = (object)lbl;
                  }
                  );
            }
        }
        
        public void GetData()
        {
            dqi.BytesPerPixel = Convert.ToUInt32(ConfigurationManager.AppSettings["BytesPerPixel"]);
            int detpb = Convert.ToInt32(ConfigurationManager.AppSettings["DetPerBoard"]);
            Random br = new Random();
            int skipcnt = 0;
            int tst = 0;
            bool bres = false;
            if (EmulatorEnabled == true) ///for debug
            {
                if (Pxe4Emulator.GetLength(0) > 0 && Pxe4Emulator.GetLength(1) > 0 && PXEDataType == true)
                {
                    dqi.DetNum = (uint)(EmulatorBoardsCount * detpb);
                }
                else
                {
                    dqi.DetNum = (uint)(detpb * EmulatorBoardsCount);
                }
                dqi.LineHeaderSize = 80;
                UpdateCtl((object)NumDetLbl, dqi.DetNum.ToString());
                UpdateCtl((object)HeaderSizeLbl, dqi.LineHeaderSize.ToString());
            }
            else if (IsTestConnection == true)
            {
                dqi.LineHeaderSize = 80;
                dqi.DetNum = dqi.DetNum = (uint)(EmulatorBoardsCount * detpb);//100;//00;

            }

            uint dwBytesPerSection = dqi.DetNum * dqi.BytesPerPixel + dqi.LineHeaderSize;
            byte[] HeaderID = new byte[dqi.LineHeaderSize];
            DataStorage = new byte[dwBytesPerSection];
            LineDataX = new uint[dqi.DetNum];
            for (; ; )
            {
                if (DisplayMode == 1 && (StopReceivingData == false))//time
                {
                    bres = PlotTime();
                    if (bres == false)
                    {
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Send, (ThreadStart)delegate()
                         {
                             ChartTabControlsSetup(true);
                         }
                        );
                        StopReceivingData = true;
                    }
                }
                else if (StopReceivingData == false && (DisplayMode == 0))
                {
                    if (skipcnt == SKIP)
                    {
                        //bres = PlotSpatial(detpb, new LineData());
                        for (int pp = 0; pp < 10; pp++)
                        {
                            tst += (int)DataStorage[pp];
                        }
                        if (DataStorage.Length != 0 && tst != 0)
                        {
                            Pxe4EmulatorCount++;
                            if (Pxe4EmulatorCount > Pxe4Emulator.GetLength(1) - 1)
                            {
                                Pxe4EmulatorCount = 0;
                            }
                            skipcnt = 0;
                        }
                    }
                    else
                    {
                        skipcnt++;
                    }
                }

            }

        }

        private bool PlotSpatial(int detpb, LineData lineData)
        {
            Random br;
            int tst = 0;
            UInt32 id;
            bool bret = false;

            dqi.DetNum = 80; //hard-coded
            dqi.BytesPerPixel = 3; //hard-coded

            uint dwBytesPerSection = dqi.DetNum * dqi.BytesPerPixel + dqi.LineHeaderSize;
            
            byte[] HeaderID = new byte[dqi.LineHeaderSize];
            
            if (DisplayMode == 1)//time
                return bret;
            else
            {
                //setup ui
                if (SetupSpatialModeUi == false)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            (ThreadStart)delegate()
                            {
                                tabControl1.SelectedIndex = 1;
                                MyChart.ChartAreas[0].AxisX.CustomLabels.Clear();
                                MyChart.Series[0].ChartType = SeriesChartType.FastPoint;
                                MyChart.Series[0].MarkerStyle = MarkerStyle.Circle;
                                MyChart.Series[0].MarkerSize = SeriesMarkerSize;//4;
                                MyChart.Series[0].MarkerColor = System.Drawing.Color.Black;
                                //how many boards to display
                                int brd = (int)dqi.DetNum / detpb;
                                for (int i = 0; i < brd; i++)
                                {
                                    MyChart.ChartAreas[0].AxisX.CustomLabels.Add(5 + detpb * i, 20 + detpb * i, "board " + (i + 1).ToString(), 0, LabelMarkStyle.None);
                                }

                            }
                   );
                    SetupSpatialModeUi = true;
                }

                if (EmulatorEnabled == true)
                {
                    {
                        br = new Random();
                        br.NextBytes(DataStorage);
                    }
                }
                else
                {
                    try
                    {
                        //if (lineData.data != null)
                        //    pipeComm.DataSrvHandle.Read(DataStorage, 0, DataStorage.Length);
                        //else
                        DataStorage = lineData.data;

                        dqi.DetNum = 80;// (uint)diPlotDetectorAppChannel.PixelsPerColumn;
                        LineDataX = new uint[dqi.DetNum];

                    }
                    catch { }
                }

                for (int pp = 0; pp < 10; pp++)
                {
                    tst += (int)DataStorage[pp];
                }

                if (DataStorage.Length != 0 && tst != 0)
                {
                    //Buffer.BlockCopy(DataStorage, 0, HeaderID, 0, HeaderID.Length);
                    //Buffer.BlockCopy(DataStorage, (int)dqi.LineHeaderSize, LineDataX, 0, ((int)dwBytesPerSection - (int)dqi.LineHeaderSize)); //?????????????

                    int index = 0;

                    for (int i = 0; i < dqi.DetNum; i++)
                    {
                        LineDataX[i] = (uint)(((DataStorage[index + 2] << 16) + (DataStorage[index + 1] << 8) + DataStorage[index]) >> 8);
                        //LineDataX[i] = (uint)(DataStorage[index + 2] << 8 + DataStorage[index + 1]);
                        index += 3;
                    }

                    //id = BitConverter.ToUInt32(HeaderID, 0);
                    //UpdateCtl((object)BlockIdLbl, id.ToString());
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new PlotChartDelegate(PlotData));

                    if (EmulatorEnabled == true)
                    {
                        System.Threading.Thread.Sleep(50);
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(1);
                    }
                }
            }

            bret = true;
            return bret;
        }


        private bool PlotTime()
        {//will be called in endless loop
            UInt32 id;
            Random br;
            int i;
            try
            {
                i = seldet.Length;
            }
            catch
            {
                IWin32Window owner = null;
                System.Windows.Forms.MessageBox.Show(owner, "Wrong Time Mode Settings");
                return false;
            }
            uint dwBytesPerSection = dqi.DetNum * dqi.BytesPerPixel + dqi.LineHeaderSize;
            byte[] HeaderID = new byte[dqi.LineHeaderSize];
            if (DisplayMode == 0 || CurrentTimeModeCounter >= NumberOfRuns)//spatial
                return false;
            else
            {
                if (SetupTimeModeUi == false)//do it onse
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new CreateChartSeriesDelegate(CreateChartSeries));
                    if (seldet.Length > 0 && NumberOfRuns > 0)//check first
                    {
                        tempstorage = new byte[NumberOfRuns * DataStorage.Length];
                        TimeModeArray = new byte[seldet.Length, NumberOfRuns];
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Send,
                         (ThreadStart)delegate()
                         {
                             tabControl1.SelectedIndex = 1;
                         }
                               );

                        SetupTimeModeUi = true;
                    }
                }

                if (EmulatorEnabled == true)
                {
                    br = new Random();
                    br.NextBytes(DataStorage);
                }
                else
                {
                    pipeComm.DataSrvHandle.Read(DataStorage, 0, DataStorage.Length);
                }
                try
                {
                    Buffer.BlockCopy(DataStorage, 0, tempstorage, CurrentTimeModeCounter * DataStorage.Length, DataStorage.Length);
                    Buffer.BlockCopy(DataStorage, 0, HeaderID, 0, HeaderID.Length);
                    Buffer.BlockCopy(DataStorage, (int)dqi.LineHeaderSize, LineDataX, 0, ((int)dwBytesPerSection - (int)dqi.LineHeaderSize));
                    id = BitConverter.ToUInt32(HeaderID, 0);
                    UpdateCtl((object)BlockIdLbl, id.ToString());
                    for (int k = 0; k < seldet.Length; k++)
                    {
                        TimeModeArray[k, CurrentTimeModeCounter] = DataStorage[seldet[k] + dqi.LineHeaderSize];//skip header 
                    }
                    this.Dispatcher.Invoke(new PlotDataTimeModeDelegate(PlotDataTimeMode), null);
                    CurrentTimeModeCounter++;
                    if (CurrentTimeModeCounter == NumberOfRuns)//end of loop
                    {
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                        (ThreadStart)delegate()
                                        {
                                            ChartSaveDataBtn.IsEnabled = true;
                                            ChartSaveImageBtn.IsEnabled = true;
                                            ChartAddCommentsBtn.IsEnabled = true;
                                            StartBtn.IsEnabled = true;
                                            ChartShowToolTipChk.IsEnabled = true;
                                        }
                                       );
                    }
                }
                catch { }
            }
            return true;
        }

        private void nofreadtbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NumberOfRuns = Convert.ToInt32(nofreadtbox.SelectedItem);
        }
        #region ChartFunctions

        private void CreateChartSeries()
        {
            ChartColorPalette cp = MyChart.Palette;
            System.Drawing.Color[] pclr = GetColors.GetPaletteColors(cp);
            GetSelectedDetectors(MainSeldetLBox);
            MyChart.Series.Clear();
            MyChart.Legends.Clear();
            MyChart.ChartAreas[0].AxisX.CustomLabels.Clear();
            MyChart.ChartAreas[0].AxisX.ScaleView.Size = NumberOfRuns;
            MyChart.ApplyPaletteColors();
            Series[] series = new Series[seldet.Length];
            Legend legend = new Legend();
            LegendItem[] li = new LegendItem[seldet.Length];
            legend.Title = "Detectors";
            legend.LegendStyle = LegendStyle.Column;
            for (int i = 0; i < seldet.Length; i++)
            {
                li[i] = new LegendItem();
                li[i].Name = "Det " + seldet[i].ToString();
                li[i].ImageStyle = LegendImageStyle.Line;
                li[i].ShadowOffset = 0;
                if (i > pclr.Length)
                {
                    li[i].Color = System.Drawing.Color.FromArgb(255, pclr[i - pclr.Length].R, pclr[i - pclr.Length].G, pclr[i - pclr.Length].B);
                }
                else
                {
                    li[i].Color = System.Drawing.Color.FromArgb(255, pclr[i].R, pclr[i].G, pclr[i].B);
                }
                series[i] = new Series();
                series[i].Name = "Det " + seldet[i].ToString();
                series[i].ChartType = SeriesChartType.FastPoint;
                series[i].MarkerStyle = MarkerStyle.Circle;
                series[i].MarkerSize = SeriesMarkerSize;
                legend.CustomItems.Add(li[i]);
                series[i].IsVisibleInLegend = false;
                MyChart.Series.Add(series[i]);
            }
            MyChart.Legends.Add(legend);
        }
        private void PlotDataTimeMode()
        {
            ushort dt;
            int chartsernum = MyChart.Series.Count;
            for (int k = 0; k < chartsernum; k++)
            {
                dt = TimeModeArray[k, CurrentTimeModeCounter];
                dt = GetMaskedData(dt);
                MyChart.Series[k].Points.AddY(dt);
                if (EmulatorEnabled == true)
                {
                    System.Threading.Thread.Sleep(10);
                }
                try
                {
                    MyChart.Update();
                }
                catch { }
            }

        }

        private void PlotData()
        {
            uint dt;
            uint hsz = dqi.LineHeaderSize;
            MyChart.Series[0].Points.Clear();
            if (Pxe4Emulator.GetLength(0) > 0)
            {
                for (int pointIndex = 0; pointIndex < Pxe4Emulator.GetLength(0); pointIndex++)
                {
                    try
                    {
                        dt = GetMaskedData((ushort)Pxe4Emulator[pointIndex, Pxe4EmulatorCount]);//, pointIndex]);
                        MyChart.Series[0].Points.AddY(dt);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(ex.Message);
                    }
                }
            }
            else
            {
                // hsz = 0;
                for (int pointIndex = (int)hsz; pointIndex < dqi.DetNum + hsz; pointIndex++)//skip header
                {
                    dt = GetMaskedData(LineDataX[pointIndex]);//DataStorage[pointIndex]);
                    MyChart.Series[0].Points.AddY(dt);
                }
                try
                {//sometimes causes stackoverflow exception
                    MyChart.Update();
                }
                catch { }
            }

        }
        #endregion

        private void ChartSaveImageBtn_Click(object sender, RoutedEventArgs e)
        {
            //save image
            System.Windows.Forms.SaveFileDialog saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog1.Filter = "Bitmap (*.bmp)|*.bmp|JPEG (*.jpg)|*.jpg|EMF (*.emf)|*.emf|PNG (*.png)|*.png|GIF (*.gif)|*.gif|TIFF (*.tif)|*.tif";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            // Set image file format
            System.Windows.Forms.DialogResult dlgres;
            dlgres = saveFileDialog1.ShowDialog();
            if (dlgres == System.Windows.Forms.DialogResult.OK)
            {
                ChartImageFormat format = ChartImageFormat.Bmp;
                string ext = System.IO.Path.GetExtension(saveFileDialog1.FileName);
                switch (ext)
                {
                    case "bmp":
                        format = ChartImageFormat.Bmp;
                        break;
                    case "jpg":
                        format = ChartImageFormat.Jpeg;
                        break;
                    case "emf":
                        format = ChartImageFormat.Emf;
                        break;
                    case "gif":
                        format = ChartImageFormat.Gif;
                        break;
                    case "png":
                        format = ChartImageFormat.Png;
                        break;
                    case "tif":
                        format = ChartImageFormat.Tiff;
                        break;
                    default:
                        format = ChartImageFormat.Bmp;
                        break;
                }
                // Save image
                MyChart.SaveImage(saveFileDialog1.FileName, format);
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            //PipeComm.Close()
        }

        private void ChartSaveDataBtn_Click(object sender, RoutedEventArgs e)
        {
            FileSubs fs = new FileSubs();
            ChartSaveDataBtn.IsEnabled = false;
            if (SaveType == 0 || SaveType == 2)
            {
                if (DisplayMode == 1)//time mode
                {
                    SavedFileName = fs.SaveTimeData(tempstorage, NumberOfRuns, seldet, true, "");
                }
                else if (DisplayMode == 0)//spatial mode
                {
                    SavedFileName = fs.SavePXEFile(savefilecnt, 1, DataStorage, true, "");
                }
                if (comments != "" && SavedFileName != "")
                {
                    fs.SaveComment(comments, SavedFileName + ".txt");
                }
            }
            if (SaveType == 1 || SaveType == 2)
            {//binary
                if (DisplayMode == 1)//time mode
                    SavedFileName = fs.SaveBinary(tempstorage, comments, NumberOfRuns, DisplayMode, true, "");
                else
                    SavedFileName = fs.SaveBinary(DataStorage, comments, 1, DisplayMode, true, "");
            }
            SaveLocation = 1;
            timercnt = 0;
            dispatcherTimer.Start();
        }

        private void GetAnDetInfo(string sername)
        {
            try
            {
                label1.Content = Math.Round(AnChart.DataManipulator.Statistics.Mean(sername), 3).ToString();
                label2.Content = Math.Round(AnChart.DataManipulator.Statistics.Median(sername), 3).ToString();
                double variance = AnChart.DataManipulator.Statistics.Variance(sername, true);
                label3.Content = Math.Round(variance, 3).ToString();//round to 3
                label4.Content = Math.Round(Math.Sqrt(variance), 3).ToString();
                DataPoint dpmax = AnChart.Series[sername].Points.FindMaxByValue();
                DataPoint dpmin = AnChart.Series[sername].Points.FindMinByValue();
                label5.Content = dpmax.YValues[0].ToString();
                label8.Content = dpmin.YValues[0].ToString();
                double val = dpmax.YValues[0];
                label7.Content = (0.1 * val).ToString();
                label6.Content = (0.9 * val).ToString();
            }
            catch { }
        }

        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabSelItem = tabControl1.SelectedIndex;
        }

        private void AnRestoreScaleBtn_Click(object sender, RoutedEventArgs e)
        {
            AnChart.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
            AnChart.ChartAreas[0].AxisY.ScaleView.ZoomReset(0);
        }

        private void ChartFileSaveTypeCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveType = ChartFileSaveTypeCmb.SelectedIndex;
        }

        private void RunModeCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DisplayMode = RunModeCmb.SelectedIndex;
            StopReceivingData = true;
            StartBtn.IsEnabled = true;
            if (DisplayMode == 0)
            {
                ClearChart(MyChart);
                SetupSpatialModeUi = false;
                TimeModeSettingsGB.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                ClearChart(MyChart);
                TimeModeSettingsGB.Visibility = System.Windows.Visibility.Visible;
                CurrentTimeModeCounter = 0;
                SetupTimeModeUi = false;
            }

        }

        private void MainBitsSowModeCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ComboBox cb = (System.Windows.Controls.ComboBox)sender;
            BitsShowMode = cb.SelectedIndex;
            if (AnFileName != "" && pxa != null)
            {
                showPXE(AnFileName);
            }
        }

        private void ChartShowToolTipChk_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox cb = (System.Windows.Controls.CheckBox)sender;
            ShowTooltips = true;
            if (cb.Name == "ChartShowToolTipChk")
            {
                CurrPointinfoGB.Visibility = System.Windows.Visibility.Visible;
                if (DisplayMode == 1)//time
                {
                    ChartTooltipSelDetCmb.Items.Clear();
                    if (MainSeldetLBox.Items.Count > 0)
                    {
                        for (int p = 0; p < MainSeldetLBox.Items.Count; p++)
                        {
                            ChartTooltipSelDetCmb.Items.Add(MainSeldetLBox.Items[p]);
                        }
                        ChartTooltipSelDetCmb.SelectedIndex = 0;
                    }
                    textBlock36.Visibility = System.Windows.Visibility.Visible;
                    ChartTooltipSelDetCmb.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    textBlock36.Visibility = System.Windows.Visibility.Hidden;
                    ChartTooltipSelDetCmb.Visibility = System.Windows.Visibility.Hidden;
                }


            }
            else
                if (cb.Name == "AnShowToolTipChk")
                {
                    AnCurrPointinfoGB.Visibility = System.Windows.Visibility.Visible;
                    if (AnShowMode == 0)
                    {
                        label9.Visibility = System.Windows.Visibility.Hidden;
                        AnDetSelCmb.Visibility = System.Windows.Visibility.Hidden;
                        AnShowToolScroll.Minimum = 1;
                        AnShowToolScroll.Value = 1;
                        AnShowToolScroll.Maximum = AnChart.Series[0].Points.Count;
                    }
                    else
                    {
                        label9.Visibility = System.Windows.Visibility.Visible;
                        AnDetSelCmb.Visibility = System.Windows.Visibility.Visible;
                        if (AnDetSelCmb.Items.Count > 0)
                        {
                            AnDetSelCmb.SelectedIndex = 1;
                        }
                        AnDetSelCmb.SelectedIndex = 0;
                    };
                }

        }

        private void ChartShowToolTipChk_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowTooltips = false;
            ChartTT.RemoveAll();
            System.Windows.Controls.CheckBox cb = (System.Windows.Controls.CheckBox)sender;
            if (cb.Name == "ChartShowToolTipChk")
                CurrPointinfoGB.Visibility = System.Windows.Visibility.Hidden;
            else
                if (cb.Name == "AnShowToolTipChk")
                    AnCurrPointinfoGB.Visibility = System.Windows.Visibility.Hidden;
        }

        private void AnAddDetBtn_Click_1(object sender, RoutedEventArgs e)
        {
            //select
            string item = AnCurrentDetectorSelUpDown.Value.ToString();
            if (AnDetSelList.Items.Contains(item) == false)
            {
                AnDetSelList.Items.Add(item);
                SortListBox(AnDetSelList);
                AnDetSelCmb.Items.Add(item);
                SortComboBox(AnDetSelCmb);
                AnDetSelCmb.SelectedIndex = 0;
            }
            GetSelectedDetectors(AnDetSelList);
            showplot();

        }

        private void AnRemoveDetBtn_Click(object sender, RoutedEventArgs e)
        {
            //remove
            if (AnDetSelList.Items.Count > 0)
            {
                int idx = AnDetSelList.SelectedIndex;
                AnDetSelList.Items.RemoveAt(idx);
                AnDetSelCmb.Items.RemoveAt(idx);
                AnDetSelCmb.SelectedIndex = 0;
            }
            GetSelectedDetectors(AnDetSelList);
            showplot();
        }

        private void AnClearDetBtn_Click(object sender, RoutedEventArgs e)
        {
            //clear
            AnDetSelList.Items.Clear();
            AnDetSelCmb.Items.Clear();
            ChartTooltipSelDetCmb.Items.Clear();
            GetSelectedDetectors(AnDetSelList);
            showplot();
        }
        private void showplot()
        {
            int nn = seldet.Length;
            //prepare chart=====================================
            AnChart.Series.Clear();
            AnChart.Legends.Clear();
            AnChart.ChartAreas.Clear();
            SetupChart(AnChart, 0);
            AnChart.ChartAreas[0].AxisX.CustomLabels.Clear();
            AnChart.ChartAreas[0].AxisX.ScaleView.Size = Double.NaN;
            AnChart.ChartAreas[0].AxisX.Minimum = 0;
            AnChart.ChartAreas[0].AxisY.ScaleView.Size = Double.NaN;
            //======================================================
            if (nn == 0)
                return;
            else
            {
                CreateChartSeriesA();
                //fill by data
                for (int k = 1; k <= nn; k++)
                {
                    for (int l = 0; l < CurrentPXEWidth; l++)
                    {
                        ushort dt = (ushort)pxedata[l + CurrentPXEWidth * (k - 1)];

                        DataPoint pt = new DataPoint(l, GetMaskedData(dt));

                        AnChart.Series[k - 1].Points.Add(pt);
                    }
                }
                AnChart.Update();
            }
        }

        private void AnDetSelList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                GetAnDetInfo("Det " + AnDetSelList.SelectedItem.ToString());
            }
            catch { }
        }

        private void AnSaveFileBtn_Click(object sender, RoutedEventArgs e)
        {
            bool? dialogresult;
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
            sfd.InitialDirectory = "C:\\Pipedata";
            sfd.Filter = "PXE files (*.pxe)|*.pxe|Binary files (*.pbn)|*.pbn";
            sfd.FilterIndex = 1;
            sfd.OverwritePrompt = true;
            dialogresult = sfd.ShowDialog();
            if (dialogresult == true)
            {
                //       MoveAndOverWrite(sfd.FileName, sfd.FileName);
                AnShowToolTipChk.IsEnabled = false;
                AnSaveFileBtn.IsEnabled = false;
                AnRestoreScaleBtn.IsEnabled = false;
                AnAddCommentBtn.IsEnabled = false;
                SaveLocation = 2;
                SaveRes(sfd.FileName);
            }
        }


        private void SaveRes(string fname)
        {
            byte[] chartdata;
            string ext = System.IO.Path.GetExtension(fname);
            string fn = "";
            int snum = AnChart.Series.Count;
            if (snum > 0)
            {
                int dlen = AnChart.Series[0].Points.Count;
                chartdata = new byte[snum * dlen];
                //prepare data
                for (int i = 0; i < snum; i++)
                {
                    for (int k = 0; k < dlen; k++)
                    {
                        chartdata[snum * i + k] = (byte)AnChart.Series[i].Points[k].YValues[0];
                    }
                }
                //=========================
                if (ext == ".pxe")
                {
                    fn = fname;
                }
                else if (ext == ".pbn")
                {
                    fn = System.IO.Path.GetFileName(fname);
                    string dir = System.IO.Path.GetDirectoryName(fname);
                    string extra = chartdata.Length.ToString() + "X" + snum.ToString();
                    fn = dir + "\\" + extra + fn;


                }
                FileSubs fs = new FileSubs();
                if (ext == ".pxe")
                {
                    AnSavedFileName = fs.SavePXEAN(fn, (uint)snum, chartdata, comments);  //something is not correct
                }
                else if (ext == ".pbn")
                {
                    AnSavedFileName = fs.SaveBinAn(fn, chartdata, comments);
                }
                //  }
                //============== start timer
                timercnt = 0;
                dispatcherTimer.Start();
            }
        }

        private void expander1_Collapsed(object sender, RoutedEventArgs e)
        {

            expander1.BorderThickness = new System.Windows.Thickness(0, 0, 0, 0);
        }

        private void expander1_Expanded(object sender, RoutedEventArgs e)
        {
            expander1.BorderThickness = new System.Windows.Thickness(1, 1, 1, 1);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //PipeComm.Close()
            try
            {
                if (testupdate != null)//test thread
                {
                    testupdate.Abort();
                }
                if (client != null)
                {
                    client.CloseMainWindow();
                    client = null;
                }
                this.Close();

            }
            catch { }
        }

        private void EmulatorLoadPEX_Click(object sender, RoutedEventArgs e)
        {
            if (EmulatorEnabled == true)
            {

                if (testupdate == null)
                {
                    testupdate = new Thread(GetData);
                    testupdate.Start();
                }
                else
                {
                    testupdate.Abort();
                    testupdate = null;
                }
            }
        }


        private bool CheckEmulatorSettings()
        {
            bool bRet = false;
            if (DisplayMode == 1)//time mode
            {
                if (seldet != null && NumberOfRuns > 0)
                    bRet = true;
            }
            else if (DisplayMode == 0)//spatial mode
            {
                if (EmulatorBoardsCount > 0)
                    bRet = true;
            }
            if (!bRet)
                System.Windows.Forms.MessageBox.Show("Wrong Emulator Settings", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return bRet;
        }

        private void ChartTooltipScroll_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int ptnum;
            int CurSer = 0;
            if (DisplayMode == 0)//spatial mode
            {
                CurSer = 0;
            }
            else if (DisplayMode == 1)
            {
                CurSer = SelTimeSer;
            }

            if (MyChart.Series.Count > 0)
            {
                try
                {
                    if (MyChart.Series[CurSer].Points.Count > 0)
                    {
                        if (ChartTooltipScroll.Value < MyChart.Series[CurSer].Points.Count)
                        {
                            ptnum = (int)ChartTooltipScroll.Value;//????????????
                            CurPtX.Content = (ptnum + 1).ToString();
                            CurPtY.Content = MyChart.Series[CurSer].Points[ptnum].YValues[0].ToString();
                            Marker(MyChart, CurSer, ptnum, 0);
                            if (LastTipPos != -1)
                                Marker(MyChart, CurSer, LastTipPos, 1);
                            LastTipPos = ptnum;
                        }
                    }
                }
                catch { }
            }
        }
        //=============================================================
        private void Marker(Chart ch, int SerNum, int PointNum, int opt)
        {
            int serlen;
            switch (opt)
            {
                case 0://change
                    ch.Series[SerNum].Points[PointNum].MarkerStyle = MarkerStyle.Cross;
                    ch.Series[SerNum].Points[PointNum].MarkerSize = 5;
                    ch.Series[SerNum].Points[PointNum].MarkerColor = System.Drawing.Color.Red;
                    ch.Update();
                    break;
                case 1://restore point
                    ch.Series[SerNum].Points[PointNum].MarkerStyle = ch.Series[SerNum].MarkerStyle;
                    ch.Series[SerNum].Points[PointNum].MarkerSize = ch.Series[SerNum].MarkerSize;
                    ch.Series[SerNum].Points[PointNum].MarkerColor = ch.Series[SerNum].MarkerColor;
                    ch.Update();
                    break;
                case 2://restore series ?????????????
                    serlen = ch.Series[SerNum].Points.Count;
                    for (int i = 0; i < serlen; i++)
                    {
                        if (ch.Series[SerNum].Points[i].MarkerStyle != ch.Series[SerNum].MarkerStyle)
                            ch.Series[SerNum].Points[i].MarkerStyle = ch.Series[SerNum].MarkerStyle;
                        if (ch.Series[SerNum].Points[i].MarkerSize != ch.Series[SerNum].MarkerSize)
                            ch.Series[SerNum].Points[i].MarkerSize = ch.Series[SerNum].MarkerSize;
                        if (ch.Series[SerNum].Points[i].MarkerColor != ch.Series[SerNum].MarkerColor)
                            ch.Series[SerNum].Points[i].MarkerColor = ch.Series[SerNum].MarkerColor;
                    }
                    ch.Update();

                    break;
                default:
                    break;
            }
        }


        private void ChartTooltipSelDetCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = ChartTooltipSelDetCmb.SelectedIndex;
            try
            {
                int dt = Convert.ToInt32(ChartTooltipSelDetCmb.Items[idx]);
                if (LastTipPos != -1 && SelTimeSer != -1)
                    Marker(MyChart, LastSelSer, LastTipPos, 2);//???????????????
                SelTimeSer = GetTimeSerIndex(MyChart, dt);
                LastTipPos = -1;
                ChartTooltipScroll.Value = 1;

                if (SelTimeSer > -1)
                {
                    ChartTooltipScroll.Minimum = 1;
                    ChartTooltipScroll.Maximum = MyChart.Series[SelTimeSer].Points.Count;
                    MyChart.Series[SelTimeSer].ChartType = SeriesChartType.Point;//enable marker
                }
                LastSelSer = idx;
            }
            catch { }

        }
        private int GetTimeSerIndex(Chart ch, int num)
        {
            int idx = -1;
            try
            {
                for (int i = 0; i < ch.Series.Count; i++)
                {
                    string sername = ch.Series[i].Name;
                    int pos = sername.IndexOf(" ");
                    string substr = sername.Substring(pos + 1);
                    int a = Convert.ToInt32(substr);
                    if (a == num)
                    {
                        return i;
                    }
                }
            }
            catch { }
            return idx;
        }

        private void AnShowToolScroll_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int ptnum;
            int CurSer = 0;
            if (AnShowMode == 0)//spatial mode
            {
                CurSer = 0;
            }
            else if (AnShowMode == 1)
            {
                CurSer = SelTimeSer;
            }

            if (AnChart.Series.Count > 0 && CurSer > -1)
            {
                if (AnChart.Series[CurSer].Points.Count > 0)
                {
                    if (ChartTooltipScroll.Value < AnChart.Series[CurSer].Points.Count)
                    {
                        ptnum = (int)AnShowToolScroll.Value;
                        AnPtXPos.Content = ptnum.ToString();
                        AnPtYpos.Content = AnChart.Series[CurSer].Points[ptnum].YValues[0].ToString();
                        Marker(AnChart, CurSer, ptnum, 0);
                        if (LastTipPos != -1)
                            Marker(AnChart, CurSer, LastTipPos, 1);
                        LastTipPos = ptnum;
                    }
                }
            }
        }

        private void AnDetSelCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                int dt = Convert.ToInt32(AnDetSelCmb.Items[AnDetSelCmb.SelectedIndex]);
                if (LastTipPos != -1 && SelTimeSer != -1)
                    Marker(AnChart, SelTimeSer, LastTipPos, 2);
                SelTimeSer = GetTimeSerIndex(AnChart, dt);
                LastTipPos = -1;
                AnShowToolScroll.Value = 1;
                if (SelTimeSer > -1)
                {
                    AnShowToolScroll.Minimum = 1;
                    AnShowToolScroll.Maximum = AnChart.Series[SelTimeSer].Points.Count;
                }
            }
            catch { }
        }
        //===============================================================================
        private void CreateChartSeriesA()
        {
            int n;
            GetSelectedDetectors(AnDetSelList);
            ChartColorPalette cp = AnChart.Palette;
            System.Drawing.Color[] pclr = GetColors.GetPaletteColors(cp);
            AnChart.Series.Clear();
            AnChart.Legends.Clear();
            AnChart.ChartAreas[0].AxisX.CustomLabels.Clear();
            AnChart.ChartAreas[0].AxisX.ScaleView.Size = NumberOfRuns;
            AnChart.ApplyPaletteColors();
            n = seldet.Length;
            Series[] series = new Series[n];
            Legend legend = new Legend();
            LegendItem[] li = new LegendItem[n];
            legend.Title = "Detectors";
            legend.LegendStyle = LegendStyle.Column;
            for (int i = 0; i < n; i++)
            {
                li[i] = new LegendItem();
                li[i].Name = "Det " + seldet[i].ToString();
                li[i].ImageStyle = LegendImageStyle.Line;
                li[i].ShadowOffset = 0;
                if (i > pclr.Length)
                {
                    li[i].Color = System.Drawing.Color.FromArgb(255, pclr[i - pclr.Length].R, pclr[i - pclr.Length].G, pclr[i - pclr.Length].B);
                }
                else
                {
                    li[i].Color = System.Drawing.Color.FromArgb(255, pclr[i].R, pclr[i].G, pclr[i].B);
                }
                series[i] = new Series();
                series[i].Name = "Det " + seldet[i].ToString();
                series[i].ChartType = SeriesChartType.Point;
                series[i].MarkerStyle = MarkerStyle.Circle;
                series[i].MarkerSize = SeriesMarkerSize;//2;
                legend.CustomItems.Add(li[i]);
                series[i].IsVisibleInLegend = false;
                AnChart.Series.Add(series[i]);
            }
            AnChart.Legends.Add(legend);
        }
        //===============================================================================
        void SortListBox(System.Windows.Controls.ListBox lb)
        {//for numbers only in listbox
            int k = lb.Items.Count;
            int[] a;
            if (k < 1) return;
            a = new int[k];
            for (int i = 0; i < k; i++)
                a[i] = Convert.ToInt32(lb.Items[i]);
            Array.Sort(a);
            lb.Items.Clear();
            for (int i = 0; i < k; i++)
                lb.Items.Add(a[i].ToString());
        }
        void SortComboBox(System.Windows.Controls.ComboBox cb)
        {//for numbers only
            int k = cb.Items.Count;
            int[] a;
            if (k < 1) return;
            a = new int[k];
            for (int i = 0; i < k; i++)
                a[i] = Convert.ToInt32(cb.Items[i]);
            Array.Sort(a);
            cb.Items.Clear();
            for (int i = 0; i < k; i++)
                cb.Items.Add(a[i].ToString());
        }

        private void ConntestBtn_Click(object sender, RoutedEventArgs e)
        {
            ////test connection with client
            //IsTestConnection = true;
            //StartDataPipe();
            //string clientpath = "C:\\Work\\TestConnection\\Data Emulator\\WindowsPipeClient1(good)\\bin\\Debug\\DataEmulator.exe";
            //if (File.Exists(clientpath))
            //{
            //    client = new Process();
            //    client.StartInfo = new ProcessStartInfo(clientpath);
            //    client.StartInfo.Arguments = "diplot";
            //    client.Start();
            //    this.Topmost = true;
            //    System.Threading.Thread.Sleep(200);
            //    GetPipedata();
            //}
        }

        private void EmulatorLoadPEX_Click_1(object sender, RoutedEventArgs e)
        {
            //load pxe for emulator
            bool? dr;
            int detpbrd;

            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "PXE files (*.pxe)|*.pxe";
            ofd.InitialDirectory = "C:\\Work\\PXE Collection";
            dr = ofd.ShowDialog();
            if (dr == false)
            {
                EmulatorDataTypeChk.IsChecked = false;
            }
            else
                if (dr == true)
                {
                    PxeAccess opxe = new PxeAccess();
                    bool r = opxe.OpenPXEFile(ofd.FileName);
                    if (r == true)
                    {
                        CurrentPXEHeight = opxe.m_Img_Ht;
                        CurrentPXEWidth = opxe.m_Img_Width;
                        float[] tmp = new float[CurrentPXEHeight * CurrentPXEWidth];
                        opxe.m_rawData.CopyTo(tmp, 0);
                        CreateLineData(CurrentPXEHeight, CurrentPXEWidth, tmp);
                        EmulatorDataTypeChk.IsChecked = true;
                        //====================
                        detpbrd = Convert.ToInt32(ConfigurationManager.AppSettings["DetPerBoard"]);
                        if (detpbrd > 0)
                        {
                            long rem;
                            EmulatorBoardsCount = (int)Math.DivRem((long)CurrentPXEHeight, (long)detpbrd, out rem);
                            if (rem > 0)
                            {
                                EmulatorBoardsCount++;//add extra board if not even
                            }
                            NumofEmulatorBoardsUpdown.Value = EmulatorBoardsCount;
                            NumofEmulatorBoardsUpdown.Refresh();
                            opxe.ClosePXE();
                            opxe = null;
                        }

                        else
                        {
                            EmulatorDataTypeChk.IsChecked = false;
                        }

                    }
                    else
                    {
                        EmulatorDataTypeChk.IsChecked = false;
                    }
                }
        }

        void CreateLineData(int CurrentPXEHeight, int CurrentPXEWidth, float[] tmp)
        {
            Pxe4Emulator = new float[CurrentPXEHeight, CurrentPXEWidth];
            for (int i = 0; i < CurrentPXEHeight; i++)
            {
                for (int j = 0; j < CurrentPXEWidth; j++)
                {
                    Pxe4Emulator[i, j] = tmp[i * CurrentPXEWidth + j];
                }
            }
        }

        private void EmulatorDataTypeChk_Checked(object sender, RoutedEventArgs e)
        {
            PXEDataType = true;
        }

        private void EmulatorDataTypeChk_Unchecked(object sender, RoutedEventArgs e)
        {
            PXEDataType = false;
        }

        void miAbout_Click(object sender, RoutedEventArgs e)
        {
            About aw = new About();
            aw.ShowDialog();
        }

        private void AddAboutDialog()
        {
            System.Windows.Controls.ContextMenu cm = new System.Windows.Controls.ContextMenu();
            this.ContextMenu = cm;
            System.Windows.Controls.MenuItem mi = new System.Windows.Controls.MenuItem();
            mi.Header = "About";
            mi.Click += new RoutedEventHandler(miAbout_Click);
            cm.Items.Add(mi);
        }

        private void ClearChart(Chart ch)
        {
            try
            {
                if (ch.Series.Count > 1)
                {
                    for (int k = 0; k < ch.Series.Count; k++)
                    {
                        ch.Series[k].Points.Clear();
                        ch.Series.RemoveAt(k);
                    }
                    ch.Legends.Clear();

                }
                else
                {
                    ch.Series[0].Points.Clear();
                }
            }
            catch { }
        }

        private void MarkerSizeCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SeriesMarkerSize = Convert.ToInt16(MarkerSizeCmb.SelectedItem);
        }

        void SetMarkerSizeComboindex()
        {
            for (int k = 0; k < MarkerSizeCmb.Items.Count; k++)
            {
                string s = MarkerSizeCmb.Items[k].ToString();
                if (s == SeriesMarkerSize.ToString())
                {
                    MarkerSizeCmb.SelectedIndex = k;
                    break;
                }
            }
        }

        private void AnAddCommentBtn_Click(object sender, RoutedEventArgs e)
        {
            //add comments and save it
            string CommentFileName;
            Comment cm = new Comment();
            cm.CommentsText = comments;//if any
            bool? dres = cm.ShowDialog();
            if (dres == true)
                comments = cm.CommentsText;
            if (AnFileName != "")
            {
                CommentFileName = AnFileName + ".txt";
                FileSubs fs = new FileSubs();
                fs.SaveComment(comments, CommentFileName);
            }
            else
                comments = "";
        }

        private void ChartAddCommentsBtn_Click(object sender, RoutedEventArgs e)
        {
            //add comments
            Comment cm = new Comment();
            cm.CommentsText = comments;//if any
            bool? dres = cm.ShowDialog();
            if (dres == true)
                comments = cm.CommentsText;
            else
                comments = "";
        }
        //=================== test sub
        private static bool MoveAndOverWrite(string sSource, string sDestn)
        {
            try
            {
                if (File.Exists(sSource) == true)
                {
                    if (File.Exists(sDestn) == true)
                    {
                        File.Copy(sSource, sDestn, true);
                        File.Delete(sSource);
                        return true;
                    }
                    else
                    {
                        File.Move(sSource, sDestn);
                        return true;
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Specifed file does not exist");
                    return false;
                }
            }
            catch (FileNotFoundException exFile)
            {
                System.Windows.MessageBox.Show("File Not Found " + exFile.Message);
                return false;
            }
            catch (DirectoryNotFoundException exDir)
            {
                System.Windows.MessageBox.Show("Directory Not Found " + exDir.Message);
                return false;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return false;
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //make window half screen and centered
            var w = SystemInformation.PrimaryMonitorSize.Width;
            var ht = SystemInformation.PrimaryMonitorSize.Height;
            //this.Width = w/2;
            //this.Height = ht/2;
            this.Left = 0;// w / 4;
            this.Top = 0;// ht / 4; 
        }
        private void ChartTabControlsSetup(bool show)
        {
            ChartShowToolTipChk.IsEnabled = show;
            ChartRestoreScaleBtn.IsEnabled = show;
            ChartSaveDataBtn.IsEnabled = show;
            ChartSaveImageBtn.IsEnabled = show;
            ChartAddCommentsBtn.IsEnabled = show;
            ChartShowToolTipChk.IsChecked = false;
        }

        private void MainViewBox_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {

        }

        private void Window_Closed(object sender, EventArgs e)
        {            
            diPlotDetectorAppChannel.Dispose();
            this.Close();
        }

        private void StartAcquisition_Click(object sender, RoutedEventArgs e)
        {
            diPlotDetectorAppChannel.StartAcq();
        }

        //================================
    }

}

