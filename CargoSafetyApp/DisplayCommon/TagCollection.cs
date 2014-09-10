using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using L3.Cargo.Common.Dashboard.Display;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace L3.Cargo.Safety.Display.Common
{
    public class TagCollection : ObservableCollection<TagInfo>
    {        
    }

    public class TagInfo : INotifyPropertyChanged
    {
        private string _tagName;
        private string _tagValue;

        public string TagName
        {
            get {

                System.Console.WriteLine("Setting tag name: " + _tagName);
                return _tagName;
            }
            set 
            {
                _tagName = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("TagName"));
            }
        }

        public string TagValue
        {
            get {
                return _tagValue;}
            set 
            {
                _tagValue = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("TagValue"));
            }
        }

        public string TagNameDisplay
        {
            get { return L3.Cargo.Safety.Display.Common.Resources.ResourceManager.GetString(_tagName, CultureResources.getCurrentCulture()); }
        }

        public string TagValueDisplay
        {
            get { 
                
                string valueOfTheTag = 
                    L3.Cargo.Safety.Display.Common.Resources.ResourceManager.GetString(_tagValue, CultureResources.getCurrentCulture());

                // It could be a number (not status with a text, ex. Speed)
                if (valueOfTheTag == null)
                {
                    valueOfTheTag = _tagValue;
                }

                return valueOfTheTag; 
            }
        }

        
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, e);
        }

        #endregion

    }
}
