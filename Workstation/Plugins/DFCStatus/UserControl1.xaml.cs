using System;
using System.Windows;
using System.Windows.Controls;
using L3.Cargo.Common;
using System.Data;
using System.Windows.Media;
using System.Configuration;
using L3.Cargo.Workstation.SystemConfigurationCore;
using System.Windows.Threading;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Data;

namespace L3.Cargo.Workstation.Plugins.DFCStatus
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl, IDisposable
    {
        private CaseObject m_CaseObj;

        private Boolean m_LoadedOnce;

        private string m_PreviousID;

        private _Schiphol_ACXS_ScriptDataSet _Schiphol_ACXS_ScriptDataSet;

        private _Schiphol_ACXS_ScriptDataSetTableAdapters.ContainerTableAdapter _Schiphol_ACXS_ScriptDataSetContainerTableAdapter;

        private SysConfiguration m_sysConfig;

        private DispatcherTimer updateDataGrid1Timer;

        public UserControl1(CaseObject caseObj, SysConfiguration sysConfig)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            m_CaseObj = caseObj;
            m_sysConfig = sysConfig;
            InfoDisplayArea.DataContext = m_CaseObj;
            m_LoadedOnce = false;
        }

        private void updateDataGrid1Timer_Tick(object sender, EventArgs e)
        {            
            _Schiphol_ACXS_ScriptDataSetContainerTableAdapter.Fill(_Schiphol_ACXS_ScriptDataSet.Container);

            System.Windows.Data.CollectionViewSource containerViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("containerViewSource")));
            using (containerViewSource.DeferRefresh()) { containerViewSource.View.Refresh(); }

            DataRow[] datarows = _Schiphol_ACXS_ScriptDataSet.Container.Select(string.Format("ULDNumber = '{0}'", ULDNumberInput.Text.Replace(" ", "")));

            if (m_CaseObj.scanInfo != null && m_CaseObj.scanInfo.container != null && m_CaseObj.scanInfo.container.Id != null)
            {
                m_CaseObj.scanInfo.container.Id = ULDNumberInput.Text;
            }

            if (datarows.Length > 0)
            {
                dataGrid1.SelectedValuePath = "ULDNumber";
                dataGrid1.SelectedValue = datarows[0]["ULDNumber"];
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!m_LoadedOnce)
            {
                try
                {
                    _Schiphol_ACXS_ScriptDataSet = ((_Schiphol_ACXS_ScriptDataSet)(this.FindResource("_Schiphol_ACXS_ScriptDataSet")));
                    // Load data into the table Container. You can modify this code as needed.
                    _Schiphol_ACXS_ScriptDataSetContainerTableAdapter = new _Schiphol_ACXS_ScriptDataSetTableAdapters.ContainerTableAdapter();

                    _Schiphol_ACXS_ScriptDataSetContainerTableAdapter.Connection.ConnectionString = m_sysConfig.ContainerDBConnectionString;
                    _Schiphol_ACXS_ScriptDataSetContainerTableAdapter.ClearBeforeFill = true;
                    _Schiphol_ACXS_ScriptDataSetContainerTableAdapter.Fill(_Schiphol_ACXS_ScriptDataSet.Container);
                    System.Windows.Data.CollectionViewSource containerViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("containerViewSource")));
                    containerViewSource.View.MoveCurrentToFirst();

                    updateDataGrid1Timer = new DispatcherTimer();
                    updateDataGrid1Timer.Interval = new TimeSpan(0, 0, 0, 0, m_sysConfig.ContainerRefreshPeriodmsecs);
                    updateDataGrid1Timer.Tick += new EventHandler(updateDataGrid1Timer_Tick);
                    updateDataGrid1Timer.Start();
                    m_LoadedOnce = true;
                }
                catch { }
            }
        }

        public void Dispose ()
        {
            if (updateDataGrid1Timer != null)
            {
                updateDataGrid1Timer.Stop();
                updateDataGrid1Timer = null;
            }
        }

        private void dataGrid1_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            IList<DataGridCellInfo> selectedcells = e.AddedCells;

            foreach (DataGridCellInfo di in selectedcells)
            {
                DataRowView dvr = (DataRowView)di.Item;

                _Schiphol_ACXS_ScriptDataSet.ContainerRow cr = (_Schiphol_ACXS_ScriptDataSet.ContainerRow)dvr.Row;

                if (m_CaseObj.scanInfo != null && m_CaseObj.scanInfo.container != null && m_CaseObj.scanInfo.container.Id != null)
                {
                    string selectedRow = cr.ULDNumber.Replace(" ","").ToUpper();
                    string contId = m_CaseObj.scanInfo.container.Id.Replace(" ", "").ToUpper();

                    if (selectedRow == contId)
                    {
                        if (m_PreviousID != contId)
                        {
                            MessageBox.Show("This is a selected container.", "DFC Match Found");
                        }
                    }
                    else
                    {
                        dataGrid1.SelectedIndex = -1;
                    }

                    m_PreviousID = contId;
                }
                else
                {
                    dataGrid1.SelectedIndex = -1;
                }

                break;
            }
        }
    }
}
