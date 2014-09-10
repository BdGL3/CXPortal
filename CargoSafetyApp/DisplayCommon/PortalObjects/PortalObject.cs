using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using L3.Cargo.Safety.Display.Common.ObjectDrawing;
using System.Windows.Media;
using System.Windows.Controls;

namespace L3.Cargo.Safety.Display.Common.PortalObjects
{
    public abstract class PortalObject : ObjectBehavior
    {
        public string ObjectName
        {
            get { return _objectName; }
        }

        public Point3D ObjectPoints
        {
            get { return _points; }
        }

        public GeometryModel3D ObjectGeometry
        {
            get { return _objectGeometry; }
        }

        #region Private Members

        private string _objectName = "";
        private Point3D _points;
        private Model3DGroup _modelGroup;

        // Distances to create the objects
        private static int CYLINDER_NUM_OF_SIDES = 40;
        public static double CYLINDER_FRONT_RADIUS = 0.25;
        private static double CYLINDER_BACK_RADIUS = 0.25;
        private static double CYLINDER_LENGTH = 0.25;

        private static double VEHICLE_SENSOR_CYLINDER_LENGTH = 0.50;
        private static double CURTAIN_EMITTERS_CYLYNDER_LENGTH = 15.8;
        public static double CURTAIN_EMITTERS_FRONT_RADIUS = 0.14;
        private static double CURTAIN_EMITTERS_BACK_RADIUS = 0.14;

        private static double CUBE_WIDTH = 0.25;
        private static double CUBE_HEIGHT = 0.25;
        private static double CUBE_DEPTH = 0.20;

        private static double RECTANGLE_WIDTH = 0.25;
        private static double RECTANGLE_HEIGHT = 0.25;
        private static double RECTANGLE_DEPTH = 0.50;

        protected GeometryModel3D _objectGeometry = null;

        #endregion

        public PortalObject(string objectName, Point3D points, ref Model3DGroup modelGroup)
        {
            _objectName = objectName;
            _points = points;
            _modelGroup = modelGroup;
        }

        public abstract void applyBehavior(UserControl control, string name, int value);

        protected void createCylinderModel3D()
        {
            Cylinder cylinder = new Cylinder(_points,
                                             CYLINDER_NUM_OF_SIDES,
                                             CYLINDER_FRONT_RADIUS,
                                             CYLINDER_BACK_RADIUS,
                                             CYLINDER_LENGTH);
            double degree = 90.0;
            cylinder.RotateXZ(_points, ObjectUtils.radians_from_degrees(degree));
            _objectGeometry = cylinder.CreateModel(Colors.AliceBlue, true, true);

            _modelGroup.Children.Add(_objectGeometry);
        }

        protected void createTrafficLIghtCylinderModel3D()
        {
            Cylinder cylinder = new Cylinder(_points,
                                             CYLINDER_NUM_OF_SIDES,
                                             CYLINDER_FRONT_RADIUS,
                                             CYLINDER_BACK_RADIUS,
                                             CYLINDER_LENGTH);
            double degree = 90.0;
            cylinder.RotateZY(_points, ObjectUtils.radians_from_degrees(degree));
            _objectGeometry = cylinder.CreateModel(Colors.AliceBlue, true, true);

            _modelGroup.Children.Add(_objectGeometry);
        }

        protected void createVehicleCylinderModel3D()
        {
            Cylinder cylinder = new Cylinder(_points,
                                             CYLINDER_NUM_OF_SIDES,
                                             CYLINDER_FRONT_RADIUS,
                                             CYLINDER_BACK_RADIUS,
                                             VEHICLE_SENSOR_CYLINDER_LENGTH);
            double degree = 90.0;
            cylinder.RotateZY(_points, ObjectUtils.radians_from_degrees(degree));
            _objectGeometry = cylinder.CreateModel(Colors.AliceBlue, true, true);

            _modelGroup.Children.Add(_objectGeometry);
        }

        protected void createEmittersCylinderModel3D()
        {
            Cylinder cylinder = new Cylinder(_points,
                                             CYLINDER_NUM_OF_SIDES,
                                             CURTAIN_EMITTERS_FRONT_RADIUS,
                                             CURTAIN_EMITTERS_BACK_RADIUS,
                                             CURTAIN_EMITTERS_CYLYNDER_LENGTH);
            //double degree = -270.0;
            //cylinder.RotateXZ(_points, ObjectUtils.radians_from_degrees(degree));
            // _objectGeometry = cylinder.CreateModel(Colors.AliceBlue, true, true);

            _objectGeometry = ObjectCube.CreateCubeModel(_points, RECTANGLE_WIDTH, RECTANGLE_HEIGHT, CURTAIN_EMITTERS_CYLYNDER_LENGTH, Colors.Red);
            _modelGroup.Children.Add(_objectGeometry);
        }


        protected void createCubeModel3D()
        {
            _objectGeometry = ObjectCube.CreateCubeModel(_points, CUBE_WIDTH, CUBE_HEIGHT, CUBE_DEPTH, Colors.AliceBlue);
            _modelGroup.Children.Add(_objectGeometry);
        }

        protected void createRectangleModel3D()
        {
            _objectGeometry = ObjectCube.CreateCubeModel(_points, RECTANGLE_WIDTH, RECTANGLE_HEIGHT, RECTANGLE_DEPTH, Colors.AliceBlue);
            _modelGroup.Children.Add(_objectGeometry);
        }

        protected void setObjectHidden()
        {
            _objectGeometry.BackMaterial = null;
            _objectGeometry.Material = null;
        }
    }
}
