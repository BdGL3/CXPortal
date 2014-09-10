using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace L3.Cargo.Subsystem.StatusManagerCore
{
    public class StatusElements : CollectionBase
    {
        #region Public Members

        public new int Count
        {
            get
            {
                return this.List.Count;
            }
        }

        public StatusElement this[int index]
        {
            get
            {
                return this.List[index] as StatusElement;
            }

            set
            {
                this.List[index] = value;
            }
        }

        #endregion Public Members


        #region Public Methods

        public void Add (StatusElement statusToAdd)
        {
            this.List.Add(statusToAdd);
        }

        public void Remove (StatusElement statusToRemove)
        {
            this.List.Remove(statusToRemove);
        }

        public void Remove (string status)
        {
            StatusElement statusElement = this.Find(status);
            if (statusElement != null)
            {
                this.Remove(statusElement);
            }
        }

        public StatusElement Find (string name)
        {
            StatusElement toReturn = null;

            foreach (StatusElement statusElement in this.List)
            {
                if (string.Equals(statusElement.Name, name))
                {
                    toReturn = statusElement;
                    break;
                }
            }
            return toReturn;
        }

        public bool Contains (string name)
        {
            return (this.Find(name) != null) ? true : false;
        }

        #endregion Public Methods
    }
}
