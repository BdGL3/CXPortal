using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;
using L3.Cargo.Common.Dashboard.Display;
using L3.Cargo.Common.Dashboard.Display.Interfaces;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Dashboard.DataAccessCore;
using L3.Cargo.Dashboard.PresentationCore;
using System.Configuration;

namespace L3.Cargo.Dashboard.AssemblyManager
{
    public class AssemblyManager
    {
        #region Private Members

        private DataAccess _DataAccess;
		
        private EventLoggerAccess _Logger;
		
        private UIManager _UIManager;
		
        private Dictionary<string, IDisplays> _AssemblyInstances;

        #endregion Private Members


        #region Constructors

        public AssemblyManager(UIManager uiManager, DataAccess dataAccess, EventLoggerAccess logger)
        {
            _DataAccess = dataAccess;
            _DataAccess.setEvent(new SubsystemServiceListUpdateHandler(SubsystemServiceListUpdate));
            _Logger = logger;
            _UIManager = uiManager;
            _AssemblyInstances = new Dictionary<string, IDisplays>();

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["StartUpClean"]))
            {
                string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                ConfigurationManager.AppSettings["SystemOperationMode"]);
                if (Directory.Exists(directory))
                {
                    DeleteDirectory(directory);
                }
            }
        }

        #endregion Constructors


        #region Private Methods

        private void SubsystemServiceListUpdate(string alias, SubsystemUpdateEnum? update, string filenameWithPath)
        {
            try
            {
                if (update == SubsystemUpdateEnum.SubsystemConnect)
                {
                    string assemblyFile = Path.Combine(Path.GetDirectoryName(filenameWithPath),
                                                       Path.GetFileNameWithoutExtension(filenameWithPath));

                    if (!Directory.Exists(assemblyFile))
                    {
                        Directory.CreateDirectory(assemblyFile);

                        using (ZipFile zip = ZipFile.Read(filenameWithPath))
                        {
                            zip.ExtractAll(assemblyFile);
                        }
                    }

                    LoadAssembly(assemblyFile);
                }
                else
                {
                    UnloadAssembly(alias);
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        private void UnloadAssembly(string alias)
        {
            try
            {
                string aliasKey = string.Empty;

                foreach (string key in _AssemblyInstances.Keys)
                {
                    if (key.Contains(alias))
                    {
                        aliasKey = key;
                        break;
                    }
                }

                if (!String.IsNullOrWhiteSpace(aliasKey))
                {
                    _AssemblyInstances[aliasKey].Dispose();
                    _UIManager.Remove(aliasKey);
                    _AssemblyInstances.Remove(aliasKey);
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        private void LoadAssembly(string directory)
        {
            try
            {
                string[] files = Directory.GetFiles(directory);

                foreach (string filename in Directory.GetFiles(directory, "*.Displays.dll"))
                {
                    System.Reflection.Assembly displayAssembly = System.Reflection.Assembly.LoadFrom(filename);

                    foreach (Type assemblyType in displayAssembly.GetTypes())
                    {
                        Type typeInterface = assemblyType.GetInterface("IDisplays", true);

                        if (assemblyType.IsPublic && !assemblyType.IsAbstract && typeInterface != null)
                        {
                            try
                            {
                            	IDisplays instance = Activator.CreateInstance(assemblyType) as IDisplays;
                                AssemblyDisplays assemblyDisplays = instance.Initialize(new object[] { _UIManager.Dispatcher, directory });
                                _UIManager.Add(assemblyDisplays);
                            	_AssemblyInstances.Add(assemblyDisplays.Name, instance);
                                break;
                            }
                            catch (Exception ex)
                            {
                                _Logger.LogError(ex);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        /// <summary>
        /// Method for deleting a directory. This avoids a .NET issue where the recursive delete directory does not delete files before
        /// directories. This will delete read-only files as well.
        /// </summary>
        /// <param name="target_dir">The directory to delete.</param>
        private void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            try
            {
                Directory.Delete(target_dir, false);
            }
            catch (System.IO.IOException ex)
            {
                // delete rarely throws an exception but the directory is still deleted?? Appears to be a rare timing issue.
                _Logger.LogError(ex);
            }
        }

        #endregion Private Methods
    }
}
