using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;

namespace L3.Cargo.Controls
{
    public class AdornerLayerManager
    {
        #region Private Members

        private AdornerLayer _AdornerLayer;

        private Dictionary<string, Adorner> _Adorners;

        #endregion Private Members


        #region Public Members

        public static String MEASUREMENT_ADORNER = "Measurement";
        public static String ANNOTATION_ADORNER = "Annotation";
        public static String AOI_ADORNER = "AOI";

        public Adorner this[string name]
        {
            get
            {
                return _Adorners[name];
            }
            set
            {
                _Adorners[name] = value;
            }
        }

        #endregion Public Members


        #region Constructors

        public AdornerLayerManager(AdornerLayer adornerLayer)
        {
            if (adornerLayer == null)
            {
                throw new ArgumentNullException();
            }

            _AdornerLayer = adornerLayer;
            _Adorners = new Dictionary<string, Adorner>();
        }

        #endregion Constructors


        #region Private Methods

        #endregion Private Methods


        #region Public Methods

        public void Add(string name, Adorner adorner)
        {
            _Adorners.Add(name, adorner);
        }

        public void Remove(string name)
        {
            if (_Adorners.ContainsKey(name))
            {
                _AdornerLayer.Remove(_Adorners[name]);
                _Adorners.Remove(name);
            }
        }

        public void Remove(Adorner adorner)
        {
            if (_Adorners.ContainsValue(adorner))
            {
                _AdornerLayer.Remove(adorner);

                string name = string.Empty;

                foreach (string key in _Adorners.Keys)
                {
                    if (_Adorners[key].Equals(adorner))
                    {
                        name = key;
                        break;
                    }
                }

                if (!String.IsNullOrWhiteSpace(name))
                {
                    _Adorners.Remove(name);
                }
            }
        }

        public void Clear()
        {
            foreach (Adorner adorner in _Adorners.Values)
            {
                _AdornerLayer.Remove(adorner);
            }

            _Adorners.Clear();
        }

        public void Show(string name)
        {
            try
            {
                _AdornerLayer.Add(_Adorners[name]);
            }
            catch { }
            _Adorners[name].Visibility = Visibility.Visible;
        }

        public void Show(Adorner adorner)
        {
            try
            {
                _AdornerLayer.Add(adorner);
            }
            catch { }
            adorner.Visibility = Visibility.Visible;
        }

        public void Hide(string name)
        {
            try
            {
                _Adorners[name].Visibility = Visibility.Hidden;
            }
            catch { }
        }

        public void Hide(Adorner adorner)
        {
            try
            {
                adorner.Visibility = Visibility.Hidden;
            }
            catch { }
        }

        public bool IsShowing(string name)
        {
            if (!_Adorners.ContainsKey(name))
            {
                return false;
            }
            Adorner adorner = _Adorners[name];
            return (adorner != null ? adorner.IsVisible : false);
        }
        
        public Adorner GetAdorner(string p)
        {
            return _Adorners[p];
        }
        
        #endregion Public Methods
    }
}
