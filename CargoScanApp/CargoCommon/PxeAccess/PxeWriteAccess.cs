using System;
using System.Runtime.InteropServices;

namespace L3.Cargo.Common.PxeAccess
{
    public class PxeWriteAccess
    {
        public struct PXEHeader
        {
            public int pxeIndex;
            public int bValidFile;
            public int EngeryBuffers;//Number of energy buffers
        };

        public float[] _32BitFloatNormData = null;
        public float[] _rawData = null;

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


        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern int WriteLoEngDarkSample(int nViewType, float[] pSampleData);

        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern int WriteHiEngDarkSample(int nViewType, float[] pSampleData);

        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern int WriteLoEngAirSample(int nViewType, float[] pSampleData);

        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern int WriteHiEngAirSample(int nViewType, float[] pSampleData);

        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern int WriteHiLineStatus(byte[] pLineStatus);

        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern int WriteLoLineStatus(byte[] pLineStatus);


        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern bool CreateImageFile(string fname);


        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern bool SetHiHeader(int nViewType, UInt32 Width, UInt32 Height);

        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern bool SetLoHeader(int nViewType, UInt32 Width, UInt32 Height);

        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern int WriteHiEngDatalines(int nViewType, float[] pImageData, UInt32 LineCount);

        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern int WriteLoEngDatalines(int nViewType, float[] pImageData, UInt32 LineCount);

        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern void CloseWriterFile();


        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern int WriteHiEngScaleFactor(int nViewType, float[] pScaleData);

        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern int WriteLoEngScaleFactor(int nViewType, float[] pScaleData);

        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern int WriteLoFullAirSample(int nViewType, float[] pImageData, UInt32 LineCount);

        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern int WriteHiFullAirSample(int nViewType, float[] pImageData, UInt32 LineCount);

        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern int WriteLoFullDarkSample(int nViewType, float[] pImageData, UInt32 LineCount);

        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern int WriteHiFullDarkSample(int nViewType, float[] pImageData, UInt32 LineCount);

        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern int WriteLoRefCorrection(int nViewType, float[] pRefData);

        [DllImport("ImageWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, PreserveSig = true)]
        public static extern int WriteHiRefCorrection(int nViewType, float[] pRefData);


        public PxeWriteAccess()
        {
        }
        
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
            return _32BitFloatNormData;
        }

        public float[] GetRawImageBuffer()
        {
            return _rawData;
        }

        public bool OpenPXEFile(string BagName)
        {
            PXEHeader pxeHeader = OpenPXEImage(BagName);
            if (pxeHeader.bValidFile != 1)
                return false;
            
            return true;            
        }

        public bool GetData(string bufferName, float[] buffer)
        {
            return GetPXEData(bufferName, buffer);
        }

        public int WriteHighLineStatus(byte[] pLineStatus)
        {
            return WriteHiLineStatus(pLineStatus);
        }

        public int WriteLowLineStatus(byte[] pLineStatus)
        {
            return WriteLoLineStatus(pLineStatus);
        }

        public int WriteHighEngDarkSample(float[] pData)
        {
            return WriteHiEngDarkSample(1, pData);
        }

        public int WriteLowEngDarkSample(float[] pData)
        {
            return WriteLoEngDarkSample(1, pData);
        }

        public int WriteLowEngAirSample(float[] pData)
        {
            return WriteLoEngAirSample(1, pData);
        }

        public int WriteHighEngAirSample(float[] pData)
        {
            return WriteHiEngAirSample(1, pData);
        }

        public bool CreateHiPXEHeader(UInt32 Wid, UInt32 Hgt)
        {
            return SetHiHeader(1, Wid, Hgt);
        }

        public bool CreateLoPXEHeader(UInt32 Wid, UInt32 Hgt)
        {
            return SetLoHeader(1, Wid, Hgt);
        }

        public int WriteHiDataLines(float[] data, UInt32 linecnt)
        {
            return WriteHiEngDatalines(1, data, linecnt);
        }

        public int WriteLoDataLines(float[] data, UInt32 linecnt)
        {
            return WriteLoEngDatalines(1, data, linecnt);
        }

        public bool CreatePXEHeader(int ViewType, UInt32 Wid, UInt32 Hgt)
        {
            return SetHiHeader(1, Wid, Hgt);
        }

        public int WriteDataLines(int ViewType, float[] data, UInt32 linecnt)
        {
            return WriteHiEngDatalines(1, data, linecnt);
        }

        public int WriteHiScaleFactor(float[] pScaleData)
        {
            return WriteHiEngScaleFactor(1, pScaleData);
        }

        public int WriteLoScaleFactor(float[] pScaleData)
        {
            return WriteLoEngScaleFactor(1, pScaleData);
        }

        public int WriteLoFullAirData(float[] pImageData, UInt32 LineCount)
        {
            return WriteLoFullAirSample(1, pImageData, LineCount);
        }

        public int WriteHiFullAirData(float[] pImageData, UInt32 LineCount)
        {
            return WriteHiFullAirSample(1, pImageData, LineCount);
        }

        public int WriteLoFullDarkData(float[] pImageData, UInt32 LineCount)
        {
            return WriteLoFullDarkSample(1, pImageData, LineCount);
        }

        public int WriteHiFullDarkData(float[] pImageData, UInt32 LineCount)
        {
            return WriteHiFullDarkSample(1, pImageData, LineCount);
        }

        public int WriteLoRef(float[] pRefData)
        {
            return WriteLoRefCorrection(1, pRefData);
        }

        public int WriteHiRef(float[] pRefData)
        {
            return WriteHiRefCorrection(1, pRefData);
        }

        public void ClosePXEWrite()
        {
            CloseWriterFile();
        }

        public void ClosePXE()
        {
            ClosePXEImage();
        }

        public bool ReadHighEngDarkSample(float[] pData)
        {
            return GetData("LinearDarkH", pData);
        }

        public bool ReadLowEngDarkSample(float[] pData)
        {
            return GetData("LinearDarkL", pData);
        }

        public bool ReadHighEngAirSample(float[] pData)
        {
            return GetData("LinearAirH", pData);
        }

        public bool ReadLowEngAirSample(float[] pData)
        {
            return GetData("LinearAirL", pData);
        }
    }
}

