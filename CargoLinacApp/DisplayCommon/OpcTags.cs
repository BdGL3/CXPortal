using L3.Cargo.Dashboard.Assembly.Common;

namespace L3.Cargo.Linac.Display.Common
{
    public class OpcTags
    {
        public static OpcTag LINAC_TURN_ON_XRAYS = new OpcTag("LINAC_TURN_ON_XRAYS", "LINAC_TURN_ON_XRAYS");
        public static OpcTag LINAC_ENERGY_TYPE = new OpcTag("LINAC_ENERGY_TYPE", "LINAC_ENERGY_TYPE");
        public static OpcTag LINAC_ENERGY_TYPE_STATE = new OpcTag("LINAC_ENERGY_TYPE_STATE", "LINAC_ENERGY_TYPE_STATE");
        public static OpcTag LINAC_STATE = new OpcTag("LINAC_STATE", "LINAC_STATE");
        public static OpcTag LINAC_STATUS = new OpcTag("LINAC_STATUS", "LINAC_STATUS");
    }
}
