using System;
using L3.Cargo.Common.Configurations;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Linac.DataAccessCore;
using L3.Cargo.Subsystem.StatusManagerCore;
using System.Collections.Generic;
using L3.Cargo.Communications.Linac;
using System.Threading;
using System.Collections;

namespace L3.Cargo.Detectors.StatusManagerCore
{
    public class LinacStatusManager: StatusManager
    {
        #region private members

        private const string _AFC_CF_Cutoff = "AFC CF Cutoff";
        private const string _AFC_Coarse = "_AFC_Coarse";
        private const string _AFC_Deadband = "AFC_Deadband";
        private const string _AFC_Fine = "AFC_Fine";
        private const string _AFC_PV = "AFC_PV";
        private const string _AFC_SP = "_AFC_SP";
        private const string _AFC_Update_Threshold = "_AFC_Update_Threshold";
        
        private const string _GD_Cathode_V_Set = "GD_Cathode_V_Set";
        private const string _GD_Filament_V_Set = "GD_Filament_V_Set";

        private const string _GD_CathodVoltageMonitor = "GD_CATHODE_VOLTAGE";
        private const string _GD_FilamentIMon = "GD_HEATER_CURRENT";
        private const string _GD_FilamentVMonitor = "GD_HEATER_VOLTAGE";

        private const string _GD_Grid_A_V_Mon = "GD_GRID_DRIVE_A_VOLTAGE";
        private const string _GD_Grid_B_V_Mon = "GD_GRID_DRIVE_B_VOLTAGE";
        private const string _GD_Grid_Bias_Mon = "GD_GRID_BIAS";
        private const string _GD_State_Mon = "GD_STATE";
        private const string _GD_Beam_I_Mon = "GD_BEAM_CURRENT";

        private const string _MD_HVPSVoltage = "MD_HVPS_VOLTAGE";
        private const string _MD_Filament_V_Mon = "MD_HEATER_VOLTAGE";
        private const string _MD_Filament_I_Mon = "MD_HEATER_CURRENT";
        private const string _Mod_State_Mon = "MD_STATE";
        private const string _Magnetron_I_Mon = "MAGNETRON_CURRENT";
        private const string _Sol_I_Mon = "SOLENOID_CURRENT";
        private const string _Sol_V_Mon = "SOLENOID_VOLTAGE";

        private const string _GD_Grid_A_V_Set = "GD_Grid_A_V_Set";
        private const string _GD_Grid_B_V_Set = "GD_Grid_B_V_Set";    
        
        private const string _Mod_Filament_V_Set = "Mod_Filament_V_Set";
        private const string _Mod_HVPS_V_Set = "Mod_HVPS_V_Set";
        private const string _Mod_PW_Set = "_Mod_PW_Set";
        private const string _PPG_E_Gun_Delay = "PPG_E_Gun_Delay";
        private const string _PPG_E_Gun_PW = "PPG_E_Gun_PW";
        private const string _PPG_Mod_Delay = "PPG_Mod_Delay";
        private const string _PPG_Mod_PW = "PPG_Mod_PW";
        private const string _PPG_PRF_Interleave = "PPG_PRF_Interleave";
        private const string _PPG_PRF_Single = "PPG_PRF_Single";
        private const string _PPG_Pulsewidth = "PPG_Pulsewidth";
        private const string _PPG_Sample_Delay = "PPG_Sample_Delay";
        private const string _PPG_Sample_Delay_AFC = "PPG_Sample_Delay_AFC";
        private const string _PPG_Sample_PW = "PPG_Sample_PW";
        private const string _PPG_Sample_PW_AFC = "PPG_Sample_PW_AFC";
        private const string _Sol_I_Set = "Sol_I_Set";
        private const string _Sol_V_Set = "Sol_V_Set";

        private const string _State = "State";
        private const string _Steering_Y_I_Set = "Steering_Y_I_Set";
        private const string _Steering_X_I_Set = "Steering_X_I_Set";
        private const string _Stepper_Derivative = "Stepper_Derivative";
        private const string _Stepper_Integral = "Stepper_Integral";
        private const string _Stepper_Proportional = "Stepper_Proportional";
        private const string _Stepper_PV = "Stepper_PV";
        private const string _Stepper_SP = "Stepper_SP";
        private const string _Warm_Up_Timer_Mon = "Warm_Up_Timer_Mon";
        private const string _Warm_Up_Timer_Set = "Warm_Up_Timer_Set";

