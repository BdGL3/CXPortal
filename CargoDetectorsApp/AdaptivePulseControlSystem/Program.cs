using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;
using GHIElectronics.NETMF.Native;

using L3.Cargo.Communications.APCS.Common;

namespace L3.Cargo.APCS
{
    public class Program
    {
        static string rlpUserId = "SARFRAZ.AZMI@L-3COM.COMCF03F0FDB";
        static byte[] rlpKey = new byte[] { 0xFE, 0xC1, 0x23, 0xE4, 0x60, 0xCF, 0x0C, 0xF4, 0x9E, 0xF1, 0x25, 0x4C, 0xE4, 0x4E, 0x7E, 0x62, 0xBF, 0xDA, 0x9A, 0x1F, 0xB1, 0x70, 0xBE, 0xD3, 0xB8, 0x76, 0x69, 0x0B, 0xEF, 0x86, 0xFE, 0x58 };

        static NetServer hostAccess;
        static PulseCounter pc;

        static float[] PulseWidthsDutyCycle= { 1.0F, 2.0F, 3.0F, 4.0F };

        static ScanEnergyMode currentEnergyMode = ScanEnergyMode.Dual;   

        static PulseWidth currentPulseWidth = PulseWidth.PulseWidth1;
        static int currentStaticPulseFreq = 200;
        static int[] StaticPulseFreq = { 200, 200 };

        static OperatingMode currentOperatingMode = OperatingMode.NonAdaptiveMobile;

        static byte[] elfImage;

        static Timer adaptiveSpeedMsgTimer;
        static float adaptiveSpeedMsgFreq = 0.0f;
        static AdaptiveSpeedFeedbackConfig speedMsgMode = AdaptiveSpeedFeedbackConfig.Disabled;

        static OutputPort Reset;
        static OutputPort Preset;

        public static void Main()
        {
            hostAccess = new NetServer();
            pc = new PulseCounter(currentOperatingMode);
            hostAccess.ProcessCommandEvent += new NetServer.ProcessCommandEventHandler(hostAccess_ProcessCommandEvent);
            Reset = new OutputPort(USBizi.Pin.IO57, true);
            Preset = new OutputPort(USBizi.Pin.IO59, true);
            Configure();
            adaptiveSpeedMsgTimer = new Timer(new TimerCallback(AdaptiveSpeedTimerCallback), null, Timeout.Infinite, Timeout.Infinite);
            hostAccess.Start();          
        }

        static void LoadRLP()
        {
            // Make sure to enable and unlock RLP before use!
            RLP.Enable();

            // Unlock RLP
            RLP.Unlock(rlpUserId, rlpKey);

            // Load RLP driver
            elfImage = Resources.GetBytes(Resources.BinaryResources.PWMGenerator);
            if (elfImage == null)
                throw new Exception("Failed to load ELF image");

            RLP.LoadELF(elfImage);
            RLP.InitializeBSSRegion(elfImage);
        }

        static void UnloadRLP()
        {
            // We don't need this anymore
            elfImage = null;
            Debug.GC(true);
        }

