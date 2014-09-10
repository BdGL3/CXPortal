using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;

using L3.Cargo.Communications.Detectors.Common;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Detectors.DataAccessCore;
using L3.Cargo.Detectors.StatusManagerCore;
using L3.Cargo.Communications.APCS.Common;
using L3.Cargo.Communications.OPC.Portal;
using L3.Cargo.Detectors.Common;
using L3.Cargo.Subsystem.DataAccessCore;
using L3.Cargo.Common.PxeAccess;

namespace L3.Cargo.Detectors.BusinessCore
{
    public abstract class Calibration : IDisposable
    {
        #region Private Members

        protected EventLoggerAccess _logger;
        protected List<int> _badDetectorsList;
        protected DetectorsDataAccess _dataAccess;
        protected bool _isCalibrationValid = false;

        #endregion Private Members


        #region Public Members

        public List<int> BadDetectorsList
        {
            get { return _badDetectorsList; }
        }

        #endregion Public Members

        /// <summary>
        /// This is a safety net to ensure that resources are disposed even if
        /// <see cref="Dispose()"/> is not called.</summary>
        ~Calibration() { Dispose(); }

        /// <summary>
        /// Dispose resources and suppress finalization. USE THIS METHOD rather than just setting a
        /// reference to null and letting the system garbage collect! It ensures that the
        /// connection(s) are tidied, informing remote client(s)/host(s) of the
        /// stand-down.</summary>
        /// <exception cref="Exception">
        /// The method is written so that it never throws an exception.</exception>
        public void Dispose()
        {
            Dispose(!Disposed);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool isDisposing)
        {
            if (/*dispose?*/ isDisposing)
            {
                Disposed = true;
            }
        }

        /// <summary>
        /// Disposed declares whether or not the class instance has been disposed.</summary>
        [DefaultValue(false)]
        public static bool Disposed { get; private set; }


        #region Protected Methods

        protected virtual void SetupCalibration(EventLoggerAccess log, DetectorsDataAccess dataAccess, DetectorsStatusManager statusManager)
        {
            _dataAccess = dataAccess;
            _logger = log;
            _badDetectorsList = new List<int>();
        }

        #endregion


        #region Public Methods

        public abstract bool IsCalibrationRunning();

        public abstract bool IsCalibrationValid();

        public virtual void AddDataLine(DataInfo dataInfo)
        {
        }

        public virtual void AddDarkDataLine(DataInfo dataInfo)
        {
        }

        public virtual void AddAirDataLine(DataInfo dataInfo)
        {
        }

        public abstract void AddReferenceCorrection(XRayInfoIDStruct lineInfo, double referenceData);

        public abstract void ResetReferenceCorrection(XRayInfoIDStruct lineInfo);

        public abstract Pixel[] GetAirData(XRayInfoIDStruct lineInfo);

        public abstract Pixel[] GetDarkData(XRayInfoIDStruct lineInfo);

        public abstract Pixel[] GetAirDataCollection(XRayInfoIDStruct lineInfo);

        public abstract Pixel[] GetDarkDataCollection(XRayInfoIDStruct lineInfo);

        public abstract float[] GetScaleFactor(XRayInfoIDStruct lineInfo);

        public abstract float[] GetReferenceCorrections(XRayInfoIDStruct lineInfo);

        #endregion Public Methods
    }
}
