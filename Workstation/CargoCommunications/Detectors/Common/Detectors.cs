using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3.Cargo.Communications.Detectors.Common
{
    public enum XRayEnergyEnum
    {
        LowEnergy,
        HighEnergy        
    }

    public enum messageType : ushort
    {
        CX_IDENTFY_QUERY = 0x0102,
        CX_IDENTFY_ACK = 0x0103,
        CX_DEVICE_CONFIG_NORMAL_WITH_ACK = 0x0201,
        CX_DEVICE_CONFIG_ACK = 0x0203,
        CX_CONFIG_PARAMETERS_NORMAL_WITH_ACK = 0x0301,
        CX_CONFIG_PARAMETERS_QUERY = 0x0302,
        CX_CONFIG_PARAMETERS_ACK = 0x0303,
        CX_XRAY_DATA_STATE_NORMAL_WITH_ACK = 0x0501,
        CX_XRAY_DATA_STATE_QUERY = 0x0502,
        CX_XRAY_DATA_STATE_ACK = 0x0503,
        CX_DEVICE_STATE_QUERY = 0x0602,
        CX_DEVICE_STATE_ACK = 0x0603,
        CX_SIGNOFLIFE_NORMAL = 0x0700,
        CX_RESET_LINE_COUNT_WITH_ACK = 0x0801,
        CX_RESET_LINE_COUNT_ACK = 0x0803,
        CX_CONFIG_PARAMETERS_WRITE_NORMAL_WITH_ACK = 0x1001,
        CX_CONFIG_PARAMETERS_WRITE_ACK = 0x1003
    }

    public enum configurationMode : ushort
    {
        End = 0x0000,
        Begin = 0x0001
    }

    public enum dataTransferMode : ushort
    {
        Stop = 0x0000,
        Start = 0x0001
    }

    public enum deviceState : ushort
    {
        Ready = 0x0003,
        Active = 0x0004,
        Configuration = 0x0005
    }

    public enum xrayDataState : ushort
    {
        Ready = 0x0000,
        Active = 0x0001
    }

    public enum nvmWriteStatus : ushort
    {
        Failed = 0x0000,
        Success = 0x0001
    }
}
