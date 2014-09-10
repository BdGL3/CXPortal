using System.Windows;
using System.Windows.Controls;
using L3.Cargo.Detectors.DataAccessCore;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Detectors.BusinessCore;
using L3.Cargo.Communications.Detectors.Common;
using System;

namespace DetectorsApp
{
    /// <summary>
    /// Interaction logic for DetectorSimulator.xaml
    /// </summary>
    public partial class DetectorSimulator : UserControl
    {
        #region private members

        private DataAccessDetectors _dataAccess;
        private EventLoggerAccess _logger;
        private BusinessManager _businessManager;

        #endregion

        public DetectorSimulator(DataAccessDetectors dataAccess, EventLoggerAccess logger, BusinessManager businessManager)
        {
            try
            {
                InitializeComponent();

                _dataAccess = dataAccess;
                _logger = logger;
                _businessManager = businessManager;
                _businessManager.RecordData = false;
            }
            catch
            {
                throw;
            }
        }            

        private void Calibration_Click(object sender, RoutedEventArgs e)
        {
            _dataAccess.ScanType = DataAccessDetectors.ScanTypeEnum.CalibrationScan;
            _dataAccess.ScanDriveStatus = DataAccessDetectors.ScanDriveStatusEnum.Running;
            _businessManager.dataAccess_TagUpdate("SCAN_TYPE", 0);
        }

        private void Regular_Click(object sender, RoutedEventArgs e)
        {
            _dataAccess.ScanType = DataAccessDetectors.ScanTypeEnum.RegularScan;
            _businessManager.dataAccess_TagUpdate("SCAN_TYPE", 1);
        }        
             
        private void StopData_Click(object sender, RoutedEventArgs e)
        {
            _dataAccess.Simulator.StopData();
        }

        private void DualEnergyBtn_Click(object sender, RoutedEventArgs e)
        {
            _dataAccess.Simulator.SetEnergyType(EnumEnergyType.DualEnergy);
        }

        private void HighEnergyBtn_Click(object sender, RoutedEventArgs e)
        {
            _dataAccess.Simulator.SetEnergyType(EnumEnergyType.HighEnergy);
        }

        private void LowEnergyBtn_Click(object sender, RoutedEventArgs e)
        {
            _dataAccess.Simulator.SetEnergyType(EnumEnergyType.LowEnergy);
        }

        private void PulseWidth0Btn_Click(object sender, RoutedEventArgs e)
        {
            _dataAccess.Simulator.SetPulseWidth(0);
        }

        private void PulseWidth1Btn_Click(object sender, RoutedEventArgs e)
        {
            _dataAccess.Simulator.SetPulseWidth(1);
        }

        private void PulseWidth2Btn_Click(object sender, RoutedEventArgs e)
        {
            _dataAccess.Simulator.SetPulseWidth(2);
        }

        private void PulseWidth3Btn_Click(object sender, RoutedEventArgs e)
        {
            _dataAccess.Simulator.SetPulseWidth(3);
        }

        private void RegularStartData_Click(object sender, RoutedEventArgs e)
        {
            _dataAccess.ScanDriveStatus = DataAccessDetectors.ScanDriveStatusEnum.Running;
            _dataAccess.LinacXrayEnable = true;
            _dataAccess.Simulator.StartRawData();
        }

        private void RegularStopData_Click(object sender, RoutedEventArgs e)
        {
            _dataAccess.Simulator.StopData();
        }

        private void RegularStartRecord_Click(object sender, RoutedEventArgs e)
        {
            _businessManager.StartRecordingData = true;
        }

        private void RegularStopRecord_Click(object sender, RoutedEventArgs e)
        {
            _businessManager.StopRecordingData = true;
        }

        private void RecordDataCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)RecordDataCheckBox.IsChecked)
                _businessManager.RecordData = true;
            else
                _businessManager.RecordData = false;
        }        
    }
}
