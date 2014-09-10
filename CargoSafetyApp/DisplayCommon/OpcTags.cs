using L3.Cargo.Dashboard.Assembly.Common;

namespace L3.Cargo.Safety.Display.Common
{
    public class OpcTags
    {
        public static OpcTag V_MUX_SIGN_OF_LIFE = new OpcTag("V_MUX_SIGN_OF_LIFE", "V_MUX_SIGN_OF_LIFE");
        public static OpcTag ASI_SIGN_OF_LIFE = new OpcTag("ASI_SIGN_OF_LIFE", "ASI_SIGN_OF_LIFE");
        public static OpcTag SAFETY_RESET = new OpcTag("SAFETY_RESET", "SAFETY_RESET");

        /* MDM E-Stop Values for the Truck
        public static OpcTag ESTOP_DRIVERS_CAB = new OpcTag("ESTOP_DRIVERS_CAB", "ESTOP_STATE");
        public static OpcTag ESTOP_OPERATOR_AREA = new OpcTag("ESTOP_OPERATOR_AREA", "ESTOP_STATE");
        public static OpcTag ESTOP_LINAC_AREA = new OpcTag("ESTOP_LINAC_AREA", "ESTOP_STATE");
        public static OpcTag ESTOP_INDUSTRIAL_AREA = new OpcTag("ESTOP_INDUSTRIAL_AREA", "ESTOP_STATE");
        public static OpcTag ESTOP_FRONT_LEFT = new OpcTag("ESTOP_FRONT_LEFT", "ESTOP_STATE");
        public static OpcTag ESTOP_FRONT_RIGHT = new OpcTag("ESTOP_FRONT_RIGHT", "ESTOP_STATE");
        public static OpcTag ESTOP_REAR_LEFT = new OpcTag("ESTOP_REAR_LEFT", "ESTOP_STATE");
        public static OpcTag ESTOP_REAR_RIGHT = new OpcTag("ESTOP_REAR_RIGHT", "ESTOP_STATE");
        public static OpcTag ESTOP_ROOF_MAST = new OpcTag("ESTOP_ROOF_MAST", "ESTOP_STATE");
        public static OpcTag ESTOP_VERTICAL_BOOM = new OpcTag("ESTOP_VERTICAL_BOOM", "ESTOP_STATE");
        public static OpcTag ESTOP_CLICKER = new OpcTag("ESTOP_CLICKER", "ESTOP_STATE");
        */
        
        // E-Stop values for the Portal
        public static OpcTag ESTOP_OPERATORS_CONSOLE = new OpcTag("ESTOP_OPERATORS_CONSOLE", "ESTOP_STATE");
        public static OpcTag ESTOP_XRAY_SOURCE_AREA = new OpcTag("ESTOP_XRAY_SOURCE_AREA", "ESTOP_STATE");
        public static OpcTag ESTOP_TUNNEL_ENTRY_RIGHT = new OpcTag("ESTOP_TUNNEL_ENTRY_RIGHT", "ESTOP_STATE");
        public static OpcTag ESTOP_TUNNEL_ENTRY_LEFT = new OpcTag("ESTOP_TUNNEL_ENTRY_LEFT", "ESTOP_STATE");
        public static OpcTag ESTOP_TUNNEL_EXIT_RIGHT = new OpcTag("ESTOP_TUNNEL_EXIT_RIGHT", "ESTOP_STATE");
        public static OpcTag ESTOP_TUNNEL_EXIT_LEFT = new OpcTag("ESTOP_TUNNEL_EXIT_LEFT", "ESTOP_STATE");

        /* Interlock Values for the Truck
        public static OpcTag INTERLOCK_BYPASS = new OpcTag("INTERLOCK_BYPASS", "INTERLOCK_BYPASS");
        public static OpcTag INTERLOCK_LEFT_CABIN_DOOR = new OpcTag("INTERLOCK_LEFT_CABIN_DOOR", "INTERLOCK_LEFT_CABIN_DOOR");
        public static OpcTag INTERLOCK_RIGHT_CABIN_DOOR = new OpcTag("INTERLOCK_RIGHT_CABIN_DOOR", "INTERLOCK_RIGHT_CABIN_DOOR");
        public static OpcTag INTERLOCK_FOOT_BRAKE = new OpcTag("INTERLOCK_FOOT_BRAKE", "INTERLOCK_FOOT_BRAKE");
        public static OpcTag INTERLOCK_CLICKER = new OpcTag("INTERLOCK_CLICKER", "ESTOP_STATE");
        */

