using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Xml.Serialization;
using L3.Cargo.Common;
using L3.Cargo.Workstation.Plugins.XRayImageBase;
using L3.Cargo.Workstation.SystemConfigurationCore;
using L3.Cargo.Common.Xml.Annotations_1_0;
using L3.Cargo.Translator;
using L3.Cargo.Workstation.Plugins.Common;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using L3.Cargo.Workstation.Plugins.XRayImageBase.Common;
using L3.Cargo.Workstation.Plugins.XRayImageBase.Adorners;

namespace L3.Cargo.Workstation.Plugins.XRayImages
{
    public static class AlgClientInstances
    {
        public static uint InstanceNumber;
    }

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

        private Dictionary<string, List<AnnotationInfo>> m_Annotations;

        private bool m_DisplayFTIError;

        private PxeAccess _PxeAccess;

        private uint instanceID;

        #endregion Private Members


        #region Public Members

        public List<LayoutInfo> Displays;

        public HistogramDisplay HistogramDisplay
        {
            get
            {
                return m_HistogramDisplay;
            }
        }
        
        #endregion Public Members


        #region Constructor

        public XRayDisplays (CaseObject caseObj, SysConfiguration SysConfig)
        {
            m_CaseObj = caseObj;
            m_SysConfig = SysConfig;
            m_statusBarItems = new StatusBarItems();
            m_HistogramDisplay = new HistogramDisplay();
            Displays = new List<LayoutInfo>();
            m_DisplayFTIError = false;
            
            instanceID = ++AlgClientInstances.InstanceNumber;
            _PxeAccess = new PxeAccess(instanceID);

            caseObj.DisplayTIPEvent += new CaseObject.DisplayTIPHandler(caseObj_DisplayTIPEvent);
            Initialize();
        }

        #endregion Constructor


        #region Private Methods

        private void caseObj_DisplayTIPEvent (CaseType caseType, bool isCorrect)
        {
            TIPPopUp tipPopUp = new TIPPopUp();
            string message = string.Empty;

            if (caseType == CaseType.CTICase)
            {
                message += "The x-ray image was a Combined Threat Image (CTI).\n\n";

                if (isCorrect)
                {
                    message += "You have correctly identified the CTI.";
                }
                else
                {
                    message += "You have missed identifying the CTI.";
                }
            }
            else if (caseType == CaseType.FTICase)
            {
                message += "The x-ray image includes a Fictional Threat Image (FTI).\n\n";

                if (isCorrect)
                {
                    message += "You have correctly identified the FTI.";
                }
                else
                {
                    message += "You have missed identifying the FTI.";
                }

                if (m_XrayImage.Count > 0)
                {
                    foreach (XRayImage xrayImage in m_XrayImage)
                    {
                        xrayImage.ShowFTILocations();
                    }
                }
            }

            if (m_XrayImage.Count > 0)
            {
                tipPopUp.PlacementTarget = m_XrayImage[0];
                tipPopUp.SetMessage(message);
                tipPopUp.SetIcon(isCorrect);
                tipPopUp.IsOpen = true;
            }
        }

        private void DisplayTIPError()
        {
            TIPPopUp tipPopUp = new TIPPopUp();
            string message = "The FTI image was unable to be inserted into this x-ray image\nPlease press \"ClearScreen\" to process the next image.";

            if (m_XrayImage.Count > 0)
            {
                tipPopUp.PlacementTarget = m_XrayImage[0];
                tipPopUp.SetMessage(message);
                tipPopUp.SetIcon(false);
                tipPopUp.IsOpen = true;
            }
        }


        private void StartAlgServer ()
        {
            _PxeAccess.LoadAlgServer("127.0.0.1", m_SysConfig.AlgServerAbsolutePath);
        }

        private List<AnnotationInfo> GetAnnotationsForView (string view)
        {
            if (m_Annotations == null)
            {
                foreach (DataAttachment attachment in m_CaseObj.attachments.GetAnnotationsAttachments())
                {
                    m_Annotations = AnnotationsTranslator.TranslateXML(attachment.attachmentData);
                    break;
                }
            }

            if (m_Annotations!= null && m_Annotations.ContainsKey(view))
            {
                return m_Annotations[view];
            }
            else
            {
                return new List<AnnotationInfo>();
            }
        }

