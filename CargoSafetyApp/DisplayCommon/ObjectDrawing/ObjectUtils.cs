// <copyright file="ObjectUtils.cs" company="Foliage">
// Copyright (c) 2013 Foliage. All rights reserved.
// </copyright>
// <date>12/31/2013</date>
// <summary>Implements the object utilities class</summary>
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;



using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using System.Windows.Shapes;



namespace L3.Cargo.Safety.Display.Common.ObjectDrawing
{
    /// <summary>   An object utilities. </summary>
    class ObjectUtils
    {

        static double one_rad_in_degrees = (double)57.0 + ((double)17.0 / (double)60.0) + ((double)44.6 / ((double)3600.0));	///< The one radians in degrees

        /// <summary>   Rotate point xy. </summary>
        ///
        /// <param name="p">                The Point3D to process. </param>
        /// <param name="rotation_point">   The rotation point. </param>
        /// <param name="radians">          The radians. </param>
        ///
        /// <returns>   A Point3D. </returns>
        public static Point3D RotatePointXY(Point3D p, Point3D rotation_point, double radians)
        {
            Point3D new_point = new Point3D(rotation_point.X, rotation_point.Y, rotation_point.Z);

            try
            {
                if (radians != 0)
                {
                    double ydiff = p.Y - rotation_point.Y;
                    double xdiff = p.X - rotation_point.X;

                    double xd = (xdiff * Math.Cos(radians)) - (ydiff * Math.Sin(radians));

                    double yd = (xdiff * Math.Sin(radians)) + (ydiff * Math.Cos(radians));

                    new_point.X += xd;
                    new_point.Y += yd;
                    new_point.Z = p.Z;
                }
                else
                {
                    new_point.X = p.X;
                    new_point.Y = p.Y;
                    new_point.Z = p.Z;
                }
            }
            catch
            {
                
            }

            return new_point;
        }

        /// <summary>   Rotate point xz. </summary>
        ///
        /// <param name="p">                The Point3D to process. </param>
        /// <param name="rotation_point">   The rotation point. </param>
        /// <param name="radians">          The radians. </param>
        ///
        /// <returns>   A Point3D. </returns>
        public static Point3D RotatePointXZ(Point3D p, Point3D rotation_point, double radians)
        {
            Point3D new_point = new Point3D(rotation_point.X, rotation_point.Y, rotation_point.Z);

            try
            {
                if (radians != 0)
                {
                    double ydiff = p.Z - rotation_point.Z;
                    double xdiff = p.X - rotation_point.X;

                    double xd = (xdiff * Math.Cos(radians)) - (ydiff * Math.Sin(radians));

                    double yd = (xdiff * Math.Sin(radians)) + (ydiff * Math.Cos(radians));

                    new_point.X += xd;
                    new_point.Z += yd;
                    new_point.Y = p.Y;
                }
                else
                {
                    new_point.X = p.X;
                    new_point.Y = p.Y;
                    new_point.Z = p.Z;
                }
            }
            catch
            {
                
            }

            return new_point;
        }

        /// <summary>   Rotate point zy. </summary>
        ///
        /// <param name="p">                The Point3D to process. </param>
        /// <param name="rotation_point">   The rotation point. </param>
        /// <param name="radians">          The radians. </param>
        ///
        /// <returns>   A Point3D. </returns>
        public static Point3D RotatePointZY(Point3D p, Point3D rotation_point, double radians)
        {
            Point3D new_point = new Point3D(rotation_point.X, rotation_point.Y, rotation_point.Z);

            try
            {
                if (radians != 0)
                {
                    double ydiff = p.Y - rotation_point.Y;
                    double xdiff = p.Z - rotation_point.Z;

                    double xd = (xdiff * Math.Cos(radians)) - (ydiff * Math.Sin(radians));
                    double yd = (xdiff * Math.Sin(radians)) + (ydiff * Math.Cos(radians));

                    new_point.Z += xd;
                    new_point.Y += yd;
                    new_point.X = p.X;
                }
                else
                {
                    new_point.X = p.X;
                    new_point.Y = p.Y;
                    new_point.Z = p.Z;
                }
            }
            catch
            {
                
            }

            return new_point;
        }

        /// <summary>   Radians from degrees. </summary>
        ///
        /// <param name="degrees">  The degrees. </param>
        ///
        /// <returns>   A double. </returns>
        public static double radians_from_degrees(double degrees)
        {
            return degrees / one_rad_in_degrees;
        }

        /// <summary>   Degrees from radians. </summary>
        ///
        /// <param name="radians">  The radians. </param>
        ///
        /// <returns>   A double. </returns>
        public static double degrees_from_radians(double radians)
        {
            return radians * one_rad_in_degrees;
        }


    }
}
