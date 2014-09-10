
namespace L3.Cargo.Communications.APCS.Common
{
    public delegate void ApcsUpdateHandler(CommandEnum command, ActionEnum action, byte subAction, object data);

    public enum BooleanValue : byte
    {
        True = 1,
        False = 0
    }

    public enum ScanEnergyMode : byte
    {
        High = 1,
        Low = 2,
        Dual = 3,
		LowDose = 4
    }

    public enum OperatingMode : byte
    {
        AdaptiveMobile = 1,
        AdaptivePortal = 2,
        NonAdaptiveMobile = 3,
        NonAdpativePortal = 4
    }

    public enum AdaptiveSpeedFeedbackConfig : byte
    {
        Disabled = 0,
        EnabledWithFreq = 1
    }

    public enum PWMOutputConfig : byte
    {
        OutputDisabled = 0,
        OutputEnabled = 1
    }

    public enum PulseWidth : byte
    {
        PulseWidth1 = 1,
        PulseWidth2 = 2,
        PulseWidth3 = 3,
        PulseWidth4 = 4
    }

    public enum CommandEnum : byte
    {
        ScanMode = 1,
        OperatingMode = 2,
        StaticPulseFreq = 3,
        AdaptiveModeToTrigRatio = 4,
        AdaptiveSpeedFeedbackConfig = 5,
        ConfigPulseWidth = 6,
        PulseWidth = 7,
        PWMOutput = 8,
        ResetBoard = 31,
        CANMessage = 61,
        AdaptiveModeSpeed = 62,
        SignOfLife = 63
    }

    public enum ActionEnum
    {
        Response = 0,
        Set = 1,
        Get = 2,
        UnsolicitedMsg = 3
    }
}