        private List<ViewObject> GetImageData()
        {
            List<ViewObject> ret = new List<ViewObject>();

            MemoryStream tipMS = null;

            bool injectTIP = false;

            TIPStatus tipStatus;

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

                System.Windows.Rect view0Rect = new System.Windows.Rect();
                System.Windows.Rect view1Rect = new System.Windows.Rect();

                if (injectTIP)
                {
                    tipStatus = new TIPStatus();

                    _PxeAccess.OpenPXEImageFromMemoryWithTIP(ms, tipMS, ref tipStatus);

                    if (tipStatus.injectionsucess == 0)
                    {
                        m_CaseObj.IsCaseEditable = false;
                        m_SysConfig.SelectedArchiveDuringAutoSelect = true;
                        m_DisplayFTIError = true;
                    }
                    else
                    {
                        view0Rect = new System.Windows.Rect(new Point((double)tipStatus.injectLocation_view0.left,
                                                       (double)tipStatus.injectLocation_view0.top),
                                             new Point((double)tipStatus.injectLocation_view0.right,
                                                       (double)tipStatus.injectLocation_view0.bottom));

                        view1Rect = new System.Windows.Rect(new Point((double)tipStatus.injectLocation_view1.left,
                                                       (double)tipStatus.injectLocation_view1.top),
                                             new Point((double)tipStatus.injectLocation_view1.right,
                                                       (double)tipStatus.injectLocation_view1.bottom));
                    }
                }
                else
                {
                    _PxeAccess.OpenPXEImageFromMemory(ms);
                }

                if (_PxeAccess.GetPxeHeader().sequenceNum != null)
                {
                    if (m_CaseObj.scanInfo == null)
                    {
                        m_CaseObj.scanInfo = new ScanInfo();
                    }

                    if (m_CaseObj.scanInfo.container == null)
                    {
                        m_CaseObj.scanInfo.container = new Container();
                    }

                    m_CaseObj.scanInfo.container.SequenceNum = _PxeAccess.GetPxeHeader().sequenceNum.ToString();
                }

                if (_PxeAccess.GetPxeHeader().batchNumber != null)
                {
                    if (m_CaseObj.scanInfo == null)
                    {
                        m_CaseObj.scanInfo = new ScanInfo();
                    }

                    if (m_CaseObj.scanInfo.conveyance == null)
                    {
                        m_CaseObj.scanInfo.conveyance = new Conveyance();
                    }

                    m_CaseObj.scanInfo.conveyance.BatchNum = _PxeAccess.GetPxeHeader().batchNumber.ToString();
                }

                if (_PxeAccess.GetPxeHeader().viewBuffer_0.isValidView != 0)
                {
                    SourceObject highEnergy = null;
                    SourceObject lowEnergy = null;
                    ViewType viewType = ViewType.Unknown;

                    int width = (int)_PxeAccess.GetPxeHeader().viewBuffer_0.width;
                    int height = (int)_PxeAccess.GetPxeHeader().viewBuffer_0.height;

                    if (_PxeAccess.GetPxeHeader().viewBuffer_0.isDualEnergy != 0)
                    {
                        highEnergy = new SourceObject(_PxeAccess.GetImageBuffer("RawH"), width, height, m_SysConfig.FlipView1XAxis, m_SysConfig.FlipView1YAxis);
                        lowEnergy = new SourceObject(_PxeAccess.GetImageBuffer("RawL"), width, height, m_SysConfig.FlipView1XAxis, m_SysConfig.FlipView1YAxis);
                        viewType = ViewType.DualEnergy;
                    }
                    else if (width > 0 && height > 0 && _PxeAccess.GetPxeHeader().viewBuffer_0.isHighEnergy != 0)
                    {
                        highEnergy = new SourceObject(_PxeAccess.GetImageBuffer("RawH"), width, height, m_SysConfig.FlipView1XAxis, m_SysConfig.FlipView1YAxis);
                        viewType = ViewType.HighEnergy;
                    }
                    else if (width > 0 && height > 0 && _PxeAccess.GetPxeHeader().viewBuffer_0.isHighEnergy == 0)
                    {
                        lowEnergy = new SourceObject(_PxeAccess.GetImageBuffer("RawL"), width, height, m_SysConfig.FlipView1XAxis, m_SysConfig.FlipView1YAxis);
                        viewType = ViewType.LowEnergy;
                    }

                    ViewObject viewObj = new ViewObject("View0",
                                                        _PxeAccess.GetPxeHeader().pxeIndex,
                                                        viewType,
                                                        highEnergy,
                                                        lowEnergy,
                                                        _PxeAccess.GetPxeHeader().detectorsPerBoard,
                                                        _PxeAccess.GetPxeHeader().bitsPerPixel,
                                                        _PxeAccess.GetPxeHeader().samplingSpeed,
                                                        _PxeAccess.GetPxeHeader().samplingSpace,
                                                        GetAnnotationsForView("View0"));

                    if (injectTIP)
                    {
                        viewObj.TIPMarkings = new List<System.Windows.Rect>();
                        viewObj.TIPMarkings.Add(view0Rect);
                    }

                    ret.Add(viewObj);
                }

                if (_PxeAccess.GetPxeHeader().viewBuffer_1.isValidView != 0)
                {
                    SourceObject highEnergy = null;
                    SourceObject lowEnergy = null;
                    ViewType viewType = ViewType.Unknown;

                    int width = (int)_PxeAccess.GetPxeHeader().viewBuffer_1.width;
                    int height = (int)_PxeAccess.GetPxeHeader().viewBuffer_1.height;

                    if (_PxeAccess.GetPxeHeader().viewBuffer_0.isDualEnergy != 0)
                    {
                        highEnergy = new SourceObject(_PxeAccess.GetImageBuffer("RawH1"), width, height, m_SysConfig.FlipView2XAxis, m_SysConfig.FlipView2YAxis);
                        lowEnergy = new SourceObject(_PxeAccess.GetImageBuffer("RawL1"), width, height, m_SysConfig.FlipView2XAxis, m_SysConfig.FlipView2YAxis);
                        viewType = ViewType.DualEnergy;
                    }
                    else if (width > 0 && height > 0 && _PxeAccess.GetPxeHeader().viewBuffer_0.isHighEnergy != 0)
                    {
                        highEnergy = new SourceObject(_PxeAccess.GetImageBuffer("RawH1"), width, height, m_SysConfig.FlipView2XAxis, m_SysConfig.FlipView2YAxis);
                        viewType = ViewType.HighEnergy;
                    }
                    else if (width > 0 && height > 0 && _PxeAccess.GetPxeHeader().viewBuffer_0.isHighEnergy == 0)
                    {
                        lowEnergy = new SourceObject(_PxeAccess.GetImageBuffer("RawL1"), width, height, m_SysConfig.FlipView2XAxis, m_SysConfig.FlipView2YAxis);
                        viewType = ViewType.LowEnergy;
                    }

                    ViewObject viewObj = new ViewObject("View1",
                                                        _PxeAccess.GetPxeHeader().pxeIndex,
                                                        viewType,
                                                        highEnergy,
                                                        lowEnergy,
                                                        _PxeAccess.GetPxeHeader().detectorsPerBoard,
                                                        _PxeAccess.GetPxeHeader().bitsPerPixel,
                                                        _PxeAccess.GetPxeHeader().samplingSpeed,
                                                        _PxeAccess.GetPxeHeader().samplingSpace,
                                                        GetAnnotationsForView("View1"));

                    if (injectTIP)
                    {
                        viewObj.TIPMarkings = new List<System.Windows.Rect>();
                        viewObj.TIPMarkings.Add(view1Rect);
                    }

                    ret.Add(viewObj);
                }
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
                L3.Cargo.Workstation.Plugins.XRayImageBase.XRayImages xrayview = new L3.Cargo.Workstation.Plugins.XRayImageBase.XRayImages();
                m_XrayImage = xrayview.CreateImages(views, m_statusBarItems, m_CaseObj.CaseHistories, m_SysConfig);
                xrayview.AlgServerRequestEvent += new AlgServerRequestEventHandler(xrayview_AlgServerRequestEvent);

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

                if (m_DisplayFTIError)
                {
                    DisplayTIPError();
                }
            }
        }

        private void xrayview_AlgServerRequestEvent (object sender, AlgServerRequestEventArgs e)
        {
            XRayView xrayView = sender as XRayView;

            if (xrayView != null)
            {
                //uint[] data = pxeAccess.GetTrimatBuffer(xrayView.ViewName, (int)e.RequestType);

                //for (int i = 0; i < data.Length; i++)
                //{
                //    //Switch the Red and Blue Colors
                //    uint newDataValue = data[i];
                //    uint redColor = newDataValue % 256;
                //    uint blueColor = (newDataValue << 8) >> 24;
                //    newDataValue = (newDataValue - redColor) - (blueColor << 16);
                //    data[i] = (newDataValue + blueColor) + (redColor << 16);
                //    data[i] += (uint)255 << 24;
                //}

                PixelFormat pixelFormat = PixelFormats.Rgb24;
                int pixelBytes = pixelFormat.BitsPerPixel / 8;
                int stride = (int)xrayView.Image.Source.Width * pixelBytes;

                uint[] data = _PxeAccess.GetTrimatBuffer(xrayView.ViewName, (int)e.RequestType);
                byte[] convertedData = new byte[data.Length * pixelBytes];

                for (int i = 0; i < data.Length; i++)
                {
                    byte redColor = Convert.ToByte((data[i] <<24) >> 24);
                    byte greenColor = Convert.ToByte((data[i] << 16) >> 24);
                    byte blueColor = Convert.ToByte((data[i] << 8) >> 24);

                    convertedData[i * pixelBytes] = redColor;
                    convertedData[i * pixelBytes + 1] = greenColor;
                    convertedData[i * pixelBytes + 2] = blueColor;
                }

                xrayView.Image.Source = BitmapSource.Create((int)xrayView.Image.Source.Width, (int)xrayView.Image.Source.Height, 96, 96, pixelFormat, null, convertedData, stride);
            }
        }

        #endregion Private Methods


        #region Public Methods

        public void Dispose ()
        {
            _PxeAccess.Dispose();

            AlgClientInstances.InstanceNumber--;

            m_HistogramDisplay.Dispose();

            if (m_Annotations == null)
                m_Annotations = new Dictionary<string, List<AnnotationInfo>>();
            else
                m_Annotations.Clear();

            if (m_XrayImage != null)
            {
                foreach (XRayImage image in m_XrayImage)
                {
                    if (!m_Annotations.ContainsKey(image.ViewName))
                    {
                        List<Annotation> annots = image.GetAnnotations();
                        List<AnnotationInfo> infos = new List<AnnotationInfo>();
                        foreach (Annotation ann in annots)
                        {
                            infos.Add(new AnnotationInfo(ann.Marking.Rect.TopLeft, ann.Marking.Rect.Width, ann.Marking.Rect.Height, ann.Marking.RadiusX, ann.Marking.RadiusY, ann.CommentText));
                        }
                        m_Annotations.Add(image.ViewName, infos);
                    }
                }
            }

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

            DataAttachment dataAttachment = null;

            foreach (DataAttachment attachment in m_CaseObj.attachments.GetAnnotationsAttachments())
            {
                dataAttachment = attachment;
                break;
            }

            if (dataAttachment == null)
            {
                dataAttachment = new DataAttachment();
                dataAttachment.attachmentId = "Annotations.xml";
                dataAttachment.attachmentType = AttachmentType.Annotations;
                dataAttachment.CreateTime = CultureResources.ConvertDateTimeToStringForData(DateTime.Now);     
            }

            dataAttachment.attachmentData = (MemoryStream)AnnotationsTranslator.CreateXML(m_Annotations);
            dataAttachment.IsNew = true;
            m_CaseObj.NewAttachments.Add(dataAttachment);

            m_statusBarItems.StatusDisplay.Clear();
            m_statusBarItems = null;
            m_CaseObj = null;
        }        

        #endregion Public Methods
    }
}
