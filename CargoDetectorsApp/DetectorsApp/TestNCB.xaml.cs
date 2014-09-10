using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using L3.Cargo.Common.PxeAccess;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Detectors.Client;
using L3.Cargo.Communications.Detectors.Common;
using L3.Cargo.Communications.OPC.Portal;
using L3.Cargo.Detectors.DataAccessCore;
using L3.Cargo.Detectors.BusinessCore;

namespace DetectorsApp
{
    public partial class TestNCB : UserControl
    {
        [DefaultValue(null)]
        private BusinessManager AccessBusinessManager { get; set; }

        [DefaultValue(null)]
        private DetectorsDataAccess AccessDetectorsData { get; set; }

        private void AnomalyShow(Exception exc)
        {
            LogMessage(exc.ToString());
            if (DetectorsAccess.Logger != null)
                DetectorsAccess.Logger.LogError(Utilities.TextTidy(exc.ToString()));
            MessageBox.Show(Utilities.TextTidy(exc.ToString()), "Anomaly!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public TestNCB(DetectorsDataAccess detectorsDataAccess, BusinessManager businessManager)
        {
            InitializeComponent();

            AccessBusinessManager = businessManager;
            AccessDetectorsData = detectorsDataAccess;

            // disable fake objects
            FakeObjectCHK.IsChecked = false;
            FakeObjectUpdate();

            CurrentLineIdTextBlk.DataContext = AccessDetectorsData.Detectors;
        }

        private void ConfigParameterQueryBtnOnClick(object sender, RoutedEventArgs eventArguments)
        {
            try
            {
                ushort delay = 0;
                ushort signOfLife = 0;
                AccessDetectorsData.Detectors.GetConfigParameters(out delay, out signOfLife);
                DelayPeriod.Text = delay.ToString();
                SignOfLife.Text = signOfLife.ToString();
                LogMessage("NCB[0] reports pulse delay = " + DelayPeriod.Text + " * 20ns, sign of life = " + SignOfLife.Text + "cs");
            }
            catch (Exception ex) { AnomalyShow(ex); }
        }

        private void DeviceStateQueryBtnOnClick(object sender, RoutedEventArgs eventArguments)
        {
            for (int /*NCB index*/ ix = 0; ix < AccessDetectorsData.Detectors.NCBCount; ix++)
                try
                {
                    ushort referenceCount;
                    ushort sensorCount;
                    deviceState? state = AccessDetectorsData.Detectors.GetDeviceState(out sensorCount, out referenceCount, ix);
                    LogMessage("NCB[" + ix.ToString() + "] reports number of channels = " + sensorCount.ToString() + ", number of references = " + referenceCount.ToString() + ", device state = " + state.ToString());
                }
                catch (Exception ex) { AnomalyShow(ex); }
        }

        private void FakeObjectOnChange(object sender, RoutedEventArgs eventArguments) { FakeObjectUpdate(); }

        private void FakeObjectUpdate()
        {
            bool enabled = FakeObjectCHK.IsChecked.Value;
            if ((NoObjectRDO != null) && (ObjectRDO != null))
                NoObjectRDO.IsEnabled = ObjectRDO.IsEnabled = enabled;
            try
            {
                if (AccessBusinessManager != null)
                    AccessBusinessManager.FakeEndOfObject(enabled, ObjectRDO.IsChecked.Value);
            }
            catch (Exception ex) { AnomalyShow(ex); }
        }

        private void FakeXrayOnChange(object sender, RoutedEventArgs eventArguments)
        {
            FakeXRayOnBTN.IsEnabled = FakeObjectCHK.IsEnabled = FakeXRayCHK.IsChecked.Value;
            FakeXRayOffBTN.IsEnabled = false;
            FakeObjectUpdate();
            try { AccessDetectorsData.OpcTags.IgnoreOpcUpdatesForXrays = FakeXRayCHK.IsChecked.Value; }
            catch (Exception ex) { AnomalyShow(ex); }
        }

        private void FakeXrayOnClickOff(object sender, RoutedEventArgs eventArguments)
        {
            FakeXRayOnBTN.IsEnabled = true;
            FakeXRayOffBTN.IsEnabled = false;
            try { AccessDetectorsData.OpcTags.LINAC_STATE.Value = LINAC_STATE_VALUE.XraysOff; }
            catch (Exception ex) { AnomalyShow(ex); }
        }

        private void FakeXrayOnClickOn(object sender, RoutedEventArgs eventArguments)
        {
            FakeXRayOnBTN.IsEnabled = false;
            FakeXRayOffBTN.IsEnabled = true;
            try { AccessDetectorsData.OpcTags.LINAC_STATE.Value = LINAC_STATE_VALUE.XRaysOn; }
            catch (Exception ex) { AnomalyShow(ex); }
        }

        private void IdentifyQueryBtn_Click(object sender, RoutedEventArgs eventArguments)
        {
            try
            {
                int identity = AccessDetectorsData.Detectors.GetIdentification();
                LogMessage("NCB[0] reports identity " + identity.ToString());
            }
            catch (Exception ex) { AnomalyShow(ex); }
        }

        public void LogMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                message = Utilities.TextTidy(message);
                try { DetectorsAccess.Logger.LogInfo(message); }
                catch { }
                DateTime now = DateTime.Now;
                message = now + ":" + now.Millisecond + ": " + message;
                if (!message.EndsWith("\r"))
                    message += "\r";
                if (NCBLogArea != null)
                    if (!NCBLogArea.CheckAccess())
                        NCBLogArea.Dispatcher.Invoke(
                                DispatcherPriority.Normal,
                                new Action(
                                        delegate()
                                        {
                                            NCBLogArea.AppendText(message);
                                            NCBLogArea.LineDown();
                                        }
                                    )
                            );
                    else
                    {
                        NCBLogArea.AppendText(message);
                        NCBLogArea.LineDown();
                    }
            }
        }

