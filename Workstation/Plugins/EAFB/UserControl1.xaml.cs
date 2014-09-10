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

namespace L3.Cargo.Workstation.Plugins.EAFB
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl, IDisposable
    {        

        #region Constructor

        public UserControl1 (XRayImage xrayImage)
        {
            InitializeComponent();

            View0Panel.Children.Add(xrayImage);
            View1PanelDef.Width = new GridLength(0);            
        }

        public UserControl1 (List<XRayImage> xrayImages)
        {
            InitializeComponent();

            try
            {
                View0Panel.Children.Add(xrayImages[0]);
            }
            catch
            {
                View0PanelDef.Width = new GridLength(0);
            }

            try
            {
                View1Panel.Children.Add(xrayImages[1]);
            }
            catch
            {
                View1PanelDef.Width = new GridLength(0);
            }
            
        }        

        #endregion Constructor


        #region Public Methods

        public void Dispose ()
        {
            if (View0Panel.Children.Count > 0)
            {
                XRayImage xrayImage0 = (XRayImage)View0Panel.Children[0];
                xrayImage0.Dispose();
            }

            if (View1Panel.Children.Count > 0)
            {
                XRayImage xrayImage1 = (XRayImage)View1Panel.Children[0];
                xrayImage1.Dispose();
            }
        }        

        #endregion Public Methods
    }
}
