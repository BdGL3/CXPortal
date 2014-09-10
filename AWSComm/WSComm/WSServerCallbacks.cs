using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using L3.Cargo.Communications.Interfaces;
using System.Collections.ObjectModel;

namespace L3.Cargo.WSCommunications
{
    public class WSServerCallbacks : Dictionary<string, WSServerCallback>
    {
        #region Public Methods

        public void Add(string alias, IWSCommCallback callback)
        {
            if (!base.ContainsKey(alias))
            {
                base.Add(alias, new WSServerCallback(callback));
            }
            else
            {
                base[alias].Callback = callback;
                base[alias].LastPingTime = DateTime.Now;
            }
        }

        #endregion Public Methods
    }


    public class WSServerCallback
    {
        #region Private Members

        private IWSCommCallback m_WSCommCallback;

        private DateTime m_LastPingTime;

        #endregion Private Members


        #region Public Members

        public IWSCommCallback Callback
        {
            get
            {
                return m_WSCommCallback;
            }
            set
            {
                m_WSCommCallback = value;
            }
        }

        public DateTime LastPingTime
        {
            get
            {
                return m_LastPingTime;
            }
            set
            {
                m_LastPingTime = value;
            }
        }

        #endregion Public Members


        #region Constructors

        public WSServerCallback(IWSCommCallback callback)
        {
            m_WSCommCallback = callback;
            m_LastPingTime = DateTime.Now;
        }

        #endregion Constructors
    }

}
