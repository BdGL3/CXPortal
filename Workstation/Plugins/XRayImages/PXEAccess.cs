using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using L3.Cargo.Workstation.Plugins.XRayImageBase;
using System.Diagnostics;

namespace L3.Cargo.Workstation.Plugins.XRayImages
{
    /// <summary>
    /// Defined for use as image buffer indices with GetDataBuffer()
    public enum ImageBufferIndices
    {
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
    public enum ImageViewIndices
    {
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

    [StructLayout(LayoutKind.Explicit)]
    public struct Rect
    {
        [FieldOffset(0)]
        public int left;
        [FieldOffset(4)]
        public int top;
        [FieldOffset(8)]
        public int right;
        [FieldOffset(12)]
        public int bottom;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct TIPStatus
    {
        public uint isValidFile;
        public uint pxeindex; // system type
        public uint injectbuffercount;
        public uint injectionsucess;
        public Rect injectLocation_view0;
        public Rect injectLocation_view1;
    };

    public class PxeAccess
    {
        private const int ERROR_RETURN = -1;

        public PXEHeader pxeHeader;

        private uint _InstanceId;
        
        public Dictionary<string, float[]> m_32BitFloatNormData = new Dictionary<string, float[]>();
        public Dictionary<string, float[]> m_rawData = new Dictionary<string, float[]>();
        //=================== for my convenience
        public string[] buffers = { "RawH", "RawH1", "RawL", "RawL1"};
        public List<string> buffersAvailable = new List<string>();
        //=====================================
        [DllImport("AlgClientWrap.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern int CreateAlgServer (uint instance, [In, MarshalAs(UnmanagedType.LPStr)] string serverIP, 
                                                                                    uint serverNum);
        [DllImport("AlgClientWrap.dll", SetLastError = true, CallingConvention=CallingConvention.Cdecl)]
        public static extern PXEHeader OpenPXEImageFromMemory (uint instance, byte[] pxeData, int memSize);

        [DllImport("AlgClientWrap.dll", SetLastError = true, CallingConvention=CallingConvention.Cdecl)]
        public static extern PXEHeader OpenAndProcessPXEImageFromMemory (uint instance, byte[] pxeData, int memSize);

        [DllImport("AlgClientWrap.dll", SetLastError = true, CallingConvention=CallingConvention.Cdecl)]
        public static extern PXEHeader OpenAndProcessPXEImageFromMemoryWithTIP (uint instance, byte[] pxeData, int memSize, byte[] tipPXEData, int tipDataMemSize, ref TIPStatus ts);

        [DllImport("AlgClientWrap.dll", SetLastError = true, CallingConvention=CallingConvention.Cdecl)]
        public static extern PXEHeader OpenPXEImageFromFile (uint instance, [In, MarshalAs(UnmanagedType.LPStr)] string imageFile);

        [DllImport("AlgClientWrap.dll", SetLastError = true, CallingConvention=CallingConvention.Cdecl)]
        public static extern PXEHeader OpenAndProcessPXEImageFromFile (uint instance, [In, MarshalAs(UnmanagedType.LPStr)] string imageFile);

        [DllImport("AlgClientWrap.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern void GetDataBuffer (uint instance, uint index, // convert enumeration ImageBufferIndices to int
                                    int view,               // convert enumeration ImageViewIndices1 to int
                                    float[] EngBuffer,      // buffer to recieve the x-ray data
                                    bool bNormalize);       // true to normalize


        [DllImport("AlgClientWrap.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool ApplyTrimatOption (uint instance, uint view,                 // view number
                                                     int trimatOption,          // the trimat filter that should be applied
                                                     float edgeLevel,           // edge level
                                                     uint[] data);              // returned data
        
        [DllImport("AlgClientWrap.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CloseActiveBag(uint instance);

        [DllImport("AlgClientWrap.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UnInit(uint instance);


        public PxeAccess(uint instanceId)
        {
            _InstanceId = instanceId;
        }

        public int LoadAlgServer(string serverIP, string path)
        {
            int ret = CreateAlgServer(_InstanceId, serverIP, _InstanceId);

            if (ret == ERROR_RETURN)
            {
                Process processAlgServer = new Process();
                processAlgServer.StartInfo.WorkingDirectory = path;
                processAlgServer.StartInfo.FileName = "AlgServer.exe";
                processAlgServer.StartInfo.Arguments = "-n " + _InstanceId.ToString();
                processAlgServer.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                processAlgServer.Start();

                ret = CreateAlgServer(_InstanceId, serverIP, _InstanceId);
            }

            return ret;
        }

        public PXEHeader GetPxeHeader ()
        {
            return pxeHeader;
        }

        public void UnInitAlgServer()
        {
            UnInit(_InstanceId);
        }

        public uint ImageWidth(string buffer)
        {
            if (pxeHeader.isValidFile != 0)
            {
                if ((String.CompareOrdinal(buffer, "RawH") == 0) && pxeHeader.viewBuffer_0.isValidView == 1
                                    && pxeHeader.viewBuffer_0.isHighEnergy == 1 || pxeHeader.viewBuffer_0.isDualEnergy == 1)
                {
                    return pxeHeader.viewBuffer_0.width;
                }
                else
                    if ((String.CompareOrdinal(buffer, "RawL") == 0) && pxeHeader.viewBuffer_0.isValidView == 1
                                        && pxeHeader.viewBuffer_0.isHighEnergy == 0 || pxeHeader.viewBuffer_0.isDualEnergy == 1)
                    {
                        return pxeHeader.viewBuffer_0.width;
                    }
                    else
                        if ((String.CompareOrdinal(buffer, "RawH1") == 0) && pxeHeader.viewBuffer_1.isValidView == 1
                                            && pxeHeader.viewBuffer_1.isHighEnergy == 1 || pxeHeader.viewBuffer_0.isDualEnergy == 1)
                        {
                            return pxeHeader.viewBuffer_1.width;
                        }
                        else
                            if ((String.CompareOrdinal(buffer, "RawL1") == 0) && pxeHeader.viewBuffer_1.isValidView == 1
                                                && pxeHeader.viewBuffer_1.isHighEnergy == 0 || pxeHeader.viewBuffer_0.isDualEnergy == 1)
                            {
                                return pxeHeader.viewBuffer_1.width;
                            }
            }

            return 0;
        }

        public uint ImageHeight(string buffer)
        {
            if (pxeHeader.isValidFile != 0)
            {
                if ((String.CompareOrdinal(buffer, "RawH") == 0) && pxeHeader.viewBuffer_0.isValidView == 1
                                    && pxeHeader.viewBuffer_0.isHighEnergy == 1 || pxeHeader.viewBuffer_0.isDualEnergy == 1)
                {
                    return pxeHeader.viewBuffer_0.height;
                }
                else
                    if ((String.CompareOrdinal(buffer, "RawL") == 0) && pxeHeader.viewBuffer_0.isValidView == 1
                                        && pxeHeader.viewBuffer_0.isHighEnergy == 0 || pxeHeader.viewBuffer_0.isDualEnergy == 1)
                    {
                        return pxeHeader.viewBuffer_0.height;
                    }
                    else
                        if ((String.CompareOrdinal(buffer, "RawH1") == 0) && pxeHeader.viewBuffer_1.isValidView == 1
                                            && pxeHeader.viewBuffer_1.isHighEnergy == 1 || pxeHeader.viewBuffer_0.isDualEnergy == 1)
                        {
                            return pxeHeader.viewBuffer_1.height;
                        }
                        else
                            if ((String.CompareOrdinal(buffer, "RawL1") == 0) && pxeHeader.viewBuffer_1.isValidView == 1
                                                && pxeHeader.viewBuffer_1.isHighEnergy == 0 || pxeHeader.viewBuffer_0.isDualEnergy == 1)
                            {
                                return pxeHeader.viewBuffer_1.height;
                            }
            }

            return 0;
        }

        public float[] Get32BitFloatNormImageBuffer(string engbufname)
        {
            return m_rawData[engbufname];
        }

        public float[] GetImageBuffer(string engbufname)
        {
            return m_rawData[engbufname];
        }

        public uint[] GetTrimatBuffer (string view, int trimatOption)
        {
            uint[] _trimatData;

            if (view.Equals("View0"))
            {
                _trimatData = new uint[pxeHeader.viewBuffer_0.height * pxeHeader.viewBuffer_0.width];
                ApplyTrimatOption(_InstanceId, 0, trimatOption, 0, _trimatData);
            }
            else
            {
                _trimatData = new uint[pxeHeader.viewBuffer_1.height * pxeHeader.viewBuffer_1.width];
                ApplyTrimatOption(_InstanceId, 1, trimatOption, 0, _trimatData);
            }

            return _trimatData;
        }

        public bool OpenPXEImageFromMemory(MemoryStream pxeFile)
        {
            byte[] pxeData = new byte[pxeFile.Length];

            using (BinaryReader r = new BinaryReader(pxeFile))
            {
                // Read data from Test.data.
                pxeData = r.ReadBytes((int)pxeFile.Length);
            }

            buffersAvailable.Clear();

            pxeHeader = OpenAndProcessPXEImageFromMemory(_InstanceId, pxeData, (int)pxeData.Length);

            if (pxeHeader.isValidFile != 1)
            {
                CloseActiveBag(_InstanceId);
                return false;
            }

            LoadPXEData();

            pxeFile.Close();
            pxeFile.Dispose();

            return true;
        }

        public bool OpenPXEImageFromMemoryWithTIP(MemoryStream pxeFile, MemoryStream tipPXEFile, ref TIPStatus tipStatus)
        {
            byte[] pxeData = new byte[pxeFile.Length];

            using (BinaryReader r = new BinaryReader(pxeFile))
            {
                // Read data from Test.data.
                pxeData = r.ReadBytes((int)pxeFile.Length);
            }

            buffersAvailable.Clear();

            byte[] tipPXEData = new byte[tipPXEFile.Length];

            using (BinaryReader r = new BinaryReader(tipPXEFile))
            {
                // Read data from Test.data.
                tipPXEData = r.ReadBytes((int)tipPXEFile.Length);
            }

            buffersAvailable.Clear();

            pxeHeader = OpenAndProcessPXEImageFromMemoryWithTIP(_InstanceId, pxeData, (int)pxeData.Length, tipPXEData, (int)tipPXEData.Length, ref tipStatus);

            if (pxeHeader.isValidFile != 1)
            {
                CloseActiveBag(_InstanceId);
                return false;
            }

            LoadPXEData();

            tipPXEFile.Close();
            tipPXEFile.Dispose();

            pxeFile.Close();
            pxeFile.Dispose();

            return true;
        }

        private void LoadPXEData()
        {
            if (pxeHeader.viewBuffer_0.isValidView != 0)
            {
                if (pxeHeader.viewBuffer_0.isDualEnergy != 0)
                {
                    m_rawData["RawH"] = new float[pxeHeader.viewBuffer_0.height * pxeHeader.viewBuffer_0.width];
                    GetDataBuffer(_InstanceId, (int)ImageBufferIndices.RawH, (int)ImageViewIndices.View1, m_rawData["RawH"], true);
                    buffersAvailable.Add("RawH");

                    m_rawData["RawL"] = new float[pxeHeader.viewBuffer_0.height * pxeHeader.viewBuffer_0.width];
                    GetDataBuffer(_InstanceId, (int)ImageBufferIndices.RawL, (int)ImageViewIndices.View1, m_rawData["RawL"], true);
                    buffersAvailable.Add("RawL");
                }
                else if (pxeHeader.viewBuffer_0.isHighEnergy != 0)
                {
                    m_rawData["RawH"] = new float[pxeHeader.viewBuffer_0.height * pxeHeader.viewBuffer_0.width];
                    GetDataBuffer(_InstanceId, (int)ImageBufferIndices.RawH, (int)ImageViewIndices.View1, m_rawData["RawH"], true);
                    buffersAvailable.Add("RawH");
                }
                else
                {
                    m_rawData["RawL"] = new float[pxeHeader.viewBuffer_0.height * pxeHeader.viewBuffer_0.width];
                    GetDataBuffer(_InstanceId, (int)ImageBufferIndices.RawL, (int)ImageViewIndices.View1, m_rawData["RawL"], true);
                    buffersAvailable.Add("RawL");
                }
            }

            if (pxeHeader.viewCount > 1 && pxeHeader.viewBuffer_1.isValidView != 0)
            {
                if (pxeHeader.viewBuffer_1.isDualEnergy != 0)
                {
                    m_rawData["RawH1"] = new float[pxeHeader.viewBuffer_1.height * pxeHeader.viewBuffer_1.width];
                    GetDataBuffer(_InstanceId, (int)ImageBufferIndices.RawH1, (int)ImageViewIndices.View2, m_rawData["RawH1"], true);
                    buffersAvailable.Add("RawH1");

                    m_rawData["RawL1"] = new float[pxeHeader.viewBuffer_1.height * pxeHeader.viewBuffer_1.width];
                    GetDataBuffer(_InstanceId, (int)ImageBufferIndices.RawL1, (int)ImageViewIndices.View2, m_rawData["RawL1"], true);
                    buffersAvailable.Add("RawL1");
                }
                else if (pxeHeader.viewBuffer_1.isHighEnergy != 0)
                {
                    m_rawData["RawH1"] = new float[pxeHeader.viewBuffer_1.height * pxeHeader.viewBuffer_1.width];
                    GetDataBuffer(_InstanceId, (int)ImageBufferIndices.RawH1, (int)ImageViewIndices.View2, m_rawData["RawH1"], true);
                    buffersAvailable.Add("RawH1");
                }
                else
                {
                    m_rawData["RawL1"] = new float[pxeHeader.viewBuffer_1.height * pxeHeader.viewBuffer_1.width];
                    GetDataBuffer(_InstanceId, (int)ImageBufferIndices.RawL1, (int)ImageViewIndices.View2, m_rawData["RawL1"], true);
                    buffersAvailable.Add("RawL1");
                }
            }
        }

        public void Dispose ()
        {
            CloseActiveBag(_InstanceId);
        }
    }
}
