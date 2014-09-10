using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Xml.Serialization;
using L3.Cargo.Common;
using L3.Cargo.Workstation.SystemConfigurationCore;
using L3.Cargo.Common.Xml.Annotations_1_0;
using L3.Cargo.Translator;
using L3.Cargo.Workstation.Plugins.Common;
using L3.Cargo.Workstation.Plugins.XRayImageBase;
using System.Collections.ObjectModel;

namespace L3.Cargo.Workstation.Plugins.EAFB
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public class XRayDisplays : IDisposable
    {        

        #region Private Members

        private CaseObject m_CaseObj;

        private SysConfiguration m_SysConfig;

        private HistogramDisplay m_HistogramDisplay;

        private StatusBarItems m_statusBarItems;

        private List<XRayImage> m_XrayImage;
       
        private FragmentDataDisplay m_FragmentDataDisplay;          

        double m_XPosRefOffset = 0;
        Point m_View0MarkPoint;
        double m_MarkPtXPosRadians;
        FragmentAdorner.FragmentMarkMode m_CurrentMarkMode;
        FragmentAdorner m_View0FragmentAdorner;
        FragmentAdorner m_View1FragmentAdorner;

        SourceObject m_View0HighEnergySource;
        bool m_FragmentObjectMarkComplete = true;

        private float m_defaultMarkRadiusSizeMillimeters = 25; //millimeters

        #endregion Private Members


        #region Public Members

        public string UserName
        {
            get { return m_SysConfig.Profile.UserName; }
        }

        public List<LayoutInfo> Displays;

        public float DefaultMarkRadiusSizeMillimeters
        {
            get { return m_defaultMarkRadiusSizeMillimeters; }
        }

        public StatusBarItems statusBarItems
        {
            get
            {
                return m_statusBarItems;
            }
        }

        public HistogramDisplay HistogramDisplay
        {
            get
            {
                return m_HistogramDisplay;
            }
        }

        public FragmentDataDisplay FragmentDataDisplay
        {
            get
            {
                return m_FragmentDataDisplay;
            }
        }

        public Collection<FragmentObject> FragmentMarkInfoList;

        public string UniformityData;

        public double XValOffset = 0;
        
        #endregion Public Members


        #region Constructor

        public XRayDisplays (CaseObject caseObj, SysConfiguration SysConfig)
        {
            m_CaseObj = caseObj;
            m_SysConfig = SysConfig;
            m_statusBarItems = new StatusBarItems();
            m_HistogramDisplay = new HistogramDisplay();
            Displays = new List<LayoutInfo>();
            FragmentMarkInfoList = new Collection<FragmentObject>();

            m_FragmentDataDisplay = new FragmentDataDisplay(this);            

            foreach (DataAttachment attachment in caseObj.attachments.GetUnknownAttachments())
            {                
                if (attachment.attachmentId == "FragmentDataTable.csv")
                {
                    byte[] fragmentData = new byte[attachment.attachmentData.Length];
                    attachment.attachmentData.Read(fragmentData, 0, fragmentData.Length);
                    
                    FragmentMarkInfoList = m_FragmentDataDisplay.UpdateDisplay(fragmentData);
                }
            }

            StatusBarItem caseIDStatus = new StatusBarItem();
            caseIDStatus.Content = "Case ID :" + m_CaseObj.CaseId;
            m_statusBarItems.StatusDisplay.Add(caseIDStatus);

            Initialize();
        }

        #endregion Constructor


        #region Private Methods

        private static bool IsProcessOpen (string name)
        {
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.Contains(name))
                    return true;
            }
            return false;
        }

        private void StartAlgServer ()
        {
            Process processAlgServer = new Process();

            if (!IsProcessOpen("AlgServer"))
            {
                processAlgServer.StartInfo.WorkingDirectory = m_SysConfig.AlgServerAbsolutePath;
                processAlgServer.StartInfo.FileName = "AlgServer.exe";
                processAlgServer.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                processAlgServer.Start();
            }

            int Ret = PxeAccess.LoadAlgServer("127.0.0.1", 1);
        }

        private List<AnnotationInfo> GetAnnotationsForView (string view)
        {
            return null;
        }

        private List<ViewObject> GetImageData()
        {
            List<ViewObject> ret = new List<ViewObject>();

            MemoryStream tipMS = null;

            bool injectTIP = false;

            PxeAccess.TIPStatus tipStatus;

            if (m_CaseObj.caseType == CaseType.FTICase)
            {
                DataAttachment ftiAttachment = null;
                foreach (DataAttachment attachment in m_CaseObj.attachments.GetFTIImageAttachments())
                {
                    tipMS = attachment.attachmentData;
                    injectTIP = true;
                    ftiAttachment = attachment;
                    break;
                }

                m_CaseObj.attachments.Remove(ftiAttachment);
            }

            foreach (DataAttachment attachment in m_CaseObj.attachments.GetXrayImageAttachments())
            {
                MemoryStream ms = attachment.attachmentData;

                PxeAccess pxeAccess = new PxeAccess();

                Rect view0Rect = new Rect();
                Rect view1Rect = new Rect();

                if (injectTIP)
                {
                    tipStatus = new PxeAccess.TIPStatus();
                    pxeAccess.OpenPXEImageFromMemoryWithTIP(ms, tipMS, ref tipStatus);

                    if (tipStatus.injectionsucess == 0)
                    {
                        m_CaseObj.IsCaseEditable = false;
                        m_SysConfig.SelectedArchiveDuringAutoSelect = true;                       
                    }
                    else
                    {
                        view0Rect = new Rect(new Point((double)tipStatus.injectLocation_view0.left,
                                                       (double)tipStatus.injectLocation_view0.top),
                                             new Point((double)tipStatus.injectLocation_view0.right,
                                                       (double)tipStatus.injectLocation_view0.bottom));

                        view1Rect = new Rect(new Point((double)tipStatus.injectLocation_view1.left,
                                                       (double)tipStatus.injectLocation_view1.top),
                                             new Point((double)tipStatus.injectLocation_view1.right,
                                                       (double)tipStatus.injectLocation_view1.bottom));
                    }
                }
                else
                {
                    pxeAccess.OpenPXEImageFromMemory(ms);
                }

                if (pxeAccess.pxeHeader.sequenceNum != null)
                {
                    if (m_CaseObj.scanInfo == null)
                    {
                        m_CaseObj.scanInfo = new ScanInfo();
                    }

                    if (m_CaseObj.scanInfo.container == null)
                    {
                        m_CaseObj.scanInfo.container = new Container();
                    }

                    m_CaseObj.scanInfo.container.SequenceNum = pxeAccess.pxeHeader.sequenceNum.ToString();
                }

                if (pxeAccess.pxeHeader.batchNumber != null)
                {
                    if (m_CaseObj.scanInfo == null)
                    {
                        m_CaseObj.scanInfo = new ScanInfo();
                    }

                    if (m_CaseObj.scanInfo.conveyance == null)
                    {
                        m_CaseObj.scanInfo.conveyance = new Conveyance();
                    }

                    m_CaseObj.scanInfo.conveyance.BatchNum = pxeAccess.pxeHeader.batchNumber.ToString();
                }

                if (pxeAccess.pxeHeader.viewBuffer_0.isValidView != 0)
                {
                    SourceObject highEnergy = null;
                    SourceObject lowEnergy = null;
                    SourceObject trimat = null;
                    ViewType viewType = ViewType.Unknown;

                    int width = (int)pxeAccess.pxeHeader.viewBuffer_0.width;
                    int height = (int)pxeAccess.pxeHeader.viewBuffer_0.height;

                    if (pxeAccess.pxeHeader.viewBuffer_0.isDualEnergy != 0)
                    {
                        highEnergy = new SourceObject(pxeAccess.GetImageBuffer("RawH"), width, height, m_SysConfig.FlipView1XAxis, m_SysConfig.FlipView1YAxis);
                        lowEnergy = new SourceObject(pxeAccess.GetImageBuffer("RawL"), width, height, m_SysConfig.FlipView1XAxis, m_SysConfig.FlipView1YAxis);
                        trimat = new SourceObject(pxeAccess.GetImageBuffer("FinalCompHL"), pxeAccess.GetImageBuffer("FinalAlpha"), width, height, m_SysConfig.FlipView1XAxis, m_SysConfig.FlipView1YAxis);
                        viewType = ViewType.DualEnergy;
                    }
                    else if (width > 0 && height > 0 && pxeAccess.pxeHeader.viewBuffer_0.isHighEnergy != 0)
                    {
                        highEnergy = new SourceObject(pxeAccess.GetImageBuffer("RawH"), width, height, m_SysConfig.FlipView1XAxis, m_SysConfig.FlipView1YAxis);
                        viewType = ViewType.HighEnergy;
                    }
                    else if (width > 0 && height > 0 && pxeAccess.pxeHeader.viewBuffer_0.isHighEnergy == 0)
                    {
                        lowEnergy = new SourceObject(pxeAccess.GetImageBuffer("RawL"), width, height, m_SysConfig.FlipView1XAxis, m_SysConfig.FlipView1YAxis);
                        viewType = ViewType.LowEnergy;
                    }

                    ViewObject viewObj = new ViewObject("View0",
                                                        pxeAccess.pxeHeader.pxeIndex,
                                                        viewType,
                                                        highEnergy,
                                                        lowEnergy,
                                                        trimat,
                                                        pxeAccess.pxeHeader.detectorsPerBoard,
                                                        pxeAccess.pxeHeader.bitsPerPixel,
                                                        pxeAccess.pxeHeader.samplingSpeed,
                                                        pxeAccess.pxeHeader.samplingSpace,
                                                        GetAnnotationsForView("View0"));

                    if (injectTIP)
                    {
                        viewObj.TIPMarkings = new List<Rect>();
                        viewObj.TIPMarkings.Add(view0Rect);
                    }

                    m_View0HighEnergySource = highEnergy;

                    ret.Add(viewObj);                    
                }

                if (pxeAccess.pxeHeader.viewBuffer_1.isValidView != 0)
                {
                    SourceObject highEnergy = null;
                    SourceObject lowEnergy = null;
                    SourceObject trimat = null;
                    ViewType viewType = ViewType.Unknown;

                    int width = (int)pxeAccess.pxeHeader.viewBuffer_1.width;
                    int height = (int)pxeAccess.pxeHeader.viewBuffer_1.height;

                    if (pxeAccess.pxeHeader.viewBuffer_0.isDualEnergy != 0)
                    {
                        highEnergy = new SourceObject(pxeAccess.GetImageBuffer("RawH1"), width, height, m_SysConfig.FlipView2XAxis, m_SysConfig.FlipView2YAxis);
                        lowEnergy = new SourceObject(pxeAccess.GetImageBuffer("RawL1"), width, height, m_SysConfig.FlipView2XAxis, m_SysConfig.FlipView2YAxis);
                        trimat = new SourceObject(pxeAccess.GetImageBuffer("FinalCompHL1"), pxeAccess.GetImageBuffer("FinalAlpha1"), width, height, m_SysConfig.FlipView2XAxis, m_SysConfig.FlipView2YAxis);
                        viewType = ViewType.DualEnergy;
                    }
                    else if (width > 0 && height > 0 && pxeAccess.pxeHeader.viewBuffer_0.isHighEnergy != 0)
                    {
                        highEnergy = new SourceObject(pxeAccess.GetImageBuffer("RawH1"), width, height, m_SysConfig.FlipView2XAxis, m_SysConfig.FlipView2YAxis);
                        viewType = ViewType.HighEnergy;
                    }
                    else if (width > 0 && height > 0 && pxeAccess.pxeHeader.viewBuffer_0.isHighEnergy == 0)
                    {
                        lowEnergy = new SourceObject(pxeAccess.GetImageBuffer("RawL1"), width, height, m_SysConfig.FlipView2XAxis, m_SysConfig.FlipView2YAxis);
                        viewType = ViewType.LowEnergy;
                    }

                    ViewObject viewObj = new ViewObject("View1",
                                                        pxeAccess.pxeHeader.pxeIndex,
                                                        viewType,
                                                        highEnergy,
                                                        lowEnergy,
                                                        trimat,
                                                        pxeAccess.pxeHeader.detectorsPerBoard,
                                                        pxeAccess.pxeHeader.bitsPerPixel,
                                                        pxeAccess.pxeHeader.samplingSpeed,
                                                        pxeAccess.pxeHeader.samplingSpace,
                                                        GetAnnotationsForView("View1"));

                    if (injectTIP)
                    {
                        viewObj.TIPMarkings = new List<Rect>();
                        viewObj.TIPMarkings.Add(view1Rect);
                    }

                    ret.Add(viewObj);                    
                }

                Conversion.SamplingSpace = pxeAccess.pxeHeader.samplingSpace;
            }

            return ret;
        }

        private void Initialize ()
        {
            try
            {
                StartAlgServer();
                CreateXRayDisplays();
            }
            catch (Exception ex)
            {
                //TODO: Log error message here.
                throw ex;
            }
        }

        private void CreateXRayDisplays ()
        {
            List<ViewObject> views = GetImageData();

            if (views.Count > 0)
            {
                L3.Cargo.Workstation.Plugins.EAFB.XRayImages xrayview = new L3.Cargo.Workstation.Plugins.EAFB.XRayImages();
                m_XrayImage = xrayview.CreateImages(views, m_statusBarItems, m_CaseObj.CaseHistories, m_SysConfig);

                m_XrayImage[0].FragmentMark_MenuItem.Click += new RoutedEventHandler(FragmentMark_MenuItem_Click);
                m_XrayImage[1].FragmentMark_MenuItem.Click += new RoutedEventHandler(FragmentMark_MenuItem_Click);

                m_XrayImage[0].FragmentRegisterView_Ellipse.Click += new RoutedEventHandler(FragmentRegisterView_MenuItem_Click);
                m_XrayImage[1].FragmentRegisterView_Ellipse.Click += new RoutedEventHandler(FragmentRegisterView_MenuItem_Click);

                m_XrayImage[0].FragmentUniformity_MenuItem.Click += new RoutedEventHandler(FragmentUniformity_MenuItem_Click);
                m_XrayImage[1].FragmentUniformity_MenuItem.Click += new RoutedEventHandler(FragmentUniformity_MenuItem_Click);

                m_XrayImage[0].CreateFramgmentMark_Ellipse.Click += new RoutedEventHandler(CreateFramgmentMark_MenuItem_Click);
                m_XrayImage[1].CreateFramgmentMark_Ellipse.Click += new RoutedEventHandler(CreateFramgmentMark_MenuItem_Click);

                m_XrayImage[0].PanZoom_MenuItem.Click += new RoutedEventHandler(PanZoom_MenuItem_Click);
                m_XrayImage[1].PanZoom_MenuItem.Click += new RoutedEventHandler(PanZoom_MenuItem_Click);

                m_XrayImage[0].MainXRayView.adonerImageObject.fragmentAdorner.Loaded += new RoutedEventHandler(fragmentAdornerView0_Loaded);

                //Conversion.SamplingSpace = m_SamplingSpace;
                m_View0FragmentAdorner = m_XrayImage[0].MainXRayView.adonerImageObject.fragmentAdorner;
                m_View1FragmentAdorner = m_XrayImage[1].MainXRayView.adonerImageObject.fragmentAdorner;

                m_View0FragmentAdorner.XrayViewNum = 0;
                m_View0FragmentAdorner.DefaultMarkRadiusSizePixels = m_defaultMarkRadiusSizeMillimeters / Conversion.SamplingSpace;

                if (views.Count > 1)
                {
                    m_View1FragmentAdorner.Loaded += new RoutedEventHandler(fragmentAdornerView1_Loaded);
                    m_View1FragmentAdorner.XrayViewNum = 1;
                    m_View1FragmentAdorner.DefaultMarkRadiusSizePixels = m_defaultMarkRadiusSizeMillimeters / Conversion.SamplingSpace;
                }                             

                foreach (XRayImage image in m_XrayImage)
                {
                    image.SetCreateNewAnnotation(m_CaseObj.IsCaseEditable);

                    XRayView xrayView = image.MainView;                    

                    Histogram histogram = new Histogram();
                    histogram.Setup(xrayView.XrayImageEffect, xrayView.Image, xrayView.History, xrayView.CurrentSource.Data);
                    m_HistogramDisplay.AddHistogram(histogram);

                }
                LayoutInfo layoutInfo = new LayoutInfo();
                layoutInfo.Name = "Histogram";
                layoutInfo.Panel = PanelAssignment.InfoPanel;
                layoutInfo.Display = m_HistogramDisplay;

                Displays.Add(layoutInfo);
            }

            if (m_XrayImage != null && m_XrayImage.Count > 0)
            {
                               
                System.Windows.Forms.Screen[] screens = System.Windows.Forms.Screen.AllScreens;

                if (views[0].ImageIndex == 61 && views.Count > 1 && screens.Length > 1)
                {
                    UserControl1 panel0 = new UserControl1(m_XrayImage[0]);

                    LayoutInfo layoutInfo0 = new LayoutInfo();
                    layoutInfo0.Name = "XRayPlugin-View0";
                    layoutInfo0.Panel = PanelAssignment.MainPanel;
                    layoutInfo0.BringToFront = true;
                    layoutInfo0.Display = panel0;
                    layoutInfo0.StatusItems = m_statusBarItems.StatusDisplay;

                    Displays.Add(layoutInfo0);

                    UserControl1 panel1 = new UserControl1(m_XrayImage[1]);
                    panel1.Margin = new Thickness(20);
                    LayoutInfo layoutInfo1 = new LayoutInfo();
                    layoutInfo1.Name = "XRayPlugin-View1";
                    layoutInfo1.Panel = PanelAssignment.SecondaryPanel;
                    layoutInfo1.BringToFront = true;
                    layoutInfo1.Display = panel1;

                    Displays.Add(layoutInfo1);                    

                }
                else
                {
                    
                    UserControl1 panel = new UserControl1(m_XrayImage);

                    LayoutInfo layoutInfo = new LayoutInfo();
                    layoutInfo.Name = "XRayPlugin";
                    layoutInfo.Panel = PanelAssignment.MainPanel;
                    layoutInfo.BringToFront = true;
                    layoutInfo.Display = panel;
                    layoutInfo.StatusItems = m_statusBarItems.StatusDisplay;

                    Displays.Add(layoutInfo);
                }
                
            }
        }

        void fragmentAdornerView0_Loaded(object sender, RoutedEventArgs e)
        {
            Point nullPoint = new Point(0, 0);
            FrameworkElement adornedElement = (FrameworkElement)m_View0FragmentAdorner.AdornedElement;

            foreach (FragmentObject fragObj in FragmentMarkInfoList)
            {
                if (fragObj.MarkType == FragmentObject.MarkTypeEnum.Mark)
                {                    
                    fragObj.CenterPoint = Conversion.ToCartesianPoint(fragObj.Theta1, fragObj.ZValue, adornedElement.ActualHeight);                    
                }

                //original radius value is in millimeters convert it to pixels to draw it on screen.
                fragObj.Radius = Conversion.LengthToPixels(fragObj.Radius);

                m_View0FragmentAdorner.AddFragmentObject(fragObj.CenterPoint,
                                                            /*fragObj.UniqueID,*/
                                                            fragObj.TrimatMarkType,
                                                            fragObj.Radius,
                                                            fragObj.Theta1,
                                                            fragObj.Theta2,
                                                            fragObj.ZValue,
                                                            fragObj.MarkType);

            }

            if (m_View0FragmentAdorner.UniformityMarkPointList.Count > 0)
            {
                m_FragmentDataDisplay.UpdateUniformityInfo(m_View0FragmentAdorner.UniformityMarkPointList[0].CenterPoint,
                        m_View0FragmentAdorner.UniformityMarkPointList[1].CenterPoint, m_View0HighEnergySource);
            }
			
            m_View0FragmentAdorner.RemoveMenuItem.Click += new RoutedEventHandler(RemoveMenuItem_Click);
        }

        void fragmentAdornerView1_Loaded(object sender, RoutedEventArgs e)
        {
            Point nullPoint = new Point(0, 0);
            FrameworkElement adornedElement = (FrameworkElement)m_View1FragmentAdorner.AdornedElement;

            foreach (FragmentObject fragObj in FragmentMarkInfoList)
            {
                if (fragObj.MarkType == FragmentObject.MarkTypeEnum.Mark)
                {
                    if (fragObj.MarkType == FragmentObject.MarkTypeEnum.Mark)
                    {
                        fragObj.CenterPoint = Conversion.ToCartesianPoint(fragObj.Theta2, fragObj.Z1Value, adornedElement.ActualHeight);
                    }                    

                    m_View1FragmentAdorner.AddFragmentObject(fragObj.CenterPoint,
                                                                /*fragObj.UniqueID,*/
                                                                fragObj.TrimatMarkType,
                                                                fragObj.Radius,
                                                                fragObj.Theta1,
                                                                fragObj.Theta2,
                                                                fragObj.Z1Value,
                                                                fragObj.MarkType);
                }

            }
        }

        private void UnsubscribeMouseEvents()
        {
            //unsubscribe to left mouse click event on view0
            m_View0FragmentAdorner.AdornedElement.MouseLeftButtonDown -=
                new System.Windows.Input.MouseButtonEventHandler(fragmentAdornerView0_MouseLeftButtonDown);

            //unsubscribe to left mouse click event on view1
            m_View1FragmentAdorner.AdornedElement.MouseLeftButtonDown -=
                new System.Windows.Input.MouseButtonEventHandler(fragmentAdornerView1_MouseLeftButtonDown);
        }

        void PanZoom_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            UnsubscribeMouseEvents();

            m_View0FragmentAdorner.FragmentLayerEnabled = false;
            
        }

        void RemoveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (m_View0FragmentAdorner.FocusedObject != null)
            {
                m_View0FragmentAdorner.EraseFragmentMark(m_View0FragmentAdorner.FocusedObject.Theta1,
                                                         m_View0FragmentAdorner.FocusedObject.Theta2,
                                                         m_View0FragmentAdorner.FocusedObject.ZValue);

                m_View1FragmentAdorner.EraseFragmentMark(m_View0FragmentAdorner.FocusedObject.Theta1,
                                                         m_View0FragmentAdorner.FocusedObject.Theta2,
                                                         m_View0FragmentAdorner.FocusedObject.ZValue);

                //remove this item from the table
                m_FragmentDataDisplay.RemoveFragmentObject(m_View0FragmentAdorner.FocusedObject.Theta1,
                                                         m_View0FragmentAdorner.FocusedObject.Theta2,
                                                         m_View0FragmentAdorner.FocusedObject.ZValue);
            }
        }

        void FragmentMark_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            UnsubscribeMouseEvents();
            //set Fragment mode to mark on both views
            m_View0FragmentAdorner.MarkMode = FragmentAdorner.FragmentMarkMode.Mark;
            m_View1FragmentAdorner.MarkMode = FragmentAdorner.FragmentMarkMode.Mark;            

            if (m_FragmentObjectMarkComplete)
            {
                //subscribe to left mouse click event on view0
                m_View0FragmentAdorner.AdornedElement.MouseLeftButtonDown +=
                    new System.Windows.Input.MouseButtonEventHandler(fragmentAdornerView0_MouseLeftButtonDown);
                
            }
            else
            {
                m_View1FragmentAdorner.AdornedElement.MouseLeftButtonDown +=
                    new System.Windows.Input.MouseButtonEventHandler(fragmentAdornerView1_MouseLeftButtonDown);
            }

            m_View0FragmentAdorner.FragmentLayerEnabled = true;            

            m_CurrentMarkMode = FragmentAdorner.FragmentMarkMode.Mark;

        }

        void FragmentUniformity_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            UnsubscribeMouseEvents();
            //set Fragment mode to mark on both views
            m_View0FragmentAdorner.MarkMode = FragmentAdorner.FragmentMarkMode.Uniformity;
            m_View1FragmentAdorner.MarkMode = FragmentAdorner.FragmentMarkMode.Uniformity;

            m_CurrentMarkMode = FragmentAdorner.FragmentMarkMode.Uniformity;

            //subscribe to left mouse click event on view0
            m_View0FragmentAdorner.AdornedElement.MouseLeftButtonDown +=
                new System.Windows.Input.MouseButtonEventHandler(fragmentAdornerView0_MouseLeftButtonDown);
        }

        void FragmentRegisterView_MenuItem_Click(object sender, RoutedEventArgs e)
        {            
            //set Fragment mode to mark on both views
            m_View0FragmentAdorner.MarkMode = FragmentAdorner.FragmentMarkMode.RegisterViews;
            m_View1FragmentAdorner.MarkMode = FragmentAdorner.FragmentMarkMode.RegisterViews;
            
            m_CurrentMarkMode = FragmentAdorner.FragmentMarkMode.RegisterViews;
        }        

        void CreateFramgmentMark_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //set Fragment mode to mark on both views
            m_View0FragmentAdorner.MarkMode = FragmentAdorner.FragmentMarkMode.Mark;
            m_View1FragmentAdorner.MarkMode = FragmentAdorner.FragmentMarkMode.Mark;

            m_CurrentMarkMode = FragmentAdorner.FragmentMarkMode.Mark;
        }

        void fragmentAdornerView0_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point MousePoint = e.GetPosition(m_View0FragmentAdorner.AdornedElement);
            MousePoint.X = (int)MousePoint.X;
            MousePoint.Y = (int)MousePoint.Y;
            
            m_View0MarkPoint = MousePoint;

            m_View0FragmentAdorner.RemoveRightClickMenu();

            if (m_CurrentMarkMode == FragmentAdorner.FragmentMarkMode.RegisterViews)
            {
                m_View1FragmentAdorner.EraseRegisterViewMarks();
                m_View0FragmentAdorner.EraseRegisterViewMarks();
            }            

            //mark fragment point on view0 where mouse is clicked as a center point
            m_View0FragmentAdorner.DrawMark(MousePoint);

            if (m_CurrentMarkMode == FragmentAdorner.FragmentMarkMode.Mark)
            {
                //draw verticle line going across the image on both view0 and view1 X position of where mouse was clicked
                m_View0FragmentAdorner.DrawVerticalLine(MousePoint, 0);
                m_View1FragmentAdorner.DrawVerticalLine(MousePoint, m_XPosRefOffset);
            }

            if (m_CurrentMarkMode != FragmentAdorner.FragmentMarkMode.Uniformity)
            {
                //unsubscribe left mouse click event on view0
                m_View0FragmentAdorner.AdornedElement.MouseLeftButtonDown -=
                    new System.Windows.Input.MouseButtonEventHandler(fragmentAdornerView0_MouseLeftButtonDown);

                //subscribe to left mouse click event on view1
                m_View1FragmentAdorner.AdornedElement.MouseLeftButtonDown +=
                    new System.Windows.Input.MouseButtonEventHandler(fragmentAdornerView1_MouseLeftButtonDown);

                m_FragmentObjectMarkComplete = false;
            }
            else
                m_FragmentObjectMarkComplete = true;

            if (m_CurrentMarkMode == FragmentAdorner.FragmentMarkMode.Mark)
            {
                //update Fragment Display table
                FrameworkElement adornedElement = (FrameworkElement)m_View0FragmentAdorner.AdornedElement;
                m_MarkPtXPosRadians = m_FragmentDataDisplay.AddData2Table1M(MousePoint, adornedElement.ActualHeight);
            }

            if (m_CurrentMarkMode == FragmentAdorner.FragmentMarkMode.Uniformity && m_View0FragmentAdorner.UniformityMarkPointList.Count > 1)
            {
                m_FragmentDataDisplay.UpdateUniformityInfo(m_View0FragmentAdorner.UniformityMarkPointList[0].CenterPoint, 
                    m_View0FragmentAdorner.UniformityMarkPointList[1].CenterPoint, m_View0HighEnergySource);
            }

            //invalidate visual for adorner layer to display marks
            m_View1FragmentAdorner.InvalidateVisual();
            m_View0FragmentAdorner.InvalidateVisual();
        }

        void fragmentAdornerView1_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point MousePoint = e.GetPosition(m_View1FragmentAdorner.AdornedElement);
            MousePoint.X = (int)MousePoint.X;
            MousePoint.Y = (int)MousePoint.Y;

            if (m_CurrentMarkMode == FragmentAdorner.FragmentMarkMode.RegisterViews)
            {
                m_XPosRefOffset = MousePoint.X - m_View0MarkPoint.X;
                m_View1FragmentAdorner.UpdateMarksOffsetPosX(m_XPosRefOffset);
            }

            //mark fragment point on view1 where mouse is clicked as a center point
            m_View1FragmentAdorner.DrawMark(MousePoint);

            if (m_CurrentMarkMode == FragmentAdorner.FragmentMarkMode.Mark)
            {
                //erase vertical line on view0 and view1
                m_View0FragmentAdorner.EraseVerticalLine();
                m_View1FragmentAdorner.EraseVerticalLine();
            }

            //unsubscribe left mouse click event on view1
            m_View1FragmentAdorner.AdornedElement.MouseLeftButtonDown -=
                new System.Windows.Input.MouseButtonEventHandler(fragmentAdornerView1_MouseLeftButtonDown);

            //subscribe to left mouse click event on view0
            m_View0FragmentAdorner.AdornedElement.MouseLeftButtonDown +=
                new System.Windows.Input.MouseButtonEventHandler(fragmentAdornerView0_MouseLeftButtonDown);

            if (m_CurrentMarkMode == FragmentAdorner.FragmentMarkMode.Mark)
            {
                //update Fragment Display table
                FrameworkElement adornedElement = (FrameworkElement)m_View1FragmentAdorner.AdornedElement;
                m_FragmentDataDisplay.AddData2Table2M(MousePoint, adornedElement.ActualHeight, m_MarkPtXPosRadians);
            }

            m_FragmentObjectMarkComplete = true;

            //invalidate visual for adorner layer to display marks
            m_View1FragmentAdorner.InvalidateVisual();
            m_View0FragmentAdorner.InvalidateVisual();
        }

        #endregion Private Methods


        #region Public Methods

        public void FragmentObjSelectionChanged(List<int> tableIdxList)
        {
            m_View0FragmentAdorner.SelectedFragmentObjsByIndex(tableIdxList);
            m_View1FragmentAdorner.SelectedFragmentObjsByIndex(tableIdxList);
        }

        public void ChangeMarkRadiusAndTrimatType(int tableIndex, double NewRadiusInPixels, FragmentObject.TrimatMarkEnum trimatType)
        {
            m_View1FragmentAdorner.UpdateMarkRadiusAndTrimatType(tableIndex, NewRadiusInPixels, trimatType);
            m_View0FragmentAdorner.UpdateMarkRadiusAndTrimatType(tableIndex, NewRadiusInPixels, trimatType);
        }

        public void eraseFragmentMark(double theta1, double theta2, double zVal )
        {
            m_View0FragmentAdorner.EraseFragmentMark(theta1, theta2, zVal);
            m_View1FragmentAdorner.EraseFragmentMark(theta1, theta2, zVal);
        }

        public void eraseFragmentMark(int tableIdx)
        {
            m_View0FragmentAdorner.EraseFragmentMark(tableIdx);
            m_View1FragmentAdorner.EraseFragmentMark(tableIdx);
        } 

        public void eraseUniformityMarks()
        {
            m_View0FragmentAdorner.EraseUniformityMarks();
            m_View1FragmentAdorner.EraseUniformityMarks();
        }

        public void eraseAllFragmentMarks()
        {
            m_View0FragmentAdorner.EraseAllFragmentMarks();
            m_View1FragmentAdorner.EraseAllFragmentMarks();
        }

        public void Dispose ()
        {
            m_HistogramDisplay.Dispose();            

            if (Displays != null)
            {
                foreach (LayoutInfo layoutInfo in Displays)
                {
                    UserControl1 userControl1 = layoutInfo.Display as UserControl1;

                    if (userControl1 != null)
                    {
                        userControl1.Dispose();
                    }
                    else
                    {
                        HistogramDisplay histogramDisplay = layoutInfo.Display as HistogramDisplay;

                        if (histogramDisplay != null)
                        {
                            histogramDisplay.Dispose();
                        }
                    }

                }

                Displays.Clear();
            }
                       
            byte[] filedata = null;
            try
            {
                string tfn = Path.GetTempFileName();
                m_FragmentDataDisplay.SaveReport(tfn);
                if (File.Exists(tfn))
                {
                    string data = File.ReadAllText(tfn);
                    System.Text.ASCIIEncoding en = new System.Text.ASCIIEncoding();
                    filedata = en.GetBytes(data);
                    DataAttachment attachment = new DataAttachment();
                    attachment.attachmentId = "FragmentDataTable.csv";
                    attachment.attachmentType = AttachmentType.Unknown;
                    attachment.attachmentData = new MemoryStream(filedata, true); //MemoryStream
                    //   MemoryStream ms = new MemoryStream();
                    m_CaseObj.NewAttachments.Add(attachment);
                    File.Delete(tfn); //not working
                }
            }
            catch { }
            m_statusBarItems.StatusDisplay.Clear();
            m_statusBarItems = null;
            m_CaseObj = null;

        }        

        #endregion Public Methods
    }
}
