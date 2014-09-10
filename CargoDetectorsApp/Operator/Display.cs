using System;
using L3.Cargo.Common.Dashboard.Display;
using L3.Cargo.Detectors.Display.Common;

namespace L3.Cargo.Detectors.Operator
{
    public class Displays : DisplayBase
    {
        #region Constructors

        public Displays() :
            base()
        {
        }

        #endregion Constructors


        #region Public Methods

        public override AssemblyDisplays Initialize (Object passedObj)
        {
            return base.Initialize(passedObj);
        }

        #endregion Public Methods
    }
}
