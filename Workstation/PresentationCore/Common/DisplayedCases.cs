using System.Collections;
using L3.Cargo.Workstation.Plugins.Common;

namespace L3.Cargo.Workstation.PresentationCore.Common
{
    public class DisplayedCases : CollectionBase
    {
        public new int Count
        {
            get
            {
                return this.List.Count;
            }
        }

        public void Add(DisplayedCase caseToAdd)
        {
            this.List.Add(caseToAdd);
        }

        public void Remove(DisplayedCase caseToRemove)
        {
            caseToRemove.Dispose();
            this.List.Remove(caseToRemove);
        }

        public void Remove(string caseName)
        {
            DisplayedCase displayedCase = this.Find(caseName);
            if (displayedCase != null)
            {
                this.Remove(displayedCase);
            }
        }

        public new void RemoveAt(int index)
        {
            DisplayedCase displayCase = this.List[index] as DisplayedCase;
            if (displayCase != null)
            {
                displayCase.Dispose();
            }
            this.List.RemoveAt(index);
        }

        public new void Clear()
        {
            foreach (DisplayedCase displayCase in this.List)
            {
                displayCase.Dispose();
            }

            this.List.Clear();
        }

        public DisplayedCase Find(string caseName)
        {
            DisplayedCase toReturn = null;

            foreach (DisplayedCase displayCase in this.List)
            {
                if (string.Equals(displayCase.CaseID, caseName))
                {
                    toReturn = displayCase;
                    break;
                }
            }
            return toReturn;
        }

        public bool Contains(string caseName)
        {
            return (this.Find(caseName) != null) ? true : false;
        }

        public bool ContainsLiveCase()
        {
            bool toReturn = false;

            foreach (DisplayedCase displayCase in this.List)
            {
                if (displayCase.IsCaseEditable == true)
                {
                    toReturn = true;
                    break;
                }
            }
            return toReturn;
        }

        public DisplayedCase GetPrimaryCase()
        {
            DisplayedCase toReturn = null;

            foreach (DisplayedCase displayCase in this.List)
            {
                if (displayCase.IsPrimaryCase == true)
                {
                    toReturn = displayCase;
                    break;
                }
            }
            return toReturn;
        }

        public DisplayedCase GetLiveCase()
        {
            DisplayedCase toReturn = null;

            foreach (DisplayedCase displayCase in this.List)
            {
                if (displayCase.IsCaseEditable == true)
                {
                    toReturn = displayCase;
                    break;
                }
            }
            return toReturn;
        }

        public PrinterObjects GetPrinterObjects ()
        {
            PrinterObjects printerObjects = null;

            DisplayedCase displayCase = GetPrimaryCase();

            if (displayCase != null)
            {
                printerObjects = displayCase.PrinterObjects;
            }

            return printerObjects;
        }

        public DisplayedCase this[int index]
        {
            get
            {
                return this.List[index] as DisplayedCase;
            }

            set
            {
                this.List[index] = value;
            }
        }
    }
}
