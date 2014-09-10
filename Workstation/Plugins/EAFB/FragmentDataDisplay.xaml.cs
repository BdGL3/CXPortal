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
using Microsoft.Win32;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using L3.Cargo.Workstation.Plugins.XRayImageBase;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using L3.Cargo.Common;

namespace L3.Cargo.Workstation.Plugins.EAFB
{
    /// <summary>
    /// Interaction logic for FragmentData.xaml
    /// </summary>
    public partial class FragmentDataDisplay : UserControl
    {
        
        private XRayDisplays m_XrayImageDisplay;        
        private ObservableCollection<string> _TypeCollection = new ObservableCollection<string>();
        private ObservableCollection<string> _ShapeCollection = new ObservableCollection<string>();
        private ObservableCollection<string> _SizeCollection = new ObservableCollection<string>();
        private ObservableCollection<ObjectData> _TableStrinsCollection = new ObservableCollection<ObjectData>();        

        public int TableSelection = -1;
        public bool DensityAnalysisMode;
        public string UniformityData;

        public FragmentDataDisplay(XRayDisplays xrayDisplays)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            m_XrayImageDisplay = xrayDisplays;

            //============ lisview settings
            _TypeCollection.Add("Not Specified");
            _TypeCollection.Add("Metal");
            _TypeCollection.Add("Organic");
            _TypeCollection.Add("Inorganic");

            _ShapeCollection.Add("Not Specified");
            _ShapeCollection.Add("Cylindical");
            _ShapeCollection.Add("Rectangular");
            _ShapeCollection.Add("C Shape");

            _SizeCollection.Add("Not Specified");
            _SizeCollection.Add(" < 4 mm");
            _SizeCollection.Add("5mm-8mm");
            _SizeCollection.Add("9mm-12mm");
            _SizeCollection.Add("13mm-16mm");
            _SizeCollection.Add("17mm-20mm");
            _SizeCollection.Add(">20mm");

            for (int i = 0; i < 99; i++)
            {
                DataPoint dp = new DataPoint(i + 1, 0);
                UniformityChartSeries.Points.Add(dp);
            }
        }
        public ObservableCollection<string> TypeCollection
        { get { return _TypeCollection; } }
        public ObservableCollection<string> ShapeCollection
        { get { return _ShapeCollection; } }
        public ObservableCollection<string> SizeCollection
        { get { return _SizeCollection; } }
        public ObservableCollection<ObjectData> TableStrinsCollection
        { get { return _TableStrinsCollection; } }

        private FragmentObject AddReportSting(String s)
        {
            FragmentObject.TrimatMarkEnum clr;
            //CircleData c_d;            
            string[] ss = s.Split(new Char[] { ',' });
            double zval;
            double z1val=0;
            double theta1;
            double theta2;
            double rad;
            FragmentObject fragmentMark = null;

            if (ss.Length >= 9)
            {
                if (ss[7].Trim() == "") ss[7] = "Not Specified";
                if (ss[8].Trim() == "") ss[8] = "Not Specified";
                if (ss[6].Trim() == "") ss[6] = "Not Specified";

                ObjectData ob = new ObjectData();

                ob.FragData = ss[0].Trim();
                ob.Theta1Data = ss[1].Trim();
                ob.Theta2Data = ss[2].Trim();
                ob.ZData = ss[3].Trim();
                ob.XData = ss[4].Trim();
                ob.YData = ss[5].Trim();
                ob.TypeData = ss[6].Trim();
                ob.ShapeData = ss[7].Trim();
                ob.SizeData = ss[8].Trim();

                if(ss.Length > 9)
                    ob.Z1Data = ss[9].Trim();

                TableStrinsCollection.Add(ob);
                //add circles collection.  will be used later ================
                {
                    zval = Convert.ToDouble(ss[3].Trim());

                    if (ss.Length > 9 && ss[9] != string.Empty)
                        z1val = Convert.ToDouble(ss[9].Trim());
                    theta1 = Convert.ToDouble(ss[1].Trim());
                    theta2 = Convert.ToDouble(ss[2].Trim());
                    switch (ss[6].Trim())
                    {
                        case "Metal":
                            clr = FragmentObject.TrimatMarkEnum.Metal;
                            break;
                        case "Organic":
                            clr = FragmentObject.TrimatMarkEnum.Organic;
                            break;
                        case "Inorganic":
                            clr = FragmentObject.TrimatMarkEnum.Inorganic;
                            break;
                        default:
                            clr = FragmentObject.TrimatMarkEnum.Unknown;
                            break;
                    }
                    switch (ss[8].Trim())
                    {
                        case "< 4 mm":
                            rad = 4;
                            break;
                        case "5mm-8mm":
                            rad = 8;
                            break;
                        case "9mm-12mm":
                            rad = 12;
                            break;
                        case "13mm-16mm":
                            rad = 16;
                            break;
                        case "17mm-20mm":
                            rad = 20;
                            break;
                        case ">20mm":
                            rad = 25;
                            break;
                        default:
                            rad = m_XrayImageDisplay.DefaultMarkRadiusSizeMillimeters;
                            break;
                    }

                    fragmentMark = new FragmentObject(new Point(), clr, rad, theta1, theta2, zval, z1val, FragmentObject.MarkTypeEnum.Mark);
                }
            }

            return fragmentMark;
        }

