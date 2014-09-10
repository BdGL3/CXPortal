using System.ComponentModel;

namespace L3.Cargo.Workstation.Common
{
    public class CaseSourcesObject : INotifyPropertyChanged
    {
        #region Private Members

        private string m_Name;

        private bool m_IsLoggedIn;

        #endregion Private Members


        #region Public Members

        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;
                NotifyPropertyChanged("Name");
            }
        }

        public bool IsLoggedIn
        {
            get
            {
                return m_IsLoggedIn;
            }
            set
            {
                m_IsLoggedIn = value;
                NotifyPropertyChanged("IsLoggedIn");
            }
        }

        #endregion Public Members


        #region INotifyPropertyChanged

        void NotifyPropertyChanged (string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion


        #region Constructors

        public CaseSourcesObject (string name, bool isLoggedIn)
        {
            m_Name = name;
            m_IsLoggedIn = isLoggedIn;
        }

        #endregion Constructors
    }
}
