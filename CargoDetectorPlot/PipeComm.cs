using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Security.AccessControl;
using System.Collections.Specialized;
using System.Configuration;

namespace L3.Cargo.DetectorPlot
{
    public class PipeComm
    {
        #region Pipes
        public string SaveDataPath = "C:\\Pipe Data";
        String strPipeName1 = "CXMSG1";
        String strPipeName2 = "CXDATA1";
        public string strServerName = ".";//same computer;"HOST";//for test only 
        public NamedPipeServerStream MsgSrvHandle;
        public NamedPipeServerStream DataSrvHandle;
        #endregion


        #region CX_Constants
        public const int CX_IDENIFY = 1;
        public const int CX_INIT_INFO = 2;
        public const int CX_STATE = 3;
        public const int CX_SIGNOFLIFE = 4;
        public const int CX_ERROR = 5;
        public const int CX_ADVANCE_INIT_INFO = 6;
        public const int MSG_BUFFER_SIZE = 16;
        //msg-type
        public const int CX_MSG_NORMAL = 0x0000;
        public const int CX_MSG_QUERY = 0x0001;
        public const int CX_MSG_ACK = 0x0002;
        public const int CX_MSG_ERROR = 0x00FF;
        // computer group ids
        public const uint CX_HOST_ID = 0x30303030;
        public const uint CX_CG_00 = 0x30303030;
        public const uint CX_CG_01 = 0x31313131;
        public const uint CX_CG_02 = 0x32323232;
        public const uint CX_CG_03 = 0x33333333;
        public const uint CX_CG_04 = 0x34343434;
        public const uint CX_CG_05 = 0x35353535;
        public const uint CX_CG_06 = 0x36363636;
        public const uint CX_CG_07 = 0x37373737;
        public const uint CX_CG_08 = 0x38383838;
        public const uint CX_CG_09 = 0x39393939;
        public const uint CX_CG_10 = 0x3A3A3A3A;
        public const uint CX_CG_11 = 0x3B3B3B3B;
        public const uint CX_CG_12 = 0x3C3C3C3C;
        public const uint CX_CG_13 = 0x3D3D3D3D;
        public const uint CX_CG_14 = 0x3E3E3E3E;
        public const uint CX_CG_15 = 0x3F3F3F3F;
        public const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        public const int MAX_DATA_SIZE = 65536;
        //mode
        public const uint CX_MODE_START = 0;  //Start Scan
        public const uint CX_MODE_RUN = 1;        //Xray On
        public const uint CX_MODE_STOP = 2;        //Stop Scan
        #endregion

        #region Pipe_variables
        public struct DAQInfo
        {
            public uint DetNum;
            public uint LineHeaderSize;
            public uint BytesPerPixel;
            public uint Unknown1;
            public uint Unknown2;
        }
        public DAQInfo dqi;

        //============= threads
        public Thread PDatath;
        public Thread pipeData;
        public bool DataPipeConnected;
        public bool DoNotRead;//test
        public bool ShowTooltips;
        public byte[] DataStorage;
        public ushort[] LineData;
        int[] seldet;
        public UInt32 NumofSavedFiles;
        public bool StopReceivingData;
        public int savefilecnt;
        public uint bState;
        public int SaveType;//0-pxe,1-bin,2-both
        public int AnShowMode;//0-spatial,1-time
        public float[,] Pxe4Emulator = new float[0, 0];
        public int Pxe4EmulatorCount = 0;
        public bool IsTestConnection = false;
        public bool PXEDataType = false;
        
        #endregion

        #region Emulator_Mode
        public bool EmulatorEnabled;
        public int EmulatorBoardsCount = 1;
        public byte[] EmulatorData;
        #endregion

        public PipeComm()
        {
        }

        ~PipeComm()
        {
        }

        void Close()
        {
            try
            {
                StopReceivingData = true;
                if (MsgSrvHandle != null)
                    MsgSrvHandle.Close();
                if (DataSrvHandle != null)
                    DataSrvHandle.Close();
                if (pipeData != null)
                {
                    pipeData.Abort();
                }
                this.Close();
            }
            catch { }
        }


        void LogMessage(string s)
        {
            return;
        }

