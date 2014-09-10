using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using L3.Cargo.Communications.Interfaces;
using L3.Cargo.Workstation.Plugins.Common;
using System.ComponentModel;
using L3.Cargo.Workstation.PresentationCore;
using L3.Cargo.Workstation.SystemConfigurationCore;
using L3.Cargo.Common;


namespace L3.Cargo.Workstation.MainPanel.ClearScreen
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl, IDisposable, INotifyPropertyChanged
    {
        private Framework m_MainFrameworkWindow;

        private SysConfigMgrAccess m_SysConfigMgr;

        #region Public Members

        public OpenCaseHandler OpenCase;
        public CloseCaseHandler CloseCase;

        public bool IsCompareAvailable
        {
            get
            {
                return m_MainFrameworkWindow.IsCompareAvailable;
            }
        }

        #endregion

        public UserControl1(Framework frameWorkWindow, SysConfigMgrAccess sysConfigMgr)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            m_MainFrameworkWindow = frameWorkWindow;
            m_SysConfigMgr = sysConfigMgr;
            if (sysConfigMgr.GetDefaultConfig().AutoSelectPendingCasesEnabled && !sysConfigMgr.GetDefaultConfig().SelectedArchiveDuringAutoSelect)
                ClearScreenButton.Visibility = System.Windows.Visibility.Collapsed;
            else
                ClearScreenButton.Visibility = System.Windows.Visibility.Visible;

            sysConfigMgr.GetDefaultConfig().SelectedArchiveDuringAutoSelectChanged += new SysConfiguration.SelectedArchiveDuringAutoSelectChangedEventHandler(ButtonDisplayChange);
        }

        private void ButtonDisplayChange (bool SelectedArchiveDuringAutoSelect)
        {
            if (!SelectedArchiveDuringAutoSelect)
                ClearScreenButton.Visibility = System.Windows.Visibility.Collapsed;
            else
                ClearScreenButton.Visibility = System.Windows.Visibility.Visible;
        }

        #region IDisposable

        public void Dispose()
        {
            ClearScreenButton.Visibility = System.Windows.Visibility.Collapsed;
        }

        #endregion

        #region INotifyPropertyChanged

        void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion               

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CloseCase(String.Empty, String.Empty, CaseUpdateEnum.CloseCase);

                if (m_SysConfigMgr.GetDefaultConfig().AutoSelectPendingCasesEnabled)
                {
                    m_SysConfigMgr.GetDefaultConfig().SelectedArchiveDuringAutoSelect = false;
                }

                NotifyPropertyChanged("IsCompareAvailable");
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }
    }
}
