using System.Collections.Generic;
using System.Windows;
using System;
using L3.Cargo.Workstation.SystemConfigurationCore;
using L3.Cargo.Common.Xml.History_1_0;
using L3.Cargo.Workstation.Plugins.XRayImageBase;

namespace L3.Cargo.Workstation.Plugins.EAFB
{
    public class XRayImages
    {

        #region Private Members

        #endregion Private Members


        #region Public Members

        #endregion Public Members


        #region Constructors

        public XRayImages ()
        {
        }

        #endregion Constructors


        #region Private Methods

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
                        xrayImage.FragmentMark_MenuItem.Click += new RoutedEventHandler(otherxrayImage.FragmentMark_MenuItem_Click);

                        xrayImage.FragmentRegisterView_Ellipse.Click += new RoutedEventHandler(otherxrayImage.FragmentRegisterView_Ellipse_MenuItem_Click);
                        xrayImage.FragmentUniformity_MenuItem.Click += new RoutedEventHandler(otherxrayImage.FragmentUniformity_MenuItem_Click);
                        xrayImage.CreateFramgmentMark_Ellipse.Click += new RoutedEventHandler(otherxrayImage.CreateFramgmentMark_Ellipse_MenuItem_Click);

                        xrayImage.FragmentMarks_Hide.Click += new RoutedEventHandler(otherxrayImage.FragmentAddMarks_Hide_MenuItem_Click);
                        xrayImage.FragmentMarks_Show.Click += new RoutedEventHandler(otherxrayImage.FragmentAddMarks_Show_MenuItem_Click);                                                                      
                    }
                }
            }

            return xrayImages;
        }

        #endregion Public Methods
    }
}