        private void ProcessTraceOnClick(object sender, RoutedEventArgs eventArguments)
        {
            if ((AccessDetectorsData != null) && (AccessDetectorsData.Detectors != null))
                AccessDetectorsData.Detectors.DataGatherTrace = 3 /*stagedLines*/;
        }

        private void XRayDataStateQueryBtn_Click(object sender, RoutedEventArgs eventArguments)
        {
            try
            {
                xrayDataState? state = AccessDetectorsData.Detectors.GetXRayDataState();
                LogMessage("NCB[0] reports xray data state=" + state);
            }
            catch (Exception ex) { AnomalyShow(ex); }
        }

        private void XRayDataStateStartBtn_Checked(object sender, RoutedEventArgs eventArguments)
        {
            try { AccessDetectorsData.Detectors.SetDataTransferMode(dataTransferMode.Start); }
            catch (Exception ex) { AnomalyShow(ex); }
        }

        private void XRayDataStateStopBtn_Checked(object sender, RoutedEventArgs eventArguments)
        {
            try { AccessDetectorsData.Detectors.SetDataTransferMode(dataTransferMode.Stop); }
            catch (Exception ex) { AnomalyShow(ex); }

            try
            {
                PxeWriteAccess pxeAccess = new PxeWriteAccess();
                pxeAccess.CreatePXE("Test" + DateTime.Now.Ticks.ToString() + ".pxe");
                pxeAccess.CreatePXEHeader(1, (uint)AccessDetectorsData.Detectors.RawDataCollection.Count, (uint)AccessDetectorsData.Detectors.PixelsPerColumn);
                while (AccessDetectorsData.Detectors.RawDataCollection.Count > 0)
                {
                    DataInfo information = AccessDetectorsData.Detectors.RawDataCollection.Take();
                    float[] data = PixelConverter.Convert(information.LineData);
                    pxeAccess.WriteDataLines(1, data, 1);
                }
                pxeAccess.ClosePXEWrite();
            }
            catch (Exception ex) { AnomalyShow(ex); }
        }
    }
}
