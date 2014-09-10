using System.Collections.Generic;
using L3.Cargo.Common.Configurations;

namespace L3.Cargo.Subsystem.StatusManagerCore
{
    public class StatusElement
    {
        #region Private Members

        private string _Name;

        private int _Value;

        private string _Type;

        private Dictionary<int, string> _ValueMapping;

        #endregion Private Members


        #region Public Members

        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }

        public int Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
            }
        }

        public string Type
        {
            get
            {
                return _Type;
            }
            set
            {
                _Type = value;
            }
        }

        public string StatusType
        {
            get
            {
                if (_ValueMapping.ContainsKey(_Value))
                {
                    return _ValueMapping[_Value];
                }
                else
                {
                    return TagValueTypes.Error;
                }
            }
        }

        public string Message
        {
            get
            {
                return Name + "_" + Value.ToString();
            }
        }

        #endregion Public Members


        #region Constructors

        public StatusElement (string name, int initialValue, string type, Dictionary<int, string> valueMapping)
        {
            _Name = name;
            _Value = initialValue;
            _Type = type;
            _ValueMapping = valueMapping;
        }

        public StatusElement(string name, int initialValue, string type)
        {
            _Name = name;
            _Value = initialValue;
            _Type = type;
            _ValueMapping = new Dictionary<int, string>();
        }

        #endregion Constructors
    }
}
