using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace L3.Cargo.Safety.Display.Common.PortalObjects
{
    public class PortalModelFactory
    {
        private Dictionary<string, PortalObject> _modelObjects = new Dictionary<string, PortalObject>();

        private Model3DGroup _modelGroup = null;

        private List<Tuple<string, Point3D>> _estops =
            new List<Tuple<string, Point3D>>()
            {
                Tuple.Create<string, Point3D>(OpcTags.ESTOP_TUNNEL_EXIT_LEFT.Name, new Point3D(1.8, 1.5, 7.6)),
                Tuple.Create<string, Point3D>(OpcTags.ESTOP_TUNNEL_ENTRY_LEFT.Name, new Point3D(1.8, 1.5, -8.0)),
                Tuple.Create<string, Point3D>(OpcTags.ESTOP_TUNNEL_EXIT_RIGHT.Name, new Point3D(-1.8, 1.5, 7.6)),
                Tuple.Create<string, Point3D>(OpcTags.ESTOP_TUNNEL_ENTRY_RIGHT.Name, new Point3D(-1.8, 1.5, -8.0)),
                Tuple.Create<string, Point3D>(OpcTags.ESTOP_XRAY_SOURCE_AREA.Name, new Point3D(9.543624, 5.4, 9.54)),
                Tuple.Create<string, Point3D>(OpcTags.ESTOP_OPERATORS_CONSOLE.Name, new Point3D(2.431624, 1.6, 6.636617 + PortalObject.CYLINDER_FRONT_RADIUS)),
            };

        private List<Tuple<string, Point3D>> _lightCurtainEmitters =
            new List<Tuple<string, Point3D>>()
            {
                Tuple.Create<string, Point3D>(OpcTags.INTERLOCK_LC1.Name, new Point3D(-1.8, 0.6, -8.3)),
                Tuple.Create<string, Point3D>(OpcTags.INTERLOCK_LC2.Name, new Point3D(1.8, 0.6, -8.3))
            };

        private List<Tuple<string, Point3D>> _motionDetectors =
            new List<Tuple<string, Point3D>>()
            {
                Tuple.Create<string, Point3D>(OpcTags.INTERLOCK_MDS_PERSONNEL_SAFETY_1.Name, new Point3D(0, 1.2, -5.6)),
                Tuple.Create<string, Point3D>(OpcTags.INTERLOCK_MDS_PERSONNEL_SAFETY_2.Name, new Point3D(0, 1.2, 6.6)),
                Tuple.Create<string, Point3D>(OpcTags.IN_MDS3_VEHICLE_MOTION_DET.Name, new Point3D(0, 1.2, 0)),
                Tuple.Create<string, Point3D>(OpcTags.IN_MDS4_VEHICLE_MOTION_DET.Name, new Point3D(0, 1.2, 7.6))
            };

        private List<Tuple<string, Point3D>> _trafficLights =
            new List<Tuple<string, Point3D>>()
            {
                Tuple.Create<string, Point3D>(OpcTags.TRAFFIC_LIGHT_STATUS.Name, new Point3D(2.7,2.7,-8.5))
            };

        private List<Tuple<string, Point3D>> _vehicleSensors =
            new List<Tuple<string, Point3D>>()
            {
                Tuple.Create<string, Point3D>(OpcTags.VEHICLE_SENSOR_AT_GATE_LEFT.Name, new Point3D(1.8, 1.2, -9.0)),
                Tuple.Create<string, Point3D>(OpcTags.VEHICLE_SENSOR_AT_GATE_RIGHT.Name, new Point3D(-1.8, 1.2, -9.0)),
                Tuple.Create<string, Point3D>(OpcTags.VEHICLE_SENSOR_AFTER_XRAY_LEFT.Name, new Point3D(1.8, 1.2, 1.1)),
                Tuple.Create<string, Point3D>(OpcTags.VEHICLE_SENSOR_AFTER_XRAY_RIGHT.Name, new Point3D(-1.8, 1.2, 1.1)),
                Tuple.Create<string, Point3D>(OpcTags.VEHICLE_SENSOR_BEFORE_XRAY_LEFT.Name, new Point3D(1.8, 1.2, -1.2)),
                Tuple.Create<string, Point3D>(OpcTags.VEHICLE_SENSOR_BEFORE_XRAY_RIGHT.Name, new Point3D(-1.8, 1.2, -1.2))
            };

        public PortalModelFactory(ref Model3DGroup modelGroup)
        {
            _modelGroup = modelGroup;

            createEStops();
            createLightCurtainEmitters();
            createMotionDetectors();
            createTrafficLights();
            createVehicleSensors();
        }

        public Dictionary<string, PortalObject> getPortalModelObjects()
        {
            return _modelObjects;
        }

        private void createEStops()
        {
            foreach (Tuple<string, Point3D> estop in _estops)
            {
                _modelObjects.Add(estop.Item1, PortalObjectFactory.createPortalObject(PortalObjectType.E_STOP,
                                                                                      estop.Item1,
                                                                                      estop.Item2,
                                                                                      ref _modelGroup));
            }

        }

        private void createLightCurtainEmitters()
        {
            foreach (Tuple<string, Point3D> lce in _lightCurtainEmitters)
            {
                _modelObjects.Add(lce.Item1, PortalObjectFactory.createPortalObject(PortalObjectType.LIGHT_CURTAIN_INTERLOCKS,
                                                                                      lce.Item1,
                                                                                      lce.Item2,
                                                                                      ref _modelGroup));
            }
        }

        private void createMotionDetectors()
        {
            foreach (Tuple<string, Point3D> motionDetectors in _motionDetectors)
            {
                _modelObjects.Add(motionDetectors.Item1, PortalObjectFactory.createPortalObject(PortalObjectType.MOTION_DETECTOR_SENSORS,
                                                                                      motionDetectors.Item1,
                                                                                      motionDetectors.Item2,
                                                                                      ref _modelGroup));
            }
        }

        private void createTrafficLights()
        {
            foreach (Tuple<string, Point3D> trafficLights in _trafficLights)
            {
                _modelObjects.Add(trafficLights.Item1, PortalObjectFactory.createPortalObject(PortalObjectType.TRAFFIC_LIGHTS,
                                                                                      trafficLights.Item1,
                                                                                      trafficLights.Item2,
                                                                                      ref _modelGroup));
            }
        }

        private void createVehicleSensors()
        {
            foreach (Tuple<string, Point3D> vehicleSensors in _vehicleSensors)
            {
                _modelObjects.Add(vehicleSensors.Item1, PortalObjectFactory.createPortalObject(PortalObjectType.VEHICLE_SENSORS,
                                                                                      vehicleSensors.Item1,
                                                                                      vehicleSensors.Item2,
                                                                                      ref _modelGroup));
            }
        }
    }
}
