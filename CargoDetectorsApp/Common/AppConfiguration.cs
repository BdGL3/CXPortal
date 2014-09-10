using System;
using System.Configuration;
using System.Globalization;

namespace L3.Cargo.Detectors.Common
{
    static public class AppConfiguration
    {
        static public float NormConstant = float.Parse(ConfigurationManager.AppSettings["NormConstant"], CultureInfo.InvariantCulture);

        static public int ObjectThreshold = int.Parse(ConfigurationManager.AppSettings["ObjectThreshold"]);

        static public int SmallObjectSizeInPixels = int.Parse(ConfigurationManager.AppSettings["SmallObjectSizeInPixels"]);

        static public int NumberOfLinesForStartOfObject = int.Parse(ConfigurationManager.AppSettings["NumLinesForSOO"]);

        static public int NumberofLinesForEndOfObject = int.Parse(ConfigurationManager.AppSettings["NumLinesForEOO"]);

        static public int PixelsPerColumn = int.Parse(ConfigurationManager.AppSettings["PixelsPerColumn"]);

        static public int BytesPerPixel = int.Parse(ConfigurationManager.AppSettings["BytesPerPixel"]);

        static public int CalibrationDataLines = int.Parse(ConfigurationManager.AppSettings["CalibrationDataLines"]);

        static public bool EnableReferenceCorrection = bool.Parse(ConfigurationManager.AppSettings["EnableReferenceCorrection"]);

        static public int ReferenceRangeLowerDetectorNum = int.Parse(ConfigurationManager.AppSettings["ReferenceRangeLowerDetectorNum"]);

        static public int ReferenceRangeUpperDetectorNum = int.Parse(ConfigurationManager.AppSettings["ReferenceRangeUpperDetectorNum"]);

        static public float ReferenceScaleFactorLowerLimit = float.Parse(ConfigurationManager.AppSettings["ReferenceScaleFactorLowerLimit"], CultureInfo.InvariantCulture);

        static public float ReferenceScaleFactorUpperLimit = float.Parse(ConfigurationManager.AppSettings["ReferenceScaleFactorUpperLimit"], CultureInfo.InvariantCulture);

        static public bool CorrectForBadDetectors = bool.Parse(ConfigurationManager.AppSettings["CorrectForBadDetectors"]);

        static public double WarningPercentBadDetectors = double.Parse(ConfigurationManager.AppSettings["WarningPercentBadDetectors"], CultureInfo.InvariantCulture) / 100.0;

        static public double ErrorPercentBadDetectors = double.Parse(ConfigurationManager.AppSettings["ErrorPercentBadDetectors"], CultureInfo.InvariantCulture) / 100.0;

        static public int MaxNumContiguousBadDetectors = int.Parse(ConfigurationManager.AppSettings["MaxNumContiguousBadDetectors"]);

        static public bool NormalizeRawData = bool.Parse(ConfigurationManager.AppSettings["NormalizeRawData"]);

        static public int DataLineProcessTimeout = 10;

        static public int DataCollectionMaxSize = 25;

        static public int MaxPXEWidth = 65536;

        static public string CargoHostServer = ConfigurationManager.AppSettings["CargoHostServer"];

        static public int CargoHostPort = int.Parse(ConfigurationManager.AppSettings["CargoHostPort"]);

        static public string DiplotConnectionUri = ConfigurationManager.AppSettings["DiplotConnectionUri"];

        static public string DiplotMulticastIPAddress = ConfigurationManager.AppSettings["DiplotMulticastIPAddress"];

        static public bool DiplotRemoveReferenceData = bool.Parse(ConfigurationManager.AppSettings["DiplotRemoveReferenceData"]);

        static public int DiplotDataPort = int.Parse(ConfigurationManager.AppSettings["DiplotDataPort"]);

        static public int DetectorsPerBoard = int.Parse(ConfigurationManager.AppSettings["DetectorsPerBoard"]);

        static public bool ShowDebugDisplays = bool.Parse(ConfigurationManager.AppSettings["ShowDebugDisplays"]);

        static public string HostTempFileLocation = ConfigurationManager.AppSettings["HostTempFileLocation"];

        static public int RealTimeViewerDataPort = int.Parse(ConfigurationManager.AppSettings["RealTimeViewerDataPort"]);

        static public string RealTimeViewerMulticastIPAddress = ConfigurationManager.AppSettings["RealTimeViewerMulticastIPAddress"];

        static public int RealTimeViewerUdpClientPort = int.Parse(ConfigurationManager.AppSettings["RealTimeViewerUdpClientPort"]);

        static public int RealTimeViewerPixelInterval = int.Parse(ConfigurationManager.AppSettings["RealTimeViewerPixelInterval"]);

        static public uint DualPulseFrequency = uint.Parse(ConfigurationManager.AppSettings["DualPulseFrequency"]);

        static public uint HighPulseFrequency = uint.Parse(ConfigurationManager.AppSettings["HighPulseFrequency"]);

        static public uint LowPulseFrequency = uint.Parse(ConfigurationManager.AppSettings["LowPulseFrequency"]);

        static public int SearchBeginDetectorNum = int.Parse(ConfigurationManager.AppSettings["SearchBeginDetectorNum"]);

        static public int SearchEndDetectorNum = int.Parse(ConfigurationManager.AppSettings["SearchEndDetectorNum"]);

        static public uint APCSOperatingMode = uint.Parse(ConfigurationManager.AppSettings["APCSOperatingMode"]);

        static public float AdaptiveModeTriggerRatio = float.Parse(ConfigurationManager.AppSettings["AdaptiveModeTriggerRatio"], CultureInfo.InvariantCulture);

        static public bool EnableAdaptiveSpeedFeedback = bool.Parse(ConfigurationManager.AppSettings["EnableAdaptiveSpeedFeedback"]);

        static public float AdaptiveSpeedFeedbackFrequency = float.Parse(ConfigurationManager.AppSettings["AdaptiveSpeedFeedbackFrequency"], CultureInfo.InvariantCulture);

        static public ushort AdaptiveMinimumFrequency = ushort.Parse(ConfigurationManager.AppSettings["AdaptiveMinimumFrequency"]);

        static public ushort AdaptiveMaximumFrequency = ushort.Parse(ConfigurationManager.AppSettings["AdaptiveMaximumFrequency"]);
        
        static public CalibrationModeEnum CalibrationMode = (CalibrationModeEnum)Enum.Parse(typeof(CalibrationModeEnum), ConfigurationManager.AppSettings["CalibrateMode"]);
        
        static public bool StoreAirDarkSamples = bool.Parse(ConfigurationManager.AppSettings["StoreAirDarkSamples"]);

        static public bool StoreScaleFactor = bool.Parse(ConfigurationManager.AppSettings["StoreScaleFactor"]);

        static public bool StoreReferenceCorrection = bool.Parse(ConfigurationManager.AppSettings["StoreReferenceCorrection"]);

        static public int XrayOffLineThreshold = int.Parse(ConfigurationManager.AppSettings["XrayOffLineThreshold"]);

        static public int NCBCount = int.Parse(ConfigurationManager.AppSettings["NCBCount"]);

        public enum CalibrationModeEnum
        {
            Inline = 0,
            Persistent = 1,
            InlineStandstill = 2
        }
    }
}
