using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using L3.Cargo.Communications.Detectors.Common;
using L3.Cargo.Detectors.Common;

namespace L3.Cargo.Detectors.BusinessCore
{
    public class CalibrationData
    {
        #region Private Members

        private List<Pixel[]> _CollectedAirData;

        private List<Pixel[]> _CollectedDarkData;

        private Pixel[] _AirData;

        private Pixel[] _DarkData;

        private float[] _ScaleFactor;

        private List<float> _ReferenceData;

        private double[] _AirReferenceRatio;

        private bool _IsAirProcessed;

        private bool _IsDarkProcessed;

        private bool _IsScaleFactorProcessed;

        private bool _IsAirReferenceRatioProcessed;

        #endregion Private Members


        #region Public Members

        public double[] AirReferenceRatio
        {
            get
            {
                if (!_IsAirReferenceRatioProcessed)
                {
                    _AirReferenceRatio = ProcessAirReferenceRatio(_CollectedAirData, DarkData, true);
                    _IsAirReferenceRatioProcessed = true;
                }

                return _AirReferenceRatio;
            }
        }

        public Pixel[] AirData
        {
            get
            {
                if (!_IsAirProcessed)
                {
                    _AirData = ProcessData(_CollectedAirData, true);
                    _IsAirProcessed = true;
                    _IsScaleFactorProcessed = false;
                }

                return _AirData;
            }
        }

        public Pixel[] DarkData
        {
            get
            {
                if (!_IsDarkProcessed)
                {
                    _DarkData = ProcessData(_CollectedDarkData, false);
                    _IsDarkProcessed = true;
                    _IsScaleFactorProcessed = false;
                }

                return _DarkData;
            }
        }

        public Pixel[] AirDataCollection
        {
            get
            {
                return CollectedDataToArray(_CollectedAirData);
            }
        }

        public Pixel[] DarkDataCollection
        {
            get
            {
                return CollectedDataToArray(_CollectedDarkData);
            }
        }

        public float[] ScaleFactor
        {
            get
            {
                if (!_IsScaleFactorProcessed)
                {
                    _ScaleFactor = ProcessScaleFactor(AirData, DarkData);
                    _IsScaleFactorProcessed = true;
                }

                return _ScaleFactor;
            }
        }

        public uint AirLineCount
        {
            get { return (uint)_CollectedAirData.Count; }
        }

        public uint DarkLineCount
        {
            get { return (uint)_CollectedDarkData.Count; }
        }

        public bool IsAirDataComplete
        {
            get
            {
                return (AirLineCount >= AppConfiguration.CalibrationDataLines);
            }
        }

        public bool IsDarkDataComplete
        {
            get
            {
                return (DarkLineCount >= AppConfiguration.CalibrationDataLines);
            }
        }

        public float[] ReferenceData
        {
            get
            {
                return _ReferenceData.ToArray();
            }
        }

        #endregion Public Members


        #region Constructors

        public CalibrationData()
        {
            _IsAirProcessed = false;
            _IsDarkProcessed = false;
            _IsScaleFactorProcessed = false;

            _CollectedAirData = new List<Pixel[]>();
            _CollectedDarkData = new List<Pixel[]>();
            _ReferenceData = new List<float>();

            _AirData = new Pixel[AppConfiguration.PixelsPerColumn];
            _DarkData = new Pixel[AppConfiguration.PixelsPerColumn];
            _ScaleFactor = new float[AppConfiguration.PixelsPerColumn];
        }

        #endregion Constructors


        #region Private Methods

        private static Pixel[] CollectedDataToArray(List<Pixel[]> collectedData)
        {
            Pixel[] ret = null;

            if (collectedData.Count > 0)
            {
                ret = new Pixel[collectedData.Count * AppConfiguration.PixelsPerColumn];

                Parallel.For(0, collectedData.Count, lineNum =>
                    {
                        collectedData[lineNum].CopyTo(ret, lineNum * AppConfiguration.PixelsPerColumn);
                    });
            }

            return ret;
        }

