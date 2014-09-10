using System;
using System.Runtime.InteropServices;

namespace L3.Cargo.Communications.Detectors.Common
{    
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct CommPacketHeader
    {
        [FieldOffset(0)]
        public uint SyncID;

        [FieldOffset(4)]
        public messageType MessageID;

        [FieldOffset(6)]
        public ushort ReservedB6toB7;

        [FieldOffset(8)]
        public configurationMode ConfigurationMode;

        [FieldOffset(8)]
        public deviceState DeviceState;

        [FieldOffset(8)]
        public xrayDataState XRayDataState;

        [FieldOffset(8)]
        public dataTransferMode DataTransferMode;

        [FieldOffset(8)]
        public nvmWriteStatus NVMWriteStatus;

        [FieldOffset(8)]
        public ushort PulseCount;

        [FieldOffset(8)]
        public ushort ReservedB8toB9;

        [FieldOffset(8)]
        public ushort NcbId;

        [FieldOffset(10)]
        public ushort PulsePeriod;

        [FieldOffset(10)]
        public ushort NumOfChannels;

        [FieldOffset(10)]
        public ushort ReservedB10toB11;

        [FieldOffset(12)]
        public ushort NumOfReferenceDetectors;

        [FieldOffset(12)]
        public ushort ReservedB12toB13;

        [FieldOffset(14)]
        public uint ReservedB14toB17;

        [FieldOffset(14)]
        public ushort PulseDelayPeriod;

        [FieldOffset(16)]
        public ushort SignOfLifeInterval;

        [FieldOffset(18)]
        public ushort ReservedB18toB19;

        [FieldOffset(20), MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] ReservedB20toB31;

        public byte[] Serialize()
        {
            int rawSize = Marshal.SizeOf(this);
            byte[] rawdata = new byte[rawSize];
            GCHandle handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);
            Marshal.StructureToPtr(this, handle.AddrOfPinnedObject(), false);
            handle.Free();

            //byte swap host to network
            //this will only work with ushort data fields, 2 bytes
            for (int i = 0; i < rawdata.Length; i += 2)
            {
                byte byte1 = rawdata[i];
                byte byte2 = rawdata[i + 1];
                rawdata[i] = byte2;
                rawdata[i + 1] = byte1;
            }

            return rawdata;
        }

        public CommPacketHeader Deserialize(byte[] buffer)
        {
            //byte swap network to host
            //this will only work with ushort data fields, 2 bytes
            for (int i = 0; i < buffer.Length; i += 2)
            {
                byte byte1 = buffer[i];
                byte byte2 = buffer[i + 1];
                buffer[i] = byte2;
                buffer[i + 1] = byte1;
            }

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            CommPacketHeader header = (CommPacketHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(CommPacketHeader));
            handle.Free();

            return header;
        }

