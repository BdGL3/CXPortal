using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using L3.Cargo.Dashboard.Assembly.Common;

namespace L3.Cargo.Scan.Display.Common
{
    public class OpcTags
    {
        public static OpcTag SCAN_AREA_CLEAR = new OpcTag("SCAN_AREA_CLEAR", "SCAN_AREA_CLEAR");
        public static OpcTag CALIBRATION_SCAN = new OpcTag("CALIBRATION_SCAN", "CALIBRATION_SCAN");
        public static OpcTag PORTAL_OBJECT_FOUND = new OpcTag("PORTAL_OBJECT_FOUND", "PORTAL_OBJECT_FOUND");
        public static OpcTag SCAN_MULTIPLE_OBJECTS = new OpcTag("SCAN_MULTIPLE_OBJECTS", "SCAN_MULTIPLE_OBJECTS");
        public static OpcTag SCAN_STATE = new OpcTag("SCAN_STATE", "SCAN_STATE");
        public static OpcTag CALIBRATION_STATE = new OpcTag("CALIBRATION_STATE", "CALIBRATION_STATE");
        public static OpcTag CLICKER_ENABLE = new OpcTag("CLICKER_ENABLE", "CLICKER_ENABLE");
        public static OpcTag CLICKER_BYPASS_ENABLE = new OpcTag("WIRELESS_CTRL_BYPASS", "WIRELESS_CTRL_BYPASS");
        public static OpcTag SITE_READY_NEXT_SCAN = new OpcTag("SITE_READY_NEXT_SCAN", "SITE_READY_NEXT_SCAN");

        public static OpcTag SCAN_STEP_LAST = new OpcTag("SCAN_STEP_LAST", "SCAN_STEP_LAST");
        public static OpcTag SCAN_STEP = new OpcTag("SCAN_STEP", "SCAN_STEP");
        //public static OpcTag SCAN_DRIVE_DIRECTION = new OpcTag("SCAN_DRIVE_DIRECTION", "SCAN_DRIVE_DIRECTION");
        //public static OpcTag SCAN_DRIVE_SELECTED_SPEED = new OpcTag("SCAN_DRIVE_SELECTED_SPEED", "SCAN_DRIVE_SELECTED_SPEED");
        //public static OpcTag SCAN_DRIVE_STATE = new OpcTag("SCAN_DRIVE_STATE", "SCAN_DRIVE_STATE");
        //public static OpcTag SCAN_DRIVE_SIGN_OF_LIFE = new OpcTag("SCAN_DRIVE_SIGN_OF_LIFE", "SCAN_DRIVE_SIGN_OF_LIFE");
        //public static OpcTag SCAN_DRIVE_POWER_STATUS = new OpcTag("SCAN_DRIVE_POWER_STATUS", "SCAN_DRIVE_POWER_STATUS");
        //public static OpcTag SCAN_DRIVE_HAND_BRAKE = new OpcTag("SCAN_DRIVE_HAND_BRAKE", "SCAN_DRIVE_HAND_BRAKE");
        //public static OpcTag SCAN_DRIVE_SPEED_FAILURE = new OpcTag("SCAN_DRIVE_SPEED_FAILURE", "SCAN_DRIVE_SPEED_FAILURE");
        //public static OpcTag SCAN_DRIVE_HYDRAULIC_OIL_TEMPERATURE = new OpcTag("SCAN_DRIVE_HYDRAULIC_OIL_TEMPERATURE", "SCAN_DRIVE_HYDRAULIC_OIL_TEMPERATURE");
        //public static OpcTag SCAN_DRIVE_HYDRAULIC_OIL_LEVEL = new OpcTag("SCAN_DRIVE_HYDRAULIC_OIL_LEVEL", "SCAN_DRIVE_HYDRAULIC_OIL_LEVEL");
        //public static OpcTag SCAN_DRIVE_REALTIME_SPEED = new OpcTag("SCAN_DRIVE_REALTIME_SPEED", "SCAN_DRIVE_REALTIME_SPEED");
    }
}
