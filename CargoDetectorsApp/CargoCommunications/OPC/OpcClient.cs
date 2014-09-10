using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading;

using L3.Cargo.Common.Configurations;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.EventsLogger.Client;

using Opc;
using Opc.Da;

namespace L3.Cargo.Communications.OPC
{
    public delegate void OpcTagUpdateHandler (string name, int value);

    public class OpcClient : IDisposable
    {
        private Opc.Da.Server _Server;
        private Subscription _groupRead;
        private Subscription _groupWrite;
        private SubscriptionState _GroupReadState;
        private SubscriptionState _GroupWriteState;
        private Item[] _Items;
        private OpcSection _OpcSection;
        private string _TagGroup;
        private EventLoggerAccess _logger;


        #region Public Members

        public event OpcTagUpdateHandler OpcTagUpdate;

        public OpcSection OPCSection
        {
            get { return _OpcSection; }
        }

        public bool Connected
        {
            get { return (_Server != null) ? _Server.IsConnected : false; }
        }

        #endregion Public Members

        public OpcClient (OpcSection opcSection, string tagGroup, EventLoggerAccess logger)
        {
            _logger = logger;
            _OpcSection = opcSection;
            _TagGroup = tagGroup;

            _GroupReadState = new SubscriptionState();
            _GroupReadState.Name = _OpcSection.Server.TagGroup.GetElement(_TagGroup).Name + " Read";
            _GroupReadState.UpdateRate = _OpcSection.Server.TagGroup.GetElement(_TagGroup).UpdateRate;
            _GroupReadState.Active = true;

            _GroupWriteState = new SubscriptionState();
            _GroupWriteState.Name = _OpcSection.Server.TagGroup.GetElement(_TagGroup).Name + " Write";
            _GroupWriteState.Active = false;

            _Items = new Item[_OpcSection.Server.TagGroup.GetElement(_TagGroup).Tags.Count];
        }

        #region Private Methods

        private void ConnectionAgent()
        {
            while (!_connectionEnd.WaitOne(0) && !Connected)
                try
                {
                    URL url = new URL(_OpcSection.Server.Name);
                    _Server = new Opc.Da.Server(new OpcCom.Factory(), null);
                    _Server.Connect(url, new ConnectData(new NetworkCredential()));

                    _groupRead = (Subscription)_Server.CreateSubscription(_GroupReadState);
                    _groupWrite = (Subscription)_Server.CreateSubscription(_GroupWriteState);

                    for (int i = 0; i < _OpcSection.Server.TagGroup.GetElement(_TagGroup).Tags.Count; i++)
                    {
                        _Items[i] = new Opc.Da.Item();
                        //_Items[i].ItemName = String.Format("{0}{1}", _OpcSection.Server.Channel, _OpcSection.Server.TagGroup.GetElement(_TagGroup).Tags[i].Name);
                        _Items[i].ItemName = _OpcSection.Server.Channel + "." + _OpcSection.Server.Device + "." + _OpcSection.Server.TagGroup.GetElement(_TagGroup).Tags[i].Name;
                        //string itmNam = String.Format("{0}]{1}", _OpcSection.Server.Channel, _OpcSection.Server.TagGroup.GetElement(_TagGroup).Tags[i].Name);
                        _logger.LogInfo(/*Mtd*/ ": recognized element " + _Items[i].ItemName);
                    }
                    _Items = _groupRead.AddItems(_Items);
                    _groupRead.DataChanged += new DataChangedEventHandler(Group_DataChanged);
                }
                catch (Exception ex) { _logger.LogError(ex); }
        }
        private ManualResetEvent _connectionEnd = new ManualResetEvent(false);
        private Thread _connectionThread;

