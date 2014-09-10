// <copyright file="ObjectRectangle.cs" company="Foliage">
// Copyright (c) 2013 Foliage. All rights reserved.
// </copyright>
// <date>12/31/2013</date>
// <summary>Implements the object rectangle class</summary>
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
    /// <summary>   An object rectangle. </summary>
    public class ObjectRectangle
    {
        private Point3D p0; ///< The p 0
        private Point3D p1; ///< The first p
        private Point3D p2; ///< The second p
        private Point3D p3; ///< The third p

        /// <summary>   Gets the dimensions. </summary>
        ///
        /// <returns>   The dimensions. </returns>
        public Point3D getDimensions()
        {
            Point3D result = new Point3D(0, 0, 0);

            // finds the 2 biggest dimensions of the rectangle
            result.X = biggestDifference(p0, p1);
            result.Y = biggestDifference(p1, p2);

            return result;
        }

        /// <summary>   Biggest difference. </summary>
        ///
        /// <param name="p0">   The p 0. </param>
        /// <param name="p1">   The first Point3D. </param>
        ///
        /// <returns>   A double. </returns>
        private double biggestDifference(Point3D p0, Point3D p1)
        {
            double diff1 = Math.Abs(p0.X - p1.X);
            double diff2 = Math.Abs(p0.Y - p1.Y);
            double diff3 = Math.Abs(p0.Z - p1.Z);

            return Math.Max(Math.Max(diff1, diff2), diff3);
        }

        /// <summary>   Initializes a new instance of the ObjectRectangle class. </summary>
        ///
        /// <param name="P0">   The p 0. </param>
        /// <param name="P1">   The first Point3D. </param>
        /// <param name="P2">   The second Point3D. </param>
        /// <param name="P3">   The third Point3D. </param>
        public ObjectRectangle(Point3D P0, Point3D P1, Point3D P2, Point3D P3)
        {
            p0 = P0;
            p1 = P1;
            p2 = P2;
            p3 = P3;
        }

        /// <summary>   Initializes a new instance of the ObjectRectangle class. </summary>
        ///
        /// <param name="P0">   The p 0. </param>
        /// <param name="w">    The width. </param>
        /// <param name="h">    The height. </param>
        /// <param name="d">    The double to process. </param>
        public ObjectRectangle(Point3D P0, double w, double h, double d)
        {
            p0 = P0;

            if (w != 0.0 && h != 0.0) // front / back
            {
                p1 = new Point3D(p0.X + w, p0.Y, p0.Z);
                p2 = new Point3D(p0.X + w, p0.Y - h, p0.Z);
                p3 = new Point3D(p0.X,     p0.Y - h, p0.Z);
            }
            else if (w != 0.0 && d != 0.0) // top / bottom
            {
                p1 = new Point3D(p0.X,     p0.Y, p0.Z + d);
                p2 = new Point3D(p0.X + w, p0.Y, p0.Z + d);
                p3 = new Point3D(p0.X + w, p0.Y, p0.Z);
            }
            else if (h != 0.0 && d != 0.0) // side / side
            {
                p1 = new Point3D(p0.X, p0.Y, p0.Z + d);
                p2 = new Point3D(p0.X, p0.Y - h, p0.Z + d);
                p3 = new Point3D(p0.X, p0.Y - h, p0.Z);
            }
        }

        /// <summary>   Adds to the mesh. </summary>
        ///
        /// <param name="mesh"> The mesh. </param>
        public void addToMesh(MeshGeometry3D mesh)
        {
            ObjectTriangle.addTriangleToMesh(p0, p1, p2, mesh);
            ObjectTriangle.addTriangleToMesh(p2, p3, p0, mesh);
        }

        /// <summary>   Adds a rectangle to mesh. </summary>
        ///
        /// <param name="p0">   The p 0. </param>
        /// <param name="p1">   The first Point3D. </param>
        /// <param name="p2">   The second Point3D. </param>
        /// <param name="p3">   The third Point3D. </param>
        /// <param name="mesh"> The mesh. </param>
        public static void addRectangleToMesh(Point3D p0, Point3D p1, Point3D p2, Point3D p3,
            MeshGeometry3D mesh)
        {
            ObjectTriangle.addTriangleToMesh(p0, p1, p2, mesh);
            ObjectTriangle.addTriangleToMesh(p2, p3, p0, mesh);
        }

        /// <summary>   Creates rectangle model. </summary>
        ///
        /// <param name="p0">   The p 0. </param>
        /// <param name="p1">   The first Point3D. </param>
        /// <param name="p2">   The second Point3D. </param>
        /// <param name="p3">   The third Point3D. </param>
        ///
        /// <returns>   The new rectangle model. </returns>
        public static GeometryModel3D CreateRectangleModel(Point3D p0, Point3D p1, Point3D p2, Point3D p3)
        {
            return CreateRectangleModel(p0, p1, p2, p3, false);
        }

        /// <summary>   Creates rectangle model. </summary>
        ///
        /// <param name="p0">       The p 0. </param>
        /// <param name="p1">       The first Point3D. </param>
        /// <param name="p2">       The second Point3D. </param>
        /// <param name="p3">       The third Point3D. </param>
        /// <param name="texture">  true to texture. </param>
        ///
        /// <returns>   The new rectangle model. </returns>
        public static GeometryModel3D CreateRectangleModel(Point3D p0, Point3D p1, Point3D p2, Point3D p3, bool texture)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            addRectangleToMesh(p0, p1, p2, p3, mesh);

            Material material = new DiffuseMaterial(
                new SolidColorBrush(Colors.White));

            GeometryModel3D model = new GeometryModel3D(
                mesh, material);

            if (texture)
            {
                // Create a collection of texture coordinates for the MeshGeometry3D.
                PointCollection textureCoordinatesCollection = new PointCollection();

                textureCoordinatesCollection.Add(new System.Windows.Point(0, 0));
                textureCoordinatesCollection.Add(new System.Windows.Point(1, 0));
                textureCoordinatesCollection.Add(new System.Windows.Point(1, 1));

                textureCoordinatesCollection.Add(new System.Windows.Point(1, 1)); 
                textureCoordinatesCollection.Add(new System.Windows.Point(0, 1));
                textureCoordinatesCollection.Add(new System.Windows.Point(0, 0));

                mesh.TextureCoordinates = textureCoordinatesCollection;
            }

            return model;
        }

   }
}
