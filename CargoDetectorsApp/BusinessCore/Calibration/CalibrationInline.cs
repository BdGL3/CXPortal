using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Detectors.DataAccessCore;
using L3.Cargo.Detectors.StatusManagerCore;
using L3.Cargo.Communications.Detectors.Common;

namespace L3.Cargo.Detectors.BusinessCore
{
    class CalibrationInline : Calibration
    {
        protected CalibrationDataCollection _DataCollection;

        public CalibrationInline(EventLoggerAccess log, DetectorsDataAccess dataAccess, DetectorsStatusManager statusManager)
        {
            SetupCalibration(log, dataAccess, statusManager);
            _DataCollection = new CalibrationDataCollection();
        }

        public override bool IsCalibrationRunning()
        {
            return false;
        }

        public override bool IsCalibrationValid()
        {
            return true;
        }

        public override void AddDarkDataLine(DataInfo dataInfo)
        {
            _DataCollection.AddDarkData(dataInfo.XRayInfo.Energy, dataInfo.LineData);
        }

        public override void AddAirDataLine(DataInfo dataInfo)
        {
            _DataCollection.AddAirData(dataInfo.XRayInfo.Energy, dataInfo.LineData);
        }

        public override void AddReferenceCorrection(XRayInfoIDStruct lineInfo, double referenceData)
        {
            _DataCollection.AddReferenceData(lineInfo.Energy, referenceData);
        }

        public override void ResetReferenceCorrection(XRayInfoIDStruct lineInfo)
        {
            _DataCollection.ClearReferenceData(lineInfo.Energy);
        }

        public override Pixel[] GetAirData(XRayInfoIDStruct lineInfo)
        {
            return _DataCollection.GetAirData(lineInfo.Energy);
        }

        public override Pixel[] GetDarkData(XRayInfoIDStruct lineInfo)
        {
            return _DataCollection.GetDarkData(lineInfo.Energy);
        }

        public override Pixel[] GetAirDataCollection(XRayInfoIDStruct lineInfo)
        {
            return _DataCollection.GetAirDataCollection(lineInfo.Energy);
        }

        public override Pixel[] GetDarkDataCollection(XRayInfoIDStruct lineInfo)
        {
            return _DataCollection.GetDarkDataCollection(lineInfo.Energy);
        }

        public override float[] GetScaleFactor(XRayInfoIDStruct lineInfo)
        {
            return _DataCollection.GetScaleFactor(lineInfo.Energy);
        }

        public override float[] GetReferenceCorrections(XRayInfoIDStruct lineInfo)
        {
            return _DataCollection.GetReferenceData(lineInfo.Energy);
        }

    }
}
