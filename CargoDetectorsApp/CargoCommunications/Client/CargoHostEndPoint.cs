using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using System.Threading;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Services;
using omg.org.CORBA;
using omg.org.CosNaming;
using L3.Cargo.Common;

namespace L3.Cargo.Communications.Client
{
    public class CargoHostEndPoint
    {
        #region Private Members

        private string m_IPAddress;

        private int m_Port;

        private static NamingContext m_NameService;

        private static l3.cargo.corba.Host m_Host;

        private NameComponent[] m_ncHost;

        #endregion


        #region Public Members

        public bool IsHostAvailable
        {
            get
            {
                bool ret = false;

                try
                {
                    ret = GetHost().isCorrectVersion(l3.cargo.corba.VersionLabel.ConstVal);
                }
                catch { }

                return ret;
            }
        }

        #endregion


        #region Constructors

        public CargoHostEndPoint(string ipAddress, int port)
        {
            m_IPAddress = ipAddress;
            m_Port = port;
            m_ncHost = new NameComponent[]{ new NameComponent("cargo", "context"),
                                            new NameComponent("host",  "object")};
        }

        #endregion


        #region Private Methods

        private l3.cargo.corba.Host GetHost()
        {
            try
            {
                return m_Host = m_NameService.resolve(m_ncHost) as l3.cargo.corba.Host;
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        private l3.cargo.corba.CaseManager GetCaseManager()
        {
            try
            {
                return GetHost().getCaseManager();
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        private l3.cargo.corba.ManifestManager GetManifestManager()
        {
            try
            {
                return GetHost().getManifestManager();
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        private l3.cargo.corba.WorkstationManager GetWorkstationManager(l3.cargo.corba.WorkstationArea area)
        {
            try
            {
                return GetHost().getWorkstationManager(area);
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        #endregion


        #region Public Methods

        public void Open()
        {
            ChannelServices.RegisterChannel(new IiopChannel(0), false);

            CorbaInit m_Init = CorbaInit.GetInit();
            m_NameService = m_Init.GetNameService(m_IPAddress, m_Port);
        }

        public void Close()
        {
        }

        public void AddCaseChangeListener(l3.cargo.corba.CaseChangeListener caseChangeListener)
        {
            try
            {
                GetCaseManager().addCaseChangeListener(caseChangeListener);
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public void AddManualCodingComm(l3.cargo.corba.WorkstationComm wsComm)
        {
            try
            {
                GetWorkstationManager(l3.cargo.corba.WorkstationArea.MCS).addWSComm(wsComm);
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public void AddAnalystWSComm(l3.cargo.corba.WorkstationComm wsComm)
        {
            try
            {
                GetWorkstationManager(l3.cargo.corba.WorkstationArea.AWS).addWSComm(wsComm);
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public void RemoveCaseChangeListener(l3.cargo.corba.CaseChangeListener caseChangeListener)
        {
            try
            {
                GetCaseManager().removeCaseChangeListener(caseChangeListener);
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public void RemoveManualCodingComm(l3.cargo.corba.WorkstationComm wsComm)
        {
            try
            {
                GetWorkstationManager(l3.cargo.corba.WorkstationArea.MCS).removeWSComm(wsComm);
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public void RemoveAnalystWSComm(l3.cargo.corba.WorkstationComm wsComm)
        {
            try
            {
                GetWorkstationManager(l3.cargo.corba.WorkstationArea.AWS).removeWSComm(wsComm);
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public string CreateInitAreaCase()
        {
            try
            {
                return GetHost().getCaseManager().makeCase();
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public void LinkCases(string caseId, string linkedCase)
        {
            try
            {
                l3.cargo.corba.XCase xcase = GetHost().getCaseManager().getLiveCase(caseId);
                xcase.setLinkedCase(linkedCase);
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public void StartSNMAreaScan(string caseId)
        {
            try
            {
                GetHost().getSNMManager().onStartScan(caseId);
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public void StopSNMAreaScan()
        {
            try
            {
                GetHost().getSNMManager().onStopScan();
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public string[] GetSNMAreaCases()
        {
            try
            {
                return GetHost().getSNMManager().getScannerQueue();
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public bool IsSNMAreaScanning()
        {
            try
            {
                return GetHost().getSNMManager().isScanning();
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public string GetSNMAreaScanningCase()
        {
            try
            {
                return GetHost().getSNMManager().getCaseBeingScanned();
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public l3.cargo.corba.AuthenticationLevel Login(string Username, string Password)
        {
            l3.cargo.corba.AuthenticationLevel authLevel = l3.cargo.corba.AuthenticationLevel.NONE;

            try
            {
                authLevel = GetHost().Login(Username, Password);
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }

            if (authLevel.Equals(l3.cargo.corba.AuthenticationLevel.NONE))
            {
                throw new Exception(ErrorMessages.INVALID_LOGIN);
            }

            return authLevel;
        }

        public void LogOut(string username)
        {
        }

        public int GetMaxManifestPerCase()
        {
            try
            {
                return GetCaseManager().getMaxManifestPerCase();
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public String GetManifestDir()
        {
            try
            {
                return GetManifestManager().getManifestPath();
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public l3.cargo.corba.XCase GetCase(string caseId)
        {
            try
            {
                return GetCaseManager().getLiveCase(caseId);
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public List<l3.cargo.corba.XCase> GetWorkstationAreaCases(l3.cargo.corba.WorkstationArea area)
        {
            List<l3.cargo.corba.XCase> ret = new List<l3.cargo.corba.XCase>();

            try
            {
                foreach (l3.cargo.corba.XCase xc in GetWorkstationManager(area).getCaseList())
                {
                    ret.Add(xc);
                }
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }

            return ret;
        }

        public List<l3.cargo.corba.XCase> GetAllWorkstationAreaCases()
        {
            List<l3.cargo.corba.XCase> ret = new List<l3.cargo.corba.XCase>();

            foreach (l3.cargo.corba.WorkstationArea area in Enum.GetValues(typeof(l3.cargo.corba.WorkstationArea)))
            {
                try
                {
                    foreach (l3.cargo.corba.XCase xc in GetWorkstationManager(area).getCaseList())
                    {
                        ret.Add(xc);
                    }
                }
                catch
                {
                    //Area does not exists, ignore and move to the next area
                }
            }

            return ret;
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
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public void ReleaseAreaCases(l3.cargo.corba.WorkstationArea area)
        {
            try
            {
                foreach (l3.cargo.corba.XCase xc in GetWorkstationManager(area).getCaseList())
                {
                    GetWorkstationManager(area).releaseCase(xc.getId());
                }
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public void AddAttachment(string caseId, string filename, string type, string username, string createtime)
        {
            try
            {
                l3.cargo.corba.Attachment attach = new l3.cargo.corba.Attachment(filename, type, username, createtime);
                l3.cargo.corba.XCase xc = GetHost().getCaseManager().getLiveCase(caseId);
                xc.addAttachment(attach);
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public void SetContainerID(string caseId, string contId)
        {
            try
            {
                l3.cargo.corba.XCase xc = GetHost().getCaseManager().getLiveCase(caseId);
                xc.setContainerId(contId);
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public void SetResult(string caseId, l3.cargo.corba.Result result)
        {
            try
            {
                l3.cargo.corba.XCase xc = GetHost().getCaseManager().getLiveCase(caseId);
                xc.addResult(result);
            }
            catch (l3.cargo.corba.CargoException ex)
            {
                throw new Exception(ex.error_msg, ex.InnerException);
            }
            catch (omg.org.CORBA.INTERNAL ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            catch (AbstractCORBASystemException ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        #endregion
    }
}