        private const string _Steering_X_I_Mon = "STEERING_X_CURRENT";
        private const string _Steering_Y_I_Mon = "STEERING_Y_CURRENT";
        private const string _REFLECTED_POWER = "REFLECTED_POWER";
        private const string _FORWARD_POWER = "FORWARD_POWER";

        private const string _IONPUMP_1_CURRENT = "IONPUMP_1_CURRENT";
        private const string _IONPUMP_2_CURRENT = "IONPUMP_2_CURRENT";
        private const string _IONPUMP_3_CURRENT = "IONPUMP_3_CURRENT";

        private const string _IONPUMP_1_VOLTAGE = "IONPUMP_1_VOLTAGE";
        private const string _IONPUMP_2_VOLTAGE = "IONPUMP_2_VOLTAGE";
        private const string _IONPUMP_3_VOLTAGE = "IONPUMP_3_VOLTAGE";

        private const string _MAGNETRON_CURRENT_STATUS = "MAGNETRON_CURRENT_STATUS";
        private const string _REFLECTED_RF_POWER_STATUS = "REFLECTED_RF_POWER_STATUS";
        private const string _WAVEGUIDE_ARC_STATUS = "WAVEGUIDE_ARC_STATUS";
        private const string _SF6_PRESSURE_STATUS = "SF6_PRESSURE_STATUS";
        private const string _ENCLOSURE_PANELS_STATUS = "ENCLOSURE_PANELS_STATUS";
        private const string _ACCELERATOR_VACUUM_STATUS = "ACCELERATOR_VACUUM_STATUS";
        private const string _MAGNETRON_VACUUM_STATUS = "MAGNETRON_VACUUM_STATUS";
        private const string _SOLENOID_CURRENT_STATUS = "SOLENOID_CURRENT_STATUS";
        private const string _ACCELERATOR_FLOW_STATUS = "ACCELERATOR_FLOW_STATUS";
        private const string _COOLANT_TEMPERATURE_STATUS = "COOLANT_TEMPERATURE_STATUS";
        private const string _ENCLOSURE_TEMPERATURE_STATUS = "ENCLOSURE_TEMPERATURE_STATUS";
        private const string _GUN_DRIVER_INTERLOCK_STATUS = "GUN_DRIVER_INTERLOCK_STATUS";
        private const string _MODULATOR_INTERLOCK_STATUS = "MODULATOR_INTERLOCK_STATUS";
        private const string _CUSTOMER_INTERLOCK1_STATUS = "CUSTOMER_INTERLOCK1_STATUS";
        private const string _CUSTOMER_INTERLOCK2_STATUS = "CUSTOMER_INTERLOCK2_STATUS";
        private const string _MODULATOR_COMMUNICATION_ERROR_STATUS = "MODULATOR_COMMUNICATION_ERROR_STATUS";
        private const string _AFC_STEPPER_MOTOR_OUT_OF_BOUND_STATUS = "AFC_STEPPER_MOTOR_OUT_OF_BOUND_STATUS";
        private const string LINAC_ETHERNET_DISCONNECTED = "LINAC_ETHERNET_DISCONNECTED";
        
        private Thread _StatesThread;

        private LinacDataAccess _dataAccess;

        private LinacAccess.ProcessCommandEventHandler _processCommandEventHandler;

        #endregion


        #region public members

        #endregion


        #region constructors

        public LinacStatusManager (LinacDataAccess dataAccess, EventLoggerAccess logger) :
            base(dataAccess, logger)
        {
            InitializeLinacTags();

            _dataAccess = dataAccess;

            //dataAccess.DetectorConnectionStateUpdate += new ConnectionStateChangeHandler(DataAccessDetectors_ConnectionStateUpdate);
            //dataAccess.APCSConnectionStateUpdate += new ConnectionStateChangeHandler(DataAccessAPCS_ConnectionStateUpdate);

            dataAccess.LinacConnectionStateChangeEvent += new ConnectionStateChangeHandler(dataAccess_LinacConnectionStateChangeEvent);            
        }        

