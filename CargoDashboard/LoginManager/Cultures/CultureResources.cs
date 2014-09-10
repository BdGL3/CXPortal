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
using System.Windows.Controls;
using System.ComponentModel;
using System.Configuration;

namespace LoginManager
{
    /// <summary>
    /// Wraps up XAML access to instance of WPFLocalize.Properties.Resources, list of available cultures, and method to change culture
    /// </summary>
    public class CultureResources
    {
        /// <summary>
        /// Flag to mark when the cultures have been found.
        /// </summary>
        private static bool bFoundInstalledCultures = false;

        /// <summary>
        /// List of the supported cultures.
        /// </summary>
        private static List<CultureInfo> pSupportedCultures = new List<CultureInfo>();
        /// <summary>
        /// List of available cultures, enumerated at startup
        /// </summary>
        public static List<CultureInfo> SupportedCultures
        {
            get { return pSupportedCultures; }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CultureResources()
        {
            String configPath = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            if (!bFoundInstalledCultures)
            {
                //determine which cultures are available to this application
                Debug.WriteLine("Get Installed cultures:");
                CultureInfo tCulture = new CultureInfo("");
                String[] dirs = Directory.GetDirectories(System.Windows.Forms.Application.StartupPath);
                foreach (string dir in dirs)
                {
                    try
                    {
                        //see if this directory corresponds to a valid culture name
                        DirectoryInfo dirinfo = new DirectoryInfo(dir);
                        tCulture = CultureInfo.GetCultureInfo(dirinfo.Name);

                        //determine if a resources dll exists in this directory that has the correct extension
                        if (dirinfo.GetFiles("*.resources.dll").Length > 0)
                        {
                            pSupportedCultures.Add(tCulture);
                            Debug.WriteLine(string.Format(" Found Culture: {0} [{1}]", tCulture.DisplayName, tCulture.Name));
                        }
                    }
                    catch (ArgumentException) //ignore exceptions generated for any unrelated directories in the bin folder
                    {
                    }
                }
                // set the language from the stored setting
                ChangeCulture(getCultureSetting());
                bFoundInstalledCultures = true;
            }
        }

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
            }
            return resources;
        }

        /// <summary>
        /// List of the providers from the controls. This list is used to update the language on all the controls.
        /// </summary>
        private static List<ObjectDataProvider> providers = new List<ObjectDataProvider>();


        /// <summary>
        /// Change the current culture used in the application.
        /// If the desired culture is available all localized elements are updated.
        /// </summary>
        /// <param name="culture">Culture to change to</param>
        public static void ChangeCulture(CultureInfo culture)
        {
            //remain on the current culture if the desired culture cannot be found
            // - otherwise it would revert to the default resources set, which may or may not be desired.
            if (pSupportedCultures.Contains(culture))
            {
                // set the culture
                Resources.Culture = culture;
                // set the input language on the computer.
                InputLanguage.CurrentInputLanguage = InputLanguage.FromCulture(culture);
                // update the providers
                foreach (ObjectDataProvider provider in providers)
                {
                    provider.Refresh();
                }
            }
            else
                Debug.WriteLine(string.Format("Culture [{0}] not available", culture));
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
                registerDataProvider(odp);
            }
            return odp;
        }

        /// <summary>
        /// Gets the stored culture in the settings. This will be the last selected language and is persisted across shutdown.
        /// </summary>
        /// <returns>The currently or last set culture.</returns>
        public static CultureInfo getCultureSetting()
        {
            string configFilePath = MainWindow.GetDashboardApplicationPath();
            try
            {
                Configuration configuration = ConfigurationManager.OpenExeConfiguration(configFilePath);
                string culture = configuration.AppSettings.Settings["SelectedLanguage"].Value;
                return new CultureInfo(culture);
            }
            catch (Exception ex)
            {
                // no dashboard configuration for language found
                return null;
            }
        }


        /// <summary>
        /// Framework views must be registered with this method to recieve updates when the language changes.
        /// </summary>
        /// <param name="frameworkElement">The framework element to register.</param>
        public static void registerDataProvider(FrameworkElement frameworkElement)
        {
            // register the data provider when the element is loaded
            frameworkElement.Loaded += FrameworkElement_Loaded;

            // deregister the provider when the element is unloaded
            frameworkElement.Unloaded += FrameworkElement_Unloaded;

            // register for visibility changes because an element can already be loaded but be invisible
            frameworkElement.IsVisibleChanged += frameworkElement_IsVisibleChanged;

            if (frameworkElement.IsLoaded)
            {
                // call the loaded event if it's already loaded
                FrameworkElement_Loaded(frameworkElement, null);
            }
        }

        /// <summary>
        /// Handles visibility changing of framework elements.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private static void frameworkElement_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement elem = sender as FrameworkElement;
            if (elem.IsVisible)
            {
                ObjectDataProvider provider = ((sender as FrameworkElement).Resources["Resources"] as ObjectDataProvider);
                provider.Refresh();
                CultureResources.registerDataProvider(provider);
            }
        }

        /// <summary>
        /// Handles loading of a framework element.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private static void FrameworkElement_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement elem = sender as FrameworkElement;
            if (elem.IsVisible == false)
            {
                // don't register the provider if this element is not visible. The problem is the element is never unloaded unless it is made
                // visible first. The visibility handler will register the provider when the element becomes visible.
                return;
            }
            ObjectDataProvider provider = (elem.Resources["Resources"] as ObjectDataProvider);
            provider.Refresh();
            CultureResources.registerDataProvider(provider);
        }

        /// <summary>
        /// Handles unloading of a framework element.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private static void FrameworkElement_Unloaded(object sender, RoutedEventArgs e)
        {
            ObjectDataProvider provider = ((sender as FrameworkElement).Resources["Resources"] as ObjectDataProvider);
            CultureResources.deregisterDataProvider(provider);
        }

        /// <summary>
        /// ODP for views must be registered with this method to recieve updates when the language changes.
        /// </summary>
        /// <param name="provider">The ODP to register.</param>
        public static void registerDataProvider(ObjectDataProvider provider)
        {
            providers.Add(provider);
        }

        /// <summary>
        /// Deregisters an ODP from recieving language change events.
        /// </summary>
        /// <param name="provider">The ODP to deregister.</param>
        public static void deregisterDataProvider(ObjectDataProvider provider)
        {
            providers.Remove(provider);
        }
    }

    /// <summary>
    /// Class for binding the error and warning messages to.
    /// </summary>
    public class NotificationHeaderControl : HeaderedContentControl
    {
        /// <summary>
        /// The error number.
        /// </summary>
        private object m_errorNum;

        /// <summary>
        /// The error number property for binding to.
        /// </summary>
        [Bindable(true)]
        public object ErrorNum
        {
            get
            {
                return m_errorNum;
            }
        }

        /// <summary>
        /// Constructor for the NotificationHeaderControl.
        /// </summary>
        /// <param name="errorNum">The error number.</param>
        public NotificationHeaderControl(object errorNum)
        {
            m_errorNum = errorNum;
        }
    }
}