        public Collection<FragmentObject> UpdateDisplay(byte[] data)
        {
            char[] delimiter = new char[2];
            delimiter[0] = (char)10;
            delimiter[1] = (char)13; 
           
            int CommentCounter1;
            int CommentCounter2;
            int charcnt;
            string Comments;
            string headerbuf;
            string[] split = null;
            String[] h_split = null;
            bool hflag = true;
            string Adornerdata = System.Text.Encoding.Default.GetString(data);
            Collection<FragmentObject> FragmentMarkList = new Collection<FragmentObject>();

            CommentCounter1 = 0;
            CommentCounter2 = 0;
            Comments = "";
            headerbuf = "";
            
            if (Adornerdata.Length > 0)
            {
                ObservableCollection<ObjectData> TSC = TableStrinsCollection;
                TSC.Clear();
                
                split = Adornerdata.Split(delimiter);
                if (split.Length > 0)
                {
                    for (int i = 0; i < split.Length; i++)
                    {
                        split[i] = split[i].Trim();
                    }
                    // header info===============================================================================
                    for (int i = 0; i < split.Length; i++)
                    {
                        if (split[i] != "" && hflag == true)
                        {
                            h_split = split[i].Split(new Char[] { '`' });
                            if (h_split.Length > 3)
                            {
                                TestNameField.Text = h_split[0].Trim();
                                ProjectNameField.Text = h_split[1].Trim();
                                MissionNameField.Text = h_split[2].Trim();
                                BundleField.Text = h_split[3].Trim();
                                hflag = false;

                            }
                        }
                        if (split[i] != "" && hflag == false)
                        {

                            charcnt = split[i].Where(c => c == ',').Count();
                            if (charcnt == 9)//this is report string
                            {
                                FragmentObject fragmentMark = AddReportSting(split[i]);
                                FragmentMarkList.Add(fragmentMark);
                            }
                        }
                    }
                    //========================================= find comments if any
                    for (int i = 0; i < split.Length; i++)
                    {
                        if (split[i].Contains("%%%"))
                        {
                            CommentCounter1 = i;
                            break;
                        }
                    }
                    if (CommentCounter1 > 0)
                    {
                        for (int i = CommentCounter1 + 1; i < split.Length; i++)
                        {
                            if (split[i].Contains("%%%"))
                            {
                                CommentCounter2 = i;
                                break;
                            }
                        }
                        for (int i = CommentCounter1 + 1; i < CommentCounter2; i++)
                        {
                            if (split[i] != "" && !split[i].Contains("Annotation"))
                            {
                                Comments += (split[i] + "\n");
                            }
                        }
                        CommentsField.Text = Comments;
                    }

                    //================= find uniformity if any
                    for (int jj = 0; jj < split.Length; jj++)
                    {
                        if (split[jj].Contains("Uniformity Data ="))
                        {
                            string[] oo = split[jj].Split(new Char[] { '=' });
                            if (oo.Length == 2 && oo[1] != string.Empty)
                            {
                                UniformityData = oo[1].Trim();

                                Point pt1 = new Point();
                                Point pt2 = new Point();

                                string[] sp = UniformityData.Split(new Char[] { '&' });
                                pt1.X = Convert.ToDouble(sp[0]);
                                pt1.Y = Convert.ToDouble(sp[1]);
                                pt2.X = Convert.ToDouble(sp[2]);
                                pt2.Y = Convert.ToDouble(sp[3]);

                                FragmentObject fragObject = new FragmentObject(pt1,FragmentObject.TrimatMarkEnum.Unknown, 0, 0, 0, 0, 0, FragmentObject.MarkTypeEnum.Uniformity);
                                FragmentMarkList.Add(fragObject);
                                fragObject = new FragmentObject(pt2,FragmentObject.TrimatMarkEnum.Unknown, 0, 0, 0, 0, 0, FragmentObject.MarkTypeEnum.Uniformity);
                                FragmentMarkList.Add(fragObject);
                            }
                        }
                    }
                }
            }

            return FragmentMarkList;
        }

