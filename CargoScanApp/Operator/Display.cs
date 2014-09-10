using System;
using L3.Cargo.Common.Dashboard.Display;
using L3.Cargo.Scan.Display.Common;

namespace L3.Cargo.Scan.Operator
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