        #endregion


        #region Private Methods

        private void dataAccess_LinacConnectionStateChangeEvent(bool isConnected)
        {
            if (isConnected)
            {
                if (_StatesThread == null)
                {
                    _StatesThread = new Thread(new ThreadStart(ProcessLinacStates));
                    _StatesThread.Start();
                }
            }
            else
            {
                _dataAccess.Linac.ProcessCommandEvent -= _processCommandEventHandler;
                _processCommandEventHandler -= Linac_ProcessCommandEvent;
                _StatesThread.Abort();
                _StatesThread = null;
            }
        }

        private void ProcessLinacStates()
        {
            AutoResetEvent evt = new AutoResetEvent(false);
            _processCommandEventHandler += Linac_ProcessCommandEvent;
            _dataAccess.Linac.ProcessCommandEvent += _processCommandEventHandler;

              for (; ; )
              {
                  Dictionary<LinacPacketFormat.VariableIdEnum, object> parameterList = new Dictionary<LinacPacketFormat.VariableIdEnum, object>();
                  _dataAccess.Linac.GetOperatingParameters(ref parameterList);

                  processUpdate(parameterList);

                  evt.WaitOne(5000);
              }

        }

        private void Linac_ProcessCommandEvent(Dictionary<LinacPacketFormat.VariableIdEnum, object> commandList)
        {
            processUpdate(commandList);
        }