        public static string Dump(CommPacketHeader hdrDPK, bool /*labelled?*/ isLbl = false)
        {
            // There's supposed to be a way to do this via Reflection ... oh well..
            string /*text (returned)*/ txt = (isLbl? "CommPacketHeader" : string.Empty) + "{";
            txt += (isLbl? "SyncID " : string.Empty) + hdrDPK.SyncID.ToString();
            txt += "," + (isLbl ? "MessageID " : string.Empty) + hdrDPK.MessageID.ToString();
            txt += "..." + (isLbl ? "ConfigurationMode " : string.Empty) + hdrDPK.ConfigurationMode.ToString();
            txt += "," + (isLbl ? "DeviceState " : string.Empty) + hdrDPK.DeviceState.ToString();
            txt += "," + (isLbl ? "XRayDataState " : string.Empty) + hdrDPK.XRayDataState.ToString();
            txt += "," + (isLbl ? "DataTransferMode " : string.Empty) + hdrDPK.DataTransferMode.ToString();
            txt += "," + (isLbl ? "NVMWriteStatus " : string.Empty) + hdrDPK.NVMWriteStatus.ToString();
            txt += "," + (isLbl ? "PulseCount " : string.Empty) + hdrDPK.PulseCount.ToString();
            txt += "..." + (isLbl ? "NcbId " : string.Empty) + hdrDPK.NcbId.ToString();
            txt += "," + (isLbl ? "PulsePeriod " : string.Empty) + hdrDPK.PulsePeriod.ToString();
            txt += "," + (isLbl ? "NumOfChannels " : string.Empty) + hdrDPK.NumOfChannels.ToString();
            txt += "..." + (isLbl ? "NumOfReferenceDetectors " : string.Empty) + hdrDPK.NumOfReferenceDetectors.ToString();
            txt += "..." + (isLbl ? "PulseDelayPeriod " : string.Empty) + hdrDPK.PulseDelayPeriod.ToString();
            txt += "," + (isLbl ? "SignOfLifeInterval " : string.Empty) + hdrDPK.SignOfLifeInterval.ToString();
            txt += "...";
            txt += "}";
            return txt;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XRayInfoIDStruct
    {
        private byte _xRayInfoID;

        public XRayEnergyEnum Energy
        {
            get
            {
                return (XRayEnergyEnum)((_xRayInfoID) & 1);
            }
            set
            {
                byte intValue = (byte)(Enum.ToObject(typeof(XRayEnergyEnum), value));
                _xRayInfoID = (byte)(_xRayInfoID | (intValue & 1));
            }
        }

        public int PulseWidth
        {
            get
            {
                return (byte)((_xRayInfoID >> 1) & 3);
            }
            set
            {
                _xRayInfoID = (byte)(_xRayInfoID | (value << 1));
            }
        }

        public XRayInfoIDStruct(XRayEnergyEnum energy, byte pulseWidth)
        {
            byte xrayEnergy = (byte)(energy);
            _xRayInfoID = (byte)((pulseWidth << 1) | (xrayEnergy & 1));
        }

        public static string Dump(XRayInfoIDStruct xryInf, bool /*labelled?*/ isLbl = false)
        {
            string /*text (returned)*/ txt = "{" +
                (isLbl ? "Energy " : string.Empty) + xryInf.Energy.ToString() + "," +
                (isLbl ? "PulseWidth " : string.Empty) + xryInf.PulseWidth.ToString() + "}";
            return txt;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DataPacketHeader
    {
        public uint LineID;

        public ushort StartNumOfDetectors;

        public ushort EndNumOfDetectors;

        public XRayInfoIDStruct EnergyAndPulsewidth;

        public byte NumBytesPerPixel;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public byte[] Reserved;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1440)]
        public byte[] ChData;

        public DataPacketHeader Deserialize(byte[] buffer)
        {
            byte LineIDbyte3 = buffer[3];
            byte LineIDbyte2 = buffer[2];
            byte LineIDbyte1 = buffer[1];
            byte LineIDbyte0 = buffer[0];

            //swap LineID bytes network to host
            buffer[0] = LineIDbyte3;
            buffer[1] = LineIDbyte2;
            buffer[2] = LineIDbyte1;
            buffer[3] = LineIDbyte0;

            //byte swap network to host
            //only handle start and end of detector fields.
            for (int i = 4; i < 8; i += 2)
            {
                byte byte1 = buffer[i];
                byte byte2 = buffer[i + 1];
                buffer[i] = byte2;
                buffer[i + 1] = byte1;
            }

            //chData is 3bytes wide per channel, swap byte 1 and 3
            for (int i = 20; i < buffer.Length; i += 3)
            {
                byte byte1 = buffer[i];
                byte byte3 = buffer[i + 2];

                buffer[i] = byte3;
                buffer[i + 2] = byte1;
            }

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            DataPacketHeader header =
                (DataPacketHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(DataPacketHeader));
            handle.Free();
            return header;
        }

        public static string Dump(DataPacketHeader hdrDPK, bool /*labelled?*/ isLbl = false)
        {
            string /*text (returned)*/ txt = (isLbl ? "DataPacketHeader" : string.Empty) + "{";
            txt += (isLbl ? "LineID " : string.Empty) + hdrDPK.LineID.ToString();
            txt += ", " + (isLbl? "Start/EndNumOfDetectors" : string.Empty) + "[" +
                    hdrDPK.StartNumOfDetectors.ToString() + "," +
                    hdrDPK.EndNumOfDetectors.ToString() + "]";
            txt += "," + XRayInfoIDStruct.Dump(hdrDPK.EnergyAndPulsewidth, isLbl);
            txt += "," + (isLbl ? "NumBytesPerPixel " : string.Empty) +
                    hdrDPK.NumBytesPerPixel.ToString();
            txt += "}";
            return txt;
        }
    }
}
