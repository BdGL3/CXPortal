using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using L3.Cargo.Common;
using L3.Cargo.Workstation.Plugins.Manager;
using L3.Cargo.Workstation.PresentationCore.Common;
using L3.Cargo.Workstation.SystemConfigurationCore;
using L3.Cargo.Workstation.SystemManagerCore;
using L3.Cargo.Workstation.Plugins.Common;
using System.Collections.Generic;
using L3.Cargo.Communications.Interfaces;
using System.Threading;
using System.Windows.Threading;

namespace L3.Cargo.Workstation.PresentationCore
{
    public class LayoutManager : Framework
    {
        #region Private Members

        private ContentPluginManager m_ContentPluginManager;

        private MainPanelPluginManager m_MainPanelPluginManager;

        private DualViewWindow m_DualViewWindow;

        private Dictionary<String, List<StatusBarItem>> StatusList;

        private Thread m_AutoSelectPendingCasesThread;

        private EventWaitHandle m_GetAvailablePendingCaseEvent;

        private Boolean m_LiveCaseDisplayed;

        private Boolean m_AutoSelectCaseEnabled;

        #endregion Private Members


        #region Constructors

        public LayoutManager(ContentPluginManager contentPluginManager, SysConfigMgrAccess sysConfig, SystemManagerAccess sysMgr, MainPanelPluginManager mainPanelPluginMgr) :
            base(sysMgr, sysConfig)
        {
            m_ContentPluginManager = contentPluginManager;
            m_MainPanelPluginManager = mainPanelPluginMgr;
            m_DualViewWindow = new DualViewWindow();
            StatusList = new Dictionary<string, List<StatusBarItem>>();
            m_GetAvailablePendingCaseEvent = new AutoResetEvent(false);
            m_LiveCaseDisplayed = false;
            m_AutoSelectCaseEnabled = false;

            this.Title = base.m_SysConfig.GetDefaultConfig().WorkstationMode + " Workstation";

            base.m_SysConfig.GetDefaultConfig().AutoSelectPendingCasesChanged += new
                SysConfiguration.AutoSelectPendingCasesChangedEventHandler(LayoutManager_AutoSelectPendingCasesChanged);
        }

        #endregion Constructors


        #region Private Methods

        private void LayoutManager_AutoSelectPendingCasesChanged(Boolean PendingCaseAutoSelectionEnabled)
        {
            m_AutoSelectCaseEnabled = PendingCaseAutoSelectionEnabled;

            if (!PendingCaseAutoSelectionEnabled && m_AutoSelectPendingCasesThread != null)
            {
                m_AutoSelectPendingCasesThread.Abort();
                m_AutoSelectPendingCasesThread.Join();

                m_AutoSelectPendingCasesThread = null;
            }
            else if (PendingCaseAutoSelectionEnabled)
            {
                if (m_AutoSelectPendingCasesThread == null)
                {
                    m_AutoSelectPendingCasesThread = new Thread(new ThreadStart(AutoSelectPendingCaseThreadMethod));
                    m_AutoSelectPendingCasesThread.Name = "Auto Select Case Thread";
                    m_AutoSelectPendingCasesThread.Start();
                }

                m_GetAvailablePendingCaseEvent.Set();

            }
        }

        private void AutoSelectPendingCaseThreadMethod()
        {
            Exception Exp = null;

            while (true)
            {
                try
                {

                    m_GetAvailablePendingCaseEvent.WaitOne(6000);

                    if (!m_LiveCaseDisplayed)
                    {
                        //wait some time to clear the screen before displaying the next case information.
                        Thread.Sleep(100);

                        DisplayedCase displayCase = null;

                        MainPanelStackPanel.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                            {
                                displayCase = new DisplayedCase(String.Empty, true);
                            }));

                        if (displayCase != null)
                        {
                            DisplayCase(String.Empty, displayCase);
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception exp)
                {
                    if (exp.Message == ErrorMessages.NO_LIVE_CASE ||
                        exp.Message == ErrorMessages.NO_LIVE_SOURCES ||
                        exp.Message == ErrorMessages.SOURCE_NOT_AVAILABLE)
                        m_SysMgr.WaitForAvailableCase(5000);
                    else
                    {
                        Thread.Sleep(3000);
                    }

                    m_GetAvailablePendingCaseEvent.Set();
                }

            }

        }

