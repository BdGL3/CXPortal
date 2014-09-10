using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using L3.Cargo.Communications.APCS.Client;
using L3.Cargo.Communications.APCS.Common;
using L3.Cargo.Communications.EventsLogger.Client;

namespace NetClientTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static bool _anomaly = false;

        private static void AnomalyShow(Exception exc)
        {
            _anomaly = true;
            MessageBox.Show(exc.ToString(), "Anomaly!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public ApcsAccess _apcsAccess {get;set;}

        public MainWindow()
        {
            InitializeComponent();

            EventLoggerAccess log = null;

            _apcsAccess = new ApcsAccess(log);
            _apcsAccess.Start();
            SignOfLifeSequenceTBK.DataContext = _apcsAccess;

            _speedMsgThread = new Thread(new ThreadStart(SpeedMsgAgent));
            _speedMsgThread.IsBackground = true;
            _speedMsgThread.Name = "Main Window Speed Message thread";
            _speedMsgEnd.Reset();
            _speedMsgThread.Start();
        }

        private void CommunicationsError()
        {
            MessageBox.Show("Error Communicating With Target");
        }

        private void GetScanModeBtn_Click(object sender, RoutedEventArgs e)
        {
            ScanEnergyMode? mode;
            
            bool success = _apcsAccess.GetScanEnergyMode(out mode, true);

            if (success && (mode != null))
            {
                ScanModeDualEnergyPNL.Background = Brushes.Transparent;
                ScanModeHighEnergyPNL.Background = Brushes.Transparent;
                ScanModeLowEnergyPNL.Background = Brushes.Transparent;

                if (mode.Value == ScanEnergyMode.Dual)
                {
                    ScanModeDualEnergyPNL.Background = Brushes.LightGreen;
                }
                else if (mode.Value == ScanEnergyMode.High)
                {
                    ScanModeHighEnergyPNL.Background = Brushes.LightGreen;
                }
                else if (mode.Value == ScanEnergyMode.Low)
                {
                    ScanModeLowEnergyPNL.Background = Brushes.LightGreen;
                }
                else
                {
                    MessageBox.Show("Scan Mode: " + (int)mode);
                }
            }
            else
                CommunicationsError(); ;
        }

        private void GetOperatingModeBtn_Click(object sender, RoutedEventArgs e)
        {
            OperatingMode? mode = null;
            short? minFreq = null;
            short? maxFreq = null;

            bool success = _apcsAccess.GetOperatingMode(out mode, out minFreq, out maxFreq, true);

            if (success)
            {
                if (mode != null)
                {
                    OperatingModePNL.Background = OperatingModeMobileAdaptivePNL.Background = OperatingModeMobileNonAdaptivePNL.Background = OperatingModePortalAdaptivePNL.Background = OperatingModePortalNonAdaptivePNL.Background = Brushes.Transparent;
                    if (mode.Value == OperatingMode.AdaptiveMobile)
                        OperatingModeMobileAdaptivePNL.Background = Brushes.LightGreen;
                    else if (mode.Value == OperatingMode.AdaptivePortal)
                        OperatingModePNL.Background = Brushes.LightGreen;
                    else if (mode.Value == OperatingMode.NonAdaptiveMobile)
                        OperatingModeMobileNonAdaptivePNL.Background = Brushes.LightGreen;
                    else if (mode.Value == OperatingMode.NonAdpativePortal)
                        OperatingModePortalNonAdaptivePNL.Background = Brushes.LightGreen;
                    else
                        MessageBox.Show("Get Operating mode: " + (int)mode);
                }
                else
                    MessageBox.Show("Get Operating Mode did not receive mode");
                
                if (minFreq != null)
                {
                    if ((mode.Value == OperatingMode.AdaptiveMobile) || (mode.Value == OperatingMode.AdaptivePortal))
                        OperatingModeMinimumTBX.Text = minFreq.Value.ToString();
                    else
                        OperatingModeMinimumTBX.Text = "";
                    OperatingModeMinimumTBX.Background = Brushes.LightGreen;
                }
                else
                {
                    MessageBox.Show("Get Operating Mode did not receive min freq");
                }

                if (maxFreq != null)
                {
                    if ((mode.Value == OperatingMode.AdaptiveMobile) || (mode.Value == OperatingMode.AdaptivePortal))
                        OperatingModeMaximumTBX.Text = maxFreq.Value.ToString();
                    else
                        OperatingModeMaximumTBX.Text = "";
                    OperatingModeMaximumTBX.Background = Brushes.LightGreen;
                }
                else
                {
                    MessageBox.Show("Get Operating Mode did not receive max freq");
                }
            }
            else
                CommunicationsError();
        }

        private void GetStaticPulseFreqBtn_Click(object sender, RoutedEventArgs e)
        {
            OperatingMode? mode = null;
            OperatingMode? selectedMode = null;
            int? freq = null;

            if ((bool)PulseFrequencyMobileNonAdaptiveRDO.IsChecked)
                selectedMode = OperatingMode.NonAdaptiveMobile;
            else if ((bool)PulseFrequencyPortalNonAdaptiveRDO.IsChecked)
                selectedMode = OperatingMode.NonAdpativePortal;
            else
                MessageBox.Show("Select Mode");

            if (selectedMode != null)
            {
                bool success = _apcsAccess.GetStaticPulseFrequency(selectedMode, out mode, out freq, true);

                if (success)
                {
                    if (mode != null)
                    {
                        PulseFrequencyMobileNonAdaptivePNL.Background = Brushes.Transparent;
                        PulseFrequencyPortalNonAdaptivePNL.Background = Brushes.Transparent;

                        if (mode.Value == OperatingMode.NonAdaptiveMobile)
                        {
                            PulseFrequencyMobileNonAdaptivePNL.Background = Brushes.LightGreen;
                        }
                        else if (mode.Value == OperatingMode.NonAdpativePortal)
                        {
                            PulseFrequencyPortalNonAdaptivePNL.Background = Brushes.LightGreen;
                        }
                        else
                        {
                            MessageBox.Show("Get Static Freq Operating mode: " + (int)mode);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Get Static Freq did not receive mode");
                    }

                    if (freq != null)
                    {
                        PulseFrequencyFrequencyTBX.Text = freq.Value.ToString();
                        PulseFrequencyFrequencyTBX.Background = Brushes.LightGreen;
                    }
                    else
                    {
                        MessageBox.Show("Get Static Freq did not receive freq");
                    }
                }
                else
                    CommunicationsError();
            }
        }

        private void GetAdptvModeTrigRatioBtn_Click(object sender, RoutedEventArgs e)
        {
            OperatingMode? mode = null;
            OperatingMode? selectedMode = null;
            float? ratio = null;

            if ((bool)TriggerRatioMobileAdaptiveRDO.IsChecked)
                selectedMode = OperatingMode.AdaptiveMobile;
            else if ((bool)TriggerRatioPortalAdaptiveRDO.IsChecked)
                selectedMode = OperatingMode.AdaptivePortal;
            else
                MessageBox.Show("Select Mode");

            if (selectedMode != null)
            {
                bool success = _apcsAccess.GetAdaptiveModeTriggerRatio(selectedMode, out mode, out ratio, true);

                if (success)
                {
                    if (mode != null)
                    {
                        TriggerRatioMobileAdaptivePNL.Background = Brushes.Transparent;
                        TriggerRatioPortalAdaptivePNL.Background = Brushes.Transparent;

                        if (mode.Value == OperatingMode.AdaptiveMobile)
                        {
                            TriggerRatioMobileAdaptivePNL.Background = Brushes.LightGreen;
                        }
                        else if (mode.Value == OperatingMode.AdaptivePortal)
                        {
                            TriggerRatioPortalAdaptivePNL.Background = Brushes.LightGreen;
                        }
                        else
                        {
                            MessageBox.Show("Get Adpative Mode To Trigger Ratio: " + (int)mode);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Get Adpative Mode To Trigger Ratio did not receive mode");
                    }

                    if (ratio != null)
                    {
                        TriggerRatioTBX.Text = ratio.Value.ToString();
                        TriggerRatioTBX.Background = Brushes.LightGreen;
                    }
                    else
                    {
                        MessageBox.Show("Get Adpative Mode To Trigger Ratio did not receive ratio");
                    }
                }
                else
                    CommunicationsError();
            }
        }

        private void GetAdptvSpeedFdbkConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            AdaptiveSpeedFeedbackConfig? config = null;
            float? freq = null;

            bool success = _apcsAccess.GetAdaptiveSpeedFeedbackConfiguration(out config, out freq, true);

            if (success)
            {
                if (config != null)
                {
                    SpeedFeedbackDisabledPNL.Background = Brushes.Transparent;
                    SpeedFeedbackEnabledPNL.Background = Brushes.Transparent;

                    if (config.Value == AdaptiveSpeedFeedbackConfig.Disabled)
                    {
                        SpeedFeedbackDisabledPNL.Background = Brushes.LightGreen;
                        OperatingModePortalAdaptivePNL.Background = Brushes.Transparent;
                        OperatingModeMobileAdaptivePNL.Background = Brushes.Transparent;
                    }
                    else if (config.Value == AdaptiveSpeedFeedbackConfig.EnabledWithFreq)
                    {
                        SpeedFeedbackEnabledPNL.Background = Brushes.LightGreen;
                    }
                    else
                    {
                        MessageBox.Show("Get Adpative speed Feedback Config: " + (int)config);
                    }
                }
                else
                {
                    MessageBox.Show("Get Adpative speed Feedback Config did not receive config");
                }

                if (freq != null)
                {
                    SpeedFeedbackFrequencyTBX.Text = freq.Value.ToString();
                    SpeedFeedbackFrequencyTBX.Background = Brushes.LightGreen;
                }
                else
                {
                    MessageBox.Show("Get Adpative speed Feedback Config did not receive freq");
                }
            }
            else
                CommunicationsError();
        }

        private void PulseWidthConfiguredGetBTN_Click(object sender, RoutedEventArgs e)
        {
            PulseWidth? width = null;
            float? time = null;

            if ((bool)PulseWidthConfigured1RDO.IsChecked)
                width = PulseWidth.PulseWidth1;
            else if ((bool)PulseWidthConfigured2RDO.IsChecked)
                width = PulseWidth.PulseWidth2;
            else if ((bool)PulseWidthConfigured3RDO.IsChecked)
                width = PulseWidth.PulseWidth3;
            else if ((bool)PulseWidthConfigured4RDO.IsChecked)
                width = PulseWidth.PulseWidth4;

            if (width != null)
            {
                bool success = _apcsAccess.GetConfigPulseWidth(width.Value, out time, true);

                if (success)
                {
                    if (time != null)
                    {
                        PulseWidthConfiguredTimeTBX.Text = time.ToString();
                        PulseWidthConfiguredTimeTBX.Background = Brushes.LightGreen;
                    }
                    else
                    {
                        MessageBox.Show("Get Config Pulse Width did not receive time");
                    }
                }
                else
                    CommunicationsError();
            }
            else
                MessageBox.Show("Select Pulse Width");
                        
        }

        private void PulseWidthGetBTN_Click(object sender, RoutedEventArgs e)
        {
            PulseWidth? width = null;

            bool success = _apcsAccess.GetCurrentPulseWidth(out width, true);

            if (success)
            {
                if (width != null)
                {
                    PulseWidth1PNL.Background = Brushes.Transparent;
                    PulseWidth2PNL.Background = Brushes.Transparent;
                    PulseWidth3PNL.Background = Brushes.Transparent;
                    PulseWidth4PNL.Background = Brushes.Transparent;

                    if (width.Value == PulseWidth.PulseWidth1)
                    {
                        PulseWidth1PNL.Background = Brushes.LightGreen;
                    }
                    else if (width.Value == PulseWidth.PulseWidth2)
                    {
                        PulseWidth2PNL.Background = Brushes.LightGreen;
                    }
                    else if (width.Value == PulseWidth.PulseWidth3)
                    {
                        PulseWidth3PNL.Background = Brushes.LightGreen;
                    }
                    else if (width.Value == PulseWidth.PulseWidth4)
                    {
                        PulseWidth4PNL.Background = Brushes.LightGreen;
                    }
                    else
                    {
                        MessageBox.Show("Get Current Pulse Width: " + (int)width);
                    }
                }
                else
                    MessageBox.Show("Get Current Pulse Width did not receive width");
            }
            else
                CommunicationsError();
        }

        private void GetPWMOutputStatusBtn_Click(object sender, RoutedEventArgs e)
        {
            PWMOutputConfig? config = null;

            PWMOutputModeDisabledPNL.Background = Brushes.Transparent;
            PWMOutputModeEnabledPNL.Background = Brushes.Transparent;

            bool success = _apcsAccess.GetPWMOutput(out config, true);

            if (success)
            {
                if (config != null)
                {
                    if (config.Value == PWMOutputConfig.OutputDisabled)
                        PWMOutputModeDisabledPNL.Background = Brushes.LightGreen;
                    else if (config.Value == PWMOutputConfig.OutputEnabled)
                        PWMOutputModeEnabledPNL.Background = Brushes.LightGreen;
                }
                else
                    MessageBox.Show("Get Current PWM Status did not receive status");
            }
            else
                CommunicationsError();
        }

        private void ScanModeHighEnergyRDO_Click(object sender, RoutedEventArgs e)
        {
            bool success = _apcsAccess.SetScanEnergyMode(ScanEnergyMode.High, true);

            ScanModeDualEnergyPNL.Background = Brushes.Transparent;
            ScanModeHighEnergyPNL.Background = Brushes.Transparent;
            ScanModeLowEnergyPNL.Background = Brushes.Transparent;

            if (success)
                ScanModeHighEnergyPNL.Background = Brushes.LightGreen;
            else
                CommunicationsError();
        }

        private void ScanModeLowEnergyRDO_Click(object sender, RoutedEventArgs e)
        {
            bool success = _apcsAccess.SetScanEnergyMode(ScanEnergyMode.Low, true);

            ScanModeDualEnergyPNL.Background = Brushes.Transparent;
            ScanModeHighEnergyPNL.Background = Brushes.Transparent;
            ScanModeLowEnergyPNL.Background = Brushes.Transparent;

            if (success)
                ScanModeLowEnergyPNL.Background = Brushes.LightGreen;
            else
                CommunicationsError();
        }

        private void ScanModeDualEnergyRDO_Click(object sender, RoutedEventArgs e)
        {
            bool success = _apcsAccess.SetScanEnergyMode(ScanEnergyMode.Dual, true);

            ScanModeDualEnergyPNL.Background = Brushes.Transparent;
            ScanModeHighEnergyPNL.Background = Brushes.Transparent;
            ScanModeLowEnergyPNL.Background = Brushes.Transparent;

            if (success)
                ScanModeDualEnergyPNL.Background = Brushes.LightGreen;
            else
                CommunicationsError();
        }

        private void SetPulseWidth1Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool success = _apcsAccess.SetCurrentPulseWidth(PulseWidth.PulseWidth1, true);
                PulseWidth1PNL.Background = Brushes.Transparent;
                PulseWidth2PNL.Background = Brushes.Transparent;
                PulseWidth3PNL.Background = Brushes.Transparent;
                PulseWidth4PNL.Background = Brushes.Transparent;
                if (success)
                    PulseWidth1PNL.Background = Brushes.LightGreen;
                else
                    CommunicationsError();
            }
            catch (Exception ex) { AnomalyShow(ex); }
        }

        private void SetPulseWidth2Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool success = _apcsAccess.SetCurrentPulseWidth(PulseWidth.PulseWidth2, true);
                PulseWidth1PNL.Background = Brushes.Transparent;
                PulseWidth2PNL.Background = Brushes.Transparent;
                PulseWidth3PNL.Background = Brushes.Transparent;
                PulseWidth4PNL.Background = Brushes.Transparent;
                if (success)
                    PulseWidth2PNL.Background = Brushes.LightGreen;
                else
                    CommunicationsError();
            }
            catch (Exception ex) { AnomalyShow(ex); }
        }

        private void SetPulseWidth3Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool success = _apcsAccess.SetCurrentPulseWidth(PulseWidth.PulseWidth3, true);
                PulseWidth1PNL.Background = Brushes.Transparent;
                PulseWidth2PNL.Background = Brushes.Transparent;
                PulseWidth3PNL.Background = Brushes.Transparent;
                PulseWidth4PNL.Background = Brushes.Transparent;
                if (success)
                    PulseWidth3PNL.Background = Brushes.LightGreen;
                else
                    CommunicationsError();
            }
            catch (Exception ex) { AnomalyShow(ex); }
        }

        private void SetPulseWidth4Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool success = _apcsAccess.SetCurrentPulseWidth(PulseWidth.PulseWidth4, true);
                PulseWidth1PNL.Background = Brushes.Transparent;
                PulseWidth2PNL.Background = Brushes.Transparent;
                PulseWidth3PNL.Background = Brushes.Transparent;
                PulseWidth4PNL.Background = Brushes.Transparent;
                if (success)
                    PulseWidth4PNL.Background = Brushes.LightGreen;
                else
                    CommunicationsError();
            }
            catch (Exception ex) { AnomalyShow(ex); }
        }

        private void SetOperatingModeBtn_Click(object sender, RoutedEventArgs e)
        {
            OperatingMode? mode = null;
            StackPanel panel = null;

            ushort minfreq = 0;
            ushort maxFreq = 0;
            
            if ((bool)OperatingModeMobileAdaptiveRDO.IsChecked)
            {
                try
                {
                    minfreq = Convert.ToUInt16(OperatingModeMinimumTBX.Text);
                    maxFreq = Convert.ToUInt16(OperatingModeMaximumTBX.Text);
                    panel = OperatingModeMobileAdaptivePNL;

                    if (minfreq >= maxFreq)
                        MessageBox.Show("Invalid Frequency Range");
                    else
                        mode = OperatingMode.AdaptiveMobile;
                }
                catch
                {
                    MessageBox.Show("Set Adaptive Frequency Range");
                }
            }
            else if ((bool)OperatingModePortalAdaptiveRDO.IsChecked)
            {
                try
                {
                    minfreq = Convert.ToUInt16(OperatingModeMinimumTBX.Text);
                    maxFreq = Convert.ToUInt16(OperatingModeMaximumTBX.Text);
                    panel = OperatingModePNL;

                    if (minfreq >= maxFreq)
                        MessageBox.Show("Invalid Frequency Range");
                    else
                        mode = OperatingMode.AdaptivePortal;
                }
                catch
                {
                    MessageBox.Show("Set Adaptive Frequency Range");
                }
            }
            else if ((bool)OperatingModeMobileNonAdaptiveRDO.IsChecked)
            {
                mode = OperatingMode.NonAdaptiveMobile;
                panel = OperatingModeMobileNonAdaptivePNL;
            }
            else if ((bool)OperatingModePortalNonAdaptiveRDO.IsChecked)
            {
                mode = OperatingMode.NonAdpativePortal;
                panel = OperatingModePortalNonAdaptivePNL;
            }
            else
            {
                MessageBox.Show("Select Operating Mode");
            }

            if (mode != null)
            {
                bool success = _apcsAccess.SetOperatingMode(mode.Value, minfreq, maxFreq, true);

                OperatingModeMobileAdaptivePNL.Background = Brushes.Transparent;
                OperatingModePNL.Background = Brushes.Transparent;
                OperatingModeMobileNonAdaptivePNL.Background = Brushes.Transparent;
                OperatingModePortalNonAdaptivePNL.Background = Brushes.Transparent;

                if (success)
                {
                    panel.Background = Brushes.LightGreen;
                    OperatingModePortalAdaptivePNL.Background = Brushes.Transparent;
                    OperatingModeMobileAdaptivePNL.Background = Brushes.Transparent;
                }
                else
                    CommunicationsError();
            }
        }

        private void SetStaticPulseFreqBtn_Click(object sender, RoutedEventArgs e)
        {
            OperatingMode? mode = null;
            StackPanel panel = null;
            uint freq;

            if((bool) PulseFrequencyMobileNonAdaptiveRDO.IsChecked)
            {
                mode = OperatingMode.NonAdaptiveMobile;
                panel = PulseFrequencyMobileNonAdaptivePNL;
            }
            else if((bool) PulseFrequencyPortalNonAdaptiveRDO.IsChecked)
            {
                mode = OperatingMode.NonAdpativePortal;
                panel = PulseFrequencyPortalNonAdaptivePNL;
            }
            else
            {
                MessageBox.Show("Select Mode");
            }

            if (mode != null)
            {
                try
                {
                    freq = Convert.ToUInt32(PulseFrequencyFrequencyTBX.Text);
                }
                catch
                {
                    MessageBox.Show("Set Static Pulse Frequency");
                    return;
                }

                try
                {
                    bool success = _apcsAccess.SetStaticPulseFrequency(mode.Value, freq);
                    PulseFrequencyPortalNonAdaptivePNL.Background = Brushes.Transparent;
                    PulseFrequencyMobileNonAdaptivePNL.Background = Brushes.Transparent;
                    if (success)
                        panel.Background = Brushes.LightGreen;
                    else
                        CommunicationsError();
                }
                catch (Exception ex) { AnomalyShow(ex); }
            }
        }

        private void SetAdptvModeTrigRatioBtn_Click(object sender, RoutedEventArgs e)
        {
            OperatingMode? mode = null;
            StackPanel panel = null;
            float ratio;

            if ((bool)TriggerRatioMobileAdaptiveRDO.IsChecked)
            {
                mode = OperatingMode.AdaptiveMobile;
                panel = TriggerRatioMobileAdaptivePNL;
            }
            else if ((bool)TriggerRatioPortalAdaptiveRDO.IsChecked)
            {
                mode = OperatingMode.AdaptivePortal;
                panel = TriggerRatioPortalAdaptivePNL;
            }
            else
            {
                MessageBox.Show("Select Mode");
            }

            if (mode != null)
            {
                try
                {
                    ratio = Convert.ToSingle(TriggerRatioTBX.Text);
                }
                catch
                {
                    MessageBox.Show("Set Mode To Trigger Ratio");
                    return;
                }

                bool success = _apcsAccess.SetAdaptiveModeTriggerRatio(mode.Value, ratio, true);

                TriggerRatioMobileAdaptivePNL.Background = Brushes.Transparent;
                TriggerRatioPortalAdaptivePNL.Background = Brushes.Transparent;

                if (success)
                    panel.Background = Brushes.LightGreen;
                else
                    CommunicationsError();
            }
        }

        private void SetAdptvSpeedFdbkConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            AdaptiveSpeedFeedbackConfig? mode = null;
            StackPanel panel = null;

            float freq = 0.0f;

            if ((bool)SpeedFeedbackDisabledRDO.IsChecked)
            {
                mode = AdaptiveSpeedFeedbackConfig.Disabled;
                panel = SpeedFeedbackDisabledPNL;
            }
            else if ((bool)SpeedFeedbackEnabledRDO.IsChecked)
            {
                try
                {
                    freq = Convert.ToSingle(SpeedFeedbackFrequencyTBX.Text);
                    mode = AdaptiveSpeedFeedbackConfig.EnabledWithFreq;
                    panel = SpeedFeedbackEnabledPNL;
                }
                catch
                {
                    MessageBox.Show("Set Speed Config Frequency");
                }
            }
            else
            {
                MessageBox.Show("Select Mode");
            }

            if (mode != null)
            {
                bool success = _apcsAccess.SetAdaptiveSpeedFeedbackConfiguration(mode.Value, freq, true);

                SpeedFeedbackDisabledPNL.Background = Brushes.Transparent;
                SpeedFeedbackEnabledPNL.Background = Brushes.Transparent;
                OperatingModePortalAdaptivePNL.Background = Brushes.Transparent;
                OperatingModeMobileAdaptivePNL.Background = Brushes.Transparent;

                if (success)
                    panel.Background = Brushes.LightGreen;
                else
                    CommunicationsError();
            }
        }

        private void PulseWidthConfiguredSetBTN_Click(object sender, RoutedEventArgs e)
        {
            PulseWidth? width = null;
            float time;

            if ((bool)PulseWidthConfigured1RDO.IsChecked)
                width = PulseWidth.PulseWidth1;
            else if ((bool)PulseWidthConfigured2RDO.IsChecked)
                width = PulseWidth.PulseWidth2;
            else if ((bool)PulseWidthConfigured3RDO.IsChecked)
                width = PulseWidth.PulseWidth3;
            else if ((bool)PulseWidthConfigured4RDO.IsChecked)
                width = PulseWidth.PulseWidth4;
            else
                MessageBox.Show("Select Pulse Width");

            if (width != null)
            {
                try
                {
                    time = Convert.ToSingle(PulseWidthConfiguredTimeTBX.Text);
                }
                catch
                {
                    MessageBox.Show("Set Pulse Width");
                    return;
                }

                bool success = _apcsAccess.SetConfigPulseWidth(width.Value, time, true);

                if (success)
                    PulseWidthConfiguredTimeTBX.Background = Brushes.LightGreen;
                else
                    CommunicationsError();
            }
        }

        private void DisablePWMOutput_Click(object sender, RoutedEventArgs e)
        {
            PWMOutputModeDisabledPNL.Background = Brushes.Transparent;
            PWMOutputModeEnabledPNL.Background = Brushes.Transparent;

            bool success = _apcsAccess.SetPWMOutput(PWMOutputConfig.OutputDisabled, true);

            if (success)
                PWMOutputModeDisabledPNL.Background = Brushes.LightGreen;
            else
                CommunicationsError();
        }

        private void EnablePWMOutput_Click(object sender, RoutedEventArgs e)
        {
            PWMOutputModeDisabledPNL.Background = Brushes.Transparent;
            PWMOutputModeEnabledPNL.Background = Brushes.Transparent;

            bool success = _apcsAccess.SetPWMOutput(PWMOutputConfig.OutputEnabled, true);

            if (success)
                PWMOutputModeEnabledPNL.Background = Brushes.LightGreen;
            else
                CommunicationsError();
        }

        private void ResetBoardBTN_Click(object sender, RoutedEventArgs e)
        {
            try { _apcsAccess.ResetBoard(); }
            catch (Exception ex) { AnomalyShow(ex); }
        }

        private delegate void PanelUpdate();

        private void UpdateSpeedModePanel()
        {
            OperatingMode mode = _apcsAccess.AdaptiveSpeedMode;
            OperatingModePortalAdaptivePNL.Background = Brushes.Transparent;
            OperatingModeMobileAdaptivePNL.Background = Brushes.Transparent;
            if(mode == OperatingMode.AdaptiveMobile)
                OperatingModeMobileAdaptivePNL.Background = Brushes.LightGreen;
            else if(mode == OperatingMode.AdaptivePortal)
                OperatingModePortalAdaptivePNL.Background = Brushes.LightGreen;
            if ((mode == OperatingMode.AdaptiveMobile) || (mode == OperatingMode.AdaptivePortal))
                SpeedFeedbackFrequencyTBX.Text = _apcsAccess.AdaptiveSpeed.ToString();
        }

        private void SpeedMsgAgent()
        {
            PanelUpdate update = new PanelUpdate(UpdateSpeedModePanel);

            while (!_speedMsgEnd.WaitOne(0))
            {
                _apcsAccess.SpeedMessageEvent.WaitOne(Timeout.Infinite);
                if (!_speedMsgEnd.WaitOne(0))
                    OperatingModePortalAdaptivePNL.Dispatcher.BeginInvoke(update, null);
            }
        }
        private ManualResetEvent _speedMsgEnd = new ManualResetEvent(false);
        private Thread _speedMsgThread;
    }
}