        static void hostAccess_ProcessCommandEvent(CommandDefinition.CommandFormat command)
        {
            if (command.CommandAck.Command == CommandEnum.ScanMode)
            {
                if (command.Action.Action == ActionEnum.Set)
                {
                    currentEnergyMode = (ScanEnergyMode)command.Action.SubAction;

                    PWMOutputConfig pwmConfig = (PWMOutputConfig)pc.GetPWMRunStatus();

                    if (pwmConfig == PWMOutputConfig.OutputEnabled)
                        pc.PWMOutputDisable();

                    if (currentEnergyMode == ScanEnergyMode.Dual)
                    {
                        Reset.Write(true);
                        Preset.Write(true);
                    }
                    else if (currentEnergyMode == ScanEnergyMode.High)
                    {
                        Reset.Write(true);
                        Preset.Write(false);
                    }
                    else
                    {
                        Reset.Write(false);
                        Preset.Write(true);
                    }

                    if (pwmConfig == PWMOutputConfig.OutputEnabled)
                        pc.PWMOutputEnable();
                }

                hostAccess.SendScanModeResponse(currentEnergyMode);
            }
            else if (command.CommandAck.Command == CommandEnum.StaticPulseFreq)
            {
                OperatingMode mode = (OperatingMode)command.Action.SubAction;
                int freq = BitConverter.ToInt32(command.Payload, 0); ;

                if (command.Action.Action == ActionEnum.Set)
                {
                    if (mode == OperatingMode.NonAdaptiveMobile)
                        StaticPulseFreq[0] = freq;
                    else if (mode == OperatingMode.NonAdpativePortal)
                        StaticPulseFreq[1] = freq;

                    if (currentOperatingMode == mode)
                    {
                        currentStaticPulseFreq = freq;
                        pc.UpdatePWMFrequency(freq);
                    }

                    hostAccess.SendStaticPulseFreqResponse(mode, freq);
                }
                else if (command.Action.Action == ActionEnum.Get)
                {
                    if (mode == OperatingMode.NonAdaptiveMobile)
                        freq = StaticPulseFreq[0];
                    else if (mode == OperatingMode.NonAdpativePortal)
                        freq = StaticPulseFreq[1];

                    hostAccess.SendStaticPulseFreqResponse(mode, freq);
                }
            }
            else if (command.CommandAck.Command == CommandEnum.PulseWidth)
            {
                if (command.Action.Action == ActionEnum.Set)
                {
                    if (currentPulseWidth != (PulseWidth)command.Action.SubAction)
                    {
                        currentPulseWidth = (PulseWidth)command.Action.SubAction;
                        pc.UpdatePWMPulseWidth(PulseWidthsDutyCycle[(int)currentPulseWidth - 1]);
                    }
                }

                hostAccess.SendPulseWidthResponse(currentPulseWidth);
            }
            else if (command.CommandAck.Command == CommandEnum.ConfigPulseWidth)
            {
                if (command.Action.Action == ActionEnum.Set)
                {
                    int index = command.Action.SubAction - 1;
                    PulseWidthsDutyCycle[index] = BitConverter.ToSingle(command.Payload, 0);

                    if (currentPulseWidth == (PulseWidth)command.Action.SubAction)
                        pc.UpdatePWMPulseWidth(PulseWidthsDutyCycle[index]);
                }

                PulseWidth width = (PulseWidth)command.Action.SubAction;
                hostAccess.SendPulseWidthConfigResponse(width, PulseWidthsDutyCycle[(byte)width - 1]);
            }
            else if (command.CommandAck.Command == CommandEnum.OperatingMode)
            {
                short minFreq, maxFreq;

                if (command.Action.Action == ActionEnum.Set)
                {
                    minFreq = (short)BitConverter.ToInt16(command.Payload, 0);
                    maxFreq = (short)BitConverter.ToInt16(command.Payload, 2);
                    currentOperatingMode = (OperatingMode)command.Action.SubAction;

                    if ((currentOperatingMode == OperatingMode.NonAdaptiveMobile) || (currentOperatingMode == OperatingMode.NonAdpativePortal))
                    {
                        if (currentOperatingMode == OperatingMode.NonAdaptiveMobile)
                            currentStaticPulseFreq = StaticPulseFreq[0];
                        else
                            currentStaticPulseFreq = StaticPulseFreq[1];

                        pc.SetOperatingMode(currentOperatingMode);
                        pc.UpdatePWMFrequency(currentStaticPulseFreq);
                        pc.UpdatePWMPulseWidth(PulseWidthsDutyCycle[(int)currentPulseWidth - 1]);
                        pc.PWMOutputEnable();
                    }
                    else if ((currentOperatingMode == OperatingMode.AdaptiveMobile) || (currentOperatingMode == OperatingMode.AdaptivePortal))
                    {
                        pc.SetFrequencyRange(currentOperatingMode, minFreq, maxFreq);
                        pc.SetOperatingMode(currentOperatingMode);
                        pc.PWMOutputEnable();
                    }

                    hostAccess.SendOperatingModeResponse(currentOperatingMode, minFreq, maxFreq);
                }
                else if (command.Action.Action == ActionEnum.Get)
                {
                    pc.GetFrequencyRange(currentOperatingMode, out minFreq, out maxFreq);
                    hostAccess.SendOperatingModeResponse(currentOperatingMode, minFreq, maxFreq);
                }
            }
            else if (command.CommandAck.Command == CommandEnum.AdaptiveModeToTrigRatio)
            {
                float ratio;
                OperatingMode mode = (OperatingMode)command.Action.SubAction;

                if (command.Action.Action == ActionEnum.Set)
                {
                    ratio = BitConverter.ToSingle(command.Payload, 0);

                    pc.SetInputToOutputRatio(mode, ratio);

                    hostAccess.SendAdaptiveModeToTrigRatioResponse(mode, ratio);
                }
                else if (command.Action.Action == ActionEnum.Get)
                {
                    pc.GetInputToOutputRatio(mode, out ratio);
                    hostAccess.SendAdaptiveModeToTrigRatioResponse(mode, ratio);
                }
            }
            else if (command.CommandAck.Command == CommandEnum.AdaptiveSpeedFeedbackConfig)
            {
                if (command.Action.Action == ActionEnum.Set)
                {
                    adaptiveSpeedMsgFreq = BitConverter.ToSingle(command.Payload, 0);
                    speedMsgMode = (AdaptiveSpeedFeedbackConfig)command.Action.SubAction;

                    if ((speedMsgMode == AdaptiveSpeedFeedbackConfig.EnabledWithFreq) && (adaptiveSpeedMsgFreq > 0.0f))
                    {
                        int period = (int)(1000.0f / adaptiveSpeedMsgFreq);
                        adaptiveSpeedMsgTimer.Change(period, period);
                    }
                    else
                        adaptiveSpeedMsgTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }

                hostAccess.SendAdaptiveSpeedFeedbackConfigResponse(speedMsgMode, adaptiveSpeedMsgFreq);
            }
            else if (command.CommandAck.Command == CommandEnum.PWMOutput)
            {
                if (command.Action.Action == ActionEnum.Set)
                {
                    if ((PWMOutputConfig)command.Action.SubAction == PWMOutputConfig.OutputEnabled)
                        pc.PWMOutputEnable();
                    else
                        pc.PWMOutputDisable();

                    hostAccess.SendPWMOutputStatus((PWMOutputConfig)command.Action.SubAction);
                }
                else if (command.Action.Action == ActionEnum.Get)
                {
                    hostAccess.SendPWMOutputStatus((PWMOutputConfig)pc.GetPWMRunStatus());
                }
            }
            else if (command.CommandAck.Command == CommandEnum.ResetBoard)
                ResetBoard();
        }

