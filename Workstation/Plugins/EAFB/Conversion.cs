using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace L3.Cargo.Workstation.Plugins.EAFB
{
    static public class Conversion
    {
        public const double EndAngle = 26;
        public const double StartAngle = 90;
        public const double CC = 3.76;//meters
        
        static public float SamplingSpace = 0;

        static public Point ToCartesianPoint(double theta1, double zval, double ht)
        {
            Point reversept = new Point();
            if (SamplingSpace == 0) SamplingSpace = 4;//default
            reversept.X = zval / SamplingSpace;
            reversept.Y = ht - ((StartAngle - theta1) * ht) / (StartAngle - EndAngle);
            return reversept;
        }

        static public double LengthToPixels(double Length_mm)
        {
            return Length_mm / SamplingSpace;
        }

        static public double LengthToMillimeters(double Length_pixels)
        {
            return Length_pixels * SamplingSpace;
        }

        static public double ConvertY2Theta(double Y, double ht)
        {
            double result;

            if (ht == 0)
                return 0;
            else
            {
                double tmp = (StartAngle - EndAngle);
                result = (StartAngle - ((ht - Y) * tmp) / ht);
                return result;
            }
        }
        static public Point XYcalculation(Point T1T2AnglesPoint)
        {
            Point XYPoint = new Point(); ;

            //everything must be in mm
            //double res = resolution;
            //if (res == 0) res = 4;//to be sure
            double mm_CC = CC * 1000;
            double tmp = (Math.Tan(T1T2AnglesPoint.Y) + 1 / Math.Tan(T1T2AnglesPoint.X));
            XYPoint.X = mm_CC / tmp;
            XYPoint.Y = mm_CC * (1 / (Math.Tan(T1T2AnglesPoint.X))) / tmp;

            return XYPoint;
        }        

        static public double ConvertDegreesToRadians(double degrees)
        {
            double radians = (Math.PI / 180) * degrees;
            return (radians);
        }

        static public double ConvertRadiansToDegrees(double degrees)
        {
            return 0;
        }
        static public float MaxValue(float[] intArray)
        {
            float maxVal = -1000000;
            for (int i = 0; i < intArray.Length; i++)
            {
                if (intArray[i] > maxVal)
                    maxVal = intArray[i];
            }
            return maxVal;
        }
        static public float MinValue(float[] intArray)
        {
            float minVal = 1000000;
            for (int i = 0; i < intArray.Length; i++)
            {
                if (intArray[i] < minVal)
                    minVal = intArray[i];
            }
            return minVal;
        }
     
    }
}
