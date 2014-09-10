using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Dashboard.Common;
using L3.Cargo.Communications.Dashboard.Host;
using L3.Cargo.Communications.Dashboard.Interfaces;
using System.Net;

namespace L3.Cargo.Subsystem.DataAccessCore
{
    public class DashboardAccess
    {
        #region Private Members

        private FileSystemWatcher _FileSystemWatcher;

        private NetworkHost<ISubsystem> _NetworkHost;

        private SubsystemHost _SubsystemHost;

        private string _dateTimeFormat;

        #endregion Private Members


        #region Constructors

        public DashboardAccess ()
        {
            string alias = ConfigurationManager.AppSettings["Alias"];
            string ipAddressRange = ConfigurationManager.AppSettings["AllowedIPList"];
            string port = (String)ConfigurationManager.AppSettings["DashboardPort"];

            string uri = string.Format("net.tcp://{0}:{1}/{2}Comm/", Dns.GetHostName(), port, alias);

            _dateTimeFormat = "yyyy-MM-dd HH-mm-ss.fff";
            string assemblyTag = DateTime.Now.ToString(_dateTimeFormat);
            UpdateSettingsConfig();
            
            _SubsystemHost = new SubsystemHost(alias, assemblyTag);

            List<DiscoveryMetadata> list = new List<DiscoveryMetadata>();
            list.Add(new DiscoveryMetadata(SubsystemCommInfo.BraodcastMetaDataAlias, alias));
            list.Add(new DiscoveryMetadata(SubsystemCommInfo.BroadMetaDataIPAddresses, ipAddressRange));
            list.Add(new DiscoveryMetadata(SubsystemCommInfo.BroadcastMetaDataSubsystemTag, DateTime.Now.ToString(_dateTimeFormat)));
            list.Add(new DiscoveryMetadata(SubsystemCommInfo.BroadcastMetaDataAssemblyTag, assemblyTag));

            _NetworkHost = new NetworkHost<ISubsystem>(_SubsystemHost, new Uri(uri), list);

            //monitor assembly file system directory for modified time
            _FileSystemWatcher = new FileSystemWatcher();
            _FileSystemWatcher.Path = AppDomain.CurrentDomain.BaseDirectory;
            _FileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite;
            _FileSystemWatcher.Filter = "*.*";
            _FileSystemWatcher.IncludeSubdirectories = true;

            _FileSystemWatcher.Changed += new FileSystemEventHandler(OnChanged);
            _FileSystemWatcher.Error += new ErrorEventHandler(FSWatcher_Error);
        }

        #endregion Constructors


        #region Private Methods

        private static void UpdateSettingsConfig ()
        {
            foreach (string file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "Settings.config", SearchOption.AllDirectories))
            {
                var map = new ExeConfigurationFileMap();
                map.ExeConfigFilename = file;
                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                AppSettingsSection appSettingsSection = config.AppSettings;
                appSettingsSection.Settings["SubsystemServer"].Value = Dns.GetHostName();
                appSettingsSection.Settings["DisplayPort"].Value = ConfigurationManager.AppSettings["DisplayPort"];
                appSettingsSection.Settings["SubsystemPort"].Value = ConfigurationManager.AppSettings["SubsystemPort"];
                config.Save(ConfigurationSaveMode.Modified);
            }
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                _NetworkHost.UpdateMetadata(new DiscoveryMetadata(SubsystemCommInfo.BroadcastMetaDataAssemblyTag,
                                                          File.GetLastWriteTime(e.FullPath).ToString(_dateTimeFormat)));
            }
        }

        private void FSWatcher_Error (object source, ErrorEventArgs e)
        {
        }

        #endregion Private Methods


        #region Public Methods

        public void Start ()
        {
            _NetworkHost.Open();
            _FileSystemWatcher.EnableRaisingEvents = true;
        }

        public void Stop ()
        {
            _NetworkHost.Close();
            _FileSystemWatcher.EnableRaisingEvents = false;
        }

        #endregion Public Methods
    }
}