        private static double[] ProcessAirReferenceRatio(List<Pixel[]> collectedData, Pixel[] darkData, bool processSTD)
        {
            double[] ret = null;

            double[] avgRef = null;

            if (collectedData.Count > 0)
            {
                ret = new double[collectedData[0].Length];
                avgRef = new double[collectedData.Count]; 

                Parallel.For(0, collectedData.Count, lineNum =>
                {
                    for (int detectorNum = AppConfiguration.ReferenceRangeLowerDetectorNum; detectorNum <= AppConfiguration.ReferenceRangeUpperDetectorNum; detectorNum++)
                    {
                        avgRef[lineNum] += (collectedData[lineNum][detectorNum].Value - darkData[detectorNum].Value);
                    }
                    avgRef[lineNum] /= ((AppConfiguration.ReferenceRangeUpperDetectorNum - AppConfiguration.ReferenceRangeLowerDetectorNum) + 1);
                });

                Parallel.For(0, collectedData[0].Length, detectorNum =>
                {
                    double avgValue = 0;
                    double avgRatio = 0;

                    for (int lineNum = 0; lineNum < collectedData.Count; lineNum++)
                    {
                        avgValue += collectedData[lineNum][detectorNum].Value;
                        avgRatio += ((collectedData[lineNum][detectorNum].Value - darkData[detectorNum].Value) / avgRef[lineNum]);
                    }

                    avgValue /= (uint)collectedData.Count;
                    avgRatio /= (uint)collectedData.Count;

                    ret[detectorNum] = avgValue;

                    if (processSTD)
                    {
                        double stdDev = 0;
                        uint validAvgValue = 0;
                        uint validCount = 0;

                        for (int lineNum = 0; lineNum < collectedData.Count; lineNum++)
                        {
                            stdDev += Math.Pow((double)collectedData[lineNum][detectorNum].Value - (double)avgValue, 2);
                        }

                        stdDev = Math.Sqrt(stdDev / collectedData.Count);

                        uint upperBound = (uint)(avgValue + (stdDev * 4));
                        uint lowerBound = (uint)(avgValue - (stdDev * 4));

                        for (int lineNum = 0; lineNum < collectedData.Count; lineNum++)
                        {
                            if (collectedData[lineNum][detectorNum].Value >= lowerBound && collectedData[lineNum][detectorNum].Value <= upperBound)
                            {
                                validAvgValue += collectedData[lineNum][detectorNum].Value;
                                validCount++;
                            }
                        }

                        if (validCount > 0)
                        {
                            ret[detectorNum] = validAvgValue / validCount;
                        }
                    }
                });
            }

            return ret;
        }

        private static Pixel[] ProcessData(List<Pixel[]> collectedData, bool processSTD)
        {
            Pixel[] ret = null;

            if (collectedData.Count > 0)
            {
                ret = new Pixel[collectedData[0].Length];

                Parallel.For(0, collectedData[0].Length, detectorNum =>
                {
                    uint avgValue = 0;

                    for (int line = 0; line < collectedData.Count; line++)
                    {
                        avgValue += collectedData[line][detectorNum].Value;
                    }

                    avgValue /= (uint)collectedData.Count;

                    ret[detectorNum] = new Pixel(avgValue);

                    if (processSTD)
                    {
                        double stdDev = 0;
                        uint validAvgValue = 0;
                        uint validCount = 0;

                        for (int line = 0; line < collectedData.Count; line++)
                        {
                            stdDev += Math.Pow((double)collectedData[line][detectorNum].Value - (double)avgValue, 2);
                        }

                        stdDev = Math.Sqrt(stdDev / collectedData.Count);

                        uint upperBound = (uint)(avgValue + (stdDev * 4));
                        uint lowerBound = (uint)(avgValue - (stdDev * 4));

                        for (int line = 0; line < collectedData.Count; line++)
                        {
                            if (collectedData[line][detectorNum].Value >= lowerBound && collectedData[line][detectorNum].Value <= upperBound)
                            {
                                validAvgValue += collectedData[line][detectorNum].Value;
                                validCount++;
                            }
                        }

                        if (validCount > 0)
                        {
                            ret[detectorNum] = new Pixel(validAvgValue / validCount);
                        }
                    }
                });
            }

            return ret;
        }

        private static float[] ProcessScaleFactor(Pixel[] airData, Pixel[] darkData)
        {
            float[] ret = null;

            if (airData == null)
            {
                throw new Exception("airData is null");
            }

            if (darkData == null)
            {
                throw new Exception("darkData is null");
            }

            if (airData.Length > 0 && darkData.Length > 0)
            {
                ret = new float[airData.Length];

                Parallel.For(0, ret.Length, i =>
                {
                    ret[i] = AppConfiguration.NormConstant / (float)(airData[i].Value - darkData[i].Value);
                });
            }

            return ret;
        }

        #endregion Private Methods


        #region Public Methods

        public void Add(PixelDataType dataType, Pixel[] data)
        {
            if (dataType == PixelDataType.Air)
            {
                if (AppConfiguration.CalibrationDataLines == _CollectedAirData.Count)
                {
                    _CollectedAirData.RemoveAt(0);
                }

                _CollectedAirData.Add(data);
                _IsAirProcessed = false;
                _IsScaleFactorProcessed = false;
            }
            else if (dataType == PixelDataType.Dark)
            {
                if (AppConfiguration.CalibrationDataLines == _CollectedDarkData.Count)
                {
                    _CollectedDarkData.RemoveAt(0);
                }

                _CollectedDarkData.Add(data);
                _IsDarkProcessed = false;
                _IsScaleFactorProcessed = false;
            }
        }

        public void AddReferenceData(double data)
        {
            _ReferenceData.Add(Convert.ToSingle(data));
        }

        public void ClearReferenceData()
        {
            _ReferenceData.Clear();
        }

        public void ClearAll()
        {
            for (int i = 0; i < _CollectedAirData.Count; i++ )
                _CollectedAirData.RemoveAt(i);

            for (int i = 0; i < _CollectedDarkData.Count; i++)
                _CollectedDarkData.RemoveAt(i);
        }

        #endregion Public Methods
    }
}