        public double AddData2Table1M(Point pt, double Imageheight)
        {
            try
            {                
                ObjectData od;
                double xPos;
                double theta1 = Conversion.ConvertY2Theta(pt.Y, Imageheight);
                xPos = Conversion.ConvertDegreesToRadians(theta1);
                int tcount = TableStrinsCollection.Count + 1;
                od = new ObjectData
                {
                    FragData = tcount.ToString(),
                    ShapeData = "Not Specified",
                    SizeData = "Not Specified",
                    TypeData = "Not Specified",
                    Theta1Data = Math.Round(theta1, 2).ToString(),
                    Theta2Data = "",
                    ZData = Math.Round(pt.X * Conversion.SamplingSpace, 0).ToString(),
                    XData = "",
                    YData = "",
                    Z1Data = ""
                };
                
                TableStrinsCollection.Add(od);

                DataTable.SelectedIndex = DataTable.Items.Count - 1;
                DataTable.ScrollIntoView(DataTable.SelectedItem);

                return xPos;

            }
            catch { }

            return 0;

        }

        public void UpdateUniformityInfo(Point startPoint, Point endPoint, SourceObject HighEnergySourceObject)
        {
            int startidx = (int)startPoint.X;
            int fnidx = (int)endPoint.X;
            int tmp = 0;
            float [] linedata;

            if (Math.Abs(startidx - fnidx) < 2) return;
            UniformityData = startPoint.X.ToString() + "&" + startPoint.Y.ToString() + "&" + endPoint.X.ToString() + "&" + endPoint.Y.ToString();

            if (HighEnergySourceObject != null)
            {
                // some test
                if (startidx > fnidx)
                {
                    tmp = fnidx;
                    fnidx = startidx;
                    startidx = tmp;
                }

                linedata = new float[fnidx - startidx];
                startidx = HighEnergySourceObject.Width * (int)endPoint.Y + startidx;
                BitmapSource bms = HighEnergySourceObject.Source;
                Array.Copy(HighEnergySourceObject.Data, startidx, linedata, 0, linedata.Length);
                //===================================================================================================
                for (int k = 0; k < linedata.Length; k++)
                    linedata[k] *= 65356;//??????????????????????????????????????? kumsal's requirement
                //====================================================================================================

                float max = (float)(Conversion.MaxValue(linedata));
                float min = (float)(Conversion.MinValue(linedata));

                float coeff = (float)((max - min) * 0.1);
                max += coeff;
                min -= coeff;//adjust graph 
                if (UniformityChart.Series.Count > 0)
                    UniformityChart.Series[0].Points.Clear();
                UniformityChart.ChartAreas[0].AxisY.StripLines.Clear();
                UniformityChart.ChartAreas[0].AxisY.ScaleView.Size = (max - min);//?????????????????
                UniformityChart.ChartAreas[0].AxisY.Maximum = Math.Round(max, 0);
                UniformityChart.ChartAreas[0].AxisY.Minimum = Math.Round(min, 0);
                UniformityChart.ChartAreas[0].AxisX.IsStartedFromZero = true;
                for (int j = 1; j < linedata.Length + 1; j++)
                {
                    System.Windows.Forms.DataVisualization.Charting.DataPoint dp = new System.Windows.Forms.DataVisualization.Charting.DataPoint(j, linedata[j - 1]);
                    UniformityChart.Series[0].Points.Add(dp);

                }

                double mean = UniformityChart.DataManipulator.Statistics.Mean(UniformityChart.Series[0].Name);
                double std = Math.Sqrt(UniformityChart.DataManipulator.Statistics.Variance(UniformityChart.Series[0].Name, true));
                UniformityChart.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
                UniformityChart.ChartAreas[0].AxisY.ScaleView.ZoomReset(0);
                MeanValue.Text = Math.Round(UniformityChart.DataManipulator.Statistics.Mean(UniformityChart.Series[0].Name), 0).ToString();
                STDValue.Text = Math.Round(Math.Sqrt(UniformityChart.DataManipulator.Statistics.Variance(UniformityChart.Series[0].Name, true)), 0).ToString();
                STDPercentValue.Text = Math.Round((std / mean) * 100, 1).ToString("F");
                MinValue.Text = Math.Round(Conversion.MinValue(linedata), 0).ToString();
                MaxValue.Text = Math.Round((decimal)Conversion.MaxValue(linedata), 0).ToString();

            }
        }