        private void processUpdate(Dictionary<LinacPacketFormat.VariableIdEnum, object> parameterList)
        {
            string name;
            int intVal = 0;
            byte[] dataValue;

            if (parameterList == null)
            {
                // this sets the values all back to defaults
                InitializeLinacTags();
                SendDisplayUpdate();
                return;
            }

            foreach (LinacPacketFormat.VariableIdEnum id in parameterList.Keys)
            {
                name = string.Empty;
                dataValue = null;
                dataValue = (byte[])parameterList[id];

                switch (id)
                {
                    case LinacPacketFormat.VariableIdEnum.AFC_CF_Cutoff:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _AFC_CF_Cutoff;
                        break;
                    case LinacPacketFormat.VariableIdEnum.AFC_Coarse:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _AFC_Coarse;
                        break;
                    case LinacPacketFormat.VariableIdEnum.AFC_Deadband:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _AFC_Deadband;
                        break;
                    case LinacPacketFormat.VariableIdEnum.AFC_Fine:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _AFC_Fine;
                        break;
                    case LinacPacketFormat.VariableIdEnum.AFC_PV:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _AFC_PV;
                        break;
                    case LinacPacketFormat.VariableIdEnum.AFC_SP:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _AFC_SP;
                        break;
                    case LinacPacketFormat.VariableIdEnum.AFC_Update_Threshold:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _AFC_Update_Threshold;
                        break;
                    case LinacPacketFormat.VariableIdEnum.Forward_Power:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _FORWARD_POWER;
                        break;
                    case LinacPacketFormat.VariableIdEnum.GD_Beam_I_Mon:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _GD_Beam_I_Mon;
                        break;
                    case LinacPacketFormat.VariableIdEnum.GD_Cathode_V_Set:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _GD_Cathode_V_Set;
                        break;
                    case LinacPacketFormat.VariableIdEnum.GD_Cathod_V_Monitor:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name =_GD_CathodVoltageMonitor;
                        break;
                    case LinacPacketFormat.VariableIdEnum.GD_Filament_V_Set:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _GD_Filament_V_Set;
                        break;
                    case LinacPacketFormat.VariableIdEnum.GD_Filament_I_Mon:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _GD_FilamentIMon;
                        break;
                    case LinacPacketFormat.VariableIdEnum.GD_Filament_V_Monitor:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _GD_FilamentVMonitor;
                        break;
                    case LinacPacketFormat.VariableIdEnum.GD_Grid_A_V_Mon:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _GD_Grid_A_V_Mon;
                        break;
                    case LinacPacketFormat.VariableIdEnum.GD_Grid_A_V_Set:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _GD_Grid_A_V_Set;
                        break;
                    case LinacPacketFormat.VariableIdEnum.GD_Grid_B_V_Mon:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _GD_Grid_B_V_Mon;                        
                        break;
                    case LinacPacketFormat.VariableIdEnum.GD_Grid_B_V_Set:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _GD_Grid_B_V_Set;
                        break;
                    case LinacPacketFormat.VariableIdEnum.GD_Grid_Bias_Mon:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _GD_Grid_Bias_Mon;
                        break;
                    case LinacPacketFormat.VariableIdEnum.IonPump_1_I_Mon:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _IONPUMP_1_CURRENT;
                        break;
                    case LinacPacketFormat.VariableIdEnum.IonPump_1_V_Mon:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _IONPUMP_1_VOLTAGE;
                        break;
                    case LinacPacketFormat.VariableIdEnum.IonPump_2_I_Mon:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _IONPUMP_2_CURRENT;
                        break;
                    case LinacPacketFormat.VariableIdEnum.IonPump_2_V_Mon:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _IONPUMP_2_VOLTAGE;
                        break;
                    case LinacPacketFormat.VariableIdEnum.IonPump_3_I_Mon:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _IONPUMP_3_CURRENT;
                        break;
                    case LinacPacketFormat.VariableIdEnum.IonPump_3_V_Mon:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _IONPUMP_3_VOLTAGE;
                        break;
                    case LinacPacketFormat.VariableIdEnum.Magnetron_I_Mon:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _Magnetron_I_Mon;
                        break;
                    case LinacPacketFormat.VariableIdEnum.MD_Filament_I_Mon:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _MD_Filament_I_Mon;
                        break;
                    case LinacPacketFormat.VariableIdEnum.MD_Filament_V_Mon:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _MD_Filament_V_Mon;
                        break;
                    case LinacPacketFormat.VariableIdEnum.MD_HVPSVoltage:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _MD_HVPSVoltage;
                        break;
                    case LinacPacketFormat.VariableIdEnum.Mod_Filament_V_Set:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _Mod_Filament_V_Set;
                        break;
                    case LinacPacketFormat.VariableIdEnum.Mod_HVPS_V_Set:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _Mod_HVPS_V_Set;                        
                        break;
                    case LinacPacketFormat.VariableIdEnum.Mod_PW_Set:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _Mod_PW_Set;
                        break;
                    case LinacPacketFormat.VariableIdEnum.PPG_E_Gun_Delay:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _PPG_E_Gun_Delay;
                        break;
                    case LinacPacketFormat.VariableIdEnum.PPG_E_Gun_PW:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _PPG_E_Gun_PW;
                        break;
                    case LinacPacketFormat.VariableIdEnum.PPG_Mod_Delay:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _PPG_Mod_Delay;
                        break;
                    case LinacPacketFormat.VariableIdEnum.PPG_Mod_PW:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _PPG_Mod_PW;
                        break;
                    case LinacPacketFormat.VariableIdEnum.PPG_PRF_Interleave:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _PPG_PRF_Interleave;
                        break;
                    case LinacPacketFormat.VariableIdEnum.PPG_PRF_Single:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _PPG_PRF_Single;
                        break;
                    case LinacPacketFormat.VariableIdEnum.PPG_Pulsewidth:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _PPG_Pulsewidth;
                        break;
                    case LinacPacketFormat.VariableIdEnum.PPG_Sample_Delay:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _PPG_Sample_Delay;
                        break;
                    case LinacPacketFormat.VariableIdEnum.PPG_Sample_Delay_AFC:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _PPG_Sample_Delay_AFC;
                        break;
                    case LinacPacketFormat.VariableIdEnum.PPG_Sample_PW:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _PPG_Sample_PW;
                        break;
                    case LinacPacketFormat.VariableIdEnum.PPG_Sample_PW_AFC:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _PPG_Sample_PW_AFC;
                        break;
                    case LinacPacketFormat.VariableIdEnum.Reflected_Power:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _REFLECTED_POWER;
                        break;
                    case LinacPacketFormat.VariableIdEnum.Sol_I_Mon:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _Sol_I_Mon;
                        break;
                    case LinacPacketFormat.VariableIdEnum.Sol_I_Set:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _Sol_I_Set;
                        break;
                    case LinacPacketFormat.VariableIdEnum.Sol_V_Mon:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _Sol_V_Mon;
                        break;
                    case LinacPacketFormat.VariableIdEnum.Sol_V_Set:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _Sol_V_Set;
                        break;
                    case LinacPacketFormat.VariableIdEnum.Steering_X_I_Mon:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _Steering_X_I_Mon;
                        break;
                    case LinacPacketFormat.VariableIdEnum.Steering_X_I_Set:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _Steering_X_I_Set;
                        break;
                    case LinacPacketFormat.VariableIdEnum.Steering_Y_I_Mon:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _Steering_Y_I_Mon;                        
                        break;
                    case LinacPacketFormat.VariableIdEnum.Steering_Y_I_Set:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _Steering_Y_I_Set;
                        break;
                    case LinacPacketFormat.VariableIdEnum.Stepper_Derivative:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _Stepper_Derivative;
                        break;
                    case LinacPacketFormat.VariableIdEnum.Stepper_Integral:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _Stepper_Integral;
                        break;
                    case LinacPacketFormat.VariableIdEnum.Stepper_Proportional:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _Stepper_Proportional;
                        break;
                    case LinacPacketFormat.VariableIdEnum.Stepper_PV:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _Stepper_PV;
                        break;
                    case LinacPacketFormat.VariableIdEnum.Stepper_SP:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _Stepper_SP;
                        break;
                    case LinacPacketFormat.VariableIdEnum.Warm_Up_Timer_Mon:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _Warm_Up_Timer_Mon;
                        break;
                    case LinacPacketFormat.VariableIdEnum.Warm_Up_Timer_Set:
                        intVal = GetFloatAsAdjustedInt(dataValue);
                        name = _Warm_Up_Timer_Set;
                        break;
                    case LinacPacketFormat.VariableIdEnum.State:
                        intVal = BitConverter.ToInt32(dataValue, 0);
                        name = _State;
                        break;
                    case LinacPacketFormat.VariableIdEnum.Mod_State_Mon:
                        intVal = BitConverter.ToInt32(dataValue, 0);
                        name = _Mod_State_Mon;
                        break;
                    case LinacPacketFormat.VariableIdEnum.GD_State_Mon:
                        intVal = BitConverter.ToInt32(dataValue, 0);
                        name = _GD_State_Mon;
                        break;
                    case LinacPacketFormat.VariableIdEnum.Fault:
                        // this is a decimal
                        byte[] dataValue62 = (byte[]) parameterList[id];
                        int faultCode62 = BitConverter.ToInt32(dataValue62, 0);
                        UpdateFaultStatus(faultCode62);
                        break;
                    default:
                        break;
                }

                if (!string.IsNullOrEmpty(name))
                {
                    base.ProcessHardwareStatusUpdate(name, intVal);
                }
            }

        }

