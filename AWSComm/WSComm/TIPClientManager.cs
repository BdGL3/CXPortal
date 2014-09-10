using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.Threading;
using System.Xml.Linq;
using L3.Cargo.Communications.Client;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Interfaces;
using System.IO;

namespace L3.Cargo.WSCommunications
{
    public class TIPClientManager
    {
        #region Private Members

        private bool m_Shutdown;

        private string m_AliasElementTag;

        private string m_Alias;

        private Thread m_ListenerThread;

        private List<TIPClient> m_TIPManagers;

        private WSServer m_WSServer;

        private string m_CTITemplateDirectory;

        #endregion Private Members


        #region Public Members

        public string FTIImageDirectory
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory + @"FTIs\";
            }
        }

        public string CTICaseDirectory
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory + @"CTIs\";
            }
        }

        #endregion Public Members


        #region Constructor

        public TIPClientManager(WSServer wsServer, string alias, string ctiTemplateDir)
        {
            m_Shutdown = false;
            m_AliasElementTag = "HostAlias";
            m_Alias = alias;
            m_WSServer = wsServer;
            m_CTITemplateDirectory = ctiTemplateDir;
            m_TIPManagers = new List<TIPClient>();
            wsServer.TIPClientManager = this;
            CreateLocalTIPDirectories();
        }

        #endregion Constructor


        #region Private Methods

        private void CreateLocalTIPDirectories()
        {
            if (Directory.Exists(CTICaseDirectory))
            {
                Directory.Delete(CTICaseDirectory, true);
            }

            if (Directory.Exists(FTIImageDirectory))
            {
                Directory.Delete(FTIImageDirectory, true);
            }

            Directory.CreateDirectory(CTICaseDirectory);
            Directory.CreateDirectory(FTIImageDirectory);
        }

        private void ListenForSources()
        {
            HostDiscovery sourceDiscovery = new HostDiscovery(typeof(ITipManager), new TimeSpan(0, 0, 0, 1, 0));

            while (!m_Shutdown)
            {
                Collection<EndpointDiscoveryMetadata> sourceEndpoints = sourceDiscovery.GetAvailableConnections();

                if (sourceEndpoints.Count > 0)
                {
                    for (Int32 index = 0; index < sourceEndpoints.Count; index++)
                    {
                        String alias = GetAlias(sourceEndpoints[index].Extensions);
                        if (!String.IsNullOrWhiteSpace(alias))
                        {
                            if (!ContainsClient(alias))
                            {
                                TIPClient tipManager = new TIPClient(alias, CTICaseDirectory, FTIImageDirectory, m_CTITemplateDirectory,  m_WSServer);

                                InstanceContext sourceCallback = new InstanceContext(tipManager);

                                ServiceEndpoint tipManagerEndPoint =
                                    new ServiceEndpoint(ContractDescription.GetContract(typeof(TipManagerEndpoint)),
                                                        new TCPBinding(),
                                                        sourceEndpoints[index].Address);

                                tipManager.EndPoint = new TipManagerEndpoint(sourceCallback, tipManagerEndPoint);

                                m_TIPManagers.Add(tipManager);
                            }
                        }
                    }
                }
            }
        }

        private string GetAlias(Collection<XElement> extensions)
        {
            String alias = String.Empty;
            foreach (XElement xElement in extensions)
            {
                try
                {
                    if (xElement.Element(m_AliasElementTag) != null)
                    {
                        alias = xElement.Element(m_AliasElementTag).Value;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return alias;
        }

        private bool ContainsClient(string alias)
        {
            foreach (TIPClient tipManager in m_TIPManagers)
            {
                if (string.Equals(tipManager.Alias, alias))
                {
                    return true;
                }
            }

            return false;
        }

        private TIPClient FindClientByFTIFile(string filename)
        {
            TIPClient ret = null;

            foreach (TIPClient tipManager in m_TIPManagers)
            {
                if (tipManager.ContainsFTIFile(filename))
                {
                    ret = tipManager;
                    break;
                }
            }

            return ret;
        }

        private TIPClient FindClientByCTICase (string caseId)
        {
            TIPClient ret = null;

            foreach (TIPClient tipManager in m_TIPManagers)
            {
                if (tipManager.ContainsCTICase(caseId))
                {
                    ret = tipManager;
                    break;
                }
            }

            return ret;
        }

        private TIPClient FindClientByWorkstationId(string workstationId)
        {
            TIPClient ret = null;

            foreach (TIPClient tipManager in m_TIPManagers)
            {
                if (tipManager.ContainsWorkstation(workstationId))
                {
                    ret = tipManager;
                    break;
                }
            }

            return ret;
        }

        #endregion Private Methods


        #region Public Methods

        public void StartUp ()
        {
            if (m_ListenerThread == null)
            {
                m_Shutdown = false;
                m_ListenerThread = new Thread(new ThreadStart(ListenForSources));
                m_ListenerThread.Start();
            }
        }

        public void ShutDown ()
        {
            if (m_ListenerThread != null)
            {
                m_Shutdown = true;
                m_ListenerThread.Abort();
                m_ListenerThread.Join();
                m_ListenerThread = null;
            }
        }

        public void ProcessedCase (string caseId)
        {
            for (int index = 0; index < m_TIPManagers.Count; index ++ )
            {
                TIPClient tipClient = m_TIPManagers[index];

                try
                {
                    tipClient.ProcessedCase(caseId);
                }
                catch (Exception)
                {
                    tipClient.Dispose();
                    m_TIPManagers.Remove(tipClient);
                }
            }
        }

        public void TipResult(WorkstationResult workstationResult)
        {
            TIPClient tipClient = FindClientByWorkstationId(workstationResult.WorkstationId);

            try
            {
                if (tipClient != null)
                {
                    tipClient.TipResult(workstationResult);
                }
            }
            catch (Exception)
            {
                tipClient.Dispose();
                m_TIPManagers.Remove(tipClient);
            }
        }

        public void AssignCTICase(string caseId, string workstationId)
        {
            TIPClient tipClient = FindClientByCTICase(caseId);

            if (tipClient != null)
            {
                tipClient.AssignCTICase(caseId, workstationId);
            }
        }

        public void RemoveCTICaseAssignment(string caseId)
        {
            TIPClient tipClient = FindClientByCTICase(caseId);

            if (tipClient != null)
            {
                tipClient.RemoveCTICaseAssignment(caseId);
            }

        }

        public string RequestFTIFile (string workstationId)
        {
            string ret = null;

            foreach (TIPClient tipManager in m_TIPManagers)
            {
               ret = tipManager.RequestFTIFile(workstationId);

                if (!String.IsNullOrWhiteSpace(ret))
                {
                    break;
                }
            }

            return ret;
        }

        public void ClearAssignments (string workstationId)
        {
            foreach (TIPClient tipClient in m_TIPManagers)
            {
                tipClient.ClearAssignments(workstationId);

            }
        }

        #endregion Public Methods
    }
}
