using System.Collections.Generic;
using System.Windows;
using System;
using L3.Cargo.Workstation.SystemConfigurationCore;
using L3.Cargo.Common.Xml.History_1_0;
using L3.Cargo.Workstation.Plugins.XRayImageBase.Common;

namespace L3.Cargo.Workstation.Plugins.XRayImageBase
{
    public class XRayImages
    {

        #region Private Members

        #endregion Private Members


        #region Public Members

        public event AlgServerRequestEventHandler AlgServerRequestEvent;

        #endregion Public Members


        #region Constructors

        public XRayImages ()
        {
        }

        #endregion Constructors


        #region Private Methods

        void xrayImage_AlgServerRequestEvent (object sender, AlgServerRequestEventArgs e)
        {
            if (AlgServerRequestEvent != null)
            {
                AlgServerRequestEvent(sender, e);
            }
        }

        #endregion Private Methods


        #region Public Methods

        public List<XRayImage> CreateImages (List<ViewObject> viewObjs, StatusBarItems statusBarItems, Histories histories, SysConfiguration SysConfig)
        {
            List<XRayImage> xrayImages = new List<XRayImage>();

            string username = string.Empty;
            if (SysConfig.Profile != null)
            {
                username = SysConfig.Profile.UserName;
            }


            foreach (ViewObject viewObj in viewObjs)
            {
                int count = 0;
                try
                {
                    if (viewObj.HighEnergy != null || viewObj.LowEnergy != null)
                    {
                        History history = histories.Find(viewObj.Name);

                        if (history == null)
                        {
                            history = new History();
                            history.id = viewObj.Name;
                            histories.History.Add(history);
                        }

                        if (SysConfig.Profile != null)
                        {
                            history.DefaultUser = SysConfig.Profile.UserName;
                        }

                        count++;
                        XRayImage xrayImage = new XRayImage(viewObj, statusBarItems, history, SysConfig);
                        xrayImage.AlgServerRequestEvent += new AlgServerRequestEventHandler(xrayImage_AlgServerRequestEvent);

                        xrayImages.Add(xrayImage);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            foreach (XRayImage xrayImage in xrayImages)
            {
                foreach (XRayImage otherxrayImage in xrayImages)
                {
                    if (otherxrayImage != xrayImage)
                    {
                        xrayImage.PanZoom_MenuItem.Click += new RoutedEventHandler(otherxrayImage.PanZoom_MenuItem_Click);
                        xrayImage.Annotation_MenuItem.Click += new RoutedEventHandler(otherxrayImage.Annotation_MenuItem_Click);
                        xrayImage.Measurements_MenuItem.Click += new RoutedEventHandler(otherxrayImage.Measurements_MenuItem_Click);
                        xrayImage.Magnifier_MenuItem.Click += new RoutedEventHandler(otherxrayImage.Magnifier_MenuItem_Click);
                        xrayImage.AOI_MenuItem.Click += new RoutedEventHandler(otherxrayImage.AOI_MenuItem_Click);
                        xrayImage.Annotation_Rectangle.Click += new RoutedEventHandler(otherxrayImage.Annotation_Rectangle_MenuItem_Click);
                        xrayImage.Annotation_Ellipse.Click += new RoutedEventHandler(otherxrayImage.Annotation_Ellipse_MenuItem_Click);
                        xrayImage.Annotation_Show.Click += new RoutedEventHandler(otherxrayImage.Annotation_Show_MenuItem_Click);
                        xrayImage.Annotation_Hide.Click += new RoutedEventHandler(otherxrayImage.Annotation_Hide_MenuItem_Click);
                        xrayImage.Measurement_Show.Click += new RoutedEventHandler(otherxrayImage.Measurement_Show_MenuItem_Click);
                        xrayImage.Measurement_Hide.Click += new RoutedEventHandler(otherxrayImage.Measurement_Hide_MenuItem_Click);
                    }
                }
            }

            return xrayImages;
        }

        #endregion Public Methods
    }
}
