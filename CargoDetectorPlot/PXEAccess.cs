using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace L3.Cargo.DetectorPlot
{
    public class PxeAccess
    {
          public struct PXEHeader
        {
            public int bValidFile;
            public int EngeryBuffers;//Number of energy buffers
        };

        public float[] m_32BitFloatNormData = null;
        public float[] m_rawData = null;
        //=================== for my convenience
        public int m_Img_Width;
        public int m_Img_Ht;
        //===================================== extra attributes added
        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern PXEHeader OpenPXEImage(string bagName);

        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi , CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern int GetPXEHeight(string EnergyName);

        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern int GetPXEWidth(string EnergyName);

        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern bool GetPXEData(string EnergyName, float[] EngBuffer);

        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern void ClosePXEImage();
    //=================================== write stuff ==============================
        //use UInt32 instead of long.
        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern bool CreateImageFile(string fname);

        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern bool SetHiHeader(int nViewType, UInt32 Width, UInt32 Height);

        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern int WriteHiEngDatalines(int nViewType, float[] pImageData, UInt32 LineCount);

        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern void CloseWriterFile();

        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern int WriteHiReferenceSample(int nViewType, float[] pReferenceData);


        //[DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        //public static extern bool WriteComments(string Comments);

        //[DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        // public static extern string GetComment();
//===============================================================================

        public PxeAccess()
        {
        }
        //public void ReadComments(ref string ss)
        //{
        //    try
        //    {
        //        ss = GetComment();
        //    }
        //    catch(Exception ex)
        //    {
        //        ss = "";
        //    }
        //}
        //public bool WriteCommentString(string comments)
        //{
        //    return WriteComments(comments);
      //  }
        //============ new ================
        public bool CreatePXE(string fname)
        {
            return CreateImageFile(fname);
        }

        public int ImageWidth(string engbufname)
        {
            return GetPXEWidth(engbufname);
        }
        public int ImageHeight(string engbufname)
        {
            return GetPXEHeight(engbufname);
        }

        public float [] Get32BitFloatNormImageBuffer()
        {
            return m_32BitFloatNormData;
        }

        public float[] GetRawImageBuffer()
        {
            return m_rawData;
        }
        public int WriteRefSample(int nViewType, float[] pReferenceData)
        {//is it needs ?????????????????????????????????????????????????????????????
            return WriteHiReferenceSample(nViewType, pReferenceData);
        }
        public bool OpenPXEFile(string BagName)
        {
            PXEHeader pxeHeader = OpenPXEImage(BagName);
            if (pxeHeader.bValidFile != 1)
                return false;

            if (pxeHeader.EngeryBuffers == 0)
                return true;

            string engbufname = "RawH";
            int height = GetPXEHeight(engbufname);
            m_Img_Ht = height;
            if (height < 1)
            {
                engbufname = "RawL";
                height = GetPXEHeight(engbufname);
                m_Img_Ht = height;
            }

            int width = GetPXEWidth(engbufname);
            m_Img_Width = width;
            if (height * width > 0)
            {
                m_32BitFloatNormData = new float[height * width];
                m_rawData = new float[height * width];
                bool bRet = GetPXEData(engbufname, m_rawData);//m_32BitFloatNormData);
                //Adjust col to row form
                for (int j = 0; j < height; j++)
                    for (int i = 0; i < width; i++)
                    {
                        m_32BitFloatNormData[(j * width) + i] = m_rawData[(i * height) + j]/65536;
                    }

                ClosePXEImage();
                return true;
            }
            else
                return false;
        }
//==================================================== write stuff
       public bool CreatePXEHeader(int ViewType, UInt32 Wid, UInt32 Hgt)
        {
            return SetHiHeader(1, Wid, Hgt);
        }

        public int WriteDataLines(int ViewType, float[] data, UInt32 linecnt)
        {
            return WriteHiEngDatalines(1, data, linecnt);
        }
               
        public void ClosePXE()
        {
            CloseWriterFile();
        }
//==============================================================

        public float[] ReadLine(string BagName, ref bool res)
        {
            float[] linedata = new float[0];
            PXEHeader pxeHeader = OpenPXEImage(BagName);
            if (pxeHeader.bValidFile != 1)
            {
                res = false;
                return linedata;
            }
            string engbufname = "LinearReferenceH";
            int height = GetPXEHeight(engbufname);
            m_Img_Ht = height;
            if (height < 1)
            {
                engbufname = "RawL";
                height = GetPXEHeight(engbufname);
                m_Img_Ht = height;
            }

            int width = GetPXEWidth(engbufname);
            m_Img_Width = width;
            if (height * width > 0)
            {
                m_rawData = new float[height * width];
                bool bRet = GetPXEData(engbufname, m_rawData);
                ClosePXEImage();
                res = true;
                return m_rawData;
            }
            else
            {
                res = false;
                return linedata;
            }
        }




        public  BitmapSource RawPXEDataIn32Bits(string BagName)
        {
            PXEHeader pxeHeader = OpenPXEImage(BagName);
            string engbufname = "RawH";
            int height = GetPXEHeight(engbufname);
            if (height < 1)
            {
                engbufname = "RawL";
                height = GetPXEHeight(engbufname);
            }

            int width = GetPXEWidth(engbufname);
            if (height * width > 0)
            {
                m_32BitFloatNormData = new float[height * width];
                bool bRet = GetPXEData(engbufname, m_32BitFloatNormData);
                float[] rawDataCopy = new float[height * width];
                m_32BitFloatNormData.CopyTo(rawDataCopy, 0); 

                for (int j1 = 0; j1 < height; j1++)
                    for (int i1 = 0; i1 < width; i1++)
                    {
                        m_32BitFloatNormData[(j1 * width) + i1] = rawDataCopy[(i1 * height) + j1];
                    }

                for (int j = 0; j < height; j++)
                    for (int i = 0; i < width; i++)
                    {
                        m_32BitFloatNormData[(j * width) + i] = m_32BitFloatNormData[(j * width) + i] / 65536;
                    }

                PixelFormat pf = PixelFormats.Gray32Float;
                int rawStride = (width * pf.BitsPerPixel + 7) / 8;
                
                BitmapSource bitmapImg = BitmapSource.Create(width, height,
                  96, 96, pf, null, m_32BitFloatNormData, rawStride);

                ClosePXEImage();
                return bitmapImg;
            }
            else
                return null;
        }
    }

    //======================================


}

