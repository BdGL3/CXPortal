using System;
using System.ServiceModel;
using L3.Cargo.Common;
using L3.Cargo.Common.Xml.Profile_1_0;
using L3.Cargo.Communications.Interfaces;

namespace L3.Cargo.Communications.Host
{
    //------------------------------------------------------------------------------------
    // TODO: Put custom code here to process the Host side of the WCF connection.
    //------------------------------------------------------------------------------------
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class WSComm : CaseRequestManager, IWSComm
    {
    }
}
