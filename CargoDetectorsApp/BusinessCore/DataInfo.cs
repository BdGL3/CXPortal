using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using L3.Cargo.Communications.Detectors;

namespace L3.Cargo.Detectors.BusinessCore
{
    public class DataInfo
    {
        public DetectorsAccess.LineHeader LineHeader { get; set; }
        public float[] LineData { get; set; }

        public DataInfo(DetectorsAccess.LineHeader header, float[] linedata)
        {
            LineData = linedata;
            LineHeader = header;
        }
    }
}
