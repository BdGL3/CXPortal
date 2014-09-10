using System;
using System.IO;
using System.ServiceModel;
using System.Diagnostics;
using Ionic.Zip;
using System.Configuration;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Xml.Linq;
using L3.Cargo.Communications.Dashboard.Interfaces;

namespace L3.Cargo.Communications.Dashboard.Host
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class SubsystemHost : ISubsystem
    {
        #region private members

        string m_aliasTag;
        string m_assemblyTag;

        #endregion

        #region Constructors

        public SubsystemHost(string aliasTag, string assemblyTag)
        {
            m_aliasTag = aliasTag;
            m_assemblyTag = assemblyTag;
        }

        #endregion

        #region ISubsystem

        public SubsystemAssembly GetAssembly(GetAssemblyParameterMessage message)
        {
            try
            {
                string assemblyDirLocation = AppDomain.CurrentDomain.BaseDirectory;
                List<string> dirList = new List<string>(Directory.GetDirectories(assemblyDirLocation));

                Stream stream = new MemoryStream();

                string name = dirList.Find(
                    delegate(string dirName)
                    {
                        return (dirName.Contains(message.SystemMode.ToString()));
                    });

                //if specified assembly doesn't exist then 
                //find one below the specified assembly level
                //e.g. if specified assembly is Engineer then look for
                // Maintenance, if not found then Supervisor, if not found
                // then Operator
                List<EnumSystemOperationMode> SearchCriteria = new List<EnumSystemOperationMode>();

                if (String.IsNullOrWhiteSpace(name))
                {
                    if (message.SystemMode == EnumSystemOperationMode.Engineer)
                    {
                        SearchCriteria.Add(EnumSystemOperationMode.Maintenance);
                    }

                    if (message.SystemMode == EnumSystemOperationMode.Maintenance ||
                        message.SystemMode == EnumSystemOperationMode.Engineer)
                    {
                        SearchCriteria.Add(EnumSystemOperationMode.Supervisor);
                    }

                    if (message.SystemMode == EnumSystemOperationMode.Supervisor ||
                        message.SystemMode == EnumSystemOperationMode.Engineer ||
                        message.SystemMode == EnumSystemOperationMode.Maintenance)
                    {
                        SearchCriteria.Add(EnumSystemOperationMode.Operator);
                    }

                    foreach (EnumSystemOperationMode criteria in SearchCriteria)
                    {
                        name = dirList.Find(
                        delegate(string dirName)
                        {
                            return (dirName.Contains(criteria.ToString()));
                        });

                        if (!String.IsNullOrWhiteSpace(name))
                        {
                            break;
                        }
                    }
                }

                if (!String.IsNullOrWhiteSpace(name))
                {
                    using (ZipFile zip = new ZipFile())
                    {
                        string[] files = Directory.GetFiles(name);

                        // changing this defalte threshold avoids corrupted files in the zipfile.
                        zip.ParallelDeflateThreshold = -1; 

                        zip.AddFiles(files, "");

                        // zip up resource directories as well
                        string[] directories = Directory.GetDirectories(name);
                        foreach (string subDir in directories)
                        {
                            try
                            {
                                // only add resource files for this local project. Exclude the L3.Cargo.Common.Dashboard.resources.dll
                                string[] resFiles = Directory.GetFiles(subDir);
                                zip.AddDirectoryByName(Path.GetFileName(subDir));
                                foreach (string resFile in resFiles)
                                {
                                    // Avoid adding the resource file for the Dashboard.
                                    // TODO: this string should come from somewhere
                                    if (!resFile.Contains("L3.Cargo.Common.Dashboard.resources.dll"))
                                    {
                                        zip.AddFile(resFile, Path.GetFileName(subDir));
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                string ex = e.Message;
                            }
                        }

                        zip.Save(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                    }

                    OperationContext clientContext = OperationContext.Current;
                    clientContext.OperationCompleted += new EventHandler(delegate(object sender, EventArgs e)
                    {
                        if (stream != null)
                            stream.Dispose();
                    });

                    return new SubsystemAssembly(stream, m_aliasTag + "_" + m_assemblyTag + ".zip");
                }
                else
                    throw new FaultException(new FaultReason("File does not exist"));
            }
            catch (Exception exp)
            {
                throw new FaultException(new FaultReason(exp.Message));
            }
        }

        #endregion
    }
}