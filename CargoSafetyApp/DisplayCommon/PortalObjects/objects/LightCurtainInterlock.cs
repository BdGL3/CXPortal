using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Controls;

namespace L3.Cargo.Safety.Display.Common.PortalObjects
{
    public class LightCurtainInterlock : PortalObject
    {
        public LightCurtainInterlock(string objectName, Point3D points, ref Model3DGroup modelGroup)
            : base(objectName, points, ref modelGroup)
        {
            createEmittersCylinderModel3D();
        }

        override public void applyBehavior(UserControl control, string name, int value)
        {
            System.Console.WriteLine("Applying behavior to " + name + " value " + value);

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
                _objectGeometry.Material = (Material)control.FindResource("M_Interlock");
            }
        }
    
    }
}
