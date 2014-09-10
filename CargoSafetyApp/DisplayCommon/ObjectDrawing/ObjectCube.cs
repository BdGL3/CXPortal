// <copyright file="ObjectCube.cs" company="Foliage">
// Copyright (c) 2013 Foliage. All rights reserved.
// </copyright>
// <date>12/31/2013</date>
// <summary>Implements the object cube class</summary>
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
    /// <summary>   An object cube. </summary>
    public class ObjectCube
    {
        private Point3D origin; ///< The origin
        private double width;   ///< The width
        private double height;  ///< The height
        private double depth;   ///< The depth

        /// <summary>   Center bottom. </summary>
        ///
        /// <returns>   A Point3D. </returns>
        public Point3D centerBottom()
        {
            Point3D c = new Point3D(
                origin.X + (width / 2),
                origin.Y + height,
                origin.Z + (depth / 2)
                );

            return c;
        }

        /// <summary>   Center top. </summary>
        ///
        /// <returns>   A Point3D. </returns>
        public Point3D centerTop()
        {
            Point3D c = new Point3D(
                origin.X + (width / 2),
                origin.Y,
                origin.Z + (depth / 2)
                );

            return c;
        }

        /// <summary>   Initializes a new instance of the ObjectCube class. </summary>
        ///
        /// <param name="P0">   The p 0. </param>
        /// <param name="w">    The width. </param>
        /// <param name="h">    The height. </param>
        /// <param name="d">    The double to process. </param>
        public ObjectCube(Point3D P0, double w, double h, double d)
        {
            width = w;
            height = h;
            depth = d;

            origin = P0;
        }

        /// <summary>   Initializes a new instance of the ObjectCube class. </summary>
        ///
        /// <param name="cube"> The cube. </param>
        public ObjectCube(ObjectCube cube)
        {
            width = cube.width;
            height = cube.height;
            depth = cube.depth;

            origin = new Point3D(cube.origin.X, cube.origin.Y, cube.origin.Z);
        }

        /// <summary>   Gets the front. </summary>
        ///
        /// <returns>   An ObjectRectangle. </returns>
        public ObjectRectangle Front()
        {
            ObjectRectangle r = new ObjectRectangle(origin, width, height, 0);

            return r;
        }

        /// <summary>   Gets the back. </summary>
        ///
        /// <returns>   An ObjectRectangle. </returns>
        public ObjectRectangle Back()
        {
            ObjectRectangle r = new ObjectRectangle(new Point3D(origin.X + width, origin.Y, origin.Z + depth), -width, height, 0);

            return r;
        }

        /// <summary>   Gets the left. </summary>
        ///
        /// <returns>   An ObjectRectangle. </returns>
        public ObjectRectangle Left()
        {
            ObjectRectangle r = new ObjectRectangle(new Point3D(origin.X, origin.Y, origin.Z + depth), 
                0, height, -depth);

            return r;
        }

        /// <summary>   Gets the right. </summary>
        ///
        /// <returns>   An ObjectRectangle. </returns>
        public ObjectRectangle Right()
        {
            ObjectRectangle r = new ObjectRectangle(new Point3D(origin.X + width, origin.Y, origin.Z),
                0, height, depth);

            return r;
        }

        /// <summary>   Gets the top. </summary>
        ///
        /// <returns>   An ObjectRectangle. </returns>
        public ObjectRectangle Top()
        {
            ObjectRectangle r = new ObjectRectangle(origin, width, 0, depth);

            return r;
        }

        /// <summary>   Gets the bottom. </summary>
        ///
        /// <returns>   An ObjectRectangle. </returns>
        public ObjectRectangle Bottom()
        {
            ObjectRectangle r = new ObjectRectangle(new Point3D(origin.X + width, origin.Y - height, origin.Z), 
                -width, 0, depth);

            return r;
        }

        /// <summary>   Adds a cube to mesh. </summary>
        ///
        /// <param name="p0">           The p 0. </param>
        /// <param name="w">            The width. </param>
        /// <param name="h">            The height. </param>
        /// <param name="d">            The double to process. </param>
        /// <param name="mesh">         The mesh. </param>
        /// <param name="useTexture">   true to use texture. </param>
        public static void addCubeToMesh(Point3D p0, double w, double h, double d,
            MeshGeometry3D mesh, bool useTexture)
        {
            ObjectCube cube = new ObjectCube(p0, w, h, d);

            double maxDimension = Math.Max(d, Math.Max(w, h));

            PointCollection textureCoordinatesCollection = new PointCollection();

            ObjectRectangle front = cube.Front();
            ObjectRectangle back = cube.Back();
            ObjectRectangle right = cube.Right();
            ObjectRectangle left = cube.Left();
            ObjectRectangle top = cube.Top();
            ObjectRectangle bottom = cube.Bottom();

            if (useTexture)
            {
                Point3D extents = front.getDimensions();
                
                addTextureCoordinates(textureCoordinatesCollection, extents.X / maxDimension,
                                         extents.Y / maxDimension);
                extents = back.getDimensions();
                addTextureCoordinates(textureCoordinatesCollection, extents.X / maxDimension,
                                         extents.Y / maxDimension);
                extents = right.getDimensions();
                addTextureCoordinates(textureCoordinatesCollection, extents.X / maxDimension,
                                         extents.Y / maxDimension);
                extents = left.getDimensions();
                addTextureCoordinates(textureCoordinatesCollection, extents.X / maxDimension,
                                         extents.Y / maxDimension);
                extents = top.getDimensions();
                addTextureCoordinates(textureCoordinatesCollection, extents.X / maxDimension,
                                         extents.Y / maxDimension);
                extents = bottom.getDimensions();
                addTextureCoordinates(textureCoordinatesCollection, extents.X / maxDimension,
                                         extents.Y / maxDimension);
            }

            front.addToMesh(mesh);
            back.addToMesh(mesh);
            right.addToMesh(mesh);
            left.addToMesh(mesh);
            top.addToMesh(mesh);
            bottom.addToMesh(mesh);

            if (useTexture)
            {
                mesh.TextureCoordinates = textureCoordinatesCollection;
            }

        }

        /// <summary>   Adds a texture coordinates. </summary>
        ///
        /// <param name="textureCoordinatesCollection"> Collection of texture coordinates. </param>
        /// <param name="xFactor">                      The factor. </param>
        /// <param name="yFactor">                      The factor. </param>
        private static void addTextureCoordinates(PointCollection textureCoordinatesCollection,
                            double xFactor, double yFactor)
        {
            textureCoordinatesCollection.Add(new System.Windows.Point(0, 0));
            textureCoordinatesCollection.Add(new System.Windows.Point(xFactor, 0));
            textureCoordinatesCollection.Add(new System.Windows.Point(xFactor, yFactor));

            textureCoordinatesCollection.Add(new System.Windows.Point(xFactor, yFactor));
            textureCoordinatesCollection.Add(new System.Windows.Point(0, yFactor));
            textureCoordinatesCollection.Add(new System.Windows.Point(0, 0));
        }

        /// <summary>   Creates cube model. </summary>
        ///
        /// <param name="p0">       The p 0. </param>
        /// <param name="w">        The width. </param>
        /// <param name="h">        The height. </param>
        /// <param name="d">        The double to process. </param>
        /// <param name="color">    The color. </param>
        ///
        /// <returns>   The new cube model. </returns>
        public static GeometryModel3D CreateCubeModel(Point3D p0, double w, double h, double d, Color color)
        {
            return CreateCubeModel(p0, w, h, d, color, false);
        }

        /// <summary>   Creates cube model. </summary>
        ///
        /// <param name="p0">           The p 0. </param>
        /// <param name="w">            The width. </param>
        /// <param name="h">            The height. </param>
        /// <param name="d">            The double to process. </param>
        /// <param name="color">        The color. </param>
        /// <param name="useTexture">   true to use texture. </param>
        ///
        /// <returns>   The new cube model. </returns>
        public static GeometryModel3D CreateCubeModel(Point3D p0, double w, double h, double d, Color color, bool useTexture)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            addCubeToMesh(p0, w, h, d, mesh, useTexture);

            Material material = new DiffuseMaterial(new SolidColorBrush(color));

            GeometryModel3D model = new GeometryModel3D(mesh, material);

            return model;
        }


    }
}
