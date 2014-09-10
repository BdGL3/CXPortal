using System;
using L3.Cargo.Communications.OPC;
using L3.Cargo.Communications.OPC.Portal;

namespace L3.Cargo.Detectors.DataAccessCore
{
    public class OpcTags
    {
        #region Public Members

        public OpcTagBase<bool> CALIBRATION_SCAN;

        public OpcTagBase<CALIBRATION_STATE_VALUE> CALIBRATION_STATE;

        public OpcTagBase<SCAN_DRIVE_STATE_VALUE> SCAN_DRIVE_STATE;

        public OpcTagBase<LINAC_ENERGY_TYPE_VALUE> LINAC_ENERGY_TYPE_STATE;

        public OpcTagBase<LINAC_ENERGY_TYPE_VALUE> LINAC_ENERGY_TYPE;

        public OpcTagBase<LINAC_STATE_VALUE> LINAC_STATE;

        public OpcTagBase<bool> LINAC_TURN_ON_XRAYS;

        public OpcTagBase<bool> START_SCAN;

        public OpcTagBase<bool> HOST_STOP_SCAN;

        public OpcTagBase<bool> SCAN_DRIVE_HAND_BRAKE;

        public OpcTagBase<bool> SCAN_AREA_CLEAR;

        public bool IgnoreOpcUpdatesForXrays = false;

        #endregion Public Members


        #region Constructors

        public OpcTags()
        {
            CALIBRATION_SCAN = new OpcTagBase<bool>("CALIBRATION_SCAN");
            CALIBRATION_STATE = new OpcTagBase<CALIBRATION_STATE_VALUE>("CALIBRATION_STATE");
            SCAN_DRIVE_STATE = new OpcTagBase<SCAN_DRIVE_STATE_VALUE>("SCAN_DRIVE_STATE");
            LINAC_ENERGY_TYPE_STATE = new OpcTagBase<LINAC_ENERGY_TYPE_VALUE>("LINAC_ENERGY_TYPE_STATE");
            LINAC_ENERGY_TYPE = new OpcTagBase<LINAC_ENERGY_TYPE_VALUE>("LINAC_ENERGY_TYPE");
            LINAC_STATE = new OpcTagBase<LINAC_STATE_VALUE>("LINAC_STATE");
            LINAC_TURN_ON_XRAYS = new OpcTagBase<bool>("LINAC_TURN_ON_XRAYS");
            START_SCAN = new OpcTagBase<bool>("START_SCAN");
            HOST_STOP_SCAN = new OpcTagBase<bool>("HOST_STOP_SCAN");
            SCAN_DRIVE_HAND_BRAKE = new OpcTagBase<bool>("SCAN_DRIVE_HAND_BRAKE");
            SCAN_AREA_CLEAR = new OpcTagBase<bool>("SCAN_AREA_CLEAR");
        }

        #endregion Constructors

        internal void DataAccess_TagUpdate(string name, int value)
        {
            if (name == CALIBRATION_SCAN.Name)
            {
                CALIBRATION_SCAN.Value = Convert.ToBoolean(value);
            }
            else if (name == CALIBRATION_STATE.Name)
            {
                CALIBRATION_STATE.Value = (CALIBRATION_STATE_VALUE)value;
            }
            else if (name == SCAN_DRIVE_STATE.Name)
            {
                SCAN_DRIVE_STATE.Value = (SCAN_DRIVE_STATE_VALUE)value;
            }
            else if (name == LINAC_ENERGY_TYPE_STATE.Name)
            {
                LINAC_ENERGY_TYPE_STATE.Value = (LINAC_ENERGY_TYPE_VALUE)value;
            }
            else if (name == LINAC_ENERGY_TYPE.Name)
            {
                LINAC_ENERGY_TYPE.Value = (LINAC_ENERGY_TYPE_VALUE)value;
            }
            else if (name == LINAC_STATE.Name && IgnoreOpcUpdatesForXrays == false)
            {
                LINAC_STATE.Value = (LINAC_STATE_VALUE)value;
            }
            else if (name == LINAC_TURN_ON_XRAYS.Name)
            {
                LINAC_TURN_ON_XRAYS.Value = Convert.ToBoolean(value);
            }
            else if (name == START_SCAN.Name)
            {
                START_SCAN.Value = Convert.ToBoolean(value);
            }
            else if (name == HOST_STOP_SCAN.Name)
            {
                HOST_STOP_SCAN.Value = Convert.ToBoolean(value);
            }
            else if (name == SCAN_DRIVE_HAND_BRAKE.Name)
            {
                SCAN_DRIVE_HAND_BRAKE.Value = Convert.ToBoolean(value);
            }
            else if (name == SCAN_AREA_CLEAR.Name)
            {
                SCAN_AREA_CLEAR.Value = Convert.ToBoolean(value);
            }
        }
    }
}
