using System;
using System.Diagnostics;

namespace L3.Cargo.Communications.Detectors.Common
{
    public class DataInfo: IDisposable
    {
        public Pixel[] LineData { get; set; }
        public byte NumberOfBytesPerPixel;
        public int TotalBytesReceived { get; set; }
        public XRayInfoIDStruct XRayInfo;

        public DataInfo Clone()
        {
            DataInfo /*clone (returned)*/ cln =
                    new DataInfo(LineData.Length, XRayInfo, NumberOfBytesPerPixel);
            Debug.Assert(cln.NumberOfBytesPerPixel == NumberOfBytesPerPixel);
            for (int /*pixel index*/ ix = 0; ix < LineData.Length; ix++)
                if (LineData[ix] != null)
                    cln.LineData[ix] = new Pixel(LineData[ix].Value);
                else
                    Debug.Assert(cln.LineData[ix] == null);
            Debug.Assert(cln.TotalBytesReceived == 0);
            return cln;
        }

        public DataInfo(int dataSize, XRayInfoIDStruct xrayInfo, byte numberOfBytesPerPixel)
        {
            LineData = new Pixel[dataSize];
            NumberOfBytesPerPixel = numberOfBytesPerPixel;
            XRayInfo = xrayInfo;
            TotalBytesReceived = 0;
        }

        public void Dispose()
        {
            if (LineData != null)
                for (int /*pixel index*/ ix = 0; ix < LineData.Length; ix++)
                    if (LineData[ix] != null)
                        LineData[ix].Dispose();
            LineData = null;
            NumberOfBytesPerPixel = 0;
            TotalBytesReceived = 0;
        }
    }
}
