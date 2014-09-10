using System;
using System.Collections;
using L3.Cargo.Common.Xml.History_1_0;

namespace L3.Cargo.Common
{
    public delegate void MacroUpdated();

    public class Macro : HistoryStep
    {
        #region Public Members

        public string Name { get; set; }

        #endregion Public Members


        #region Constructors

        public Macro (string name)
        {
            Name = name;
            Buffer = new HistoryBuffer();
            PseudoColor = new HistoryPseudoColor();
            Histogram = new HistoryHistogram();
        }

        public Macro (string name, HistoryStep history)
        {
            Name = name;
            Buffer = history.Buffer;
            PseudoColor = history.PseudoColor;
            Histogram = history.Histogram;
            Filter = history.Filter;
        }

        #endregion Constructors
    }

    public class Macros : CollectionBase
    {
        #region Private Members

        private int m_Capacity;

        #endregion Private Members


        #region Public Members

        public Macro this[int index]
        {
            get
            {
                return this.List[index] as Macro;
            }
        }

        public bool IsFull
        {
            get
            {
                return (this.Count >= this.m_Capacity); 
            }
        }

        public MacroUpdated MacroUpdatedEvent;

        #endregion Public Members


        #region Constructors

        public Macros (int capacity)
        {
            m_Capacity = capacity;
        }

        #endregion Constructors


        #region Public Methods

        public Macro Find (string macroName)
        {
            Macro toReturn = null;

            foreach (Macro macro in this.List)
            {
                if (string.Equals(macro.Name, macroName))
                {
                    toReturn = macro;
                    break;
                }
            }
            return toReturn;
        }

        public void Add (Macro macroToAdd)
        {
            if (this.List.Count < m_Capacity)
            {
                this.List.Add(macroToAdd);
                MacroUpdatedEvent();
            }
            else
            {
                throw new Exception();
            }
        }

        public void Remove (Macro macroToAdd)
        {
            this.List.Remove(macroToAdd);
            MacroUpdatedEvent();
        }

        public void Remove (string macroName)
        {
            Macro macro = this.Find(macroName);
            if (macro != null)
            {
                this.Remove(macro);
            }
        }

        public void Update (Macro newMacro)
        {
            Macro macro = this.Find(newMacro.Name);
            if (macro != null)
            {
                macro.Buffer = newMacro.Buffer;
                macro.PseudoColor = newMacro.PseudoColor;
                macro.Histogram = newMacro.Histogram;
                macro.Filter = newMacro.Filter;
                MacroUpdatedEvent();
            }
        }

        #endregion Public Methods
    }
}
