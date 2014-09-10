using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Controls;

namespace L3.Cargo.Safety.Display.Common.PortalObjects
{
    public class TrafficLight : PortalObject
    {
        private static string TRAFFIC_RED_LIGHT_RSC_NAME = "RED_TRAFFIC";
        private static string TRAFFIC_GREEN_LIGHT_RSC_NAME = "GREEN_TRAFFIC_LIGHT";
        private static string TRAFFIC_RED_LIGHT_XML_TAG = "M_Traffic_Light_Red";
        private static string TRAFFIC_GREEN_LIGHT_XML_TAG = "M_Traffic_Light_Green";

        public TrafficLight(string objectName, Point3D points, ref Model3DGroup modelGroup)
            : base(objectName, points, ref modelGroup)
        {
            createCylinderModel3D();
        }

        override public void applyBehavior(UserControl control, string name, int value)
        {
            if (value == 0)
            {
                _objectGeometry.Material = (MaterialGroup)control.FindResource(TRAFFIC_GREEN_LIGHT_XML_TAG);
            }
            else if (value == 1)
            {
                _objectGeometry.Material = (MaterialGroup)control.FindResource(TRAFFIC_RED_LIGHT_XML_TAG);
            }
            else
            {
                setObjectHidden();
            }
        }
    }
}
