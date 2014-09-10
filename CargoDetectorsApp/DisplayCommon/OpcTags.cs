using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using L3.Cargo.Dashboard.Assembly.Common;

namespace L3.Cargo.Detectors.Display.Common
{
    public class OpcTags
    {
        public static OpcTag CALIBRATION_SCAN = new OpcTag("CALIBRATION_SCAN", "CALIBRATION_SCAN");
        public static OpcTag CALIBRATION_STATE = new OpcTag("CALIBRATION_STATE", "CALIBRATION_STATE");
        public static OpcTag SCAN_DRIVE_HAND_BRAKE = new OpcTag("SCAN_DRIVE_HAND_BRAKE", "SCAN_DRIVE_HAND_BRAKE");
        public static OpcTag SITE_READY_NEXT_SCAN = new OpcTag("SITE_READY_NEXT_SCAN", "SITE_READY_NEXT_SCAN");
        public static OpcTag SCAN_AREA_CLEAR = new OpcTag("SCAN_AREA_CLEAR", "SCAN_AREA_CLEAR");
        public static OpcTag MAINTENANCE_XRAY_ON = new OpcTag("MAINTENANCE_XRAY_ON", "MAINTENANCE_XRAY_ON");
        public static OpcTag LINAC_STATE = new OpcTag("LINAC_STATE", "LINAC_STATE");
    }
}
