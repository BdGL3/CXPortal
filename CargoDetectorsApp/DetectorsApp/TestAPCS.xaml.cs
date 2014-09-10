using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Threading;

using L3.Cargo.Communications.APCS;
using L3.Cargo.Communications.APCS.Client;
using L3.Cargo.Communications.APCS.Common;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Detectors.Client;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Detectors.DataAccessCore;

namespace DetectorsApp
{
    public partial class TestAPCS : UserControl
    {
        private static bool _anomaly = false;
        private static void AnomalyShow(Exception exc)
        {
            _anomaly = true;
            if (ApcsAccess.Logger != null)
                ApcsAccess.Logger.LogError(Utilities.TextTidy(exc.ToString()));
            MessageBox.Show(Utilities.TextTidy(exc.ToString()), "Anomaly!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        [DefaultValue(null)]
        private DetectorsDataAccess DetectorsAccess { get; set; }

        private void OperatingModeGetBTN_Click(object sender, RoutedEventArgs eventArguments)
        {
            try
            {
                OperatingMode? mode;
                short? minFreq;
                short? maxFreq;

                bool bSuccess = DetectorsAccess.Apcs.GetOperatingMode(out mode, out minFreq, out maxFreq, true);
                if (!bSuccess)
                    throw new Exception("GetOperatingMode failed");
                OperatingModeMobileAdaptivePNL.Background = Brushes.Transparent;
                OperatingModeMobileAdaptiveRDO.IsChecked = false;
                OperatingModePNL.Background = Brushes.Transparent;
                OperatingModePortalAdaptiveRDO.IsChecked = false;
                OperatingModeMobileNonAdaptivePNL.Background = Brushes.Transparent;
                OperatingModeMobileNonAdaptiveRDO.IsChecked = false;
                OperatingModePortalNonAdaptivePNL.Background = Brushes.Transparent;
                OperatingModePortalNonAdaptiveRDO.IsChecked = false;
                OperatingModeMinimumTBX.Background = Brushes.Transparent;
                OperatingModeMaximumTBX.Background = Brushes.Transparent;

                if (mode == OperatingMode.AdaptiveMobile)
                {
                    OperatingModeMobileAdaptivePNL.Background = Brushes.LightGreen;
                    OperatingModeMobileAdaptiveRDO.IsChecked = true;
                }
                else if (mode == OperatingMode.AdaptivePortal)
                {
                    OperatingModePNL.Background = Brushes.LightGreen;
                    OperatingModePortalAdaptiveRDO.IsChecked = true;
                }
                else if (mode == OperatingMode.NonAdaptiveMobile)
                {
                    OperatingModeMobileNonAdaptivePNL.Background = Brushes.LightGreen;
                    OperatingModeMobileNonAdaptiveRDO.IsChecked = true;
                }
                else if (mode == OperatingMode.NonAdpativePortal)
                {
                    OperatingModePortalNonAdaptivePNL.Background = Brushes.LightGreen;
                    OperatingModePortalNonAdaptiveRDO.IsChecked = true;
                }
                else
                    throw new Exception("GetOperatingMode returned unknown mode: " + ((int)mode).ToString());

                OperatingModeMinimumTBX.Text = minFreq.ToString();
                OperatingModeMinimumTBX.Background = Brushes.LightGreen;

                OperatingModeMaximumTBX.Text = maxFreq.ToString();
                OperatingModeMaximumTBX.Background = Brushes.LightGreen;
            }
            catch (Exception ex) { AnomalyShow(ex); }
        }

        private void OperatingModeSetBTN_Click(object sender, RoutedEventArgs eventArguments)
        {
            OperatingMode? mode = null;
            StackPanel panel = null;

            try
            {
                ushort minfreq = Convert.ToUInt16(OperatingModeMinimumTBX.Text);
                ushort maxFreq = Convert.ToUInt16(OperatingModeMaximumTBX.Text);

                if ((bool)OperatingModeMobileAdaptiveRDO.IsChecked)
                {
                    mode = OperatingMode.AdaptiveMobile;
                    panel = OperatingModeMobileAdaptivePNL;
                }
                else if ((bool)OperatingModePortalAdaptiveRDO.IsChecked)
                {
                    mode = OperatingMode.AdaptivePortal;
                    panel = OperatingModePNL;
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
                    MessageBox.Show("Select Operating Mode", "Oops!", MessageBoxButton.OK, MessageBoxImage.Hand);

                if (mode != null)
                {
                    if (!DetectorsAccess.Apcs.SetOperatingMode(mode.Value, minfreq, maxFreq, true))
                        throw new Exception("SetOperatingMode(" + mode.ToString() + ", " + minfreq.ToString() + ", " + maxFreq.ToString() + ") failed");
                    OperatingModeGetBTN_Click(sender, eventArguments);
                }
            }
            catch (Exception ex) { AnomalyShow(ex); }
        }

        private void TriggerRatioGetBTN_Click(object sender, RoutedEventArgs eventArguments)
        {
            OperatingMode? mode = null;
            OperatingMode? selectedMode = null;
            float? ratio = null;

            if ((bool)TriggerRatioMobileAdaptiveRDO.IsChecked)
                selectedMode = OperatingMode.AdaptiveMobile;
            else if ((bool)TriggerRatioPortalAdaptiveRDO.IsChecked)
                selectedMode = OperatingMode.AdaptivePortal;
            else
                MessageBox.Show("Select Operating Mode", "Oops!", MessageBoxButton.OK, MessageBoxImage.Hand);

            if (selectedMode != null)
            {
                bool ok = DetectorsAccess.Apcs.GetAdaptiveModeTriggerRatio(selectedMode, out mode, out ratio, true);

                if (ok)
                {
                    if (mode != null)
                    {
                        TriggerRatioMobileAdaptivePNL.Background = TriggerRatioPortalAdaptivePNL.Background = Brushes.Transparent;
                        if (mode.Value == OperatingMode.AdaptiveMobile)
                            TriggerRatioMobileAdaptivePNL.Background = Brushes.LightGreen;
                        else if (mode.Value == OperatingMode.AdaptivePortal)
                            TriggerRatioPortalAdaptivePNL.Background = Brushes.LightGreen;
                        else
                            MessageBox.Show("Get Adpative Mode To Trigger Ratio: " + (int)mode);
                    }
                    else
                        MessageBox.Show("Get Adpative Mode To Trigger Ratio did not receive mode");

                    if (ratio != null)
                    {
                        TriggerRatioTBX.Text = ratio.Value.ToString();
                        TriggerRatioTBX.Background = Brushes.LightGreen;
                    }
                    else
                        MessageBox.Show("Get Adpative Mode To Trigger Ratio did not receive ratio");
                }
                else
                    MessageBox.Show("GetAdpativeModeToTriggerRatio() method failed");
            }
        }

        private void SpeedFeedbackGetBTN_Click(object sender, RoutedEventArgs eventArguments)
        {
            try
            {
                AdaptiveSpeedFeedbackConfig? config;
                float? frequency;

                bool ok = DetectorsAccess.Apcs.GetAdaptiveSpeedFeedbackConfiguration(out config, out frequency, true);

                if (!ok)
                    throw new Exception("GetAdaptiveSpeedFeedbackConfiguration failed");
                SpeedFeedbackDisabledPNL.Background = Brushes.Transparent;
                SpeedFeedbackDisabledRDO.IsChecked = false;
                SpeedFeedbackEnabledPNL.Background = Brushes.Transparent;
                SpeedFeedbackEnabledRDO.IsChecked = false;

                if (config == AdaptiveSpeedFeedbackConfig.Disabled)
                {
                    SpeedFeedbackDisabledPNL.Background = Brushes.LightGreen;
                    SpeedFeedbackDisabledRDO.IsChecked = true;
                }
                else if (config == AdaptiveSpeedFeedbackConfig.EnabledWithFreq)
                {
                    SpeedFeedbackEnabledPNL.Background = Brushes.LightGreen;
                    SpeedFeedbackEnabledRDO.IsChecked = true;
                }
                else
                    throw new Exception("GetAdaptiveSpeedFeedbackConfiguration returned unknown configuration: " + ((int)config).ToString());

                SpeedFeedbackFrequencyTBX.Text = frequency.ToString();
                SpeedFeedbackFrequencyTBX.Background = Brushes.LightGreen;
            }
            catch (Exception ex) { AnomalyShow(ex); }
        }

        private void PulseWidthConfiguredGetBTN_Click(object sender, RoutedEventArgs eventArguments)
        {
            PulseWidth? width = null;

            PulseWidthConfigured1PNL.Background = PulseWidthConfigured2PNL.Background = PulseWidthConfigured3PNL.Background = PulseWidthConfigured3PNL.Background = Brushes.Transparent;
            if ((bool)PulseWidthConfigured1RDO.IsChecked)
            {
                width = PulseWidth.PulseWidth1;
                PulseWidthConfigured1PNL.Background = Brushes.LightGreen;
            }
            else if ((bool)PulseWidthConfigured2RDO.IsChecked)
            {
                width = PulseWidth.PulseWidth2;
                PulseWidthConfigured2PNL.Background = Brushes.LightGreen;
            }
            else if ((bool)PulseWidthConfigured3RDO.IsChecked)
            {
                width = PulseWidth.PulseWidth3;
                PulseWidthConfigured3PNL.Background = Brushes.LightGreen;
            }
            else if ((bool)PulseWidthConfigured4RDO.IsChecked)
            {
                width = PulseWidth.PulseWidth4;
                PulseWidthConfigured3PNL.Background = Brushes.LightGreen;
            }
            else
                MessageBox.Show("Select Pulse Width", "Oops!", MessageBoxButton.OK, MessageBoxImage.Hand);

            if (width != null)
                try
                {
                    float? time;
                    bool ok = DetectorsAccess.Apcs.GetConfigPulseWidth(width, out time, true);
                    if (!ok)
                        throw new Exception("GetConfigPulseWidth failed");
                    PulseWidthConfiguredTimeTBX.Text = time.ToString();
                    PulseWidthConfiguredTimeTBX.Background = Brushes.LightGreen;
                }
                catch (Exception ex) { AnomalyShow(ex); }
        }

        private void PulseWidthGetBTN_Click(object sender, RoutedEventArgs eventArguments)
        {
            try
            {
                PulseWidth? width;

                PulseWidth1PNL.Background = PulseWidth2PNL.Background = PulseWidth3PNL.Background = PulseWidth4PNL.Background = Brushes.Transparent;
                PulseWidth1RDO.IsChecked = PulseWidth2RDO.IsChecked = PulseWidth3RDO.IsChecked = PulseWidth4RDO.IsChecked = false;

                bool ok = DetectorsAccess.Apcs.GetCurrentPulseWidth(out width, true);
                if (!ok)
                    throw new Exception("GetCurrentPulseWidth failed");
                if (width == PulseWidth.PulseWidth1)
                {
                    PulseWidth1PNL.Background = Brushes.LightGreen;
                    PulseWidth1RDO.IsChecked = true;
                }
                else if (width == PulseWidth.PulseWidth2)
                {
                    PulseWidth2PNL.Background = Brushes.LightGreen;
                    PulseWidth2RDO.IsChecked = true;
                }
                else if (width == PulseWidth.PulseWidth3)
                {
                    PulseWidth3PNL.Background = Brushes.LightGreen;
                    PulseWidth3RDO.IsChecked = true;
                }
                else if (width == PulseWidth.PulseWidth4)
                {
                    PulseWidth4PNL.Background = Brushes.LightGreen;
                    PulseWidth4RDO.IsChecked = true;
                }
                else
                    throw new Exception("GetCurrentPulseWidth returned unknown width: " + ((int)width).ToString());
            }
            catch (Exception ex) { AnomalyShow(ex); }
        }

        private void PulseWidthSetBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool ok = false;
                PulseWidth1PNL.Background = PulseWidth2PNL.Background = PulseWidth3PNL.Background = PulseWidth4PNL.Background = Brushes.Transparent;
                if ((bool)PulseWidth1RDO.IsChecked)
                {
                    ok = DetectorsAccess.Apcs.SetCurrentPulseWidth(PulseWidth.PulseWidth1, true);
                    PulseWidth1PNL.Background = Brushes.LightGreen;
                }
                else if ((bool)PulseWidth2RDO.IsChecked)
                {
                    ok = DetectorsAccess.Apcs.SetCurrentPulseWidth(PulseWidth.PulseWidth2, true);
                    PulseWidth2PNL.Background = Brushes.LightGreen;
                }
                else if ((bool)PulseWidth3RDO.IsChecked)
                {
                    ok = DetectorsAccess.Apcs.SetCurrentPulseWidth(PulseWidth.PulseWidth3, true);
                    PulseWidth3PNL.Background = Brushes.LightGreen;
                }
                else if ((bool)PulseWidth4RDO.IsChecked)
                {
                    ok = DetectorsAccess.Apcs.SetCurrentPulseWidth(PulseWidth.PulseWidth4, true);
                    PulseWidth4PNL.Background = Brushes.LightGreen;
                }
                else
                {
                    MessageBox.Show("Select Pulse Width", "Oops!", MessageBoxButton.OK, MessageBoxImage.Hand);
                    ok = true;
                }
                if (!ok)
                    throw new Exception("GetCurrentPulseWidth failed");
            }
            catch (Exception ex) { AnomalyShow(ex); }
        }

        private void OnLoad(object sender, RoutedEventArgs eventArguments)
        {
            SpeedMsgStart();
        }

        private delegate void PanelUpdate();

        private void ResetBoardBTN_Click(object sender, RoutedEventArgs eventArguments)
        {
            try { DetectorsAccess.Apcs.ResetBoard(); }
            catch (Exception ex) { AnomalyShow(ex); }
        }

        private void ResetLineIdentityBTN_Click(object sender, RoutedEventArgs eventArguments)
        {
            bool /*OK?*/ ok = false;
            if (/*OK?*/ DetectorsAccess != null)
                ok = DetectorsAccess.StartDetectorDataAcq();
            DetectorsAccess.Logger.LogInfo("DetectorsAccess.StartDetectorDataAcq == " + ok.ToString());
        }

        private void ResetDetected(object sender, EventArgs unused)
        {
            try { ResetDetectedLBL.Visibility = DetectorsAccess.Apcs.APCSReset ? Visibility.Visible : Visibility.Hidden; }
            catch { }
        }

        private void ScanModeGetBTN_Click(object sender, RoutedEventArgs eventArguments)
        {
            try
            {
                ScanEnergyMode? mode;
                bool ok = DetectorsAccess.Apcs.GetScanEnergyMode(out mode, true);// . GetScanEnergyMode();

                ScanModeDualEnergyPNL.Background = ScanModeHighEnergyPNL.Background = ScanModeLowEnergyPNL.Background = ScanModeDualEnergyPNL.Background = Brushes.Transparent;
                ScanModeDualEnergyRDO.IsChecked = ScanModeHighEnergyRDO.IsChecked = ScanModeLowEnergyRDO.IsChecked = ScanModeLowDoseRDO.IsChecked = false;

                if (mode == ScanEnergyMode.Dual)
                {
                    ScanModeDualEnergyPNL.Background = Brushes.LightGreen;
                    ScanModeDualEnergyRDO.IsChecked = true;
                }
                else if (mode == ScanEnergyMode.High)
                {
                    ScanModeHighEnergyPNL.Background = Brushes.LightGreen;
                    ScanModeHighEnergyRDO.IsChecked = true;
                }
                else if (mode == ScanEnergyMode.Low)
                {
                    ScanModeLowEnergyPNL.Background = Brushes.LightGreen;
                    ScanModeLowEnergyRDO.IsChecked = true;
                }
                else if (mode == ScanEnergyMode.LowDose)
                {
                    ScanModeDualEnergyPNL.Background = Brushes.LightGreen;
                    ScanModeLowDoseRDO.IsChecked = true;
                }
                else
                    throw new Exception("GetScanEnergyMode returned unknown scan mode: " + ((int)mode).ToString());
            }
            catch (Exception ex) { AnomalyShow(ex); }
        }

        private void ScanModeSetBTN_Click(object sender, RoutedEventArgs eventArguments)
        {
            try
            {
                bool ok = false;
                if (ScanModeHighEnergyRDO.IsChecked == true)
                {
                    ok = DetectorsAccess.Apcs.SetScanEnergyMode(ScanEnergyMode.High, /*confirm*/ true);
                    if (!ok)
                        throw new Exception("SetScanEnergyMode failed");
                    ScanModeGetBTN_Click(sender, eventArguments);
                }
                else if (ScanModeLowEnergyRDO.IsChecked == true)
                {
                    ok = DetectorsAccess.Apcs.SetScanEnergyMode(ScanEnergyMode.Low, /*confirm*/ true);
                    if (!ok)
                        throw new Exception("SetScanEnergyMode failed");
                    ScanModeGetBTN_Click(sender, eventArguments);
                }
                else if (ScanModeLowDoseRDO.IsChecked == true)
                {
                    ok = DetectorsAccess.Apcs.SetScanEnergyMode(ScanEnergyMode.LowDose, /*confirm*/ true);
                    if (!ok)
                        throw new Exception("SetScanEnergyMode failed");
                    ScanModeGetBTN_Click(sender, eventArguments);
                }
                else if (ScanModeDualEnergyRDO.IsChecked == true)
                {
                    ok = DetectorsAccess.Apcs.SetScanEnergyMode(ScanEnergyMode.Dual, /*confirm*/ true);
                    if (!ok)
                        throw new Exception("SetScanEnergyMode failed");
                    ScanModeGetBTN_Click(sender, eventArguments);
                }
                else
                    MessageBox.Show("Select Scan Mode", "Oops!", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
            catch (Exception ex) { AnomalyShow(ex); }
        }

        private void PulseFrequencyGetBTN_Click(object sender, RoutedEventArgs eventArguments)
        {
            OperatingMode? mode = null;
            OperatingMode? selectedMode = null;
            int? frequency = null;

            try
            {
                if ((bool)PulseFrequencyMobileNonAdaptiveRDO.IsChecked)
                    selectedMode = OperatingMode.NonAdaptiveMobile;
                else if ((bool)PulseFrequencyPortalNonAdaptiveRDO.IsChecked)
                    selectedMode = OperatingMode.NonAdpativePortal;

                if (selectedMode == null)
                    MessageBox.Show("Select Operating Mode", "Oops!", MessageBoxButton.OK, MessageBoxImage.Hand);
                else
                {
                    bool ok = DetectorsAccess.Apcs.GetStaticPulseFrequency(selectedMode, out mode, out frequency, true);
                    if (!ok)
                        throw new Exception("GetStaticPulseFrequency failed");
                    if (mode == null)
                        throw new Exception("GetStaticPulseFrequency did not return mode");
                    if (mode.Value == OperatingMode.NonAdaptiveMobile)
                    {
                        PulseFrequencyMobileNonAdaptivePNL.Background = Brushes.LightGreen;
                        PulseFrequencyMobileNonAdaptiveRDO.IsChecked = true;
                        PulseFrequencyPortalNonAdaptivePNL.Background = Brushes.Transparent;
                        PulseFrequencyPortalNonAdaptiveRDO.IsChecked = false;
                    }
                    else if (mode.Value == OperatingMode.NonAdpativePortal)
                    {
                        PulseFrequencyMobileNonAdaptivePNL.Background = Brushes.Transparent;
                        PulseFrequencyMobileNonAdaptiveRDO.IsChecked = false;
                        PulseFrequencyPortalNonAdaptivePNL.Background = Brushes.LightGreen;
                        PulseFrequencyPortalNonAdaptiveRDO.IsChecked = true;
                    }
                    else
                        throw new Exception("GetStaticPulseFrequency returned unknown operating mode: " + ((int)mode).ToString());
                    if (frequency == null)
                        throw new Exception("GetStaticPulseFrequency did not return frequency");
                    PulseFrequencyFrequencyTBX.Text = frequency.Value.ToString();
                    PulseFrequencyFrequencyTBX.Background = Brushes.LightGreen;
                }
            }
            catch (Exception ex) { AnomalyShow(ex); }
        }

        private void PulseFrequencySetBTN_Click(object sender, RoutedEventArgs eventArguments)
        {
            OperatingMode? mode = null;
            StackPanel panel = null;

            uint frequency = Convert.ToUInt32(PulseFrequencyFrequencyTBX.Text);

            if ((bool)PulseFrequencyMobileNonAdaptiveRDO.IsChecked)
            {
                mode = OperatingMode.NonAdaptiveMobile;
                panel = PulseFrequencyMobileNonAdaptivePNL;
            }
            else if ((bool)PulseFrequencyPortalNonAdaptiveRDO.IsChecked)
            {
                mode = OperatingMode.NonAdpativePortal;
                panel = PulseFrequencyPortalNonAdaptivePNL;
            }
            else
                MessageBox.Show("Select Operating Mode", "Oops!", MessageBoxButton.OK, MessageBoxImage.Hand);

            if (mode != null)
                try
                {
                    DetectorsAccess.Apcs.SetStaticPulseFrequency(mode.Value, frequency);
                    PulseFrequencyPortalNonAdaptivePNL.Background = PulseFrequencyMobileNonAdaptivePNL.Background = Brushes.Transparent;
                    panel.Background = Brushes.LightGreen;
                }
                catch (Exception ex) { AnomalyShow(ex); }
        }

        private void TriggerRatioSetBTN_Click(object sender, RoutedEventArgs eventArguments)
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
                MessageBox.Show("Select Operating Mode", "Oops!", MessageBoxButton.OK, MessageBoxImage.Hand);

            if (mode != null)
            {
                try { ratio = Convert.ToSingle(TriggerRatioTBX.Text); }
                catch
                {
                    MessageBox.Show("Set Mode To Trigger Ratio");
                    return;
                }

                bool ok = DetectorsAccess.Apcs.SetAdaptiveModeTriggerRatio(mode.Value, ratio, true);
                TriggerRatioMobileAdaptivePNL.Background = TriggerRatioPortalAdaptivePNL.Background = Brushes.Transparent;
                if (ok)
                    panel.Background = Brushes.LightGreen;
                else
                    MessageBox.Show("SetModeToTriggerRatio Failed");
            }

        }

        private void SpeedFeedbackSetBTN_Click(object sender, RoutedEventArgs eventArguments)
        {
            AdaptiveSpeedFeedbackConfig? mode = null;
            StackPanel panel = null;

            try
            {
                float frequency = Convert.ToUInt32(SpeedFeedbackFrequencyTBX.Text);

                if ((bool)SpeedFeedbackDisabledRDO.IsChecked)
                {
                    mode = AdaptiveSpeedFeedbackConfig.Disabled;
                    panel = SpeedFeedbackDisabledPNL;
                }
                else if ((bool)SpeedFeedbackEnabledRDO.IsChecked)
                {
                    mode = AdaptiveSpeedFeedbackConfig.EnabledWithFreq;
                    panel = SpeedFeedbackEnabledPNL;
                }
                else
                    MessageBox.Show("Select Operating Mode", "Oops!", MessageBoxButton.OK, MessageBoxImage.Hand);

                if (mode != null)
                {
                    bool ok = DetectorsAccess.Apcs.SetAdaptiveSpeedFeedbackConfiguration(mode.Value, frequency, true);
                    if (!ok)
                        throw new Exception("SetAdaptiveSpeedFeedbackConfiguration failed");
                    SpeedFeedbackDisabledPNL.Background = SpeedFeedbackEnabledPNL.Background = Brushes.Transparent;
                    panel.Background = Brushes.LightGreen;
                }
            }
            catch (Exception ex) { AnomalyShow(ex); }
        }

        private void PulseWidthConfigureSetBTN_Click(object sender, RoutedEventArgs eventArguments)
        {
            PulseWidth? width = null;
            float microseconds = Convert.ToSingle(PulseWidthConfiguredTimeTBX.Text);
            StackPanel panel = null;

            if ((bool)PulseWidthConfigured1RDO.IsChecked)
            {
                width = PulseWidth.PulseWidth1;
                panel = PulseWidthConfigured1PNL;
            }
            else if ((bool)PulseWidthConfigured2RDO.IsChecked)
            {
                width = PulseWidth.PulseWidth2;
                panel = PulseWidthConfigured2PNL;
            }
            else if ((bool)PulseWidthConfigured3RDO.IsChecked)
            {
                width = PulseWidth.PulseWidth3;
                panel = PulseWidthConfigured3PNL;
            }
            else if ((bool)PulseWidthConfigured4RDO.IsChecked)
            {
                width = PulseWidth.PulseWidth4;
                panel = PulseWidthConfigured4PNL;
            }
            else
                MessageBox.Show("Select Pulse Width", "Oops!", MessageBoxButton.OK, MessageBoxImage.Hand);

            if (width != null)
                try
                {
                    DetectorsAccess.Apcs.SetConfigPulseWidth(width.Value, microseconds, true);
                    PulseWidthConfigured1PNL.Background = PulseWidthConfigured2PNL.Background = PulseWidthConfigured3PNL.Background = PulseWidthConfigured4PNL.Background = Brushes.Transparent;
                    panel.Background = Brushes.LightGreen;
                }
                catch (Exception ex) { AnomalyShow(ex); }
        }

        private void SpeedModePanel_Update()
        {
            OperatingMode mode = DetectorsAccess.Apcs.AdaptiveSpeedMode;
            OperatingModePortalAdaptivePNL.Background = Brushes.Transparent;
            OperatingModeMobileAdaptivePNL.Background = Brushes.Transparent;
            if (mode == OperatingMode.AdaptiveMobile)
                OperatingModeMobileAdaptivePNL.Background = Brushes.LightGreen;
            else if (mode == OperatingMode.AdaptivePortal)
                OperatingModePortalAdaptivePNL.Background = Brushes.LightGreen;
            if ((mode == OperatingMode.AdaptiveMobile) || (mode == OperatingMode.AdaptivePortal))
                SpeedFeedbackFrequencyTBX.Text = DetectorsAccess.Apcs.AdaptiveSpeed.ToString();
        }

        private void SpeedMsgAgent()
        {
            EventLoggerAccess logger = ApcsAccess.Logger;
            try
            {
                PanelUpdate update = new PanelUpdate(SpeedModePanel_Update);
                while (/*run?*/ !_speedMsgEnd.WaitOne(Utilities.TimeTENTH) && (DetectorsAccess != null) && (DetectorsAccess.Apcs != null) && (DetectorsAccess.Apcs.SpeedMessageEvent != null))
                    OperatingModePortalAdaptivePNL.Dispatcher.BeginInvoke(update, null);
            }
            catch (Exception ex) { _speedMsgException = ex; }
        }
        private void SpeedMsgStart()
        {
            if (_speedMsgThread == null)
            {
                _speedMsgThread = Threads.Create(SpeedMsgAgent, ref _speedMsgEnd, "TestAPCS Update thread");
                _speedMsgException = null;
                _speedMsgThread.Start();
            }
        }
        public static void SpeedMsgStop()
        {
            try
            {
                if (/*exists (avoid first try exceptions)?*/ _speedMsgThread != null)
                    _speedMsgThread = Threads.Dispose(_speedMsgThread, ref _speedMsgEnd);
            }
            catch { }
            finally { _speedMsgThread = null; }
        }
        private static ManualResetEvent _speedMsgEnd;
        private Exception _speedMsgException;
        private static Thread _speedMsgThread;

        public TestAPCS(DetectorsDataAccess detectorsAccess)
        {
            InitializeComponent();
            DetectorsAccess = detectorsAccess;
            SignOfLifeSequenceTBK.DataContext = DetectorsAccess.Apcs;
            DetectorsAccess.Apcs.APCSResetEvent += new EventHandler(ResetDetected);
        }

        private void QueryStateBTN_Click(object sender, RoutedEventArgs eventArguments)
        {
            _anomaly = false;
            //GetAdptvModeTrigRatioBtn_Click(sender, eventArguments);
            if (!_anomaly)
            {
                ScanModeGetBTN_Click(sender, eventArguments);
                if (!_anomaly)
                {
                    SpeedFeedbackGetBTN_Click(sender, eventArguments);
                    if (!_anomaly)
                    {
                        //PulseWidthConfiguredGetBTN_Click(sender, eventArguments);
                        if (!_anomaly)
                        {
                            PulseWidthGetBTN_Click(sender, eventArguments);
                            if (!_anomaly)
                            {
                                OperatingModeGetBTN_Click(sender, eventArguments);
                                if (!_anomaly)
                                {
                                    ScanModeGetBTN_Click(sender, eventArguments);
                                    if (!_anomaly)
                                        //PulseFrequencyGetBTN_Click(sender, eventArguments);
                                        Debug.Assert(!_anomaly);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
