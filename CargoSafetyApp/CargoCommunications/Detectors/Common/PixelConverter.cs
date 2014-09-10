using System;

namespace L3.Cargo.Communications.Detectors.Common
{
    public static class PixelConverter
    {
        public static float[] Convert(Pixel[] sourceArray)
        {
            return Array.ConvertAll(sourceArray, new Converter<Pixel, float>(PixelToFloat));
        }

        public static Pixel[] Convert(float[] sourceArray)
        {
            return Array.ConvertAll(sourceArray, new Converter<float, Pixel>(FloatToPixel));
        }

        public static Pixel BytesToPixel(byte[] bytes, int startIndex, int bytesPerPixel)
        {
            Pixel pxl = new Pixel();

            for (int index = 0; index < bytesPerPixel; index++)
            {
                pxl.Value = pxl.Value | (uint)(bytes[startIndex++] << (index * 8));
            }

            return pxl;
        }

        public static void PixelToBytes(Pixel[] src, int srcIndex, ref byte[] bytes, ref int destIndex, int bytesPerPixel)
        {
            for (int index = 0; index < bytesPerPixel; index++)
            {
                bytes[destIndex++] = (byte)(src[srcIndex].Value >> (index * 8));
            }
        }

        private static float PixelToFloat(Pixel pxl)
        {
            return (float)pxl.Value;
        }

        private static Pixel FloatToPixel(float flt)
        {
            return new Pixel(flt);
        }
    }
}
