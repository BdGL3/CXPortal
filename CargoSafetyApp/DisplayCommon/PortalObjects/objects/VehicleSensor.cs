using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Controls;

namespace L3.Cargo.Safety.Display.Common.PortalObjects
{
    class VehicleSensor : PortalObject
    {
        public VehicleSensor(string objectName, Point3D points, ref Model3DGroup modelGroup)
            : base(objectName, points, ref modelGroup)
        {
            createVehicleCylinderModel3D();
        }

        override public void applyBehavior(UserControl control, string name, int value)
        {
            if ((value != 1 && value != 2))
            {
                _objectGeometry.Material = (MaterialGroup)control.FindResource("M_Vehicle_Sensor_Good");
            }
            else if (value == 1)
            {
                _objectGeometry.Material = (MaterialGroup)control.FindResource("M_Vehicle_Sensor_Warning");
            }
            else if (value == 2)
            {
                _objectGeometry.Material = (MaterialGroup)control.FindResource("M_Vehicle_Sensor_Bad");
            }
        }
    }
}
