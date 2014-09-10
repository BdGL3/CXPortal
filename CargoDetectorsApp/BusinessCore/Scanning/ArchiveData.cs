using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using L3.Cargo.Common.PxeAccess;
using L3.Cargo.Communications.APCS.Common;
using L3.Cargo.Communications.Detectors.Common;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Communications.OPC.Portal;
using L3.Cargo.Detectors.Common;

namespace L3.Cargo.Detectors.BusinessCore
{
    public class ArchiveData
    {
        #region Private Members

        private EventLoggerAccess _log;

        private Calibration _calibration;

        private PxeWriteAccess _pxeWriteAccess;

        private const string _dateFormat = "yyMMddHHmmssfff";

        private const string _pxeExtension = ".pxe";

        private object _writeLock = new object();

        #endregion Private Members


        #region Constructors

        public ArchiveData(EventLoggerAccess log, Calibration calibration)
        {
            _log = log;
            _calibration = calibration;
            _pxeWriteAccess = new PxeWriteAccess();
        }

        #endregion Constructors


        #region Public methods

        public string CreatePXEFile(LINAC_ENERGY_TYPE_VALUE energy, PulseWidth pulseWidth, List<DataInfo> ObjectLines)
        {
            lock (_writeLock)
            {
                string pxeFile = Path.Combine(AppConfiguration.HostTempFileLocation, DateTime.Now.ToString(_dateFormat) + _pxeExtension);
                _pxeWriteAccess.CreatePXE(pxeFile);

                try
                {
                    List<Pixel[]> highEnergyLines = new List<Pixel[]>();
                    List<Pixel[]> lowEnergyLines = new List<Pixel[]>();
                    XRayInfoIDStruct highEnergyInfo = default(XRayInfoIDStruct);
                    XRayInfoIDStruct lowEnergyInfo = default(XRayInfoIDStruct);

                    if (energy == LINAC_ENERGY_TYPE_VALUE.Dual)
                    {
                        if (ObjectLines[0].XRayInfo.Energy == XRayEnergyEnum.HighEnergy)
                            ObjectLines.RemoveAt(0);
                    }

                    //store object line data
                    foreach (DataInfo dataLine in ObjectLines)
                    {
                        if (dataLine.XRayInfo.Energy == XRayEnergyEnum.HighEnergy)
                        {
                            highEnergyLines.Add(dataLine.LineData);
                            highEnergyInfo = dataLine.XRayInfo;
                        }
                        else //low energy
                        {
                            lowEnergyLines.Add(dataLine.LineData);
                            lowEnergyInfo = dataLine.XRayInfo;
                        }
                    }

                    int maxLength = Math.Min(lowEnergyLines.Count, highEnergyLines.Count);

                    if (maxLength == 0)
                    {
                        maxLength = Math.Max(lowEnergyLines.Count, highEnergyLines.Count);
                    }
                    
                    if (maxLength > AppConfiguration.MaxPXEWidth)
                    {
                        maxLength = AppConfiguration.MaxPXEWidth;
                    }

                    if (energy != LINAC_ENERGY_TYPE_VALUE.Low && highEnergyLines.Count > 0)
                    {
                        int bufferSize = maxLength * highEnergyLines[0].Length;

                        if (bufferSize > 0)
                        {
                            float[] buffer = new float[bufferSize];
                            Parallel.For(highEnergyLines.Count - maxLength, maxLength, index =>
                            {
                                float[] pixelArray = PixelConverter.Convert(highEnergyLines[index]);
                                int length = Buffer.ByteLength(pixelArray);
                                Buffer.BlockCopy(pixelArray, 0, buffer, index * length, length);
                            });


                            if (energy == LINAC_ENERGY_TYPE_VALUE.Dual)
                            {
                                _pxeWriteAccess.CreateHiPXEHeader((uint)maxLength, (uint)highEnergyLines[0].Length);
                                _pxeWriteAccess.WriteHiDataLines(buffer, (uint)maxLength);
                                _pxeWriteAccess.WriteHighEngDarkSample(PixelConverter.Convert(_calibration.GetDarkData(highEnergyInfo)));
                                _pxeWriteAccess.WriteHighEngAirSample(PixelConverter.Convert(_calibration.GetAirData(highEnergyInfo)));

                                if (AppConfiguration.StoreReferenceCorrection)
                                {
                                    _pxeWriteAccess.WriteHiRef(_calibration.GetReferenceCorrections(highEnergyInfo));
                                }

                                if (AppConfiguration.StoreScaleFactor)
                                {
                                    _pxeWriteAccess.WriteHiScaleFactor(_calibration.GetScaleFactor(highEnergyInfo));
                                }

                                if (AppConfiguration.StoreAirDarkSamples)
                                {
                                    _pxeWriteAccess.WriteHiFullAirData(PixelConverter.Convert(_calibration.GetAirDataCollection(highEnergyInfo)), Convert.ToUInt32(AppConfiguration.CalibrationDataLines));
                                    _pxeWriteAccess.WriteHiFullDarkData(PixelConverter.Convert(_calibration.GetDarkDataCollection(highEnergyInfo)), Convert.ToUInt32(AppConfiguration.CalibrationDataLines));
                                }
                            }
                            else
                            {
                                _pxeWriteAccess.CreateHiPXEHeader((uint)maxLength, (uint)highEnergyLines[0].Length);
                                _pxeWriteAccess.WriteHiDataLines(buffer, (uint)maxLength);
                                _pxeWriteAccess.WriteHighEngDarkSample(PixelConverter.Convert(_calibration.GetDarkData(highEnergyInfo)));
                                _pxeWriteAccess.WriteHighEngAirSample(PixelConverter.Convert(_calibration.GetAirData(highEnergyInfo)));

                                if (AppConfiguration.StoreReferenceCorrection)
                                {
                                    _pxeWriteAccess.WriteHiRef(_calibration.GetReferenceCorrections(highEnergyInfo));
                                }

                                if (AppConfiguration.StoreScaleFactor)
                                {
                                    _pxeWriteAccess.WriteHiScaleFactor(_calibration.GetScaleFactor(highEnergyInfo));
                                }

                                if (AppConfiguration.StoreAirDarkSamples)
                                {
                                    _pxeWriteAccess.WriteHiFullAirData(PixelConverter.Convert(_calibration.GetAirDataCollection(highEnergyInfo)), Convert.ToUInt32(AppConfiguration.CalibrationDataLines));
                                    _pxeWriteAccess.WriteHiFullDarkData(PixelConverter.Convert(_calibration.GetDarkDataCollection(highEnergyInfo)), Convert.ToUInt32(AppConfiguration.CalibrationDataLines));
                                }
                            }
                        }

                        highEnergyLines.Clear();
                    }

                    if (energy != LINAC_ENERGY_TYPE_VALUE.High && lowEnergyLines.Count > 0)
                    {
                        int bufferSize = maxLength * lowEnergyLines[0].Length;

                        if (bufferSize > 0)
                        {
                            float[] buffer = new float[bufferSize];
                            Parallel.For(0, maxLength, index =>
                            {
                                float[] pixelArray = PixelConverter.Convert(lowEnergyLines[index]);
                                int length = Buffer.ByteLength(pixelArray);
                                Buffer.BlockCopy(pixelArray, 0, buffer, index * length, length);
                            });

                            if (energy == LINAC_ENERGY_TYPE_VALUE.Dual)
                            {
                                _pxeWriteAccess.CreateLoPXEHeader((uint)maxLength, (uint)lowEnergyLines[0].Length);
                                _pxeWriteAccess.WriteLoDataLines(buffer, (uint)maxLength);
                                _pxeWriteAccess.WriteLowEngDarkSample(PixelConverter.Convert(_calibration.GetDarkData(lowEnergyInfo)));
                                _pxeWriteAccess.WriteLowEngAirSample(PixelConverter.Convert(_calibration.GetAirData(lowEnergyInfo)));

                                if (AppConfiguration.StoreReferenceCorrection)
                                {
                                    _pxeWriteAccess.WriteLoRef(_calibration.GetReferenceCorrections(lowEnergyInfo));
                                }

                                if (AppConfiguration.StoreScaleFactor)
                                {
                                    _pxeWriteAccess.WriteLoScaleFactor(_calibration.GetScaleFactor(lowEnergyInfo));
                                }

                                if (AppConfiguration.StoreAirDarkSamples)
                                {
                                    _pxeWriteAccess.WriteLoFullAirData(PixelConverter.Convert(_calibration.GetAirDataCollection(lowEnergyInfo)), Convert.ToUInt32(AppConfiguration.CalibrationDataLines));
                                    _pxeWriteAccess.WriteLoFullDarkData(PixelConverter.Convert(_calibration.GetDarkDataCollection(lowEnergyInfo)), Convert.ToUInt32(AppConfiguration.CalibrationDataLines));
                                }
                            }
                            else
                            {
                                _pxeWriteAccess.CreateLoPXEHeader((uint)maxLength, (uint)lowEnergyLines[0].Length);
                                _pxeWriteAccess.WriteLoDataLines(buffer, (uint)maxLength);
                                _pxeWriteAccess.WriteLowEngDarkSample(PixelConverter.Convert(_calibration.GetDarkData(lowEnergyInfo)));
                                _pxeWriteAccess.WriteLowEngAirSample(PixelConverter.Convert(_calibration.GetAirData(lowEnergyInfo)));

                                if (AppConfiguration.StoreReferenceCorrection)
                                {
                                    _pxeWriteAccess.WriteLoRef(_calibration.GetReferenceCorrections(lowEnergyInfo));
                                }

                                if (AppConfiguration.StoreScaleFactor)
                                {
                                    _pxeWriteAccess.WriteLoScaleFactor(_calibration.GetScaleFactor(lowEnergyInfo));
                                }

                                if (AppConfiguration.StoreAirDarkSamples)
                                {
                                    _pxeWriteAccess.WriteLoFullAirData(PixelConverter.Convert(_calibration.GetAirDataCollection(lowEnergyInfo)), Convert.ToUInt32(AppConfiguration.CalibrationDataLines));
                                    _pxeWriteAccess.WriteLoFullDarkData(PixelConverter.Convert(_calibration.GetDarkDataCollection(lowEnergyInfo)), Convert.ToUInt32(AppConfiguration.CalibrationDataLines));
                                }
                            }
                        }

                        lowEnergyLines.Clear();
                    }
                }
                catch { }

                try
                {
                    _pxeWriteAccess.ClosePXEWrite();
                }
                catch (Exception e)
                {
                    _log.LogError("Exception closing PXEWrite.");
                    _log.LogError(e.GetType().ToString() + ": " + e.Message);
                    _log.LogError(e.StackTrace);
                }
                return pxeFile;
            }
        }

        #endregion
    }
}
