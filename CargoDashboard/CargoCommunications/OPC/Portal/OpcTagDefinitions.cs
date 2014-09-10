using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3.Cargo.Communications.OPC.Portal
{
    //order should match the ICD
    public enum SCAN_OPERATIONAL_MODE_VALUE
    {
        ScanMode,
        PortalMode,
        MaintenanceMode,
        RoadMode
    }

    public enum SCAN_STATE_VALUE
    {
        Ready,
        NotReady,
        Scanning
    }

    public enum VEHICLE_TYPE
    {
        None,
        Small,
        Medium,
        Large
    }

    public enum CALIBRATION_STATE_VALUE
    {
        Ready,
        NotReady,
        Failed,
        HighEnergy,
        HighEnergyComplete,
        LowEnergy,
        LowEnergyComplete,
        DualEnergy,
        DualEnergyComplete,
        Completed,
        DarkData,
        DarkDataComplete,
        LowDoseLowEnergy,
        LowDoseLowEnergyComplete,
        TriggerStart = 99
    }

    public enum LINAC_STATE_VALUE
    {
        AcPoweredOff,
        AcPoweredOn,
        WarmingUp,
        ReadyForHVonCommand,
        XraysOff,
        XRaysOn
    }

    public enum SCAN_DRIVE_SPEED_FAILURE_VALUE
    {
        NoFailures,
        MinSpeedThreshold,
        MaxSpeedThreshold
    }

    public enum SCAN_DRIVE_STATE_VALUE
    {
        Stopped,
        Accelerating,
        Decelerating,
        Running
    }

    public enum LINAC_STATUS_VALUE
    {
        NoFailures,
        MajorFault,
        ResettableFault
    }

    public enum LINAC_TURN_ON_XRAYS_VALUE
    {
        Enable,
        Disable
    }

    public enum LINAC_ENERGY_TYPE_VALUE
    {
        Dual,
        High,
        Low,
        LowDose
    }
}
