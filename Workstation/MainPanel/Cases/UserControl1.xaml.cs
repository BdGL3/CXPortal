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
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using L3.Cargo.Workstation.PresentationCore;
using L3.Cargo.Workstation.SystemConfigurationCore;
using L3.Cargo.Workstation.SystemManagerCore;
using L3.Cargo.Workstation.Common;

namespace L3.Cargo.Workstation.MainPanel.Cases
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl, IDisposable, INotifyPropertyChanged
    {
        #region Private Members

        CaseSourcesList m_ACSSourceList;

        CaseSourcesList m_WSCommSourceList;

        private CaseSourcesObject m_SelectedCaseSource;

        private SysConfigMgrAccess m_SysConfigMgr;

        private SystemManagerAccess m_SysMgr;

        private CaseListSearch m_CaseListSearch;

        private bool m_IsACSSource;

        private CaseListDataSet m_CaseList;

        private string m_UserName;

        private string m_Password;

        private Framework m_MainFrameworkWindow;

        private Boolean m_AutoSelectPendingCaseEnabled = false;

        #endregion


        #region Public Members

        public Framework MainFrameworkWindow
        {
            get
            {
                return m_MainFrameworkWindow;
            }
        }

        public OpenCaseHandler OpenCase;
        public CloseCaseHandler CloseCase;

        public CaseSourcesObject SelectedCaseSource
        {
            get
            {
                return m_SelectedCaseSource;
            }
            set
            {
                m_SelectedCaseSource = value;
                NotifyPropertyChanged("SelectedCaseSource");
            }
        }

        public bool IsCompareAvailable
        {
            get
            {
                return m_MainFrameworkWindow.IsCompareAvailable;
            }
        }

        #endregion


        #region Constructors

        public UserControl1(SystemManagerAccess sysMgr, SysConfigMgrAccess sysConfig, CaseObject caseObj, Framework frameWorkWindow)
        {
            try
            {
                InitializeComponent();
                CultureResources.registerDataProvider(this);
                CultureResources.getDataProvider().DataChanged += new EventHandler((object sender, EventArgs e) =>
                {
                    RefreshCaseList();
                });

                SourceSelection.PlacementTarget = frameWorkWindow;
                m_MainFrameworkWindow = frameWorkWindow;
                m_SelectedCaseSource = null;
            }
            catch (Exception ex)
            {
            }

            m_SysConfigMgr = sysConfig;
            m_SysMgr = sysMgr;

            try
            {
                m_SysMgr.RequestSources(SourceType.ArchiveCase, out m_ACSSourceList);
                m_SysMgr.RequestSources(SourceType.WSComm, out m_WSCommSourceList);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            InitializeCaseListSearch();

            ACSSource_listBox.ItemsSource = m_ACSSourceList;
            AWSCommSource_listBox.ItemsSource = m_WSCommSourceList;

            if (sysConfig.GetDefaultConfig().WorkstationMode == "ManualCoding")
                ACSButtonRow.Height = new GridLength(0);

        }

        #endregion Constructors


        #region IDisposable

        public void Dispose()
        {
            CultureResources.getDataProvider().DataChanged -= this.LanguageChanged;
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


        private void InitializeCaseListSearch()
        {
            String[] collection = { "CaseID", "Analyst", "AnalystComment", "FlightNumber", "ObjectID", "Result", "UpdateTime" };

            m_CaseListSearch = new CaseListSearch(collection, RefreshCaseList);



            string configVal = m_SysConfigMgr.GetDefaultConfig().CaseFilterCaseID;
            if (configVal != "")
            {
                AddInitialFilter(m_CaseListSearch.GetListCriteriaItem("CaseID", configVal));
            }
            configVal = m_SysConfigMgr.GetDefaultConfig().CaseFilterAnalyst;
            if (configVal != "")
            {
                AddInitialFilter(m_CaseListSearch.GetListCriteriaItem("Analyst", configVal));
            }
            configVal = m_SysConfigMgr.GetDefaultConfig().CaseFilterAnalystComment;
            if (configVal != "")
            {
                AddInitialFilter(m_CaseListSearch.GetListCriteriaItem("AnalystComment", configVal));
            }
            configVal = m_SysConfigMgr.GetDefaultConfig().CaseFilterFlightNumber;
            if (configVal != "")
            {
                AddInitialFilter(m_CaseListSearch.GetListCriteriaItem("FlightNumber", configVal));
            }
            configVal = m_SysConfigMgr.GetDefaultConfig().CaseFilterObjectID;
            if (configVal != "")
            {
                AddInitialFilter(m_CaseListSearch.GetListCriteriaItem("ObjectID", configVal));
            }
            configVal = m_SysConfigMgr.GetDefaultConfig().CaseFilterResult;
            if (configVal != "")
            {
                AddInitialFilter(m_CaseListSearch.GetListCriteriaItem("Result", configVal));
            }
            configVal = m_SysConfigMgr.GetDefaultConfig().CaseFilterArea;
            if (configVal != "")
            {
                AddInitialFilter(m_CaseListSearch.GetListCriteriaItem("Area", configVal));
            }
            int updateTime = m_SysConfigMgr.GetDefaultConfig().CaseFilterUpdateTime_DaysOld;
            if (updateTime != 0)
            {
                AddInitialFilter(m_CaseListSearch.GetListCriteriaItem("UpdateTime", updateTime.ToString()));
            }


            ////Add Analyst Comment search criteria
            //item = m_CaseListSearch.GetListCriteriaItem("AnalystComment");
            //panel = CreateItemStackPanel(item);
            //CaseListSearchCriteriaWrapPanel.Children.Add(panel);
            //m_CaseListSearch.DeleteSearchCriteriaCaseList.Add(item);
            //m_CaseListSearch.AddSearchCriteriaCaseList.Remove(item);

            ////Add Update Time search criteria
            //item = m_CaseListSearch.GetListCriteriaItem("UpdateTime");
            //panel = CreateItemStackPanel(item);
            //CaseListSearchCriteriaWrapPanel.Children.Add(panel);
            //m_CaseListSearch.DeleteSearchCriteriaCaseList.Add(item);
            //m_CaseListSearch.AddSearchCriteriaCaseList.Remove(item);

            //TextBox tempTextBox = new TextBox();
            //tempTextBox.Text = m_SysConfigMgr.GetDefaultConfig().WorkstationMode;
            //m_CaseListSearch.SearchCaseListCriteriaItemList.Add(new SearchCaseListCriteriaItem(tempTextBox, "Area"));

            AddCaseListSearchComboBox.DataContext = m_CaseListSearch.AddSearchCriteriaCaseList;
            DeleteCaseListSearchComboBox.DataContext = m_CaseListSearch.DeleteSearchCriteriaCaseList;
            var m_CaseListview = (ExtendedCollectionViewSource)FindResource("cvs");
            m_CaseListview.Filter += new FilterEventHandler(CaseListview_Filter);

            CultureResources.getDataProvider().DataChanged += LanguageChanged;
        }

        private void AddInitialFilter(SearchCaseListCriteriaItem item)
        {
            if (item != null)
            {
                StackPanel panel = CreateItemStackPanel(item);
                CaseListSearchCriteriaWrapPanel.Children.Add(panel);
                m_CaseListSearch.DeleteSearchCriteriaCaseList.Add(item);
                m_CaseListSearch.AddSearchCriteriaCaseList.Remove(item);
            }
        }

        private static StackPanel CreateItemStackPanel(SearchCaseListCriteriaItem item)
        {
            StackPanel panel = new StackPanel();
            panel.Margin = new Thickness(5);
            panel.Orientation = Orientation.Horizontal;
            try
            {
                panel.Children.Add(item.ItemLabel);
                panel.Children.Add(item.Element);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return panel;
        }

        private void SourceSelection_Closed(object sender, EventArgs e)
        {
            CompareCaseCheckBox.IsChecked = false;
        }

        private void Sources_Avail_Btn_Click(object sender, RoutedEventArgs e)
        {
            //(sender as Button).GetBindingExpression(Button.ContentProperty).UpdateTarget();

            SourceSelection.IsOpen = true;
            NotifyPropertyChanged("IsCompareAvailable");
        }

        private void AWSSources_Click(object sender, RoutedEventArgs e)
        {
            Source_listBox_SelectionChanged(AWSCommSource_listBox, e);

            m_IsACSSource = false;
            ACSSources.IsChecked = false;
            AWSSources.IsChecked = true;
            AWSCommList.Height = new GridLength(1, GridUnitType.Star);
            ACSList.Height = new GridLength(0);
            CompareCaseCheckBox.IsChecked = false;
            Source_dataGrid.IsEnabled = (AutoSelectPendingCaseCheckBox.IsChecked == true) ? false : true;
        }

        void Source_listBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ListBox ListBox1 = sender as ListBox;

            try
            {
                Source_dataGrid.DataContext = null;

                SelectedCaseSource = ListBox1.SelectedItem as CaseSourcesObject;

                if (SelectedCaseSource != null && SelectedCaseSource.IsLoggedIn)
                {
                    string selectedItem = SelectedCaseSource.Name;

                    try
                    {
                        DataSet tempDataSet;
                        m_SysMgr.GetCaseList(selectedItem, out tempDataSet);
                        m_CaseList = (CaseListDataSet)tempDataSet;
                        var CaseListview = (ExtendedCollectionViewSource)FindResource("cvs");
                        CaseListview.Source = m_CaseList.Tables[0].DefaultView;
                        Source_dataGrid.DataContext = CaseListview;
                        Source_dataGrid.SelectedIndex = -1;

                        foreach (DataGridColumn column in Source_dataGrid.Columns)
                        {
                            string path = BindingOperations.GetBindingExpression(column, DataGridColumn.HeaderProperty).ParentBinding.Path.Path;
                            if (path == "UpdateTime")
                            {
                                ListSortDirection direction = ListSortDirection.Descending;
                                column.SortDirection = direction;
                                SortCaseList(direction, column);
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //TODO: Log error
                    }
                    if (selectedItem.Contains("AWSComm"))
                        m_SysConfigMgr.GetConfig(selectedItem);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ACSSources_Click(object sender, RoutedEventArgs e)
        {
            Source_listBox_SelectionChanged(ACSSource_listBox, e);

            m_IsACSSource = true;
            AWSSources.IsChecked = false;
            ACSSources.IsChecked = true;
            AWSCommList.Height = new GridLength(0);
            ACSList.Height = new GridLength(1, GridUnitType.Star);
            Source_dataGrid.IsEnabled = true;
        }

        private void AddCaseListSearchComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmbBox = sender as ComboBox;
            if (cmbBox.SelectedItem != null)
            {
                SearchCaseListCriteriaItem item;
                //create search criteria item for the selected item
                item = (SearchCaseListCriteriaItem)cmbBox.SelectedItem;
                StackPanel panel = CreateItemStackPanel(item);
                CaseListSearchCriteriaWrapPanel.Children.Add(panel);

                m_CaseListSearch.DeleteSearchCriteriaCaseList.Add((SearchCaseListCriteriaItem)cmbBox.SelectedItem);
                m_CaseListSearch.AddSearchCriteriaCaseList.Remove((SearchCaseListCriteriaItem)cmbBox.SelectedItem);
                RefreshCaseList(null, null);
            }
            return;
        }

        private void DeleteCaseListSearchComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmbBox = sender as ComboBox;
            if (cmbBox.SelectedItem != null)
            {
                SearchCaseListCriteriaItem criteriaItem = (SearchCaseListCriteriaItem)cmbBox.SelectedItem;
                
                foreach (StackPanel panel in CaseListSearchCriteriaWrapPanel.Children)
                {
                    if (panel.Children.Contains(criteriaItem.Element))
                    {
                        CaseListSearchCriteriaWrapPanel.Children.Remove(panel);
                        panel.Children.Clear();
                        break;
                    }

                }
                m_CaseListSearch.AddSearchCriteriaCaseList.Add((SearchCaseListCriteriaItem)cmbBox.SelectedItem);
                m_CaseListSearch.DeleteSearchCriteriaCaseList.Remove((SearchCaseListCriteriaItem)cmbBox.SelectedItem);
                RefreshCaseList(null, null);
            }
            return;
        }

        private void Source_dataGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            DataRowView SelectedItem = (DataRowView)dataGrid.CurrentItem;

            try
            {
                if (SelectedItem != null && (ACSSource_listBox.SelectedItem != null || AWSCommSource_listBox.SelectedItem != null))
                {
                    string caseId = SelectedItem.Row["CaseId"].ToString();
                    string SourceAlias = m_IsACSSource ? ((CaseSourcesObject)ACSSource_listBox.SelectedItem).Name : ((CaseSourcesObject)AWSCommSource_listBox.SelectedItem).Name;

                    if (m_IsACSSource && AutoSelectPendingCaseCheckBox.IsChecked == true)
                    {
                        m_SysConfigMgr.GetDefaultConfig().SelectedArchiveDuringAutoSelect = true;
                    }
                    try
                    {
                        SourceSelection.IsOpen = (!SourceSelection.IsOpen);

                        if (OpenCase != null)
                            //Case is editable if it is not a compare case
                            OpenCase(SourceAlias, caseId, (Boolean)CompareCaseCheckBox.IsChecked);

                    }
                    catch (Exception exp)
                    {
                        if (MessageBox.Show(exp.Message/*"Selected case is already being displayed."*/) == MessageBoxResult.OK)
                        {
                            SourceSelection.IsOpen = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO: Log error Message Here
            }
        }

        private void Source_dataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;

            ListSortDirection direction = (e.Column.SortDirection != ListSortDirection.Ascending) ?
                ListSortDirection.Ascending : ListSortDirection.Descending;
            e.Column.SortDirection = direction;

            SortCaseList(direction, e.Column);
        }

        private void SortCaseList(ListSortDirection direction, DataGridColumn column)
        {
            CaseListSort sort = new CaseListSort(direction, column);
            var collectionview = (ExtendedCollectionViewSource)FindResource("cvs");
            ListCollectionView view = (ListCollectionView)collectionview.View;

            using (collectionview.DeferRefresh())
            {
                view.CustomSort = sort;
            }
        }

        private void RefreshCaseList(object sender, RoutedEventArgs e)
        {
            try
            {
                RefreshCaseList();
            }
            catch (Exception ex)
            {
            }
        }

        private void RefreshCaseList()
        {
            var collectionview = (ExtendedCollectionViewSource)FindResource("cvs");
            if (collectionview != null && collectionview.View != null)
            {
                using (collectionview.DeferRefresh()) { collectionview.View.Refresh(); }
            }
        }

        private void buttonLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CaseSourcesObject caseSourceObj;
                ListBox selectedListBox;

                if (m_IsACSSource)
                {
                    caseSourceObj = ACSSource_listBox.SelectedItem as CaseSourcesObject;
                    selectedListBox = ACSSource_listBox;
                }
                else
                {
                    caseSourceObj = AWSCommSource_listBox.SelectedItem as CaseSourcesObject;
                    selectedListBox = AWSCommSource_listBox;
                }

                if (caseSourceObj != null)
                {
                    m_UserName = textBoxUserName.Text;
                    m_Password = textBoxPassword.Password;

                    textBoxUserName.Text = string.Empty;
                    textBoxPassword.Password = string.Empty;

                    m_SysMgr.Login(caseSourceObj.Name, m_UserName, m_Password);
                    m_SysConfigMgr.GetDefaultConfig().AutoSelectPendingCasesEnabled = m_AutoSelectPendingCaseEnabled;

                    ACSSource_listBox.SelectedItem = ACSSource_listBox.SelectedItem;

                    if (m_AutoSelectPendingCaseEnabled)
                        SourceSelection.IsOpen = false;

                    if (m_SysConfigMgr.GetDefaultConfig().ForceAutoSelect)
                    {
                        AutoSelectPendingCaseCheckBox.IsChecked = true;
                        AutoSelectPendingCaseCheckBox_Click(AutoSelectPendingCaseCheckBox, new RoutedEventArgs());
                    }
                    else
                    {
                        AutoSelectPendingCase.Height = new GridLength(20);
                    }


                    Source_listBox_SelectionChanged(selectedListBox, new RoutedEventArgs());
                }
            }
            catch (Exception ex)
            {
                Login_Error_Message.Text = ex.Message;
            }

        }

        private void CaseListview_Filter(object sender, FilterEventArgs e)
        {
            bool AcceptRow = true;

            try
            {
                if (e.Item != null)
                {
                    DataRowView datarow = (DataRowView)e.Item;
                    DataRow row = (DataRow)datarow.Row;

                    foreach (SearchCaseListCriteriaItem item in m_CaseListSearch.DeleteSearchCriteriaCaseList)
                    {

                        if (((String)item.Item == "Area") && ((row[(String)item.Item].ToString().ToLower() == "final") ||
                                                                    (row[(String)item.Item] == null) ||
                                                                    (row[(String)item.Item].ToString() == String.Empty)))
                        {
                            AcceptRow = true;
                        }
                        else if (!item.MachesCriteria(row[(String)item.Item].ToString()))
                        {
                            AcceptRow = false;
                            break;
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
            e.Accepted = AcceptRow;
        }

        private void AutoSelectPendingCaseCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox box = sender as CheckBox;
            m_AutoSelectPendingCaseEnabled = (Boolean)box.IsChecked;

            //change workstation station configuration to reflect the state of auto select mode.
            m_SysConfigMgr.GetDefaultConfig().AutoSelectPendingCasesEnabled = m_AutoSelectPendingCaseEnabled;

            if ((Boolean)box.IsChecked)
            {
                Source_dataGrid.IsEnabled = (AWSSources.IsChecked == true) ? false : true;
            }
            else
            {
                Source_dataGrid.IsEnabled = true;
            }

            m_SysMgr.AutoSelectEnabled(m_AutoSelectPendingCaseEnabled);

            if (m_AutoSelectPendingCaseEnabled && !m_IsACSSource)
            {
                SourceSelection.IsOpen = false;
            }
        }

        private void textBoxPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                buttonLogin_Click(null, null);
            }
        }


        private void textBoxUserName_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Keyboard.Focus(textBoxUserName);
        }

        private void LanguageChanged(object sender, EventArgs e)
        {
            // need to refresh the combobox elements when the language changes by removing and resetting the data context.
            // These comboboxes hold strings that aren't bound to the language after they are added. This resets the contents
            // so that the new language is displayed.
            var dataContext = AddCaseListSearchComboBox.DataContext;
            AddCaseListSearchComboBox.DataContext = null;
            AddCaseListSearchComboBox.DataContext = dataContext;

            dataContext = DeleteCaseListSearchComboBox.DataContext;
            DeleteCaseListSearchComboBox.DataContext = null;
            DeleteCaseListSearchComboBox.DataContext = dataContext;
        }
    }
}
