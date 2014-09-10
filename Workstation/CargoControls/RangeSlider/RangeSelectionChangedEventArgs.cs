using System.Windows;

namespace L3.Cargo.Controls
{
    public delegate void RangeSelectionChangedEventHandler (object sender, RangeSelectionChangedEventArgs e);

    public class RangeSelectionChangedEventArgs : RoutedEventArgs
    {
        #region Private Members

        private long _newRangeStart;

        private long _newRangeStop;

        #endregion Private Members


        #region Public Members

        public long NewRangeStart
        {
            get { return _newRangeStart; }
            set { _newRangeStart = value; }
        }

        public long NewRangeStop
        {
            get { return _newRangeStop; }
            set { _newRangeStop = value; }
        }

        #endregion Public Members


        #region Constructors

        internal RangeSelectionChangedEventArgs (long newRangeStart, long newRangeStop)
        {
            _newRangeStart = newRangeStart;
            _newRangeStop = newRangeStop;
        }

        internal RangeSelectionChangedEventArgs (RangeSlider slider)
            : this(slider.RangeStartSelected, slider.RangeStopSelected)
        { }

        #endregion Constructors
    }
}