        public void CreatePipe1()
        {
            string ss;
            if (strPipeName1 == "")
            {
                ss = "Cannot create pipe " + strPipeName1;
                LogMessage(ss);
                return;
            }
            if (MsgSrvHandle != null)
                return;
            try
            {
                PipeSecurity pipeSa = new PipeSecurity();

                pipeSa.SetAccessRule(new PipeAccessRule("Everyone",
                    PipeAccessRights.ReadWrite, AccessControlType.Allow));

                MsgSrvHandle = new NamedPipeServerStream(
                                                            strPipeName1,                    // The unique pipe name.
                                                            PipeDirection.InOut,            // The pipe is bi-directional
                                                            NamedPipeServerStream.MaxAllowedServerInstances,
                                                            PipeTransmissionMode.Message,   // Message type pipe 
                                                            PipeOptions.Asynchronous,               // set to asynk mode
                                                            MSG_BUFFER_SIZE,                    // Input buffer size
                                                            MSG_BUFFER_SIZE,                    // Output buffer size
                                                            pipeSa,                         // Pipe security attributes
                                                            HandleInheritability.None       // Not inheritable
                                                            );
                ss = "The server named pipe  " + strPipeName1 + " is created";

                LogMessage(ss);

                if (EmulatorEnabled == false)
                {
                    MsgSrvHandle.WaitForConnection();
                }

                if (MsgSrvHandle.IsConnected)
                {
                    byte[] res = new byte[16];
                    byte[] bReply = new byte[16];
                    res = SendClientMessage1();
                    if (res.Length == 16)
                    {
                        //old style
                        //OneMore: res = PrepareMessage(res);
                        //    if (res[4] != CX_MSG_ERROR)
                        //    {
                        //        SendPackage(res);
                        //        bReply = GetPackage();

                        //    }
                        //    if (bReply[5] == CX_MSG_ACK)
                        //    {
                        //        Buffer.BlockCopy(bReply, 0, res, 0, 16);
                        //        goto OneMore;
                        //    }
                        //=================================================
                        while (res[4] != CX_MSG_ERROR)
                        {
                            res = PrepareMessage(res);
                            if (res[4] != CX_MSG_ERROR)
                            {
                                SendPackage(res);
                                bReply = GetPackage();
                            }
                            if (bReply[5] == CX_MSG_ACK)
                            {
                                Buffer.BlockCopy(bReply, 0, res, 0, 16);
                            }
                        }
                        //===================================================
                    }//

                }
            }
            catch (Exception ex)
            {
                ss = "Cannot create pipe " + strPipeName1;
                LogMessage(ss);
                LogMessage(ex.Message);
            }
        }


        public void CreatePipe2()
        {
            string ss;
            if (strPipeName2 == "")
            {
                ss = "Cannot create pipe " + strPipeName2;
                LogMessage(ss);//DataPipeInfoTxt, ss);
                return;
            }
            if (DataSrvHandle != null)
                return;
            try
            {
                PipeSecurity pipeSa = new PipeSecurity();
                pipeSa.SetAccessRule(new PipeAccessRule("Everyone",
                    PipeAccessRights.ReadWrite, AccessControlType.Allow));
                DataSrvHandle = new NamedPipeServerStream(
                    strPipeName2,                    // The unique pipe name.
                    PipeDirection.In,            // The pipe is read only
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Message,   // Message type pipe 
                    PipeOptions.Asynchronous,               // No additional parameters
                    MAX_DATA_SIZE,
                    // Input buffer size
                    MAX_DATA_SIZE,                    // Output buffer size
                    pipeSa,                         // Pipe security attributes
                    HandleInheritability.None       // Not inheritable
                    );
                ss = "The  server named pipe  " + strPipeName2 + " is created";
                LogMessage(ss);//DataPipeInfoTxt, ss);
                if (EmulatorEnabled == false)
                {
                    DataSrvHandle.WaitForConnection();
                }
                ss = "The  named pipe  " + strPipeName2 + " is connected";
                LogMessage(ss);//DataPipeInfoTxt, ss);
                DataPipeConnected = true;
            }
            catch (Exception ex)
            {
                ss = "Cannot create pipe " + strPipeName2;
                LogMessage(ss);//DataPipeInfoTxt, ss);
                LogMessage(ex.Message);//DataPipeInfoTxt, ex.Message);
                DataPipeConnected = false;
            }
        }

        bool SendPackage(byte[] pkg)
        {
            string msg;
            DecodePackage(pkg);
            if (MsgSrvHandle.CanWrite)
            {
                MsgSrvHandle.Write(pkg, 0, pkg.Length);
                MsgSrvHandle.Flush();
                MsgSrvHandle.WaitForPipeDrain();
                msg = "Sends " + pkg.Length.ToString() + "  bytes; Message: " + GetMsgName(pkg[4]) + "Type  " + GetMsgType(pkg[5]);
                LogMessage(msg);
                return true;
            }
            else
            {
                msg = "Unable to write to pipe " + strServerName + " " + strPipeName1;
                LogMessage(msg);
                return false;
            }
        }