        private static int GetFloatAsAdjustedInt(byte[] dataValue)
        {
            float floatVal = BitConverter.ToSingle(dataValue, 0);

            // scale the float into an integer to satisfy the interface
            int intVal = Convert.ToInt32(floatVal * 100.0);
            return intVal;
        }

        private void UpdateFaultStatus(int faultValue)
        {
            string name = string.Empty;
            int value;

            _Statuses.Find(LINAC_ETHERNET_DISCONNECTED).Value = 0;

            string[] faultNames = Enum.GetNames(typeof(LinacPacketFormat.Faults));
            
            foreach (var faultName in faultNames)
            {
                LinacPacketFormat.Faults fault = (LinacPacketFormat.Faults)Enum.Parse(typeof(LinacPacketFormat.Faults), faultName);


                if (fault == LinacPacketFormat.Faults.MagnetronCurrent)
                {
                    name = _MAGNETRON_CURRENT_STATUS;
                }
                else if (fault == LinacPacketFormat.Faults.ReflectedRFPower)
                {
                    name = _REFLECTED_RF_POWER_STATUS;
                }
                else if (fault == LinacPacketFormat.Faults.WaveguideArc)
                {
                    name = _WAVEGUIDE_ARC_STATUS;
                }
                else if (fault == LinacPacketFormat.Faults.SF6Pressure)
                {
                    name = _SF6_PRESSURE_STATUS;
                }
                else if (fault == LinacPacketFormat.Faults.EnclosurePanels)
                {
                    name = _ENCLOSURE_PANELS_STATUS;
                }
                else if (fault == LinacPacketFormat.Faults.AcceleratorFlowFault)
                {
                    name = _ACCELERATOR_FLOW_STATUS;
                }
                else if (fault == LinacPacketFormat.Faults.MagnetronVacuum)
                {
                    name = _MAGNETRON_VACUUM_STATUS;
                }
                else if (fault == LinacPacketFormat.Faults.SolenoidCurrent)
                {
                    name = _SOLENOID_CURRENT_STATUS;
                }
                else if (fault == LinacPacketFormat.Faults.AcceleratorFlowFault)
                {
                    name = _ACCELERATOR_FLOW_STATUS;
                }
                else if (fault == LinacPacketFormat.Faults.CoolantTemperature)
                {
                    name = _COOLANT_TEMPERATURE_STATUS;
                }
                else if (fault == LinacPacketFormat.Faults.EnclosureTemperature)
                {
                    name = _ENCLOSURE_TEMPERATURE_STATUS;
                }
                else if (fault == LinacPacketFormat.Faults.GunDriverInterlock)
                {
                    name = _GUN_DRIVER_INTERLOCK_STATUS;
                }
                else if (fault == LinacPacketFormat.Faults.ModulatorInterlock)
                {
                    name = _MODULATOR_INTERLOCK_STATUS;
                }
                else if (fault == LinacPacketFormat.Faults.CustomerInterlock1)
                {
                    name = _CUSTOMER_INTERLOCK1_STATUS;
                }
                else if (fault == LinacPacketFormat.Faults.CustomerInterlock2)
                {
                    name = _CUSTOMER_INTERLOCK2_STATUS;
                }
                else if (fault == LinacPacketFormat.Faults.ModulatorCommunicationError)
                {
                    name = _MODULATOR_COMMUNICATION_ERROR_STATUS;
                }
                else if (fault == LinacPacketFormat.Faults.AFCStepperMotorOutOfBound)
                {
                    name = _AFC_STEPPER_MOTOR_OUT_OF_BOUND_STATUS;
                }
                else
                {
                    name = string.Empty;
                }

                if (!string.IsNullOrEmpty(name))
                {
                    if ((faultValue & (uint)fault) != 0)
                    {
                        value = 1;
                    }
                    else
                    {
                        value = 0;
                    }

                    StatusElement element = _Statuses.Find(name);

                    if (element != null)
                    {
                        element.Value = value;
                    }
                }
            }

            base.SendStatusUpdate();
        }