        static void Configure()
        {
            try
            {
                LoadRLP();
                
                // Blink board LED
                RLP.Procedure PwmStop = RLP.GetProcedure(elfImage, "PwmStop");
                RLP.Procedure PWMStart = RLP.GetProcedure(elfImage, "PWMRun");

                PwmStop.Invoke();

                if (currentEnergyMode == ScanEnergyMode.Dual)
                {
                    Reset.Write(true);
                    Preset.Write(true);
                }
                else if (currentEnergyMode == ScanEnergyMode.High)
                {
                    Reset.Write(true);
                    Preset.Write(false);
                }
                else
                {
                    Reset.Write(false);
                    Preset.Write(true);
                }

                RLP.Procedure PWMBlink = RLP.GetProcedure(elfImage, "PWMBlink");

                pc.Initialize(elfImage);

                PWMBlink.Invoke(currentStaticPulseFreq, PulseWidthsDutyCycle[(int)currentPulseWidth - 1], 1);

                PWMStart.Invoke();
            }
            catch { }
            finally { UnloadRLP(); }
        }

        static void AdaptiveSpeedTimerCallback(object param)
        {
            if (hostAccess.IsConnected)
                hostAccess.SendAdaptiveModeSpeed(pc.GetCurrentSpeed(), currentOperatingMode);
        }

        public static void ResetBoard() { pc.ResetBoard(); }
    }
}