        byte[] SendClientMessage1()
        {
            string msg;
            byte[] bRequest = new byte[0];
            byte[] bReply = new byte[MSG_BUFFER_SIZE];
            try
            {
                int cbRequestBytes;
                bRequest = PrepareMessageFirstMessage();
                cbRequestBytes = bRequest.Length;

                if (MsgSrvHandle == null)
                {

                    bReply = new byte[1];
                    return bReply;
                }

                bool bret = SendPackage(bRequest);

                bReply = GetPackage();
            }
            catch (TimeoutException ex)
            {
                msg = "Unable to open named pipe " + strServerName + " " + strPipeName1;
                LogMessage(msg);
                LogMessage(ex.Message);
                bReply = new byte[1];
                return bReply;
            }
            catch (Exception ex)
            {
                msg = "The client throws the error: " + ex.Message;
                LogMessage(msg);
                bReply = new byte[1];
                return bReply;
            }
            return bReply;
        }
        //===========================================
        byte[] PrepareMessageFirstMessage()
        {
            byte[] t = new byte[16];
            uint tt = CX_CG_01;//Can be other number
            byte[] b1;
            b1 = BitConverter.GetBytes(tt);
            for (int k = 0; k < 4; k++)
            {
                t[k] = b1[k];//fill first 4 bytes//ID
            }
            t[4] = CX_IDENIFY;//msg ID
            t[5] = CX_MSG_QUERY;//msg type
            t[6] = 1;//seq number
            BitVector32 myBV = new BitVector32(0);//disassembe to bits
            UInt16 bytesPerPixel = Convert.ToUInt16(ConfigurationManager.AppSettings["BytesPerPixel"]);
            UInt16 spatialRate = Convert.ToUInt16(ConfigurationManager.AppSettings["Spacial_Rate"]);
            byte[] ddw1 = BitConverter.GetBytes(bytesPerPixel);
            byte[] ddw2 = BitConverter.GetBytes(spatialRate);
            t[8] = ddw1[0];
            t[9] = ddw1[1];
            t[10] = ddw2[0];
            t[11] = ddw2[1];
            //word 2(32 bits)
            myBV = new BitVector32(0);
            UInt32 dw2 = (UInt32)myBV.Data;
            byte[] ddw3 = BitConverter.GetBytes(dw2);
            Buffer.BlockCopy(ddw3, 0, t, 12, ddw3.Length);
            DoNotRead = false;
            return t;
        }
        //=============================================
        string GetMsgName(int msg)
        {
            string nm = "";
            switch (msg)
            {
                case 1:
                    nm = "Idenify";
                    break;
                case 2:
                    nm = "Init Info";
                    break;
                case 3:
                    nm = "State";
                    break;
                case 4:
                    nm = "Sign of life";
                    break;
                case 5:
                    nm = "Error";
                    break;
                case 6:
                    nm = "Adv Init Info";
                    break;
                default:
                    nm = "";
                    break;
            }
            return nm;
        }

