using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace L3.Cargo.Communications.Common
{
    internal class Thumbnail
    {
        /// <summary>
        /// Defined for use as image buffer indices with GetDataBuffer()
        private enum ImageBufferIndices
        {
            Unknown = 100,
            RawH = 0,
            RawL = 1,
            RawH1 = 2,
            RawL1 = 3,
            FinalH = 5,
            FinalL = 6,
            FinalH1 = 7,
            FinalL1 = 8,
            FinalAlpha = 16,
            FinalAlpha1 = 17,
            FinalCompHL = 18,
            FinalCompHL1 = 19
        };
        /// </summary>

        /// <summary>
        /// Defined for use as image view with GetDataBuffer()
        private enum ImageViewIndices
        {
            Unknown = 0,
            View1 = 1,
            View2 = 2
        };
        /// </summary>

        [StructLayout(LayoutKind.Sequential)]
        public struct PXEViewInfo
        {
            public uint isValidView;
            public uint isDualEnergy;
            public uint isHighEnergy; //not valid if isDualEnergy is non-zero
            public uint width;
            public uint actualWidth;
            public uint height;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct PXEHeader
        {
            public uint isValidFile;
            public uint pxeIndex; // system type
            public uint bitsPerPixel;
            public uint viewCount;
            public uint algSuccess;
            public uint detectorsPerBoard;
            public uint samplingSpeed; // in mm/sec
            public float samplingSpace; // in mm
            public uint batchNumber;
            public uint sequenceNum;
            public PXEViewInfo viewBuffer_0;
            public PXEViewInfo viewBuffer_1;
        };

        private PXEHeader pxeHeader;

        private float[] m_rawData;

        [DllImport("AlgClientWrap.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern PXEHeader OpenPXEImageFromMemory(byte[] pxeData, int memSize);

        [DllImport("AlgClientWrap.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetDataBuffer(uint index, // convert enumeration ImageBufferIndices to int
                                    int view,               // convert enumeration ImageViewIndices1 to int
                                    float[] EngBuffer,      // buffer to recieve the x-ray data
                                    bool bNormalize);       // true to normalize

        [DllImport("AlgClientWrap.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool ClearActiveBagBuffer();

        [DllImport("AlgClientWrap.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CloseActiveBag();

        [DllImport("AlgClientWrap.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UnInit();

        public Thumbnail()
        {
        }

        public bool CreateJPEGFromFile(FileInfo pxeFile)
        {
            bool Ret = false;
            
            try
            {
                using (FileStream fs = new FileStream(pxeFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byte[] pxeData = new byte[pxeFile.Length];

                    using (BinaryReader r = new BinaryReader(fs))
                    {
                        // Read data from Test.data.
                        pxeData = r.ReadBytes((int)pxeFile.Length);
                    }

                    pxeHeader = OpenPXEImageFromMemory(pxeData, (int)pxeData.Length);

                    if (pxeHeader.isValidFile != 1)
                    {
                        ClearActiveBagBuffer();
                        return false;
                    }

                    ImageBufferIndices ibi = ImageBufferIndices.Unknown;
                    ImageViewIndices ivi = ImageViewIndices.Unknown;

                    int width = 0;
                    int height = 0;

                    if (pxeHeader.viewBuffer_0.isValidView != 0)
                    {
                        if (pxeHeader.viewBuffer_0.isDualEnergy != 0 || pxeHeader.viewBuffer_0.isHighEnergy != 0)
                        {
                            ibi = ImageBufferIndices.RawH;
                        }
                        else
                        {
                            ibi = ImageBufferIndices.RawL;
                        }

                        width = Convert.ToInt32(pxeHeader.viewBuffer_0.width);
                        height = Convert.ToInt32(pxeHeader.viewBuffer_0.height);

                        m_rawData = new float[pxeHeader.viewBuffer_0.height * pxeHeader.viewBuffer_0.width];
                        ivi = ImageViewIndices.View1;
                    }
                    else if (pxeHeader.viewBuffer_1.isValidView != 0)
                    {
                        if (pxeHeader.viewBuffer_1.isDualEnergy != 0 || pxeHeader.viewBuffer_1.isHighEnergy != 0)
                        {
                            ibi = ImageBufferIndices.RawH1;
                        }
                        else
                        {
                            ibi = ImageBufferIndices.RawL1;
                        }

                        width = Convert.ToInt32(pxeHeader.viewBuffer_1.width);
                        height = Convert.ToInt32(pxeHeader.viewBuffer_1.height);

                        m_rawData = new float[pxeHeader.viewBuffer_1.height * pxeHeader.viewBuffer_1.width];
                        ivi = ImageViewIndices.View2;
                    }


                    if (ibi != ImageBufferIndices.Unknown && ivi != ImageViewIndices.Unknown)
                    {
                        GetDataBuffer(Convert.ToUInt32(ibi), (int)ivi, m_rawData, true);

                        PixelFormat pf = PixelFormats.Gray32Float;
                        int rawStride = (width * pf.BitsPerPixel + 7) / 8;
                        BitmapSource Source = BitmapSource.Create(width, height, 96.0, 96.0, pf, null, m_rawData, rawStride);

                        int maxWidth = 150;
                        int maxHeight = 100;

                        double ratio = (width > height) ? (double)maxWidth / (double)width
                            : (double)maxHeight / (double)height;

                        using (FileStream stream = new FileStream(Path.Combine(pxeFile.DirectoryName, "Thumb.jpg"), FileMode.Create))
                        {
                            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                            encoder.QualityLevel = 100;
                            TransformedBitmap thumbnail = new TransformedBitmap(Source, new ScaleTransform(ratio, ratio));
                            encoder.Frames.Add(BitmapFrame.Create(thumbnail));
                            encoder.Save(stream);
                        }
                    }

                    ClearActiveBagBuffer();
                    CloseActiveBag();
                }
            }
            catch
            {
                //DLL may not exist
            }
            return Ret;
        }
    }
}
