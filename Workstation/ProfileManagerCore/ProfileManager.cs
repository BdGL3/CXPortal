using System.Collections.Generic;
using L3.Cargo.Common;

namespace L3.Cargo.Workstation.ProfileManagerCore
{
    public class ProfileManager
    {
        #region Private Members

        private ProfileObject m_Profile;

        #endregion Private Members


        #region Public Members

        public ProfileObject Profile
        {
            get
            {
                return m_Profile; 
            }
            set
            {
                m_Profile = value;
            }
        }

        #endregion Public Members


        #region Constructors

        public ProfileManager()
        {
        }

        #endregion
    }
}