        // Interlock values for the portal
        public static OpcTag INTERLOCK_BYPASS = new OpcTag("INTERLOCK_BYPASS", "INTERLOCK_BYPASS");
        public static OpcTag INTERLOCK_DOOR = new OpcTag("INTERLOCK_DOOR", "INTERLOCK_DOOR");
        public static OpcTag INTERLOCK_LC1 = new OpcTag("INTERLOCK_LC1", "INTERLOCK_LC_PERSONNEL_SAFETY_STATE");
        public static OpcTag INTERLOCK_LC2 = new OpcTag("INTERLOCK_LC2", "INTERLOCK_LC_PERSONNEL_SAFETY_STATE");

        public static OpcTag INTERLOCK_MDS_PERSONNEL_SAFETY_1 = new OpcTag("INTERLOCK_MDS_PERSONNEL_SAFETY_1", "INTERLOCK_MDS_PERSONNEL_SAFETY_STATE");
        public static OpcTag INTERLOCK_MDS_PERSONNEL_SAFETY_2 = new OpcTag("INTERLOCK_MDS_PERSONNEL_SAFETY_2", "INTERLOCK_MDS_PERSONNEL_SAFETY_STATE");

        // TBD Need to check with HMI Tags 
        public static OpcTag IN_MDS3_VEHICLE_MOTION_DET = new OpcTag("IN_MDS3_VEHICLE_MOTION_DET", "IN_MDS_VEHICLE_MOTION_DET_SAFETY_STATE");
        public static OpcTag IN_MDS4_VEHICLE_MOTION_DET = new OpcTag("IN_MDS4_VEHICLE_MOTION_DET", "IN_MDS_VEHICLE_MOTION_DET_SAFETY_STATE");

        // TBD Need to check with HMI Tags
        public static OpcTag TRAFFIC_LIGHT_STATUS = new OpcTag("TRAFFIC_LIGHT_STATUS", "TRAFFIC_LIGHT_STATUS");

        public static OpcTag WARNING_RED_LIGHT_1_STATUS = new OpcTag("BCN1A_STATUS", "BCN1A_STATUS");
        public static OpcTag WARNING_RED_LIGHT_2_STATUS = new OpcTag("BCN1B_STATUS", "BCN1B_STATUS");
        public static OpcTag WARNING_RED_LIGHT_3_STATUS = new OpcTag("BCN2A_STATUS", "BCN2A_STATUS");
        public static OpcTag WARNING_RED_LIGHT_4_STATUS = new OpcTag("BCN2B_STATUS", "BCN2B_STATUS");
        public static OpcTag WARNING_RED_LIGHT_5_STATUS = new OpcTag("BCN5_STATUS", "BCN5_STATUS");

        // Vehicle Sensor Status
        public static OpcTag VEHICLE_SENSOR_BEFORE_XRAY_LEFT = new OpcTag("VEHICLE_SENSOR_BEFORE_XRAY_LEFT", "VEHICLE_SENSOR_BEFORE_XRAY_LEFT");
        public static OpcTag VEHICLE_SENSOR_BEFORE_XRAY_RIGHT = new OpcTag("VEHICLE_SENSOR_BEFORE_XRAY_RIGHT", "VEHICLE_SENSOR_BEFORE_XRAY_RIGHT");
        public static OpcTag VEHICLE_SENSOR_AFTER_XRAY_LEFT = new OpcTag("VEHICLE_SENSOR_AFTER_XRAY_LEFT", "VEHICLE_SENSOR_AFTER_XRAY_LEFT");
        public static OpcTag VEHICLE_SENSOR_AFTER_XRAY_RIGHT = new OpcTag("VEHICLE_SENSOR_AFTER_XRAY_RIGHT", "VEHICLE_SENSOR_AFTER_XRAY_RIGHT");
        public static OpcTag VEHICLE_SENSOR_AT_GATE_RIGHT = new OpcTag("VEHICLE_SENSOR_AT_GATE_RIGHT", "VEHICLE_SENSOR_AT_GATE_RIGHT");
        public static OpcTag VEHICLE_SENSOR_AT_GATE_LEFT = new OpcTag("VEHICLE_SENSOR_AT_GATE_LEFT", "VEHICLE_SENSOR_AT_GATE_LEFT");

