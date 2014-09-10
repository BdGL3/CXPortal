using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Linq.Expressions;

namespace L3.Cargo.Safety.Display.Common.PortalObjects
{
    public enum PortalObjectType
    {
        E_STOP,
        LIGHT_CURTAIN_INTERLOCKS,
        MOTION_DETECTOR_SENSORS,
        TRAFFIC_LIGHTS,
        VEHICLE_SENSORS
    };

    public class PortalObjectFactory
    {

        static public PortalObject createPortalObject(PortalObjectType objectType,
                                                      string objectName, 
                                                      Point3D points,
                                                      ref Model3DGroup modelGroup)
        {
            PortalObject portalObject = null;

            switch (objectType)
            {
                case PortalObjectType.E_STOP:
                    portalObject = new EStopSensor(objectName, points, ref modelGroup);
                    break;
                case PortalObjectType.LIGHT_CURTAIN_INTERLOCKS:
                    portalObject = new LightCurtainInterlock(objectName, points, ref modelGroup);
                    break;
                case PortalObjectType.MOTION_DETECTOR_SENSORS:
                    portalObject = new MotionDetector(objectName, points, ref modelGroup);
                    break;
                case PortalObjectType.TRAFFIC_LIGHTS:
                    portalObject = new TrafficLight(objectName, points, ref modelGroup);
                    break;
                case PortalObjectType.VEHICLE_SENSORS:
                    portalObject = new VehicleSensor(objectName, points, ref modelGroup);
                    break;
                default:
                    break;
            }

            return portalObject;
        }


    }
}
