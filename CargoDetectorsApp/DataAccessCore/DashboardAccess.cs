using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Dashboard.Common;
using L3.Cargo.Communications.Dashboard.Host;
using L3.Cargo.Communications.Dashboard.Interfaces;

namespace L3.Cargo.Detectors.DataAccessCore
{
    public class DashboardAccess
    {
        FileSystemWatcher m_FileSystemWatcher;
        NetworkHost<ISubsystem> host;

        public DashboardAccess()
        {
            string Alias = ConfigurationManager.AppSettings["Alias"];
            string ipAddressRange = ConfigurationManager.AppSettings["AllowedIPList"];
            string uri = (String)ConfigurationManager.AppSettings["ConnectionUri"];
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string[] Directories = Directory.GetDirectories(path);
            DateTime lastModifiedTime = new DateTime();

            if (Directories.Length > 0)
            {
                foreach (string dir in Directories)
                {
                    string[] Files = Directory.GetFiles(dir);

                    foreach (string file in Files)
                    {
                        if (lastModifiedTime < File.GetLastWriteTime(file))
                        {
                            lastModifiedTime = File.GetLastWriteTime(file);
                        }
                    }
                }
            }

            List<DiscoveryMetadata> list = new List<DiscoveryMetadata>();
            string assemblyTag = lastModifiedTime.ToString("yyyy-MM-dd_h-m-s-fff");

            list.Add(new DiscoveryMetadata(SubsystemCommInfo.BraodcastMetaDataAlias, Alias));
            list.Add(new DiscoveryMetadata(SubsystemCommInfo.BroadMetaDataIPAddresses, ipAddressRange));
            list.Add(new DiscoveryMetadata(SubsystemCommInfo.BroadcastMetaDataSubsystemTag, DateTime.Now.ToString()));
            list.Add(new DiscoveryMetadata(SubsystemCommInfo.BroadcastMetaDataAssemblyTag, assemblyTag));

            host = new NetworkHost<ISubsystem>(new SubsystemHost(Alias, assemblyTag), new Uri(uri), list);

            //monitor assembly file system directory for modified time
            m_FileSystemWatcher = new FileSystemWatcher();
            m_FileSystemWatcher.Path = path;
            m_FileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite;
            m_FileSystemWatcher.Filter = "*.*";
            m_FileSystemWatcher.IncludeSubdirectories = true;

            m_FileSystemWatcher.Changed += new FileSystemEventHandler(OnChanged);
            m_FileSystemWatcher.Error += new ErrorEventHandler(FSWatcher_Error);
        }

        public void Start()
        {
            host.Open();
            m_FileSystemWatcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            host.Close();
            m_FileSystemWatcher.EnableRaisingEvents = false;
        }

        #region private methods

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                host.UpdateMetadata(new DiscoveryMetadata(SubsystemCommInfo.BroadcastMetaDataAssemblyTag, 
                                                          File.GetLastWriteTime(e.FullPath).ToString("yyyy-MM-dd_h-m-s-fff")));
            }
        }

        private void FSWatcher_Error(object source, ErrorEventArgs e)
        {
        }

        #endregion
    }
}
