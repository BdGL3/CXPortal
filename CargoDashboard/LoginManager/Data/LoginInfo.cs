using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using l3.cargo.corba;

namespace LoginManager
{
    public class LoginInfo : INotifyPropertyChanged
    {
        #region Private Members

        private volatile bool _isConnected;

        private AuthenticationLevel _accessLevel;

        private string _username;

        private string _password;

        private string _dashboardMode;

        private string _errorMessage;

        #endregion Private Members


        #region Public Members

        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
            set
            {
                _isConnected = value;
                NotifyPropertyChanged("IsConnected");
            }
        }

        public AuthenticationLevel AccessLevel
        {
            get
            {
                return _accessLevel;
            }
            set
            {
                _accessLevel = value;
                NotifyPropertyChanged("AccessLevel");
            }
        }

        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
                NotifyPropertyChanged("Username");
            }
        }

        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                NotifyPropertyChanged("Password");
            }
        }

        public string DashboardMode
        {
            get
            {
                return _dashboardMode;
            }
            set
            {
                _dashboardMode = value;
                NotifyPropertyChanged("DashboardMode");
            }
        }

        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                NotifyPropertyChanged("ErrorMessage");
            }
        }

        #endregion Public Members


        #region Constructors

        public LoginInfo()
        {
            _accessLevel = AuthenticationLevel.NONE;
            _isConnected = false;
            _dashboardMode = "Operator";
        }

        #endregion Constructors


        #region Public Methods

        public void Clear()
        {
            ClearUserInfo();
            ClearAccessInfo();
        }

        public void ClearAccessInfo ()
        {
            AccessLevel = AuthenticationLevel.NONE;
            DashboardMode = "Operator";
        }

        public void ClearUserInfo ()
        {
            Username = string.Empty;
            ClearPassword();
            ClearErrorMessage();
        }

        public void ClearPassword ()
        {
            Password = string.Empty;
        }

        public void ClearErrorMessage ()
        {
            ErrorMessage = string.Empty;
        }

        #endregion Public Methods


        #region INotifyPropertyChanged

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