        protected override void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {

                CleanUpCases(CaseUpdateEnum.CloseCase);
            }
            catch (Exception)
            {

            }
            finally
            {
                if (m_DualViewWindow != null)
                {
                    m_DualViewWindow.Close();
                    m_DualViewWindow = null;
                }

                if (m_AutoSelectPendingCasesThread != null)
                {
                    m_AutoSelectPendingCasesThread.Abort();
                    m_AutoSelectPendingCasesThread.Join();
                    m_AutoSelectPendingCasesThread = null;
                }

                if (m_GetAvailablePendingCaseEvent != null)
                {
                    m_GetAvailablePendingCaseEvent.Close();
                }

                m_SysMgr.Shutdown();

                base.Window_Closing(sender, e);
            }
        }

        private void DisplayCase(String sourceAlias, DisplayedCase displayCase)
        {
            try
            {
                CaseObject caseObj = null;

                if (m_AutoSelectCaseEnabled && !m_SysConfig.GetDefaultConfig().SelectedArchiveDuringAutoSelect)
                {

                    m_SysMgr.AutoSelectCase(out caseObj);

                    if (caseObj != null)
                    {
                        displayCase.CaseID = caseObj.CaseId;

                        displayCase.IsCTICase = (caseObj.caseType == Cargo.Common.CaseType.CTICase) ? true : false;
                        displayCase.IsFTICase = (caseObj.caseType == Cargo.Common.CaseType.FTICase) ? true : false;
                    }

                }
                else if (!String.IsNullOrWhiteSpace(sourceAlias) && !String.IsNullOrWhiteSpace(displayCase.CaseID))
                {
                    m_SysMgr.GetCase(sourceAlias, displayCase.CaseID, out caseObj, displayCase.IsCaseEditable);

                    displayCase.IsCTICase = (caseObj.caseType == Cargo.Common.CaseType.CTICase) ? true : false;
                    displayCase.IsFTICase = (caseObj.caseType == Cargo.Common.CaseType.FTICase) ? true : false;
                }

                if (caseObj != null)
                {

                    MainPanelStackPanel.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                            {
                                try
                                {
                                    displayCase.IsCaseEditable = caseObj.IsCaseEditable;

                                    ContentParameter parameters = new ContentParameter(caseObj, m_SysConfig.GetConfig(caseObj.SourceAlias));

                                    displayCase.ContentInstances = m_ContentPluginManager.GetInstances(parameters);

                                    ConstructLayout(displayCase);

                                    m_DisplayedCases.Add(displayCase);

                                    if (displayCase.IsPrimaryCase)
                                    {
                                        DisplayedCase dispalyedCase = m_DisplayedCases.Find(displayCase.CaseID);

                                        MainPanelParameter Parameters = new MainPanelParameter(caseObj, m_SysConfig, m_SysMgr, m_DisplayedCases.GetPrinterObjects(), FrameWorkWindow);

                                        dispalyedCase.mainPanelInstances = m_MainPanelPluginManager.GetInstances(Parameters);

                                        MainPanelInstance DecisionPlugin = null;

                                        foreach (MainPanelInstance mainPanelInst in dispalyedCase.mainPanelInstances)
                                        {
                                            if (mainPanelInst.Instance.Name.Contains("Decision"))
                                                DecisionPlugin = mainPanelInst;
                                            else
                                                MainPanelStackPanel.Children.Add(mainPanelInst.Instance.UserControlDisplay);

                                            if (mainPanelInst.Instance.Name.Contains("ClearCase"))
                                            {
                                                mainPanelInst.Instance.SetOpenAndCloseCaseCallback(DefaultOpenCase, ClearScreenCloseCase);
                                            }
                                            else if (mainPanelInst.Instance.Name.Contains("Decision"))
                                            {
                                                mainPanelInst.Instance.SetOpenAndCloseCaseCallback(DefaultOpenCase, DecisionCloseCase);
                                            }
                                        }

                                        if (DecisionPlugin != null)
                                            MainPanelStackPanel.Children.Add(DecisionPlugin.Instance.UserControlDisplay);

                                        caseObj.AnalysisStartTime = DateTime.Now;
                                    }

                                    NotifyPropertyChanged("IsCompareAvailable");
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message);
                                }

                            }));
                }
            }
            catch (Exception ex)
            {
                if (m_SysConfig.GetDefaultConfig().AutoSelectPendingCasesEnabled)
                {
                    throw;
                }
                else
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void ConstructLayout(DisplayedCase displayCase)
        {
            Boolean DualViewAvailable = (m_DualViewWindow != null && m_DualViewWindow.IsWindowAvailable
                && (m_DisplayedCases.Count == 0) && !IsComparing);

            Boolean MoveDualViewTabItems = ((m_DisplayedCases.Count == 1) && IsComparing);

            if (!StatusList.ContainsKey(displayCase.CaseID))
                StatusList.Add(displayCase.CaseID, displayCase.StatusBarItems);

            StatusBar.Items.Clear();

            foreach (StatusBarItem statusBarItem in displayCase.StatusBarItems)
            {
                if (!StatusBar.Items.Contains(statusBarItem))
                {
                    StatusBar.Items.Add(statusBarItem);
                }
            }

            if (MoveDualViewTabItems)
            {
                DisplayedCase primaryCase = m_DisplayedCases.GetPrimaryCase();
                if (primaryCase != null)
                {
                    MoveTabItems(m_DualViewWindow.PrimaryTabControl, primaryCase.PanelLayout.MainTabControl);
                }
            }

            if (DualViewAvailable)
            {
                MoveTabItems(displayCase.SecTabControl, m_DualViewWindow.PrimaryTabControl);
            }
            else
            {
                MoveTabItems(displayCase.SecTabControl, displayCase.PanelLayout.MainTabControl);
            }


            displayCase.CaseTabItem = new TabItem();
            displayCase.CaseTabItem.Header = displayCase.CaseID;
            displayCase.CaseTabItem.Style = (Style)FindResource("TabItemTemplate");
            displayCase.CaseTabItem.Content = displayCase.PanelLayout;

            displayCase.CaseTabItem.AddHandler(CloseTabItemButton.ClickEvent, new RoutedEventHandler(this.CloseCase));

            if (IsComparing)
            {
                if (m_DualViewWindow != null && m_DualViewWindow.IsWindowAvailable)
                {
                    displayCase.Parent = m_DualViewWindow.PrimaryTabControl;
                    m_DualViewWindow.PrimaryTabControl.MouseEnter += new MouseEventHandler(PrimaryTabControl_MouseEnter);
                }
                else
                {
                    displayCase.Parent = CompareTabControl;
                    CompareTabControl.MouseEnter += new MouseEventHandler(PrimaryTabControl_MouseEnter);
                    CompareTabControl.SelectionChanged += new SelectionChangedEventHandler(CompareTabControl_SelectionChanged);
                }
                displayCase.CaseTabItem.Visibility = Visibility.Visible;
            }
            else
            {
                displayCase.Parent = PrimaryTabControl;
                PrimaryTabControl.MouseEnter += new MouseEventHandler(PrimaryTabControl_MouseEnter);
            }

            displayCase.Parent.Items.Add(displayCase.CaseTabItem);
            displayCase.Parent.SelectedItem = displayCase.CaseTabItem;

            if (displayCase.Parent == PrimaryTabControl)
                m_LiveCaseDisplayed = true;
        }

        private void CompareTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PrimaryTabControl_MouseEnter(sender, null);
        }

        private void PrimaryTabControl_MouseEnter(object sender, MouseEventArgs e)
        {
            TabControl tabCtrl = sender as TabControl;

            StatusBar.Items.Clear();

            TabItem selectedTab = (TabItem)tabCtrl.SelectedItem;
            if (selectedTab != null)
            {
                String CaseID = (String)selectedTab.Header;

                if (StatusList.ContainsKey(CaseID))
                {
                    foreach (StatusBarItem statusBarItem in StatusList[CaseID])
                    {
                        if (!StatusBar.Items.Contains(statusBarItem))
                        {
                            StatusBar.Items.Add(statusBarItem);
                        }
                    }
                }
            }
        }

        private void MoveTabItems(TabControl oldTabControl, TabControl newTabControl)
        {
            for (Int32 index = oldTabControl.Items.Count - 1; index >= 0; index--)
            {
                TabItem tabItem = oldTabControl.Items[index] as TabItem;
                if (tabItem != null)
                {
                    oldTabControl.Items.RemoveAt(index);
                    newTabControl.Items.Add(tabItem);
                }

                newTabControl.SelectedItem = newTabControl.Items[0];
            }
        }

        private void CleanUpCases(CaseUpdateEnum updateType)
        {
            try
            {
                for (int index = m_DisplayedCases.Count - 1; index >= 0; index--)
                {
                    if (m_DisplayedCases[index].CaseTabItem != null)
                    {
                        m_DisplayedCases[index].CaseTabItem.RemoveHandler(CloseTabItemButton.ClickEvent, new RoutedEventHandler(this.CloseCase));
                    }
                    String CaseId = m_DisplayedCases[index].CaseID;
                    CleanUpCase(CaseId, updateType);
                }
            }
            finally
            {
                if (m_DualViewWindow != null)
                {
                    m_DualViewWindow.PrimaryTabControl.Items.Clear();
                }

                if (PrimaryTabControl.Items.Count > 0)
                    PrimaryTabControl.Items.Clear();

                if (CompareTabControl.Items.Count > 0)
                    CompareTabControl.Items.Clear();

                if (StatusBar.Items.Count > 0)
                    StatusBar.Items.Clear();
            }

            NotifyPropertyChanged("IsCompareAvailable");
        }

        private void CleanUpCase(string caseId, CaseUpdateEnum updateType)
        {
            DisplayedCase displayedCase = m_DisplayedCases.Find(caseId);
            if (displayedCase != null)
            {
                if (displayedCase.Parent == PrimaryTabControl)
                    m_LiveCaseDisplayed = false;

                if ((updateType == CaseUpdateEnum.CloseCase) || (updateType == CaseUpdateEnum.ReleaseCase))
                {
                    m_DisplayedCases.Remove(displayedCase);

                    for (int index = MainPanelStackPanel.Children.Count - 1; index > 0; index--)
                    {
                        MainPanelStackPanel.Children.RemoveAt(index);
                    }
                }

                m_SysMgr.CloseCase(caseId, updateType);

                NotifyPropertyChanged("IsCompareAvailable");
            }
        }

        private void CloseCase(object source, RoutedEventArgs args)
        {
            TabItem tabItem = args.Source as TabItem;
            if (tabItem != null)
            {
                string caseId = tabItem.Header.ToString();
                TabControl tabControl = tabItem.Parent as TabControl;
                if (tabControl != null)
                {
                    tabControl.Items.Remove(tabItem);
                }

                m_DisplayedCases.Remove(caseId);

                CleanUpCase(caseId, CaseUpdateEnum.CloseCase);

                StatusList.Remove(caseId);
            }

            NotifyPropertyChanged("IsCompareAvailable");
        }

        private void DefaultOpenCase(String SourceAlias, String CaseId, Boolean CompareCase)
        {
            //Default open case function.  Currently does nothing
        }

        private void DefaultCloseCase(String SourceAlias, String CaseId, CaseUpdateEnum updateType)
        {
            //Default close case function.  Currently does nothing
        }

        private void CasesOpenCase(String SourceAlias, String CaseID, Boolean CompareCase)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(SourceAlias) && !m_DisplayedCases.Contains(CaseID))
                {
                    CompareCaseChecked = CompareCase;

                    if (!IsComparing)
                    {
                        CleanUpCases(CaseUpdateEnum.CloseCase);
                    }

                    DisplayedCase displayCase = new DisplayedCase(CaseID, !CompareCase);
                    DisplayCase(SourceAlias, displayCase);
                }
                else if (m_DisplayedCases.Contains(CaseID))
                {
                    throw new Exception(L3.Cargo.Common.Resources.SelectedCaseIsAlreadyBeingDisplayed);
                }
            }
            catch (Exception ex)
            {
                //TODO: Log error Message Here
                throw ex;
            }
        }

        private void ClearScreenCloseCase(String SourceAlias, String CaseId, CaseUpdateEnum updateType)
        {
            try
            {
                CleanUpCases(updateType);

                if (m_AutoSelectCaseEnabled)
                    m_GetAvailablePendingCaseEvent.Set();
            }
            catch
            {
                throw;
            }
        }

        private void DecisionCloseCase(String SourceAlias, String CaseId, CaseUpdateEnum updateType)
        {
            try
            {
                DisplayedCase displayedCase = m_DisplayedCases.Find(CaseId);


                if (displayedCase != null && (!displayedCase.IsCTICase && !displayedCase.IsFTICase) && (updateType == CaseUpdateEnum.ReleaseCase))
                {
                    CleanUpCases(updateType);

                }
                else if (displayedCase != null && ((!displayedCase.IsCTICase && !displayedCase.IsFTICase) || updateType == CaseUpdateEnum.Result))
                {
                    CleanUpCase(CaseId, updateType);
                }
                else
                {
                    m_SysConfig.GetDefaultConfig().SelectedArchiveDuringAutoSelect = true;
                }
            }
            finally
            {
                if (m_AutoSelectCaseEnabled)
                {
                    m_GetAvailablePendingCaseEvent.Set();
                }
            }
        }

        #endregion Private Methods


        #region Public Methods

        public new void Show()
        {
            base.Show();

            MainPanelParameter Parameter = new MainPanelParameter(null, m_SysConfig, m_SysMgr, m_DisplayedCases.GetPrinterObjects(), FrameWorkWindow);
            MainPanelInstance mainPanelInstance = m_MainPanelPluginManager.GetCasesMainPanelInstance(Parameter);

            if (mainPanelInstance != null)
            {
                mainPanelInstance.Instance.SetOpenAndCloseCaseCallback(CasesOpenCase, DefaultCloseCase);
                MainPanelStackPanel.Children.Add(mainPanelInstance.Instance.UserControlDisplay);
            }

            if (m_DualViewWindow != null)
            {
                m_DualViewWindow.Show();
                m_DualViewWindow.WindowState = WindowState.Maximized;
            }
        }

        #endregion Public Methods
    }
}
