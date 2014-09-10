
using System;
namespace L3.Cargo.Common
{
    public delegate void ProfileUpdated();


    public class ProfileObject
    {
        #region Private Methods

        private bool m_DensityAlarmSet;

        private Double m_DensityAlarmValue;

        private Macros m_UserMacros;

        private string m_SourceAlias;

        private string m_UserName;

        private string m_Password;

        #endregion


        #region Public Methods

        public Macros UserMacros
        {
            get
            {
                return m_UserMacros;
            }
            set
            {
                m_UserMacros = value;
            }
        }

        public string SourceAlias
        {
            get
            {
                return m_SourceAlias;
            }
            set
            {
                m_SourceAlias = value;
            }
        }

        public string UserName
        {
            get
            {
                return m_UserName;
            }
            set
            {
                m_UserName = value;
            }
        }

        public string Password
        {
            get
            {
                return m_Password;
            }
            set
            {
                m_Password = value;
            }
        }

        public Double DensityAlarmValue
        {
            get
            {
                return m_DensityAlarmValue;
            }
            set
            {
                if (m_DensityAlarmValue != value)
                {
                    m_DensityAlarmValue = value;
                    if (ProfileUpdatedEvent != null)
                    {
                        ProfileUpdatedEvent();
                    }
                    m_DensityAlarmValue = value;
                }
            }
        }

        public ProfileUpdated ProfileUpdatedEvent;

        #endregion


        #region Constructors

        public ProfileObject(int capacity)
        {
            m_UserMacros = new Macros(capacity);
            m_UserMacros.MacroUpdatedEvent += new MacroUpdated(MacrosUpdated);
        }
        
        #endregion


        #region Private Methods

        private void MacrosUpdated()
        {
            if (ProfileUpdatedEvent != null)
            {
                ProfileUpdatedEvent();
            }
        }

        #endregion


        #region Public Methods
        #endregion
    }
}