        string GetMsgType(int msg)
        {
            string nm = "";
            switch (msg)
            {
                case 0:
                    nm = "Normal";
                    break;
                case 1:
                    nm = "Query";
                    break;
                case 2:
                    nm = "Ask";
                    break;
                case 0x00FF:
                    nm = "Error";
                    break;
                default:
                    nm = "";
                    break;
            }
            return nm;
        }
        //=========================== 
        byte[] GetPackage()
        {
            int cbBytesRead;
            byte[] bReply = new byte[16];
            string strMessage, msg;
            if (DoNotRead == true)
                return bReply;
            try
            {
                if (MsgSrvHandle.IsConnected)
                {
                    if (MsgSrvHandle.CanRead && MsgSrvHandle.InBufferSize >= MSG_BUFFER_SIZE && MsgSrvHandle.IsMessageComplete)
                    {
                        cbBytesRead = MsgSrvHandle.Read(bReply, 0, 16);
                        strMessage = Encoding.Unicode.GetString(bReply).TrimEnd('\0');
                        msg = "Receives " + cbBytesRead.ToString() + " bytes; Message: " + strMessage;
                        LogMessage(msg);
                        DecodePackage(bReply);
                        return bReply;
                    }
                    else
                    {
                        msg = "Cannot read";
                        LogMessage(msg);
                        bReply = new byte[1];
                        return bReply;
                    }
                }
                else
                {
                    msg = "Lost connection";
                    LogMessage(msg);
                    bReply = new byte[1];
                    return bReply;
                }
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message);
                bReply = new byte[1];
                return bReply;
            }
        }
        //==============================================
        private void DecodePackage(byte[] pkg)
        {
            byte[] id = new byte[4];
            byte[] dw1 = new byte[4];
            byte[] dw2 = new byte[4];
            Buffer.BlockCopy(pkg, 0, id, 0, 4);
            Int32 Nid = BitConverter.ToInt32(id, 0);

            Buffer.BlockCopy(pkg, 8, dw1, 0, sizeof(UInt32));
            Buffer.BlockCopy(pkg, 12, dw2, 0, sizeof(UInt32));
            Int32 dww1 = BitConverter.ToInt32(dw1, 0);
            //dw1
            BitVector32 myBV1 = new BitVector32(dww1);//disassembe to bits
            BitVector32.Section overSample = BitVector32.CreateSection(15);
            BitVector32.Section mode = BitVector32.CreateSection(7, overSample);
            BitVector32.Section scanRate = BitVector32.CreateSection(8191, mode);
            BitVector32.Section dmaBuffMultiplier = BitVector32.CreateSection(4095, scanRate);
            //dw2
            Int32 dww2 = BitConverter.ToInt32(dw2, 0);
            BitVector32 myBV2 = new BitVector32(dww2);
            BitVector32.Section linePerDma = BitVector32.CreateSection(15);
            BitVector32.Section integrationTime = BitVector32.CreateSection(16383, linePerDma);
            BitVector32.Section delayPeriod = BitVector32.CreateSection(16383, integrationTime);
            LogMessage("ID= " + String.Format("{0:X}", Nid));
            LogMessage("Message " + GetMsgName((int)pkg[4]));
            LogMessage("Message Type " + GetMsgType((int)pkg[5]));
            LogMessage("Seq number " + pkg[6].ToString());
            LogMessage("overSample " + String.Format("{0}", myBV1[overSample]));
            LogMessage("mode " + String.Format("{0}", myBV1[mode]));
            LogMessage("scanRate " + String.Format("{0}", myBV1[scanRate]));
            LogMessage("dmaBuffMultiplier " + String.Format("{0}", myBV1[dmaBuffMultiplier]));
            LogMessage("linePerDma " + String.Format("{0}", myBV2[linePerDma]));
            LogMessage("integrationTime " + String.Format("{0}", myBV2[integrationTime]));
            LogMessage("delayPeriod " + String.Format("{0}", myBV2[delayPeriod]));
            LogMessage("*************************");
        }