        // Barcode Reader Stattus
        public static OpcTag BARCODE_READ = new OpcTag("BARCODE_READ", "BARCODE_READ_STATE");

        // Doppler Radar Status
        public static OpcTag DOPPLER_RADAR_SENSOR_RIGHT = new OpcTag("DOPPLER_RADAR_SENSOR_RIGHT", "DOPPLER_RADAR_SENSOR_RIGHT");
        public static OpcTag DOPPLER_RADAR_SENSOR_LEFT = new OpcTag("DOPPLER_RADAR_SENSOR_LEFT", "DOPPLER_RADAR_SENSOR_LEFT");

        // Distance Measurment Sensors
        public static OpcTag VEHICLE_TYPE = new OpcTag("VEHICLE_TYPE", "VEHICLE_TYPE_STATE");

        public static OpcTag PERIMETER_MODE = new OpcTag("PERIMETER_MODE", "PERIMETER_MODE");
        public static OpcTag PERIMETER_INTRUSIONS_STATE = new OpcTag("PERIMETER_INTRUSIONS_STATE", "PERIMETER_INTRUSIONS_STATE");

        public static OpcTag HORIZONTAL_BOOM_ANGLE = new OpcTag("HORIZONTAL_BOOM_ANGLE", "");
        public static OpcTag HORIZONTAL_BOOM_DEPLOY_AT_POSITION = new OpcTag("HORIZONTAL_BOOM_DEPLOY_AT_POSITION", "");
        public static OpcTag HORIZONTAL_BOOM_STOW_AT_POSITION = new OpcTag("HORIZONTAL_BOOM_STOW_AT_POSITION", "");

        public static OpcTag MAST_ANGLE = new OpcTag("MAST_ANGLE", "");
        public static OpcTag MAST_DEPLOY_AT_POSITION = new OpcTag("MAST_DEPLOY_AT_POSITION", "");
        public static OpcTag MAST_STOW_AT_POSITION = new OpcTag("MAST_STOW_AT_POSITION", "");

        public static OpcTag VERTICAL_BOOM_ANGLE = new OpcTag("VERTICAL_BOOM_ANGLE", "");
        public static OpcTag VERTICAL_BOOM_DEPLOY_AT_POSITION = new OpcTag("VERTICAL_BOOM_DEPLOY_AT_POSITION", "");
        public static OpcTag VERTICAL_BOOM_STOW_AT_POSITION = new OpcTag("VERTICAL_BOOM_STOW_AT_POSITION", "");

        public static OpcTag LINAC_DEPLOY_LHS_AT_POSITION = new OpcTag("LINAC_DEPLOY_LHS_AT_POSITION", "");
        public static OpcTag LINAC_DEPLOY_RHS_AT_POSITION = new OpcTag("LINAC_DEPLOY_RHS_AT_POSITION", "");
        public static OpcTag LINAC_STOW_AT_POSITION = new OpcTag("LINAC_STOW_AT_POSITION", "");

        public static OpcTag COLLISION_DETECTION_ENABLE = new OpcTag("COLLISION_DETECTION_ENABLE", "");

        public static OpcTag RADIATION_MONITOR = new OpcTag("RADIATION_MONITOR", "RADIATION_MONITOR");

        public static OpcTag BOOM_SIREN_ENABLE = new OpcTag("BOOM_SIREN_ENABLE", "");
    }
}