        public void AddData2Table2M(Point pt, double Imageheight, double xPos)
        {            
            ObjectData od;
            int chcnt = TableStrinsCollection.Count;
            double yPos;
            try
            {
                od = TableStrinsCollection[chcnt - 1];
                double theta2 = Conversion.ConvertY2Theta(pt.Y, Imageheight);
                yPos = Conversion.ConvertDegreesToRadians(theta2);
                Point XYPoint = Conversion.XYcalculation(new Point(xPos, yPos));
                od.Theta2Data = Math.Round(theta2, 2).ToString();
                od.XData = Math.Round(XYPoint.X, 2).ToString();
                od.YData = Math.Round(XYPoint.Y, 2).ToString();
                od.Z1Data = Math.Round(pt.X * Conversion.SamplingSpace, 2).ToString();

                TableStrinsCollection.RemoveAt(chcnt - 1);
                
                TableStrinsCollection.Add(od);

                if (DataTable.Items.Count > 0)
                {
                    DataTable.SelectedIndex = DataTable.Items.Count - 1;
                }                
                
            }
            catch (Exception ex)
            { string s = ex.Message; }
        }

        public void RemoveFragmentObject(double Theta1, double Theta2, double ZVal)
        {
            ObjectData ObjTobeRemoved = null;
            foreach (ObjectData obj in TableStrinsCollection)
            {
                if (obj.Theta1Data == Theta1.ToString() &&
                    obj.Theta2Data == Theta2.ToString() &&
                    obj.ZData == ZVal.ToString())
                {
                    ObjTobeRemoved = obj;
                    break;
                }
            }
            if (ObjTobeRemoved != null)
            {
                TableStrinsCollection.Remove(ObjTobeRemoved);

                int UniqueID = 1;

                foreach (ObjectData obj in TableStrinsCollection)
                {
                    obj.FragData = UniqueID.ToString();
                    UniqueID++;
                }
            }
        }

        #region SaveReport
        private void SaveReportBtn_Click(object sender, RoutedEventArgs e)
        {
            bool? res;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text files| *.txt|All Files |*.*";
            res = sfd.ShowDialog();
            if (res == true)
            {
                SaveTextReport(sfd.FileName);
            }
        }