        private void InitializeLinacTags()
        {
            _Statuses = new StatusElements();
            int emptyVal = int.MinValue;
            StatusElement statusElement = new StatusElement(_GD_Cathode_V_Set, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);

            statusElement = new StatusElement(_GD_CathodVoltageMonitor, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);
            statusElement = new StatusElement(_GD_FilamentVMonitor, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);
            statusElement = new StatusElement(_GD_FilamentIMon, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);
            statusElement = new StatusElement(_GD_Grid_A_V_Mon, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);
            statusElement = new StatusElement(_GD_Grid_B_V_Mon, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);
            statusElement = new StatusElement(_GD_Grid_Bias_Mon, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);
            statusElement = new StatusElement(_GD_Beam_I_Mon, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);
            statusElement = new StatusElement(_GD_State_Mon, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);

            statusElement = new StatusElement(_MD_HVPSVoltage, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);
            statusElement = new StatusElement(_MD_Filament_I_Mon, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);
            statusElement = new StatusElement(_MD_Filament_V_Mon, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);
            statusElement = new StatusElement(_Mod_State_Mon, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);
            statusElement = new StatusElement(_Magnetron_I_Mon, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);
            statusElement = new StatusElement(_Sol_I_Mon, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);
            statusElement = new StatusElement(_Sol_V_Mon, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);


            statusElement = new StatusElement(_Steering_X_I_Mon, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);
            statusElement = new StatusElement(_Steering_Y_I_Mon, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);
            statusElement = new StatusElement(_IONPUMP_1_CURRENT, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);
            statusElement = new StatusElement(_IONPUMP_2_CURRENT, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);
            statusElement = new StatusElement(_IONPUMP_3_CURRENT, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);
            statusElement = new StatusElement(_IONPUMP_1_VOLTAGE, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);
            statusElement = new StatusElement(_IONPUMP_2_VOLTAGE, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);
            statusElement = new StatusElement(_IONPUMP_3_VOLTAGE, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);
            //statusElement = new StatusElement(_TCU_TEMPERATURE, emptyVal, TagTypes.Information);
            //_Statuses.Add(statusElement);
            statusElement = new StatusElement(_REFLECTED_POWER, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);
            statusElement = new StatusElement(_FORWARD_POWER, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);
            statusElement = new StatusElement(_AFC_PV, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);
            statusElement = new StatusElement(_Stepper_PV, emptyVal, TagTypes.Information);
            _Statuses.Add(statusElement);

            Dictionary<int, string> valueMapping = new Dictionary<int, string>();
            valueMapping.Add(0, "Clear");
            valueMapping.Add(1, "Error");

            statusElement = new StatusElement(_MAGNETRON_CURRENT_STATUS, 0, TagTypes.Status, valueMapping);
            _Statuses.Add(statusElement);

            statusElement = new StatusElement(_REFLECTED_RF_POWER_STATUS, 0, TagTypes.Status, valueMapping);
            _Statuses.Add(statusElement);

            statusElement = new StatusElement(_WAVEGUIDE_ARC_STATUS, 0, TagTypes.Status, valueMapping);
            _Statuses.Add(statusElement);

            statusElement = new StatusElement(_SF6_PRESSURE_STATUS, 0, TagTypes.Status, valueMapping);
            _Statuses.Add(statusElement);

            statusElement = new StatusElement(_ENCLOSURE_PANELS_STATUS, 0, TagTypes.Status, valueMapping);
            _Statuses.Add(statusElement);

            statusElement = new StatusElement(_ACCELERATOR_VACUUM_STATUS, 0, TagTypes.Status, valueMapping);
            _Statuses.Add(statusElement);

            statusElement = new StatusElement(_MAGNETRON_VACUUM_STATUS, 0, TagTypes.Status, valueMapping);
            _Statuses.Add(statusElement);

            statusElement = new StatusElement(_SOLENOID_CURRENT_STATUS, 0, TagTypes.Status, valueMapping);
            _Statuses.Add(statusElement);

            statusElement = new StatusElement(_ACCELERATOR_FLOW_STATUS, 0, TagTypes.Status, valueMapping);
            _Statuses.Add(statusElement);

            statusElement = new StatusElement(_COOLANT_TEMPERATURE_STATUS, 0, TagTypes.Status, valueMapping);
            _Statuses.Add(statusElement);

            statusElement = new StatusElement(_ENCLOSURE_TEMPERATURE_STATUS, 0, TagTypes.Status, valueMapping);
            _Statuses.Add(statusElement);

            statusElement = new StatusElement(_GUN_DRIVER_INTERLOCK_STATUS, 0, TagTypes.Status, valueMapping);
            _Statuses.Add(statusElement);

            statusElement = new StatusElement(_MODULATOR_INTERLOCK_STATUS, 0, TagTypes.Status, valueMapping);
            _Statuses.Add(statusElement);

            statusElement = new StatusElement(_CUSTOMER_INTERLOCK1_STATUS, 0, TagTypes.Status, valueMapping);
            _Statuses.Add(statusElement);

            statusElement = new StatusElement(_CUSTOMER_INTERLOCK2_STATUS, 0, TagTypes.Status, valueMapping);
            _Statuses.Add(statusElement);

            statusElement = new StatusElement(_MODULATOR_COMMUNICATION_ERROR_STATUS, 0, TagTypes.Status, valueMapping);
            _Statuses.Add(statusElement);

            statusElement = new StatusElement(_AFC_STEPPER_MOTOR_OUT_OF_BOUND_STATUS, 0, TagTypes.Status, valueMapping);
            _Statuses.Add(statusElement);

            statusElement = new StatusElement(LINAC_ETHERNET_DISCONNECTED, 1, TagTypes.Status, valueMapping);
            _Statuses.Add(statusElement);

            ReadTagConfig();
        }

        #endregion Private Methods


        #region public members

        #endregion
    }
}
