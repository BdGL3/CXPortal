using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;

namespace L3.Cargo.Common.Dashboard.Display
{
    public class Widget
    {
        #region Private Members

        private UIElement _Display;

        private int _Row;

        private int _Column;

        private int _RowSpan;

        private int _ColumnSpan;

        private int _Page;

        private string _Name;

        #endregion Private Members


        #region Public Members

        public UIElement Display
        {
            get
            {
                return _Display;
            }
            set
            {
                _Display = value;
                Grid.SetRow(_Display, _Row);
                Grid.SetColumn(_Display, _Column);
                Grid.SetRowSpan(_Display, _RowSpan);
                Grid.SetColumnSpan(_Display, _ColumnSpan);
            }
        }

        public int Row
        {
            get
            {
                return _Row;
            }
            set
            {
                _Row = value;
                if (_Display != null)
                {
                    Grid.SetRow(_Display, _Row);
                }
            }
        }

        public int Column
        {
            get
            {
                return _Column;
            }
            set
            {
                _Column = value;
                if (_Display != null)
                {
                    Grid.SetColumn(_Display, _Column);
                }
            }
        }

        public int RowSpan
        {
            get
            {
                return _RowSpan;
            }
            set
            {
                _RowSpan = value;
                if (_Display != null)
                {
                    Grid.SetRowSpan(_Display, _RowSpan);
                }
            }
        }

        public int ColumnSpan
        {
            get
            {
                return _ColumnSpan;
            }
            set
            {
                _ColumnSpan = value;
                if (_Display != null)
                {
                    Grid.SetColumnSpan(_Display, _ColumnSpan);
                }
            }
        }

        public int Page
        {
            get
            {
                return _Page;
            }
            set
            {
                _Page = value;
            }
        }

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

        #endregion Public Members


        #region Constructors

        public Widget (string name)
        {
            _Name = name;
            _Display = null;
            _Row = 0;
            _Column = 0;
            _RowSpan = 1;
            _ColumnSpan = 1;
        }

        public Widget (string name, UIElement display)
        {
            _Name = name;
            _Display = display;
            _Row = 0;
            _Column = 0;
            _RowSpan = 1;
            _ColumnSpan = 1;
        }

        public Widget (string name, UIElement display, int row, int column, int rowspan, int columnspan)
        {
            _Name = name;
            _Display = display;
            Row = row;
            Column = column;
            RowSpan = rowspan;
            ColumnSpan = columnspan;
        }

        public Widget (string name, UIElement display, int row, int column, int rowspan, int columnspan, int page)
        {
            _Name = name;
            _Display = display;
            Row = row;
            Column = column;
            RowSpan = rowspan;
            ColumnSpan = columnspan;
            Page = page;
        }

        #endregion Constructors
    }
}
