using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Reflection;
using System;
using System.Windows.Data;
using System.IO;
using System.Windows.Forms;
using System.Windows;

namespace L3.Cargo.Linac.Display.Common
{
    /// <summary>
    /// Wraps up XAML access to instance of WPFLocalize.Properties.Resources, list of available cultures, and method to change culture
    /// </summary>
    public class CultureResources
    {
        /// <summary>
        /// Local resources.
        /// </summary>
        private static Resources resources = null;
        /// <summary>
        /// The Resources ObjectDataProvider uses this method to get an instance of the WPFLocalize.Properties.Resources class
        /// </summary>
        /// <returns></returns>
        public static Resources GetResourceInstance()
        {
            if (resources == null)
            {
                resources = new Resources();
                Resources.Culture = L3.Cargo.Common.Dashboard.CultureResources.getCultureSetting();
                L3.Cargo.Common.Dashboard.CultureResources.getDataProvider().DataChanged += new EventHandler(CultureResources_DataChanged);
            }
            return resources;
        }

        static void CultureResources_DataChanged(object sender, EventArgs e)
        {
            Resources.Culture = L3.Cargo.Common.Dashboard.CultureResources.getCultureSetting();
        }

        /// <summary>
        /// Local ODP object.
        /// </summary>
        private static ObjectDataProvider odp;

        /// <summary>
        /// Gets an ODP to use for registering for language changing.
        /// </summary>
        /// <returns>The ODP.</returns>
        public static ObjectDataProvider getDataProvider()
        {
            if (odp == null)
            {
                odp = new ObjectDataProvider();
                odp.ObjectType = (new CultureResources()).GetType();
                odp.MethodName = "GetResourceInstance";
                L3.Cargo.Common.Dashboard.CultureResources.registerDataProvider(odp);
            }
            return odp;
        }

        /// <summary>
        /// Framework views must be registered with this method to recieve updates when the language changes.
        /// </summary>
        /// <param name="frameworkElement">The framework element to register.</param>
        public static void registerDataProvider(FrameworkElement frameworkElement)
        {
            L3.Cargo.Common.Dashboard.CultureResources.registerDataProvider(frameworkElement);
        }
    }
}