        private void Group_DataChanged (object subscriptionHandle, object requestHandle, ItemValueResult[] values)
        {
            if (OpcTagUpdate != null)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    string name = values[i].ItemName.Substring(values[i].ItemName.LastIndexOf(".") + 1);
                    int value = System.Convert.ToInt32(values[i].Value);
                    OpcTagUpdate(name, value);

                    string opcTagInfoMessage = String.Format("{0}{1}{2}{3}{4}", "OPC - TAGUPDATE > ", "Name:", name, " Value:", value.ToString());
                    _logger.LogInfo(opcTagInfoMessage);
                }
            }
        }

        #endregion


        #region Public Methods

        public void Open ()
        {
            try { _connectionThread = Threads.Dispose(_connectionThread, ref _connectionEnd); }
            catch { }
            _connectionThread = Threads.Create(ConnectionAgent, ref _connectionEnd, "OPC Connection thread");
            _connectionThread.Start();
        }

        public void Close ()
        {
        }

        public static void TestConnection (string connection)
        {
            URL url = new URL(connection);
            Opc.Da.Server server = new Opc.Da.Server(new OpcCom.Factory(), null);
            server.Connect(url, new ConnectData(new NetworkCredential()));
        }

        public int ReadValue(string tag)
        {
            if (!Connected)
                return -1;
            tag = _OpcSection.Server.Channel + "." + _OpcSection.Server.Device + "." + tag;
            //tag = _OpcSection.Server.Channel + tag;
            _logger.LogInfo("OPC - Read Tag: " + tag);
            //_logger.LogInfo(/*Mtd*/ ": " + tag);
            Item[] itemToRead = new Item[1];
            itemToRead[0] = new Item();
            itemToRead[0].ItemName = tag;
            bool itemFound = false;
            foreach (Item item in _groupRead.Items)
            {
                if (item.ItemName == itemToRead[0].ItemName)
                {
                    itemToRead[0].ServerHandle = item.ServerHandle;
                    itemFound = true;
                    break;
                }
            }
            if (!itemFound)
            {
                int /*original length before AddItems*/ len = _groupWrite.Items.Length;
                try
                {
                    ItemResult[] itmRsl = _groupWrite.AddItems(itemToRead);
                    if (/*OK?*/ _groupWrite.Items.Length > len)
                        itemToRead[0].ServerHandle = _groupWrite.Items[_groupWrite.Items.Length - 1].ServerHandle;
                    else
                    {
                        string /*diagnostic message*/ msg = MethodBase.GetCurrentMethod().Name + ": " +
                        itemToRead[0].ItemName + " rejected; " + itmRsl[0].DiagnosticInfo;
                        _logger.LogError(/*Mtd*/ ": " + Utilities.TextTidy(msg));
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex);
                    throw ex;
                }
            }
            ItemValueResult[] valueResults = _groupRead.Read(itemToRead);
            return System.Convert.ToInt32(valueResults[0].Value);
        }

        public void WriteBoolean (string tag, bool value)
        {
            WriteShort(tag, System.Convert.ToInt16(value));
        }

        public void WriteShort (string tag, short value)
        {
            WriteWord(tag, System.Convert.ToInt32(value));
        }

        public void WriteWord (string tag, int value)
        {
            if (Connected)
            {
                tag = _OpcSection.Server.Channel + "." + _OpcSection.Server.Device + "." + tag;
                //tag = _OpcSection.Server.Channel + tag;
                _logger.LogInfo("OPC - Write Tag: " + tag + "; Value: " + value.ToString());
                Item[] itemToAdd = new Item[1];
                itemToAdd[0] = new Item();
                itemToAdd[0].ItemName = tag;

                ItemValue[] writeValues = new ItemValue[1];
                writeValues[0] = new ItemValue(itemToAdd[0]);

                bool itemFound = false;
                foreach (Item item in _groupWrite.Items)
                {
                    if (item.ItemName == itemToAdd[0].ItemName)
                    {
                        writeValues[0].ServerHandle = item.ServerHandle;
                        itemFound = true;
                        break;
                    }
                }

                if (!itemFound)
                {
                    _groupWrite.AddItems(itemToAdd);
                    writeValues[0].ServerHandle = _groupWrite.Items[_groupWrite.Items.Length - 1].ServerHandle;
                }

                writeValues[0].Value = value;
                _groupWrite.Write(writeValues);
                /*Marlon: try catch added by Bruce but forgot to write values to array, see above.
                if (!itemFound)
                    try
                    {
                        _GroupWrite.AddItems(itemToAdd);
                        writeValues[0].ServerHandle = _GroupWrite.Items[_GroupWrite.Items.Length - 1].ServerHandle;
                        writeValues[0].Value = value;
                        _GroupWrite.Write(writeValues);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex);
                        throw ex;
                    }*/
            }
        }

        public void Dispose ()
        {
            try
            {
                if (/*exists (avoid first try exceptions)?*/ _connectionThread != null)
                    _connectionThread = Threads.Dispose(_connectionThread, ref _connectionEnd);
            }
            catch { }
            finally { _connectionThread = null; }
            try
            {
                if (/*exists (avoid first try exceptions)?*/ _groupRead != null)
                    _groupRead.Dispose();
            }
            catch { }
            finally { _groupRead = null; }
            try
            {
                if (/*exists (avoid first try exceptions)?*/ _groupWrite != null)
                    _groupWrite.Dispose();
            }
            catch { }
            finally { _groupWrite = null; }
        }
        #endregion
    }
}
