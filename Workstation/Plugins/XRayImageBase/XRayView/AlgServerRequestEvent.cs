using System;

namespace L3.Cargo.Workstation.Plugins.XRayImageBase
{
    public delegate void AlgServerRequestEventHandler (object sender, AlgServerRequestEventArgs e);

    public enum AlgServerRequest
    {
        TrimatImage = 0,
        OrganicStripImage = 1,
        InOrganicStripImage = 2,
        MetalStripImage = 3,
        QuadmatImage = 5
    }

    public class AlgServerRequestEventArgs : EventArgs
    {
        public AlgServerRequest RequestType;

        public AlgServerRequestEventArgs (AlgServerRequest requestType)
        {
            RequestType = requestType;
        }
    }
}
