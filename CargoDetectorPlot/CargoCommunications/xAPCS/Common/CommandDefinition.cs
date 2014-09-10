using System;
using System.Runtime.InteropServices;

namespace L3.Cargo.Communications.APCS.Common
{
    public class CommandDefinition
    {
        const int _packetSize = 12;

        public static int PacketSize
        {
            get { return _packetSize; }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 1)]
        public struct ActionStruct
        {
            private byte _actionAndSubAction;

            public byte ActionAndSubAction
            {
                get { return _actionAndSubAction; }
                set { _actionAndSubAction = value; }
            }

            public ActionEnum Action
            {
                get
                {
                    ActionEnum action = (ActionEnum)(_actionAndSubAction >> 4);
                    return action;
                }
                set
                {
                    _actionAndSubAction = (byte)((_actionAndSubAction & 0x0F) | ((byte)value << 4));
                }
            }

            public byte SubAction
            {
                get
                {
                    return (byte)(_actionAndSubAction & 0x0F);
                }
                set
                {
                    _actionAndSubAction = (byte)((_actionAndSubAction & 0xF0) | value);
                }
            }

            public ActionStruct(ActionEnum action, byte subAction)
            {
                _actionAndSubAction = (byte)(((byte)action << 4) | subAction);
            }

            public ActionStruct(byte action)
            {
                _actionAndSubAction = action;
            }
        }

        [StructLayout(LayoutKind.Sequential, Size = 1, Pack = 1)]
        public struct CommandAckStruct
        {

            private byte _commandWithAck;

            public byte CommandWithAck
            {
                get { return _commandWithAck; }
                set { _commandWithAck = value; }
            }

            public CommandEnum Command
            {
                get
                {
                    CommandEnum cmd = (CommandEnum)(_commandWithAck >> 2);
                    return cmd;
                }
                set
                {
                    byte intValue = (byte)value;
                    _commandWithAck = (byte)((_commandWithAck & 3) | (intValue << 2));
                }
            }

            public BooleanValue AckRequired
            {
                get
                {
                    return (BooleanValue)((_commandWithAck >> 1) & 1);
                }
                set
                {
                    _commandWithAck = (byte)((_commandWithAck & 0xFD) | ((byte)value << 1));
                }
            }

            public BooleanValue IsAck
            {
                get
                {
                    return (BooleanValue)(_commandWithAck & 1);
                }
                set
                {
                    _commandWithAck = (byte)((_commandWithAck & 0xFE) | (byte)value);
                }
            }

            public CommandAckStruct(CommandEnum cmd, BooleanValue ackReq, BooleanValue isAck)
            {
                _commandWithAck = (byte)(((byte)cmd << 2) | ((byte)ackReq << 1) | (byte)isAck);
            }

            public CommandAckStruct(byte cmd)
            {
                _commandWithAck = cmd;
            }

        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = _packetSize)]
        public struct CommandFormat
        {
            public CommandAckStruct CommandAck;

            public ActionStruct Action;

            public byte[] Payload;

            public CommandFormat(CommandAckStruct cmd, ActionStruct action, byte[] payload)
            {
                CommandAck = cmd;
                Action = action;
                Payload = new byte[10];
                payload.CopyTo(Payload, 0);
            }

            public CommandFormat(CommandAckStruct cmd, ActionStruct action)
            {
                CommandAck = cmd;
                Action = action;
                Payload = new byte[10];
            }

            public CommandFormat(byte cmd, byte action, byte[] payload)
            {
                CommandAck = new CommandAckStruct(cmd);
                Action = new ActionStruct(action);
                Payload = new byte[10];
                payload.CopyTo(Payload, 0);
            }


            public CommandFormat(byte cmd, byte action)
            {
                CommandAck = new CommandAckStruct(cmd);
                Action = new ActionStruct(action);
                Payload = new byte[10];
            }


            public byte[] Serialize()
            {
                byte[] rawdata = new byte[_packetSize];

                rawdata[0] = CommandAck.CommandWithAck;
                rawdata[1] = Action.ActionAndSubAction;
                Payload.CopyTo(rawdata, 2);

                return rawdata;
            }

            public CommandFormat Deserialize(byte[] buffer)
            {
                CommandFormat header = new CommandFormat(buffer[0], buffer[1]);

                for (int i = 0; i < 10; i++)
                {
                    header.Payload[i] = buffer[2 + i];
                }

                return header;
            }
        }
    }
}