        public void SaveTextReport(string fname)
        {
            string Header = "", tmp = "";
            string[] ReportStrings;
            int DtCnt = DataTable.Items.Count;
            string Header2 = "%# " + "           Bundle  " + "         Z (mm)    " + "    X (mm)    " + "      Y (mm)    " + "      Type    " + "        Shape    " + "        Size   ";
            if (DtCnt > 0)
            {

                StreamWriter sr = new StreamWriter(fname);
                sr.WriteLine();
                Header = "%Test Name: " + TestNameField.Text;
                sr.WriteLine(Header);
                Header = "%Project: " + ProjectNameField.Text;
                sr.WriteLine(Header);
                Header = "%Mission: " + MissionNameField.Text;
                sr.WriteLine(Header);
                Header = "%Date: " + DateTime.Now.ToString();
                sr.WriteLine(Header);
                Header = "%Analyst: " + m_XrayImageDisplay.UserName;
                sr.WriteLine(Header);
                Header = "%Notes: " + CommentsField.Text;
                sr.WriteLine(Header);
                tmp = STDPercentValue.Text;
                Header = "%STD Percent: " + tmp.Replace("=", ":");
                sr.WriteLine(Header);


                sr.WriteLine();
                sr.WriteLine(Header2);
                sr.WriteLine();
                ReportStrings = PrepareReportStringsM();
                for (int i = 0; i < ReportStrings.Length; i++)
                {
                    sr.WriteLine(ReportStrings[i]);
                }
                sr.Close();
            }
        }

        public void SaveReport(string fname)
        {
            string[] ReportStrings;
            int DtCnt = DataTable.Items.Count;
            string Header = "  " + TestNameField.Text + "` " + ProjectNameField.Text + "` " + MissionNameField.Text + "` " + BundleField.Text + "` " + DateTime.Now.ToString();
            string Header2 = "Frag # " + "        Theta 1(deg)  " + "  Theta 2(deg)  " + "  Z (mm)    " + "      X (mm)    " + "      Y (mm)    " + "      Type    " + "        Shape    " + "        Size   ";
            if (DtCnt > 0)
            {
                StreamWriter sr = new StreamWriter(fname);
                sr.WriteLine();
                sr.WriteLine(Header);
                sr.WriteLine();
                sr.WriteLine(Header2);
                sr.WriteLine();
                ReportStrings = PrepareReportStrings();
                for (int i = 0; i < ReportStrings.Length; i++)
                {
                    sr.WriteLine(ReportStrings[i]);
                }
                //add comments
                if (CommentsField.Text.Length > 0)
                {
                    sr.WriteLine("%%%");//mark comments
                    sr.WriteLine("             Annotation");
                    sr.WriteLine();
                    sr.WriteLine(CommentsField.Text);
                    sr.WriteLine("%%%");//mark comments
                }
                sr.WriteLine();
                //add uniformity data
                sr.WriteLine("Uniformity Data =" + UniformityData);
                sr.Close();
            }
            else
            {
                StreamWriter sr = new StreamWriter(fname);
                sr.WriteLine();
                sr.WriteLine(Header);
                sr.WriteLine();
                //ad comments
                if (CommentsField.Text.Length > 0)
                {
                    sr.WriteLine("%%%");//mark comments
                    sr.WriteLine("             Annotation");
                    sr.WriteLine();
                    sr.WriteLine(CommentsField.Text);
                    sr.WriteLine("%%%");//mark comments
                }
                sr.WriteLine();
                //add uniformity data
                sr.WriteLine("Uniformity Data =" + UniformityData);
                sr.Close();

            }

        }

        private string[] PrepareReportStringsM()
        {
            string[] result = new string[0];
            string[] Fragments = new string[8];
            ObjectData ob;
            int Fraglen = 15, ln;
            int num = _TableStrinsCollection.Count;
            if (num == 0)
                return result;
            else
            {
                result = new string[num];
                for (int i = 0; i < num; i++)
                {
                    ob = _TableStrinsCollection[i];
                    Fragments[0] = ob.FragData;
                    Fragments[1] = BundleField.Text;
                    Fragments[2] = ob.ZData;
                    Fragments[3] = ob.XData;
                    Fragments[4] = ob.YData;
                    Fragments[5] = ob.TypeData;
                    Fragments[6] = ob.ShapeData;
                    Fragments[7] = ob.SizeData;
                    for (int k = 0; k < Fragments.Length; k++)//clean
                    {
                        if (Fragments[k] != null)
                        {
                            Fragments[k] = Fragments[k].Trim();
                            ln = Fragments[k].Length;
                            if (ln < Fraglen)
                            {
                                Fragments[k] += GetEmptyString(Fraglen - ln);
                            }
                        }
                        else
                        {
                            Fragments[k] = GetEmptyString(Fraglen);

                        }
                        result[i] += Fragments[k];
                    }
                }
            }
            return result;
        }

