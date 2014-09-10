using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace L3.Cargo.Workstation.Plugins.XRayImageBase.Common
{
    public class SourceObject : IDisposable
    {
        #region Private Members

        private BitmapSource m_BitmapSource;

        private float[] m_Data;

        private float[] m_AlphaData;

        private int m_Width;

        private int m_Height;

        #endregion Private Members


        #region Public Members

        public float[] Data
        {
            get
            {
                return m_Data;
            }
        }

        public float[] AlphaData
        {
            get
            {
                return m_AlphaData;
            }
        }

        public int Width
        {
            get
            {
                return m_Width;
            }
        }

        public int Height
        {
            get
            {
                return m_Height;
            }
        }

        public BitmapSource Source
        {
            get
            {
                return m_BitmapSource;
            }
        }

        #endregion Public Members


        #region Constructors

        public SourceObject (float[] data, int width, int height, bool flipX, bool flipY)
        {
            m_Width = width;
            m_Height = height;

            if (flipX)
            {
                float[] tempData = new float[data.Length];
                Parallel.For(0, height, i =>
                {
                    Parallel.For(0, width, j =>
                    {
                        tempData[i * width + j] = data[i * width + ((width - j) - 1)];
                    });
                });
                data = tempData;
            }

            if (flipY)
            {
                float[] tempData = new float[data.Length];
                Parallel.For(0, height, i =>
                {
                    Parallel.For(0, width, j =>
                    {
                        tempData[i * width + j] = data[(width * height) - (i * width + ((width - j)))];
                    });
                });
                data = tempData;
            }


            PixelFormat pixelFormat = PixelFormats.Bgr24;

            int pixelOffset = pixelFormat.BitsPerPixel / 8;
            int stride = width * pixelOffset;

            byte[] newData = new byte[pixelOffset * data.Length];

            Parallel.For(0, data.Length, i =>
            {
                UInt32 value = (UInt32)(data[i] * 16777216.0);

                newData[i * pixelOffset + 0] = (byte)(Math.Pow((float)((value << 8) >> 24) / 256F, 1 / 2.3) * 256);
                newData[i * pixelOffset + 1] = (byte)(Math.Pow((float)((value << 16) >> 24) / 256F, 1 / 2.3) * 256);
                newData[i * pixelOffset + 2] = (byte)(Math.Pow((float)((value << 24) >> 24) / 256F, 1 / 2.3) * 256);
            });

            m_Data = data;
            m_BitmapSource = BitmapSource.Create(width, height, 96.0, 96.0, pixelFormat, null, newData, stride);
        }

        public SourceObject (float[] compData, float[] alphaData, int width, int height, bool flipX, bool flipY)
        {
            m_Width = width;
            m_Height = height;

            if (flipX)
            {
                float[] tempCompData = new float[compData.Length];
                float[] tempAlphaData = new float[alphaData.Length];

                Parallel.For(0, height, i =>
                {
                    Parallel.For(0, width, j =>
                    {
                        tempCompData[i * width + j] = compData[i * width + ((width - j) - 1)];
                        tempAlphaData[i * width + j] = alphaData[i * width + ((width - j) - 1)];
                    });
                });

                compData = tempCompData;
                alphaData = tempAlphaData;
            }

            if (flipY)
            {
                float[] tempCompData = new float[compData.Length];
                float[] tempAlphaData = new float[alphaData.Length];

                Parallel.For(0, height, i =>
                {
                    Parallel.For(0, width, j =>
                    {
                        tempCompData[i * width + j] = compData[(width * height) - (i * width + ((width - j)))];
                        tempAlphaData[i * width + j] = alphaData[(width * height) - (i * width + ((width - j)))];
                    });
                });

                compData = tempCompData;
                alphaData = tempAlphaData;
            }


            PixelFormat pixelFormat = PixelFormats.Bgr24;

            int pixelOffset = pixelFormat.BitsPerPixel / 8;
            int stride = width * pixelOffset;

            byte[] newData = new byte[pixelOffset * compData.Length];

            Parallel.For(0, compData.Length, i =>
            {
                UInt32 compValue = (UInt32)(compData[i] * 256.0);

                newData[i * pixelOffset + 0] = (byte)(Math.Pow((float)((compValue << 24) >> 24) / 256F, 1 / 2.3) * 256);

                UInt32 alphaValue = (UInt32)(alphaData[i] * 256.0);

                newData[i * pixelOffset + 1] = (byte)(Math.Pow((float)((alphaValue << 24) >> 24) / 256F, 1 / 2.3) * 256);
            });

            m_Data = compData;
            m_AlphaData = alphaData;
            m_BitmapSource = BitmapSource.Create(width, height, 96.0, 96.0, pixelFormat, null, newData, stride);
        }

        #endregion Constructors


        #region Public Methods

        public void Dispose ()
        {
            m_BitmapSource = null;
            m_Data = null;
        }

        #endregion Public Methods
    }
}
