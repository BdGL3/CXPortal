// <copyright file="WpfCircle.cs" company="Foliage">
// Copyright (c) 2013 Foliage. All rights reserved.
// </copyright>
// <date>12/31/2013</date>
// <summary>Implements the WPF circle class</summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using System.Windows.Shapes;


namespace L3.Cargo.Safety.Display.Common.ObjectDrawing
{
    /// <summary>   An object with a circle. </summary>
    class ObjectCircle
    {
        private int nSides = 6; ///< The sides

        private Point3D top;	///< The top

        private double angle;   ///< The angle

        private Point3D center; ///< The center

        private List<Point3D> points;   ///< The points

        private double radiusY; ///< The radius y coordinate
        private double radiusX; ///< The radius x coordinate

        /// <summary>   Gets the center. </summary>
        ///
        /// <returns>   The center. </returns>
        public Point3D getCenter()
        {
            return center;
        }

        /// <summary>   Gets the points. </summary>
        ///
        /// <returns>   The points. </returns>
        public List<Point3D> getPoints()
        {
            return points;
        }

        /// <summary>   Initializes a new instance of the WpfCircle class. </summary>
        ///
        /// <param name="NSides">   The sides. </param>
        /// <param name="Center">   The center. </param>
        /// <param name="Radius">   The radius. </param>
        public ObjectCircle(int NSides, Point3D Center, double Radius)
        {
            nSides = NSides;

            angle = (double)360.0 / (double)nSides;

            center = new Point3D(Center.X, Center.Y, Center.Z);

            radiusY = Radius;
            radiusX = Radius;

            makeCircle();
        }

        /// <summary>   Initializes a new instance of the WpfCircle class. </summary>
        ///
        /// <param name="NSides">   The sides. </param>
        /// <param name="Center">   The center. </param>
        /// <param name="RadiusY">  The radius y coordinate. </param>
        /// <param name="RadiusX">  The radius x coordinate. </param>
        public ObjectCircle(int NSides, Point3D Center, double RadiusY, double RadiusX)
        {
            nSides = NSides;

            angle = (double)360.0 / (double)nSides;

            center = new Point3D(Center.X, Center.Y, Center.Z);

            radiusY = RadiusY;
            radiusX = RadiusX;

            makeCircle();
        }

        /// <summary>   Makes the circle. </summary>
        private void makeCircle()
        {
            points = new List<Point3D>();

            top = new Point3D(center.X, center.Y + radiusY, center.Z);
            points.Add(top);

            for (int i = 1; i < nSides; i++)
            {
                Point3D p = ObjectUtils.RotatePointXY(top, center, ObjectUtils.radians_from_degrees(angle * i));

                if (radiusX != radiusY)
                {
                    double diff = p.X - center.X;
                    diff *= radiusX;
                    diff /= radiusY;
                    p = new Point3D(center.X + diff, p.Y, p.Z);
                }

                points.Add(p);
            }
        }

        /// <summary>   Reverse points. </summary>
        public void reversePoints()
        {
            points.Reverse();
        }

        /// <summary>   Rotate zy. </summary>
        ///
        /// <param name="rotation_point">   The rotation point. </param>
        /// <param name="radians">          The radians. </param>
        public void RotateZY(Point3D rotation_point, double radians)
        {
            List<Point3D> newlist = new List<Point3D>();

            foreach (Point3D p in points)
            {
                newlist.Add(ObjectUtils.RotatePointZY(p, rotation_point, radians));
            }

            center = ObjectUtils.RotatePointZY(center, rotation_point, radians);

            points = newlist;
        }

        /// <summary>   Rotate xy. </summary>
        ///
        /// <param name="rotation_point">   The rotation point. </param>
        /// <param name="radians">          The radians. </param>
        public void RotateXY(Point3D rotation_point, double radians)
        {
            List<Point3D> newlist = new List<Point3D>();

            foreach (Point3D p in points)
            {
                newlist.Add(ObjectUtils.RotatePointXY(p, rotation_point, radians));
            }

            center = ObjectUtils.RotatePointXY(center, rotation_point, radians);

            points = newlist;
        }

        /// <summary>   Rotate xz. </summary>
        ///
        /// <param name="rotation_point">   The rotation point. </param>
        /// <param name="radians">          The radians. </param>
        public void RotateXZ(Point3D rotation_point, double radians)
        {
            List<Point3D> newlist = new List<Point3D>();

            foreach (Point3D p in points)
            {
                newlist.Add(ObjectUtils.RotatePointXZ(p, rotation_point, radians));
            }

            center = ObjectUtils.RotatePointXZ(center, rotation_point, radians);

            points = newlist;
        }

        /// <summary>   Adds to the mesh. </summary>
        ///
        /// <param name="mesh">             The mesh. </param>
        /// <param name="combineVertices">  true to combine vertices. </param>
        public void addToMesh(MeshGeometry3D mesh, bool combineVertices)
        {
            if (points.Count > 2)
            {
                List<Point3D> temp = new List<Point3D>();

                foreach (Point3D p in points)
                {
                    temp.Add(p);
                }

                temp.Add(points[0]);

                for (int i = 1; i < temp.Count; i++)
                {
                    ObjectTriangle.addTriangleToMesh(temp[i], center, temp[i - 1], mesh, combineVertices);
                }
                
            }
        }

        /// <summary>   Creates a model. </summary>
        ///
        /// <param name="color">            The color. </param>
        /// <param name="combineVertices">  true to combine vertices. </param>
        ///
        /// <returns>   The new model. </returns>
        public GeometryModel3D createModel(Color color, bool combineVertices)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            addToMesh(mesh, combineVertices);

            Material material = new DiffuseMaterial(
                new SolidColorBrush(color));

            GeometryModel3D model = new GeometryModel3D(mesh, material);

            return model;
        }

        /// <summary>   Creates model two sided. </summary>
        ///
        /// <param name="color">            The color. </param>
        /// <param name="combineVertices">  true to combine vertices. </param>
        ///
        /// <returns>   The new model two sided. </returns>
        public GeometryModel3D createModelTwoSided(Color color, bool combineVertices)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            addToMesh(mesh, combineVertices);

            Material material = new DiffuseMaterial(
                new SolidColorBrush(color));

            GeometryModel3D model = new GeometryModel3D(mesh, material);

            model.BackMaterial = material;

            return model;
        }

        /// <summary>   Creates circle model. </summary>
        ///
        /// <param name="NSides">   The sides. </param>
        /// <param name="Center">   The center. </param>
        /// <param name="Diameter"> The diameter. </param>
        ///
        /// <returns>   The new circle model. </returns>
        public static GeometryModel3D CreateCircleModel(int NSides, Point3D Center, double Diameter)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            ObjectCircle circle = new ObjectCircle(NSides, Center, Diameter);

            circle.addToMesh(mesh, false);

            Material material = new DiffuseMaterial(
                new SolidColorBrush(Colors.White));

            GeometryModel3D model = new GeometryModel3D(mesh, material);

            return model;
        }



    }
}
