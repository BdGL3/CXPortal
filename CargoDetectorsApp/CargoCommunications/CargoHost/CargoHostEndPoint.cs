using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Remoting.Channels;

using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Services;

using omg.org.CORBA;
using omg.org.CosNaming;
using l3.cargo.corba;

namespace L3.Cargo.Communications.CargoHost
{
    public class CargoHostEndPoint
    {

        public void AbortScanCase()
        {
            try
            {
                if (GetScanManager().isScanning())
                    GetScanManager().onAbortScan();
            }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public void AddAttachment(string caseId, string filename, string type, string username, string createtime)
        {
            try
            {
                l3.cargo.corba.Attachment attach = new l3.cargo.corba.Attachment(filename, type, username, createtime);
                l3.cargo.corba.XCase xc = GetHost().getCaseManager().getLiveCase(caseId);
                xc.addAttachment(attach);
            }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public void AddCaseChangeListener(l3.cargo.corba.CaseChangeListener caseChangeListener)
        {
            try { GetCaseManager().addCaseChangeListener(caseChangeListener); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public void AddManualCodingComm(l3.cargo.corba.WorkstationComm wsComm)
        {
            try { GetWorkstationManager(l3.cargo.corba.WorkstationArea.MCS).addWSComm(wsComm); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public void AddAnalystWSComm(l3.cargo.corba.WorkstationComm wsComm)
        {
            try { GetWorkstationManager(l3.cargo.corba.WorkstationArea.AWS).addWSComm(wsComm); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public CargoHostEndPoint(string ipAdr, int ipPort)
        {
            IPAddress = ipAdr; /*ensure value*/
            IPPort = ipPort; /*ensure value*/
            _iiopChannel = new IiopChannel(0);
            _ncHost = new NameComponent[] { new NameComponent("cargo", "context"), new NameComponent("host", "object") };
            _ncXi = new NameComponent[] { new NameComponent("cargo", "context"), new NameComponent("xi", "object") };
        }
        private NameComponent[] _ncHost;
        private NameComponent[] _ncXi;

        public void Close()
        {
            IsConnected = false;
            try
            {
                if (/*exists (avoid first try exceptions)?*/ _nameService != null)
                    _nameService.unbind(_ncXi);
            }
            catch { }
            finally { _nameService = null; }
#if false
            // HACK! ChannelServices.UnregisterChannel hangs.
            ChannelServices.UnregisterChannel(_iiopChannel);
#endif
            _iiopChannel = null;
        }

        public string CreateInitAreaCase()
        {
            try { return GetHost().getCaseManager().makeCase(); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public string CreateScanAreaCase()
        {
            try { return GetHost().getCaseManager().makeScanCase(); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public List<l3.cargo.corba.XCase> GetAllWorkstationAreaCases()
        {
            List<l3.cargo.corba.XCase> ret = new List<l3.cargo.corba.XCase>();
            foreach (l3.cargo.corba.WorkstationArea area in Enum.GetValues(typeof(l3.cargo.corba.WorkstationArea)))
            {
                try
                {
                    foreach (l3.cargo.corba.XCase xc in GetWorkstationManager(area).getCaseList())
                        ret.Add(xc);
                }
                catch { /* Area does not exist: ignore and move to the next area*/ }
            }
            return ret;
        }

        public l3.cargo.corba.XCase GetCase(string caseId)
        {
            try { return GetCaseManager().getLiveCase(caseId); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        private l3.cargo.corba.CaseManager GetCaseManager()
        {
            try { return GetHost().getCaseManager(); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        private l3.cargo.corba.Host GetHost()
        {
            try { return _host = _nameService.resolve(_ncHost) as l3.cargo.corba.Host; }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }
        private static l3.cargo.corba.Host _host;
        private static NamingContext _nameService;

        public String GetManifestDir()
        {
            try { return GetManifestManager().getManifestPath(); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        private l3.cargo.corba.ManifestManager GetManifestManager()
        {
            try { return GetHost().getManifestManager(); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public int GetMaxManifestPerCase()
        {
            try { return GetCaseManager().getMaxManifestPerCase(); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public string GetNextScanAreaCase()
        {
            try
            {
                string[] caseList = GetScanManager().getScannerQueue();
                return (caseList.Length > 0) ? caseList[0] : string.Empty;
            }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public string[] GetScanAreaCases()
        {
            try { return GetScanManager().getScannerQueue(); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public string GetScanAreaScanningCase()
        {
            try { return GetScanManager().getCaseBeingScanned(); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        private l3.cargo.corba.XIManager GetScanManager()
        {
            try { return GetHost().getXIManager(); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public string[] GetSNMAreaCases()
        {
            try { return GetHost().getSNMManager().getScannerQueue(); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        private l3.cargo.corba.SNMManager GetSNMManager()
        {
            try { return GetHost().getSNMManager(); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public string GetSNMAreaScanningCase()
        {
            try { return GetSNMManager().getCaseBeingScanned(); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        private l3.cargo.corba.TDSManager GetTDSManager()
        {
            try { return GetHost().getTDSManager(); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public List<l3.cargo.corba.XCase> GetWorkstationAreaCases(l3.cargo.corba.WorkstationArea area)
        {
            List<l3.cargo.corba.XCase> ret = new List<l3.cargo.corba.XCase>();
            try
            {
                foreach (l3.cargo.corba.XCase xc in GetWorkstationManager(area).getCaseList())
                    ret.Add(xc);
            }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
            return ret;
        }

        private l3.cargo.corba.WorkstationManager GetWorkstationManager(l3.cargo.corba.WorkstationArea area)
        {
            try { return GetHost().getWorkstationManager(area); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public string IPAddress
        {
            get { return _ipAddress; }
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(MethodBase.GetCurrentMethod().DeclaringType.Name + ": value must not be null or empty");
                _ipAddress = value;
            }
        }
        private string _ipAddress = /*invalid, causes preparation*/ null;

        public int IPPort
        {
            get { return _ipPort; }
            private set
            {
                // Validate the value and, if it is different from the current value, apply it and
                // log the change.
                if (/*invalid?*/ (value < IPEndPoint.MinPort) || (value > IPEndPoint.MaxPort))
                    throw new ArgumentOutOfRangeException(MethodBase.GetCurrentMethod().DeclaringType.Name + ": value (" + value.ToString() + ") must be in the domain [" + IPEndPoint.MinPort.ToString() + ", " + IPEndPoint.MaxPort.ToString() + "]");
                _ipPort = value;
            }
        }
        private int _ipPort = /*invalid, causes preparation*/ int.MinValue;

        [DefaultValue(false)]
        public bool IsConnected { get; private set; }

        public bool IsHostAvailable
        {
            get
            {
                try { return GetHost().isCorrectVersion(l3.cargo.corba.VersionLabel.ConstVal); }
                catch { }
                return false;
            }
        }

        public bool IsScanAreaScanning()
        {
            try { return GetScanManager().isScanning(); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public bool IsSNMAreaScanning()
        {
            try { return GetSNMManager().isScanning(); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public void LinkCases(string caseId, string linkedCase)
        {
            try
            {
                l3.cargo.corba.XCase xcase = GetHost().getCaseManager().getLiveCase(caseId);
                xcase.setLinkedCase(linkedCase);
            }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public l3.cargo.corba.AuthenticationLevel Login(string Username, string Password)
        {
            l3.cargo.corba.AuthenticationLevel authLevel = l3.cargo.corba.AuthenticationLevel.NONE;
            try { authLevel = GetHost().Login(Username, Password); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
            if (authLevel.Equals(l3.cargo.corba.AuthenticationLevel.NONE))
                throw new Exception(@"Username and password are incorrect.  Please contact your system administrator.");
            return authLevel;
        }

        public void LogOut(string username) { }

        public void Open()
        {
            ChannelServices.RegisterChannel(_iiopChannel, false);
            CorbaInit m_Init = CorbaInit.GetInit();
            _nameService = m_Init.GetNameService(_ipAddress, _ipPort);
            RebindToNameService();
            IsConnected = true;
        }
        private IiopChannel _iiopChannel;

        public int Ping(int value)
        {
            int nRet;
            try { nRet = GetHost().ping(value); }
            catch { nRet = -1; }
            return nRet;
        }

        public void RebindToNameService()
        {
            try
            {
                if (/*exists (avoid first try exceptions)?*/ _nameService != null)
                    _nameService.rebind(_ncXi, new XiManagerCallback());
            }
            catch
            {
                _nameService = null;
#if false
                // HACK! ChannelServices.UnregisterChannel hangs.
                ChannelServices.UnregisterChannel(_iiopChannel);
#endif
                throw;
            }
            finally
            {
                _nameService = null;
                _iiopChannel = null;
            }
        }

        public void ReleaseCase(string caseId)
        {
            try
            {
                l3.cargo.corba.XCase xc = GetHost().getCaseManager().getLiveCase(caseId);
                l3.cargo.corba.WorkstationArea area;
                switch (xc.getCurrentArea())
                {
                    case "Analyst":
                        area = l3.cargo.corba.WorkstationArea.AWS;
                        break;
                    case "EWS":
                        area = l3.cargo.corba.WorkstationArea.EWS;
                        break;
                    case "Inspector":
                        area = l3.cargo.corba.WorkstationArea.IWS;
                        break;
                    case "ManualCoding":
                        area = l3.cargo.corba.WorkstationArea.MCS;
                        break;
                    case "Supervisor":
                        area = l3.cargo.corba.WorkstationArea.SWS;
                        break;
                    default:
                        area = l3.cargo.corba.WorkstationArea.AREA_UNKNOWN;
                        break;
                }
                GetHost().getWorkstationManager(area).releaseCase(caseId);
            }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public void ReleaseAreaCases(l3.cargo.corba.WorkstationArea area)
        {
            try
            {
                foreach (l3.cargo.corba.XCase xc in GetWorkstationManager(area).getCaseList())
                    GetWorkstationManager(area).releaseCase(xc.getId());
            }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public void RemoveCaseChangeListener(l3.cargo.corba.CaseChangeListener caseChangeListener)
        {
            try { GetCaseManager().removeCaseChangeListener(caseChangeListener); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public void RemoveManualCodingComm(l3.cargo.corba.WorkstationComm wsComm)
        {
            try { GetWorkstationManager(l3.cargo.corba.WorkstationArea.MCS).removeWSComm(wsComm); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }
        
        public void RemoveAnalystWSComm(l3.cargo.corba.WorkstationComm wsComm)
        {
            try { GetWorkstationManager(l3.cargo.corba.WorkstationArea.AWS).removeWSComm(wsComm); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public void ScanAreaStartScan(string caseId)
        {
            try { GetScanManager().onStartScan(caseId); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public void ScanAreaStopScan()
        {
            try { GetScanManager().onStopScan(); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public void SetContainerID(string caseId, string contId)
        {
            try
            {
                l3.cargo.corba.XCase xc = GetHost().getCaseManager().getLiveCase(caseId);
                xc.setContainerId(contId);
            }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public void SetResult(string caseId, l3.cargo.corba.Result result)
        {
            try
            {
                l3.cargo.corba.XCase xc = GetHost().getCaseManager().getLiveCase(caseId);
                xc.addResult(result);
            }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public void SetTDSAreaReady()
        {
            try
            {
                TDSManager tdsManager = GetTDSManager();
                tdsManager.onTDSReady();
            }
            catch { }
        }

        public void ShutDown()
        {
            try { GetHost().shutdown(); }
            catch { }
        }
        
        public void StartSNMAreaScan(string caseId)
        {
            try { GetHost().getSNMManager().onStartScan(caseId); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }

        public void StopScanWithScanResult(XRayScanResult result)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(result.mImagePath))
                    GetCaseManager().getLiveCase(result.mCaseId).setImageName(result.mImagePath);
                else
                    result.mImagePath = string.Empty;
                GetScanManager().onStopScanWithScanResult(result);
            }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }
        
        public void StopSNMAreaScan()
        {
            try { GetHost().getSNMManager().onStopScan(); }
            catch (l3.cargo.corba.CargoException ex) { throw new Exception(ex.error_msg, ex.InnerException); }
            catch (omg.org.CORBA.INTERNAL ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (System.Reflection.TargetInvocationException ex) { throw new Exception(ex.Message, ex.InnerException); }
            catch (AbstractCORBASystemException ex) { throw new Exception(ex.Message, ex.InnerException); }
        }
    }
}