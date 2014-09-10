using System.Collections.Generic;
using L3.Cargo.Communications.Detectors.Common;

namespace L3.Cargo.Detectors.BusinessCore
{
    public class CalibrationDataCollection
    {
        #region Private Members

        private Dictionary<XRayEnergyEnum, CalibrationData> _calibrationData;

        #endregion Private Members


        #region Constructors

        public CalibrationDataCollection()
        {
            _calibrationData = new Dictionary<XRayEnergyEnum, CalibrationData>();
            _calibrationData.Add(XRayEnergyEnum.HighEnergy, new CalibrationData());
            _calibrationData.Add(XRayEnergyEnum.LowEnergy, new CalibrationData());
        }

        #endregion Constructors


        #region Public Methods

        public void AddData(XRayEnergyEnum energy, PixelDataType dataType, Pixel[] data)
        {
            if (dataType == PixelDataType.Air)
            {
                AddAirData(energy, data);
            }
            else if (dataType == PixelDataType.Dark)
            {
                AddDarkData(energy, data);
            }
        }

        public void AddAirData(XRayEnergyEnum energy, Pixel[] data)
        {
            _calibrationData[energy].Add(PixelDataType.Air, data);
        }

        public void AddDarkData(XRayEnergyEnum energy, Pixel[] data)
        {
            _calibrationData[energy].Add(PixelDataType.Dark, data);
        }

        public void AddReferenceData(XRayEnergyEnum energy, double data)
        {
            _calibrationData[energy].AddReferenceData(data);
        }

        public void ClearReferenceData(XRayEnergyEnum energy)
        {
            _calibrationData[energy].ClearReferenceData();
        }

        public Pixel[] GetAirData(XRayEnergyEnum energy)
        {
            return _calibrationData[energy].AirData;
        }

        public bool IsComplete(XRayEnergyEnum energy, PixelDataType dataType)
        {
            bool ret = false;

            if (_calibrationData.ContainsKey(energy))
            {
                if (dataType == PixelDataType.Air)
                {
                    ret = _calibrationData[energy].IsAirDataComplete;
                }
                else if (dataType == PixelDataType.Dark)
                {
                    ret = _calibrationData[energy].IsDarkDataComplete;
                }
            }

            return ret;
        }

        public Pixel[] GetDarkData(XRayEnergyEnum energy)
        {
            return _calibrationData[energy].DarkData;
        }

        public Pixel[] GetAirDataCollection(XRayEnergyEnum energy)
        {
            return _calibrationData[energy].AirDataCollection;
        }

        public Pixel[] GetDarkDataCollection(XRayEnergyEnum energy)
        {
            return _calibrationData[energy].DarkDataCollection;
        }

        public float[] GetScaleFactor(XRayEnergyEnum energy)
        {
            return _calibrationData[energy].ScaleFactor;
        }

        public float[] GetReferenceData(XRayEnergyEnum energy)
        {
            return _calibrationData[energy].ReferenceData;
        }

        public void Clear()
        {
            _calibrationData[XRayEnergyEnum.HighEnergy].ClearAll();
            _calibrationData[XRayEnergyEnum.LowEnergy].ClearAll();
        }

        #endregion Public Methods
    }
}
