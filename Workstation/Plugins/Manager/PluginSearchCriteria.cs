using System;
using System.Collections.Generic;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;

namespace L3.Cargo.Workstation.Plugins.Manager
{
    public class PluginSearchCriteria
    {
        #region Private Members

        private List<String> m_Keywords;

        private String m_Path;

        #endregion Private Members


        #region Public Members

        public List<String> Keywords
        {
            get
            {
                return m_Keywords;
            }

            set
            {
                m_Keywords = value;
            }
        }

        public String Path
        {
            get
            {
                return m_Path;
            }

            set
            {
                m_Path = value;
            }
        }

        #endregion Public Members


        #region Constructors

        public PluginSearchCriteria()
        {
            m_Keywords = new List<string>();
            m_Keywords.Add(".dll");
            m_Keywords.Add("L3Plugin");
            m_Path = AppDomain.CurrentDomain.BaseDirectory + "Plugins\\";
        }
		
        public PluginSearchCriteria (String path)
        {
            m_Keywords = new List<string>();
            m_Keywords.Add(".dll");
            m_Keywords.Add("L3Plugin");
            m_Path = path;
        }

        public PluginSearchCriteria (String path, List<String> keywords)
        {
            m_Keywords = keywords;
            m_Path = path;
        }

        #endregion Constructors
    }
}