        //=============================================
        byte[] PrepareMessage(byte[] pkg)
        {
            byte[] res = new byte[16];
            byte[] b = new byte[4];
            Buffer.BlockCopy(pkg, 0, b, 0, 4);
            //get data from  message
            UInt32 ServerID = BitConverter.ToUInt32(b, 0);
            int MessageCode = Convert.ToInt32(pkg[4]);
            int MsgType = Convert.ToInt32(pkg[5]);
            string msg = "Send message  " + GetMsgName(MessageCode) + "Type  " + GetMsgType(MsgType);
            LogMessage(msg);

            switch (MessageCode)
            {
                case CX_IDENIFY:
                    if (MsgType == CX_MSG_ACK)
                    {
                        if (ServerID > 0x31313130 && ServerID < 0x37373738)
                        {
                            Buffer.BlockCopy(b, 0, res, 0, 4);
                            BitVector32 myBV = new BitVector32(0);//disassembe to bits

                            BitVector32.Section overSample = BitVector32.CreateSection(15);
                            myBV[overSample] = Convert.ToSByte(ConfigurationManager.AppSettings["OverSample"]);

                            BitVector32.Section mode = BitVector32.CreateSection(7, overSample);
                            myBV[mode] = Convert.ToSByte(ConfigurationManager.AppSettings["Driver_MODE"]);

                            BitVector32.Section scanRate = BitVector32.CreateSection(8191, mode);
                            myBV[scanRate] = Convert.ToInt32(ConfigurationManager.AppSettings["ScanRate"]);

                            BitVector32.Section dmaBuffMultiplier = BitVector32.CreateSection(4095, scanRate);
                            myBV[dmaBuffMultiplier] = Convert.ToInt32(ConfigurationManager.AppSettings["DmaBuffMultiplier"]);

                            UInt32 dw1 = (UInt32)myBV.Data;
                            UInt64 dw13 = (UInt64)myBV.Data;
                            byte[] ddw1 = BitConverter.GetBytes(dw1);
                            Buffer.BlockCopy(ddw1, 0, res, 8, ddw1.Length);
                            UInt32 dw11 = (UInt32)myBV.Data;
                            //word 2(32 bits)
                            myBV = new BitVector32(0);
                            BitVector32.Section linePerDma = BitVector32.CreateSection(15);
                            BitVector32.Section integrationTime = BitVector32.CreateSection(16383, linePerDma);
                            BitVector32.Section delayPeriod = BitVector32.CreateSection(16383, integrationTime);

                            myBV[linePerDma] = Convert.ToSByte(ConfigurationManager.AppSettings["LinesPerDma"]);
                            myBV[integrationTime] = Convert.ToInt16(ConfigurationManager.AppSettings["IntegrationTime"]);
                            myBV[delayPeriod] = Convert.ToInt16(ConfigurationManager.AppSettings["DelayPeriod"]);

                            UInt32 dw2 = (UInt32)myBV.Data;
                            byte[] ddw2 = BitConverter.GetBytes(dw2);
                            Buffer.BlockCopy(ddw2, 0, res, 12, ddw2.Length);

                            //====================================================================== message type
                            res[4] = CX_INIT_INFO;
                            res[5] = CX_MSG_QUERY;
                            DoNotRead = false;

                        }
                        else
                        {
                            res[4] = CX_MSG_ERROR;
                            DoNotRead = true;
                        }
                    }
                    break;
                case CX_INIT_INFO:
                    if (MsgType == CX_MSG_ACK)
                    {
                        Buffer.BlockCopy(pkg, 0, res, 0, 16);
                        byte[] binfo = new byte[2];
                        int bSplitDataLine = Convert.ToInt16(ConfigurationManager.AppSettings["SplitDataLine"]);
                        int AdaptiveScan = Convert.ToInt16(ConfigurationManager.AppSettings["AdaptiveScan"]);

                        binfo[0] = pkg[8];//m_cbParamWord1
                        binfo[1] = pkg[9];
                        dqi.DetNum = (uint)BitConverter.ToUInt16(binfo, 0);
                        binfo[0] = pkg[10];//m_cbParamWord2
                        binfo[1] = pkg[11];

                        try
                        {
                            dqi.LineHeaderSize = (uint)BitConverter.ToUInt16(binfo, 0);
                        }
                        catch (Exception exp1)
                        {
                            System.Windows.MessageBox.Show(exp1.Message + "\n" + exp1.InnerException);
                        }

                        if (bSplitDataLine > 0)
                            dqi.DetNum /= 2;

                        //UpdateCtl((object)NumDetLbl, dqi.DetNum.ToString());//NumDetLbl
                        //UpdateCtl((object)HeaderSizeLbl, dqi.LineHeaderSize.ToString());

                        if (AdaptiveScan == 0)//no advance info
                        {
                            res[4] = CX_MSG_QUERY;
                            res[5] = CX_STATE;
                            res[8] = 0;

                            if (bState == 0)
                                bState = 1;
                            else
                                bState = 0;
                        }
                        else
                        {
                            res[4] = CX_ADVANCE_INIT_INFO;
                            res[5] = CX_STATE;
                            res[8] = 0;
                            if (bState == 0)
                                bState = 1;
                            else
                                bState = 0;
                        }

                        if (pipeData == null || !pipeData.IsAlive)
                        {
                            GetPipedata();
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("No Data");
                        }
                    }
                    DoNotRead = false;
                    break;
                case CX_ADVANCE_INIT_INFO:
                    if (MsgType == CX_MSG_ACK)
                    {
                        Buffer.BlockCopy(b, 0, res, 0, 4);//id
                        res[4] = CX_MSG_QUERY;
                        res[5] = CX_STATE;
                        bState ^= bState;
                        res[8] = (byte)CX_MODE_START;
                    }
                    break;
                case CX_STATE:
                    if (MsgType == CX_MSG_ACK)
                    {
                        res[4] = CX_INIT_INFO;
                        res[5] = CX_MSG_QUERY;

                    }
                    break;
                case CX_SIGNOFLIFE:
                    if (MsgType == CX_MSG_ACK)
                    {
                    }
                    break;
                case CX_ERROR:
                    if (MsgType == CX_MSG_NORMAL)
                    {
                    }
                    break;
                default:
                    res[4] = CX_MSG_ERROR;
                    break;

            }
            return res;
        }
        
        public void GetPipedata()
        {
        //    if (DataPipeConnected == true)
        //    {
        //        PDatath.Abort();
        //        PDatath = null;
        //        pipeData = new Thread(GetData);
        //        pipeData.Priority = ThreadPriority.Normal;
        //        pipeData.Start();
        //    }
        }
    }
}
