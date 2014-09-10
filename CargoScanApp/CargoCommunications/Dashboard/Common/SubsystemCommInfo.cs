using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3.Cargo.Communications.Dashboard.Common
{
    public static class SubsystemCommInfo
    {
        #region private members

        private const string m_BroadcastMetaDataAlias = "HostAlias";

        private const string m_BroadMetaDataIPAddresses = "AcceptedIPAddresses";

        private const string m_BroadcastMetaDataSubsystemTag = "SubsystemTag";

        private const string m_BroadcastMetaDataAssemblyTag = "AssemblyTag";

        #endregion

        #region public members

        public static string BraodcastMetaDataAlias
        {
            get { return m_BroadcastMetaDataAlias; }
        }

        public static string BroadMetaDataIPAddresses
        {
            get { return m_BroadMetaDataIPAddresses; }
        }

        public static string BroadcastMetaDataSubsystemTag
        {
            get { return m_BroadcastMetaDataSubsystemTag; }
        }

        public static string BroadcastMetaDataAssemblyTag
        {
            get { return m_BroadcastMetaDataAssemblyTag; }
        }

        #endregion
    }
}