using System;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using L3.Cargo.Common;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Interfaces;
using L3.Cargo.Workstation.SystemManagerCore;
using L3.Cargo.Workstation.PresentationCore.Common;
using L3.Cargo.Workstation.SystemConfigurationCore;
using System.Globalization;
using System.Diagnostics;

namespace L3.Cargo.Workstation.PresentationCore
{
    /// <summary>
    /// Interaction logic for Framework.xaml
    /// </summary>
    public partial class Framework : Window, INotifyPropertyChanged
    {

        protected CaseListDataSet m_CaseList;

        protected SystemManagerAccess m_SysMgr;

        protected DisplayedCases m_DisplayedCases;

        protected int m_MaxCasesOpen;

        protected SysConfigMgrAccess m_SysConfig;

        protected Boolean CompareCaseChecked;

        protected bool initialized;

        protected void NotifyPropertyChanged (String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsComparing
        {
            get
            {                               
                return (CompareCaseChecked && IsCompareAvailable); 
            }
        }

        public bool IsCompareAvailable
        {
            get
            {
                bool IsAvailable = false;

                if (m_DisplayedCases != null)
                {
                    IsAvailable = ((m_DisplayedCases.Count < m_MaxCasesOpen) && (m_DisplayedCases.Count > 0)) ? true : false;
                }

                return IsAvailable;
            }
        }
        
        public Framework(SystemManagerAccess sysMgr, SysConfigMgrAccess sysConfig) : 
            base ()
        {
            initialized = false;
            InitializeComponent();
            CultureResources.registerDataProvider(this);
            CultureResources.getDataProvider().DataChanged += new System.EventHandler(LanguageChangeControl_DataChanged);

            m_SysMgr = sysMgr;
            m_SysConfig = sysConfig;
            m_DisplayedCases = new DisplayedCases();           

            SysConfiguration sysConfiguration = sysConfig.GetDefaultConfig();
            VersionNumber.Text = sysConfiguration.VersionNumber;
            BuildNumber.Text = sysConfiguration.BuildNumber;
            BuildDate.Text = sysConfiguration.BuildDate;

            m_MaxCasesOpen = (int)sysConfiguration.MaxCompareCases + 1;
        }

        protected virtual void Window_Closing (object sender, System.ComponentModel.CancelEventArgs e)
        {
            CultureResources.getDataProvider().DataChanged -= this.LanguageChangeControl_DataChanged;
        }

        private void Button_Click (object sender, RoutedEventArgs e)
        {
            AboutBoxPopUp.IsOpen = true;
        }

        /// <summary>
        /// Handles the event of selection changed 
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Event arguments</param>
        private void cbLanguages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CultureInfo selected_culture = cbLanguages.SelectedItem as CultureInfo;

            //if not current language
            //could check here whether the culture we want to change to is available in order to provide feedback / action
            if (initialized && selected_culture != null && !selected_culture.Equals(L3.Cargo.Common.Resources.Culture))
            {
                Debug.WriteLine(string.Format("Change Current Culture to [{0}]", selected_culture));

                //change resources to new culture
                CultureResources.ChangeCulture(selected_culture);
            }
        }

        /// <summary>
        /// Updates the combo box selection to the current langauge.
        /// </summary>
        private void updateComboBox()
        {
            // update the combo box to match the current language
            cbLanguages.SelectedItem = L3.Cargo.Common.Resources.Culture;
        }

        /// <summary>
        /// Handles the language change event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The parameters of the event.</param>
        private void LanguageChangeControl_DataChanged(object sender, System.EventArgs e)
        {
            if (cbLanguages != null)
            {
                updateComboBox();
            }
        }

        /// <summary>
        /// Handles the initialization complete even of the language selector combo box.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void cbLanguages_Initialized(object sender, EventArgs e)
        {
            // after initialization update the combo box to match the current language
            initialized = true;
            updateComboBox();
        }
    }
}
