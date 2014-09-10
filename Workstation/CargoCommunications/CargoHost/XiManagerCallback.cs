using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using l3.cargo.corba;

namespace L3.Cargo.Communications.CargoHost
{
    public class XiManagerCallback : MarshalByRefObject, XI
    {
        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void addCase(string caseId)
        {
            return;
        }

        public bool isFull()
        {
            return false;
        }

        public void onAreaOverflowRecovered(string areaId)
        {
            return;
        }
    }
}