        private string[] PrepareReportStrings()
        {
            string[] result = new string[0];
            string[] Fragments = new string[10];
            ObjectData ob;
            int Fraglen = 15, ln;
            int num = _TableStrinsCollection.Count;
            if (num == 0)
                return result;
            else
            {
                result = new string[num];
                for (int i = 0; i < num; i++)
                {
                    ob = _TableStrinsCollection[i];
                    Fragments[0] = ob.FragData;
                    Fragments[1] = ob.Theta1Data;
                    Fragments[2] = ob.Theta2Data;
                    Fragments[3] = ob.ZData;
                    Fragments[4] = ob.XData;
                    Fragments[5] = ob.YData;
                    Fragments[6] = ob.TypeData;
                    Fragments[7] = ob.ShapeData;
                    Fragments[8] = ob.SizeData;
                    Fragments[9] = ob.Z1Data;
                    for (int k = 0; k < Fragments.Length; k++)//clean
                    {
                        if (Fragments[k] != null)
                        {
                            Fragments[k] = Fragments[k].Trim();
                            ln = Fragments[k].Length;
                            if (ln < Fraglen)
                            {
                                Fragments[k] += GetEmptyString(Fraglen - ln);
                                if (k < Fragments.Length - 1)
                                {
                                    Fragments[k] += ",";
                                }
                            }
                        }
                        else
                        {
                            Fragments[k] = GetEmptyString(Fraglen);
                            if (k < Fragments.Length - 1)
                            {
                                Fragments[k] += ",";
                            }
                        }
                        result[i] += Fragments[k];
                    }
                }
            }
            return result;
        }


