using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Controls;

namespace L3.Cargo.Safety.Display.Common.PortalObjects
{
    public class EStopSensor : PortalObject
    {
        public EStopSensor(string objectName, Point3D points, ref Model3DGroup modelGroup)
            : base(objectName, points, ref modelGroup)
        {
            createCylinderModel3D();
        }

        override public void applyBehavior(UserControl control, string name, int value)
        {
            if (value == 1)
            {
                _objectGeometry.Material = (MaterialGroup)control.FindResource("M_Error");
            }
            else if (value == 2)
            {
                _objectGeometry.Material = (MaterialGroup)control.FindResource("M_Warning");
            }
            else
            {
                setObjectHidden();
            }
        }
    }
}
