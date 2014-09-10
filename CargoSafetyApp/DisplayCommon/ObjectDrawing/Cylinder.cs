// <copyright file="Cylinder.cs" company="Foliage">
// Copyright (c) 2013 Foliage. All rights reserved.
// </copyright>
// <date>12/31/2013</date>
// <summary>Implements the cylinder class</summary>
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
    /// <summary>   A cylinder. </summary>
    class Cylinder
    {
        private ObjectCircle front;	///< The front

        private ObjectCircle back; ///< The back

        private int nSides; ///< The sides

        private double frontRadius; ///< The front radius

        private double backRadius;  ///< The back radius

        private double length;  ///< The length

        private Point3D center; ///< The center

        private Point3D backcenter; ///< The backcenter

        /// <summary>   Gets the center. </summary>
        ///
        /// <returns>   The center. </returns>
        public Point3D getCenter()
        {
            return center;
        }

        /// <summary>   Initializes a new instance of the Cylinder class. </summary>
        ///
        /// <param name="Center">       The center. </param>
        /// <param name="NSides">       The sides. </param>
        /// <param name="FrontRadius">  The front radius. </param>
        /// <param name="BackRadius">   The back radius. </param>
        /// <param name="Length">       The length. </param>
        public Cylinder(Point3D Center, int NSides, double FrontRadius, double BackRadius, 
            double Length)
        {
            center = Center;
            nSides = NSides;
            frontRadius = FrontRadius;
            backRadius = BackRadius;
            length = Length;

            front = new ObjectCircle(nSides, center, frontRadius);

            backcenter = new Point3D(center.X, center.Y, center.Z - length);

            back = new ObjectCircle(nSides, backcenter, backRadius);
        }

        /// <summary>   Initializes a new instance of the Cylinder class. </summary>
        ///
        /// <param name="Center">           The center. </param>
        /// <param name="NSides">           The sides. </param>
        /// <param name="FrontRadius">      The front radius. </param>
        /// <param name="BackRadius">       The back radius. </param>
        /// <param name="Length">           The length. </param>
        /// <param name="rotation_point">   The rotation point. </param>
        /// <param name="radians">          The radians. </param>
        public Cylinder(Point3D Center, int NSides, double FrontRadius, double BackRadius,
                            double Length, Point3D rotation_point, double radians)
        {
            center = Center;
            nSides = NSides;
            frontRadius = FrontRadius;
            backRadius = BackRadius;
            length = Length;

            front = new ObjectCircle(nSides, center, frontRadius);
            backcenter = new Point3D(center.X, center.Y, center.Z - length);
            back = new ObjectCircle(nSides, backcenter, backRadius);

            RotateZY(rotation_point, radians);
        }

        /// <summary>   Rotate zy. </summary>
        ///
        /// <param name="rotation_point">   The rotation point. </param>
        /// <param name="radians">          The radians. </param>
        public void RotateZY(Point3D rotation_point, double radians)
        {
            front.RotateZY(rotation_point, radians);
            back.RotateZY(rotation_point, radians);
            backcenter = ObjectUtils.RotatePointZY(backcenter, rotation_point, radians);
        }

        /// <summary>   Rotate xz. </summary>
        ///
        /// <param name="rotation_point">   The rotation point. </param>
        /// <param name="radians">          The radians. </param>
        public void RotateXZ(Point3D rotation_point, double radians)
        {
            front.RotateXZ(rotation_point, radians);
            back.RotateXZ(rotation_point, radians);
            backcenter = ObjectUtils.RotatePointXZ(backcenter, rotation_point, radians);
        }

        /// <summary>   Rotate xy. </summary>
        ///
        /// <param name="rotation_point">   The rotation point. </param>
        /// <param name="radians">          The radians. </param>
        public void RotateXY(Point3D rotation_point, double radians)
        {
            front.RotateXY(rotation_point, radians);
            back.RotateXY(rotation_point, radians);
            backcenter = ObjectUtils.RotatePointXY(backcenter, rotation_point, radians);
        }

        /// <summary>   Adds to the mesh. </summary>
        ///
        /// <param name="mesh"> The mesh. </param>
        public void addToMesh(MeshGeometry3D mesh)
        {
            addToMesh(mesh, false, false);
        }

        /// <summary>   Adds to the mesh. </summary>
        ///
        /// <param name="mesh">             The mesh. </param>
        /// <param name="encloseTop">       true to enclose top. </param>
        /// <param name="combineVertices">  true to combine vertices. </param>
        public void addToMesh(MeshGeometry3D mesh, bool encloseTop, bool combineVertices)
        {
            if (front.getPoints().Count > 2)
            {
                List<Point3D> frontPoints = new List<Point3D>();
                foreach (Point3D p in front.getPoints())
                {
                    frontPoints.Add(p);
                }
                frontPoints.Add(front.getPoints()[0]);

                List<Point3D> backPoints = new List<Point3D>();
                foreach (Point3D p in back.getPoints())
                {
                    backPoints.Add(p);
                }
                backPoints.Add(back.getPoints()[0]);

                for (int i = 1; i < frontPoints.Count; i++)
                {
                    ObjectTriangle.addTriangleToMesh(frontPoints[i - 1], backPoints[i - 1], frontPoints[i], mesh, combineVertices);
                    ObjectTriangle.addTriangleToMesh(frontPoints[i], backPoints[i - 1], backPoints[i], mesh, combineVertices);
                }

                if (encloseTop)
                {
                    front.addToMesh(mesh, false);
                    back.addToMesh(mesh, false);
                }
            }
        }

        /// <summary>   Creates a model. </summary>
        ///
        /// <param name="color">    The color. </param>
        ///
        /// <returns>   The new model. </returns>
        public GeometryModel3D CreateModel(Color color)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            addToMesh(mesh);

            Material material = new DiffuseMaterial(new SolidColorBrush(color));

            GeometryModel3D model = new GeometryModel3D(mesh, material);

            return model;
        }

        /// <summary>   Creates a model. </summary>
        ///
        /// <param name="color">    The color. </param>
        /// <param name="enclose">  true to enclose. </param>
        /// <param name="combine">  true to combine. </param>
        ///
        /// <returns>   The new model. </returns>
        public GeometryModel3D CreateModel(Color color, bool enclose, bool combine)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            addToMesh(mesh, enclose, combine);

            Material material = new DiffuseMaterial(new SolidColorBrush(color));

            GeometryModel3D model = new GeometryModel3D(mesh, material);

            model.BackMaterial = material;

            return model;
        }



    }
}
