using L3.Cargo.Common;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Threading;
using System;

namespace L3.Cargo.Workstation.Common
{
    public class CaseSourcesList : ObservableCollection<CaseSourcesObject>
    {
        private Dispatcher dispatcher;

        public CaseSourcesList () :
            base()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
        }

        public void Add (string nameToAdd, bool isLoginRequired)
        {
            if (!this.Contains(nameToAdd))
            {
                if (Thread.CurrentThread == dispatcher.Thread)
                {
                    this.Add(new CaseSourcesObject(nameToAdd, !isLoginRequired));
                }
                else
                {
                    dispatcher.BeginInvoke((Action)(() => { this.Add(new CaseSourcesObject(nameToAdd, !isLoginRequired)); }));
                }
            }
        }

        public void Remove (string nameToRemove)
        {
            CaseSourcesObject caseSource = this.Find(nameToRemove);
            if (caseSource != null)
            {
                if (Thread.CurrentThread == dispatcher.Thread)
                {
                    this.Remove(caseSource);
                }
                else
                {
                    dispatcher.BeginInvoke((Action)(() => { this.Remove(caseSource); }));
                }
            }
        }

        public CaseSourcesObject Find (string name)
        {
            CaseSourcesObject toReturn = null;

            foreach (CaseSourcesObject caseSource in this.Items)
            {
                if (string.Equals(caseSource.Name, name))
                {
                    toReturn = caseSource;
                    break;
                }
            }
            return toReturn;
        }

        public bool Contains (string name)
        {
            return (this.Find(name) != null) ? true : false;
        }
    }
}
