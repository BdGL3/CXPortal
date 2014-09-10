using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace L3.Cargo.Communications.Linac
{
    public class LinacPacketFormat
    {
        public enum CommandEnum : byte
        {
            Ping = 0x00,
            Read = 0x01,
            Write = 0x02,
            Unsolicited = 0xFF
        }


        public enum TypeEnum : byte
        {
            Query = 0xAA,
            OneWay = 0xBB,
            Response = 0xCC
        }

      

        public enum VariableIdEnum : short
        {
            Unused = 0x0000,
            OperatingParameters = 0x0001,
            State = 0x0002,
            Fault = 0x0003,

            GD_Cathod_V_Monitor = 0x000A,
            GD_Filament_V_Monitor = 0x000B,
            GD_Filament_I_Mon = 0x000C,

            GD_Grid_A_V_Mon = 0x000D,
            GD_Grid_B_V_Mon = 0x000E,
            GD_Grid_Bias_Mon = 0x000F,
            GD_Beam_I_Mon = 0x0010,
            GD_State_Mon = 0x0011,

            MD_HVPSVoltage = 0x0014,
            MD_Filament_V_Mon = 0x0015,
            MD_Filament_I_Mon = 0x0016,
            Mod_State_Mon = 0x0017,
            Magnetron_I_Mon = 0x0018,
            Warm_Up_Timer_Mon = 0x001D,
            Sol_V_Mon = 0x001E,
            Sol_I_Mon = 0x001F,
            Steering_X_I_Mon = 0x0023,
            Steering_Y_I_Mon = 0x0024,
            IonPump_1_I_Mon = 0x0028,
            IonPump_1_V_Mon = 0x0029,
            IonPump_2_I_Mon = 0x002A,
            IonPump_2_V_Mon = 0x002B,
            IonPump_3_I_Mon = 0x002C,
            IonPump_3_V_Mon = 0x002D,
            Reflected_Power = 0x0032,
            Forward_Power = 0x0033,
            Stepper_PV = 0x0034,
            AFC_PV = 0x0035,
            PPG_PRF_Single = 0x0064,
            PPG_PRF_Interleave = 0x0065,
            PPG_Pulsewidth = 0x0066,
            PPG_Mod_Delay = 0x0067,
            PPG_Mod_PW = 0x0068,
            PPG_E_Gun_Delay = 0x0069,
            PPG_E_Gun_PW = 0x006A,
            PPG_Sample_Delay = 0x006B,
            PPG_Sample_PW = 0x006C,
            PPG_Sample_Delay_AFC = 0x006D,
            PPG_Sample_PW_AFC = 0x006E,
            GD_Cathode_V_Set = 0x006F,
            GD_Filament_V_Set = 0x0070,
            GD_Grid_A_V_Set = 0x0071,
            GD_Grid_B_V_Set = 0x0072,
            Mod_HVPS_V_Set = 0x0078,
            Mod_Filament_V_Set = 0x0079,
            Mod_PW_Set = 0x007A,
            Warm_Up_Timer_Set = 0x0081,
            Sol_V_Set = 0x0082,
            Sol_I_Set = 0x0083,
            Steering_X_I_Set = 0x0087,
            Steering_Y_I_Set = 0x0088,
            Stepper_SP = 0x0098,
            AFC_SP = 0x0099,
            Stepper_Proportional = 0x00A0,
            Stepper_Integral = 0x00A1,
            Stepper_Derivative = 0x00A2,
            AFC_CF_Cutoff = 0x00A3,
            AFC_Coarse = 0x00A4,
            AFC_Fine = 0x00A5,
            AFC_Update_Threshold =0x00A6,
            AFC_Deadband = 0x00A7
        }

        public enum DataTypeEnum : byte
        {
            None = 0x00,
            Decimal=0xDD,
            Float=0xFF,
            String=0xEE
        }

        public enum Errors : uint
        {
            VariableIDNotFound = 0x0000,
            OutOfRange = 0x0001,
            AccessNotAllowed = 0x0002,
            WriteProtected = 0x0003,
            IncompatibleType = 0x0004,
            Reserved_5 = 0x0005,
            ErrorByteNotZero = 0x0006,
            Reserved_7 = 0x0007
        }


        [Flags]
        public enum Faults : uint
        {
            MagnetronCurrent = ((uint)1) << 0,
            ReflectedRFPower = ((uint)1) << 1,
            WaveguideArc = ((uint)1) << 2,
            SF6Pressure = ((uint)1) << 3,
            EnclosurePanels = ((uint)1) << 4,
            AcceleratorVacuum = ((uint)1) << 5,
            MagnetronVacuum = ((uint)1) << 6,
            SolenoidCurrent =((uint)1)<< 7,
            AcceleratorFlowFault =((uint)1)<< 8,
            CoolantTemperature =((uint)1)<< 9,
            EnclosureTemperature =((uint)1)<< 10,
            GunDriverInterlock =((uint)1)<< 11,
            ModulatorInterlock =((uint)1)<< 12,
            CustomerInterlock1 =((uint)1)<< 13,
            CustomerInterlock2 =((uint)1)<< 14,
            ModulatorCommunicationError =((uint)1)<< 15,
            AFCStepperMotorOutOfBound =((uint)1)<< 16,
            Reserved_17 =((uint)1)<< 17,
            Reserved_18 =((uint)1)<< 18,
            Reserved_19 =((uint)1)<< 19,
            Reserved_20 =((uint)1)<< 20,
            Reserved_21 =((uint)1)<< 21,
            Reserved_22 =((uint)1)<< 22,
            Reserved_23 =((uint)1)<< 23,
            Reserved_24 =((uint)1)<< 24,
            Reserved_25 =((uint)1)<< 25,
            Reserved_26 =((uint)1)<< 26,
            Reserved_27 =((uint)1)<< 27,
            Reserved_28 =((uint)1)<< 28,
            Reserved_29 =((uint)1)<< 29,
            Reserved_30 =((uint)1)<< 30,
        };

      

        [StructLayout(LayoutKind.Sequential, Size = 8, Pack = 1)]
        public struct CommandPacket
        {
            //Sync - 4 bytes
            public byte Reserved_1;

            public byte Reserved_2;

            public byte Reserved_3;

            public byte Reserved_4;

            public CommandEnum Command; //Command - 1 byte

            public TypeEnum Type; // Type - 1 Byte

            public byte Reserved2; // Padding - 1 Byte

            public byte Size; // Size - 1 byte - Indicates number of packet(s) following the Command Packet

            public CommandPacket(CommandEnum command, TypeEnum type)
            {
                Reserved_1 = 0x16;
                Reserved_2 = 0x16;
                Reserved_3 = 0x16;
                Reserved_4 = 0x16;

                this.Command = command;
                this.Type = type;
                Size = 0;
                Reserved2 = 0;

            }

            public byte[] Serialize()
            {
                int rawSize = Marshal.SizeOf(this);
                byte[] rawdata = new byte[rawSize];
                GCHandle handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);
                Marshal.StructureToPtr(this, handle.AddrOfPinnedObject(), false);
                handle.Free();

                //byte swap host to network
                //this will only work with ushort data fields, 2 bytes
                //for (int i = 0; i < rawdata.Length; i += 2)
                //{
                //    byte byte1 = rawdata[i];
                //    byte byte2 = rawdata[i + 1];
                //    rawdata[i] = byte2;
                //    rawdata[i + 1] = byte1;
                //}

                return rawdata;
            }

            public CommandPacket Deserialize(byte[] buffer)
            {
                //byte swap network to host
                //this will only work with ushort data fields, 2 bytes
                //for (int i = 0; i < buffer.Length; i += 2)
                //{
                //    byte byte1 = buffer[i];
                //    byte byte2 = buffer[i + 1];
                //    buffer[i] = byte2;
                //    buffer[i + 1] = byte1;
                //}

                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                CommandPacket header = (CommandPacket)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(CommandPacket));
                handle.Free();

                return header;
            }
        }

        [StructLayout(LayoutKind.Sequential, Size = 8, Pack = 1)]
        public struct DataPacket
        {
            public VariableIdEnum variableId;

            public byte error;

            public DataTypeEnum dataType;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] data;

            public byte[] Serialize()
            {
                int rawSize = Marshal.SizeOf(this);
                byte[] rawdata = new byte[rawSize];
                GCHandle handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);
                Marshal.StructureToPtr(this, handle.AddrOfPinnedObject(), false);
                handle.Free();

                //swap first two bytes
                byte byte0 = rawdata[0];
                rawdata[0] = rawdata[1];
                rawdata[1] = byte0;                

                return rawdata;
            }

            public DataPacket Deserialize(byte[] buffer)
            {
               //swap first two bytes
                byte byte0 = buffer[0];
                buffer[0] = buffer[1];
                buffer[1] = byte0;

                //swap 4th and last (7th)
                byte byte4 = buffer[4];
                buffer[4] = buffer[7];
                buffer[7] = byte4;

                // swap 5th and 6th byte
                byte byte5 = buffer[5];
                buffer[5] = buffer[6];
                buffer[6] = byte5;



                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                DataPacket header = (DataPacket)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(DataPacket));
                handle.Free();

                return header;
            }
        }
    }
}