        private string GetEmptyString(int num)
        {
            string emptystring = "";

            for (int i = 0; i < num; i++)
            {
                emptystring += " ";
            }

            return emptystring;
        }
        #endregion

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeCircles();
        }

        private void DataTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<int> SelectedIndexList = new List<int>();
            if (DataTable.SelectedItems.Count > 0)
            {
                for (int index = DataTable.SelectedItems.Count - 1; index >= 0; index--)
                {
                    ObjectData od = DataTable.SelectedItems[index] as ObjectData;

                    if (od != null)
                    {                        
                        SelectedIndexList.Add(Convert.ToInt32(od.FragData) - 1);
                    }
                }
            }            

            if (SelectedIndexList.Count > 0)
            {
                m_XrayImageDisplay.FragmentObjSelectionChanged(SelectedIndexList);
            }
        }

        private void ChangeCircles()
        {
            FragmentObject.TrimatMarkEnum trimatType = FragmentObject.TrimatMarkEnum.Unknown;
            double rad = 0;
            if (DataTable.SelectedIndex > -1)
            {
                ObjectData od = _TableStrinsCollection[DataTable.SelectedIndex];

                switch (od.TypeData)
                {
                    case "Not Specified":                        
                        trimatType = FragmentObject.TrimatMarkEnum.Unknown;
                        break;
                    case "Metal":
                        trimatType = FragmentObject.TrimatMarkEnum.Metal;                        
                        break;
                    case "Organic":
                        trimatType = FragmentObject.TrimatMarkEnum.Organic;                        
                        break;
                    case "Inorganic":
                        trimatType = FragmentObject.TrimatMarkEnum.Inorganic;
                        break;
                }
                switch (od.SizeData)
                {
                    case "Not Specified":
                        rad = m_XrayImageDisplay.DefaultMarkRadiusSizeMillimeters;
                        break;
                    case " < 4 mm":
                        rad = 4;
                        break;
                    case "5mm-8mm":
                        rad = 8;
                        break;
                    case "9mm-12mm":
                        rad = 12;
                        break;
                    case "13mm-16mm":
                        rad = 16;
                        break;
                    case "17mm-20mm":
                        rad = 20;
                        break;
                    case ">20mm":
                        rad = 25;
                        break;
                }
                 
                //convert radius from millimeters to pixels.
                rad = rad / Conversion.SamplingSpace;
                m_XrayImageDisplay.ChangeMarkRadiusAndTrimatType(DataTable.SelectedIndex, rad, trimatType);
            }
        }
        private void DelString_Click(object sender, RoutedEventArgs e)
        {
            if (DataTable.SelectedItems.Count > 0)
            {
                for (int index = DataTable.SelectedItems.Count-1; index >= 0; index--)
                {
                    ObjectData od  = DataTable.SelectedItems[index] as ObjectData;

                    if (od != null)
                    {
                        m_XrayImageDisplay.eraseFragmentMark(Convert.ToInt32(od.FragData) - 1);

                        double zval = Convert.ToDouble(od.ZData);
                        double theta1 = Convert.ToDouble(od.Theta1Data);
                        double theta2 = Convert.ToDouble(od.Theta2Data);
                        RemoveFragmentObject(theta1, theta2, zval);
                    }
                }
            }

            if (DataTable.SelectedItems.Count > 0)
            {
                ObjectData od = _TableStrinsCollection[DataTable.SelectedIndex];
                double zval = Convert.ToDouble(od.ZData);
                double theta1 = Convert.ToDouble(od.Theta1Data);
                double theta2 = Convert.ToDouble(od.Theta2Data);
                m_XrayImageDisplay.eraseFragmentMark(DataTable.SelectedIndex);

                RemoveFragmentObject(theta1, theta2, zval);                
            }
        }

        private void ClearTable_Click(object sender, RoutedEventArgs e)
        {
            if (_TableStrinsCollection.Count > 0)
            {
                _TableStrinsCollection.Clear();
                m_XrayImageDisplay.eraseAllFragmentMarks();

            }
        }        

        private void RemoveUnifirmity_Click(object sender, RoutedEventArgs e)
        {
            MeanValue.Text = "0";
            STDValue.Text = "0";
            STDPercentValue.Text = "0";
            MinValue.Text = "0";
            MaxValue.Text = "0";
            UniformityChart.Series[0].Points.Clear();
            for (int i = 0; i < 99; i++)
            {
                DataPoint dp = new DataPoint(i + 1, 0);
                UniformityChartSeries.Points.Add(dp);
            }
            m_XrayImageDisplay.eraseUniformityMarks();
        }
    }

    #region ListViewHelper

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
          object parameter, System.Globalization.CultureInfo culture)
        {
            bool param = bool.Parse(parameter as string);
            bool val = (bool)value;

            return val == param ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType,
          object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class ObjectData : INotifyPropertyChanged
    {
        private string m_Type;
        private string m_Shape;
        private string m_Size;
        private string m_Frag;
        private string m_Theta1;
        private string m_Theta2;
        private string m_ZValue;
        private string m_XValue;
        private string m_YValue;
        private string m_Z1Value;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }


        public string TypeData
        {
            get { return m_Type; }
            set
            {                
                m_Type = value;
                NotifyPropertyChanged("TypeData");
            }
        }

        public string ShapeData
        {
            get { return m_Shape; }
            set
            {
                m_Shape = value;
                NotifyPropertyChanged("ShapeData");     
            }
        }


        public string SizeData
        {
            get { return m_Size; }
            set
            {
                m_Size = value;
                NotifyPropertyChanged("SizeData");
            }
        }

        public string FragData
        {
            get { return m_Frag; }
            set
            {
                m_Frag = value;
                NotifyPropertyChanged("FragData");
            }
        }

        public string Theta1Data
        {
            get { return m_Theta1; }
            set
            {
                m_Theta1 = value;
                NotifyPropertyChanged("Theta1Data");
            }
        }

        public string Theta2Data
        {
            get { return m_Theta2; }
            set
            {
                m_Theta2 = value;
                NotifyPropertyChanged("Theta2Data");
            }
        }

        public string ZData
        {
            get { return m_ZValue; }
            set
            {
                m_ZValue = value;
                NotifyPropertyChanged("ZData");
            }
        }

        public string XData
        {
            get { return m_XValue; }
            set
            {
                m_XValue = value;
                NotifyPropertyChanged("XData");
            }
        }

        public string YData
        {
            get { return m_YValue; }
            set
            {
                m_YValue = value;
                NotifyPropertyChanged("YData");
            }
        }

        public string Z1Data
        {
            get { return m_Z1Value; }
            set
            {
                m_Z1Value = value;
                //NotifyPropertyChanged("YData");
            }
        }
    }
    #endregion
}
