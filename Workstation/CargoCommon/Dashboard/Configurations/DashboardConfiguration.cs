using System.Configuration;

namespace L3.Cargo.Common.Dashboard.Configurations
{
    /// <summary>
    /// Defines class that reads Dashboard configuration from app.config
    /// </summary>
    public static class DashboardConfiguration
    {
        #region Public Members

        /// <summary>
        /// Gets the version number
        /// </summary>
        public static string VersionNumber
        {
            get
            {
                return (ConfigurationManager.AppSettings["VersionNumber"] != null) ? ConfigurationManager.AppSettings["VersionNumber"] : @"Unknown";
            }
        }

        /// <summary>
        /// Gets the build number
        /// </summary>
        public static string BuildNumber
        {
            get
            {
                return (ConfigurationManager.AppSettings["BuildNumber"] != null) ? ConfigurationManager.AppSettings["BuildNumber"] : @"Unknown";
            }
        }

        /// <summary>
        /// Gets the build date
        /// </summary>
        public static string BuildDate
        {
            get
            {
                return (ConfigurationManager.AppSettings["BuildDate"] != null) ? ConfigurationManager.AppSettings["BuildDate"] : @"Unknown";
            }
        }

        #endregion
    }
}
