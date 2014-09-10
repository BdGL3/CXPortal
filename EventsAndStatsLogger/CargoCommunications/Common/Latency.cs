using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace L3.Cargo.Communications.Common
{
    /// <summary>
    /// Latency is a form of <see cref="StopWatch"/> that uses <see cref="DateTime"/>. Why?
    /// Under certain circumstances, <see cref="StopWatch"/> and <see cref="TimeSpan"/> can
    /// produce spurious results and values.</summary>
    /// <remarks>Unlike timers, <see cref="Latency"/> need not be disposed.</remarks>
    public class Latency
    {
        /// <summary>Default period value</summary>
        public const int DftValue = /*10s*/ 10000 /*ms*/;
        /// <summary>Maximum period value; MinValue is zero (0)</summary>
        public const int MaxValue = int.MaxValue;

        /// <summary>
        /// Latency creates and prepares a new Latency item.</summary>
        /// <param name="value">
        /// Value specifies, in milliseconds, a new latency period and should be in the domain
        /// [0, <see cref="MaxValue"/>]... Values outside of the valid domain are adjusted,
        /// appropriately, to be valid, negative values being converted to their positive
        /// counterparts. If omitted, the default value is <see cref="DftValue"/>.</param>
        public Latency(int value = DftValue) { Reset(value); }

        private DateTime _epoch = DateTime.UtcNow;

        /// <summary>
        /// Expired gets false if the latency period has not passed or true if it has.</summary>
        /// <returns>whether or not the latency period has passed</returns>
        public bool Expired { get { return DateTime.UtcNow >= _epoch; } }

        /// <summary>Normalize adjusts a value to be within [0, <see cref="MaxValue"/>].</summary>
        /// <param name="value">
        /// Value specifies, in milliseconds, a latency period and, while it should be in the
        /// domain [0, <see cref="MaxValue"/>], if outside, it will be adjusted appropriately, to
        /// be valid, negative values being converted to their positive counterparts.</param>
        /// <returns>valid latency period</returns>
        public static int Normalize(int value) { return (int)((ulong)Math.Abs(value) % (ulong)MaxValue);  }

        /// <summary>Period gets/sets the latency period.</summary>
        /// <value>
        /// The settor's value specifies, in milliseconds, a new latency period and should be in
        /// the domain [0, <see cref="MaxValue"/>]... Values outside of the valid domain are
        /// adjusted, appropriately, to be valid, negative values being converted to their positive
        /// counterparts.</value>
        /// <returns>latency period (see <see cref="Remaining"/>)</returns>
        public int Period
        {
            get { return _period; }
            set { _period = Normalize(value); }
        }
        /// <summary>
        /// Period is OK verifies the validity of a candidate period: whether or not a candidate
        /// value is within the valid domain [0, <see cref="MaxValue"/>].</summary>
        /// <param name="value">Value specifies the candidate period...</param>
        /// <returns>validity of candidate: {false, true}</returns>
        public static bool PeriodOK(int value) { return (value >= 0) && (value <= MaxValue); }
        private int _period = DftValue;

        /// <summary>
        /// Remaining gets the remaining latency, in milliseconds. If the latency period has
        /// passed, the result is zero (0).</summary>
        /// <returns>remaining latency (in the domain [0, <see cref="MaxValue"/>)</returns>
        public int Remaining { get { return Expired ? 0 : (int)(_epoch.Subtract(DateTime.UtcNow).TotalMilliseconds % (double)MaxValue); } }

        /// <summary>Reset resets the latency epoch, from which latency is measured.</summary>
        public void Reset() { _epoch = DateTime.UtcNow.AddMilliseconds(Period); }
        /// <summary>Reset resets the latency epoch, from which latency is measured.</summary>
        /// <param name="value">
        /// Value specifies, in milliseconds, a new latency period and should be in the domain
        /// [0, <see cref="MaxValue"/>]... Values outside of the valid domain are adjusted,
        /// appropriately, to be valid, negative values being converted to their positive
        /// counterparts.</param>
        public void Reset(int value)
        {
            Period = value;
            Reset();
        }
    }
}
