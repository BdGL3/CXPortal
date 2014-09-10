using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using L3.Cargo.Safety.Display.Common;
using L3.Cargo.Communications.Dashboard.Display.Host;
using System.Windows.Threading;

namespace L3.Cargo.Safety.Display.Common.ViewModel
{
    public class PortalViewModel : BaseViewModel
    {
        public VehicleType VehicleStatus
        {
            get
            {
                System.Console.WriteLine("########### TESTING #############");

                return _vehicleType;
            }

            set
            {
                _vehicleType = value;

                System.Console.WriteLine("########### Property Change Raised #############");
                RaisePropertyChanged(() => VehicleStatus);
            }
        }

        private VehicleType _vehicleType;

        public PortalViewModel()
        {
            _vehicleType = VehicleType.No_Detection;

        }

        public void setVehicleStatus(int status)
        {
            System.Console.WriteLine("########################   " + status );
            if (status == 1)
            {
                System.Console.WriteLine("########################   " + status);
                VehicleStatus = VehicleType.Small_Vehicle;
            }
        }
       
    }
}
