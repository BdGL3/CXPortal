using System;
using System.Data;
using System.Threading;
using System.ServiceModel;

namespace L3.Cargo.Dashboard.DataAccessCore
{
    public class SubsystemService : IDisposable
    {
        #region Private Members

        private EndpointAddress _EndpointAddress;

        private string _Alias;

        private string _AssemblyTag;

        private Timer _CommCheckTimer;

        private string _ZippedFilename;

        #endregion Private Members


        #region Public Members

        public string ZippedFilename
        {
            get { return _ZippedFilename; }
            set { _ZippedFilename = value; }
        }

        public Timer CommCheckTimer
        {
            get { return _CommCheckTimer; }
            set { _CommCheckTimer = value; }
        }

        public EndpointAddress EndpointAddress
        {
            get { return _EndpointAddress; }
            set { _EndpointAddress = value; }
        }

        public string AssemblyTag
        {
            get { return _AssemblyTag; }
            set { _AssemblyTag = value; }
        }

        public string Alias
        {
            get { return _Alias; }
            set { _Alias = value; }
        }

        #endregion Public Members


        #region Constructors

        public SubsystemService (string alias, string assemblyTag, EndpointAddress endpointAddress)
        {
            _Alias = alias;
            _AssemblyTag = assemblyTag;
            _EndpointAddress = endpointAddress;
        }

        #endregion Constructors


        #region Public Methods

        public void Dispose ()
        {
            _CommCheckTimer.Dispose();
            _CommCheckTimer = null;
        }

        #endregion Public Methods
    }
}
