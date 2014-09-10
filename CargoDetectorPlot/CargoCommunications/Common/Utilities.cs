using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Management;
using System.Net;
using System.Reflection;

namespace L3.Cargo.Communications.Common
{
    public static class Utilities
    {
        /// <summary>
        /// Internet Protocol Address is OK verifies that a candidate IP address string is properly
        /// formed. Note that the method does not verify the existence of any entity using the
        /// candidate address.</summary>
        /// <param name="address">Address specifies the candidate IP address...</param>
        /// <returns>false if <paramref name="address"/> is invalid; true if valid</returns>
        public static bool IPAddressOK(IPAddress address)
        {
            bool ok = /*not OK*/ false;
            try
            {
                if (/*prepared?*/ address != null)
                    ok = IPAddressOK(address.ToString());
            }
            catch { ok = false; }
            return ok;
        }
        /// <summary>
        /// Internet Protocol Address is OK verifies that a candidate IP address string is properly
        /// formed. Note that the method does not verify the existence of any entity using the
        /// candidate address.</summary>
        /// <param name="address">Address specifies, as a string, the candidate IP address...</param>
        /// <returns>false if <paramref name="address"/> is invalid; true if valid</returns>
        public static bool IPAddressOK(string address)
        {
            bool ok = /*not OK*/ false;
            try
            {
                IPAddress addressValue;
                ok = IPAddress.TryParse(address, out addressValue);
            }
            catch { ok = false; }
            return ok;
        }
        /// <summary>
        /// Internet Protocol Port is OK verifies that a candidate IP port number value is within
        /// the domain [IPEndPoint.MinPort, IPEndPoint.MaxPort].</summary>
        /// <param name="port">Port number specifies a candidate IP port number...</param>
        /// <returns>false if <paramref name="port"/> is invalid; true if valid</returns>
        public static bool IPPortOK(int port)
        {
            Debug.Assert(IPEndPoint.MinPort < IPEndPoint.MaxPort);
            return (port >= IPEndPoint.MinPort) && (port <= IPEndPoint.MaxPort);
        }

        /// <summary>Find Processes finds all processes decended from a parent process.</summary>
        /// <param name="identity">Identity specifies the identity of the root process...</param>
        /// <returns><see cref="ManagementObjectCollection"/></returns>
        /// <example><code>
        /// ManagementObjectCollection collection = ProcessFind(Process.GetCurrentProcess().Id);
        /// foreach (ManagementObject information in collection)</code>...
        /// </example>
        public static ManagementObjectCollection ProcessFind(int identity)
        {
            if (/*use current process?*/ identity == int.MinValue)
                identity = Process.GetCurrentProcess().Id;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + identity);
            return searcher.Get();
        }

        /// <summary>Kill a process's children, grandchildren...</summary>
        /// <param name="identity">
        /// Identity specifies the identity/index of the root process (which is not
        /// killed)..</param>
        /// <param name="logKill">
        /// Log Kill specifies whether or not to log the kills in the Windows event log... If
        /// omitted, the default value is false.</param>
        /// <returns>string log of processes killed</returns>
        /// <remarks>
        /// Why not kill the root process too? If the root process is killed, there is no way that
        /// the returned string detailing the kills can be presented.</remarks>
        /// <example><code>string text = ProcessKill();</code></example>
        /// <exception cref="System.Exception">
        /// This method is written so as to never throw an
        /// <see cref="System.Exception"/>.</exception>
        public static string ProcessKill(int identity, bool logKill = false)
        {
            string text = string.Empty;
            try { text = ProcessKill(identity, text, 0, /*don't kill root*/ false); }
            catch { /*nothing can be done*/ }
            finally
            {
                try
                {
                    if (/*substantive?*/ !string.IsNullOrWhiteSpace(text))
                        if (/*log?*/ logKill)
                            using (EventLog eventLog = new EventLog())
                            {
                                if (/*create new event source?*/ !EventLog.SourceExists(text))
                                    EventLog.CreateEventSource(text, "Application");
                                eventLog.Source = text;
                                eventLog.WriteEntry(text, EventLogEntryType.Warning, /*use process identity*/ identity);
                            }
                }
                catch { /*nothing can be done*/ }
            }
            return text;
        }
        private static string ProcessKill(int identity, string logText, int indent, bool kill = true)
        {
            Debug.Assert(logText != null);
            try
            {
                Process victim = Process.GetProcessById(identity);
                if (/*kill>?*/ kill)
                {
                    logText += (string.IsNullOrWhiteSpace(logText) ? string.Empty : "\r") + Repeat(" ", indent) + (!string.IsNullOrWhiteSpace(victim.ProcessName) ? victim.ProcessName.Trim() : identity.ToString());
                    indent++;
                }
                ManagementObjectCollection collection = ProcessFind(identity);
                foreach (ManagementObject information in collection)
                    logText += ProcessKill(Convert.ToInt32(information["ProcessID"]), logText, indent, /*kill*/ true);
                if (/*kill?*/ kill)
                    try { victim.Kill(); }
                    catch { /*already exited*/ }
            }
            catch { /*nothing can be done*/ }
            return logText;
        }

        /// <summary>
        /// Property, Configure fetches an int arithmetic value from the application's
        /// configuration and returns it for a property value.</summary>
        /// <param name="key">
        /// Key specifies the configuration key... This argument's value must be both non-null and
        /// not "white space" or an <see cref="ArgumentOutOfRangeException"/> is thrown.</param>
        /// <param name="minValue">Value, Minimum specifies the minimum allowed value...</param>
        /// <param name="maxValue">Value, Maximum specifies the maximum allowed value...</param>
        /// <param name="dftValue">
        /// Value, Default specifies the default value... If this argument specifies
        /// <see cref="int.MinValue"/>, there is no default value and the configuration
        /// specification must be present or an exception is thrown.</param>
        /// <param name="units">
        /// Unit Text specifies text for units (such as "cs" for "centiseconds")... If null or
        /// white space, this argument's value is not used.</param>
        /// <returns>fetched value</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="key"/> is null or empty, an exception is thrown.</exception>
        /// <exception cref="ConfigurationErrorsException">
        /// If any anomaly is detected, an exception, detailing the key and source texts is thrown.
        /// </exception>
        public static int PropertyConfigureInt(string key, int minValue, int maxValue, int dftValue = int.MinValue, string units = null)
        {
            int value;
            if (/*invalid?*/ string.IsNullOrWhiteSpace(key))
                throw new ArgumentOutOfRangeException(MethodBase.GetCurrentMethod().Name + ": key argument must not be null or empty/white space");
            key = key.Trim();
            if (/*invalid?*/ maxValue < minValue)
                maxValue ^= minValue ^= maxValue; /*swap minValue with maxValue*/
            string text = null;
            try
            {
                if (/*specified?*/ ConfigurationManager.AppSettings[key] != null)
                {
                    text = ConfigurationManager.AppSettings[key].Trim();
                    value = int.Parse(text, CultureInfo.InvariantCulture);
                }
                else /*not specified*/ if (/*default supplied?*/ dftValue != int.MinValue)
                    value = dftValue;
                else /*no default*/
                    throw new Exception();
                if (/*invalid?*/ (value < minValue) || (value > maxValue))
                    throw new Exception();
            }
            catch
            {
                key = new StackTrace(true).GetFrame(1).GetMethod().ReflectedType.Name + "/" + key;
                text = string.IsNullOrWhiteSpace(text) ? string.Empty : text;
                throw new ConfigurationErrorsException(key + " of \"" + text + "\" must specify an integer in the domain [" + minValue.ToString() + ", " + maxValue.ToString() + "]");
            }
            return value;
        }

        /// <summary>
        /// Property, Configure fetches a real arithmetic value from the application's
        /// configuration and returns it for a property value.</summary>
        /// <param name="key">
        /// Property Key specifies the configuration key... This argument's value must be both
        /// non-null and not just "white space" or an <see cref="ArgumentOutOfRangeException"/> is
        /// thrown.</param>
        /// <param name="minValue">Value, Minimum specifies the minimum allowed value...</param>
        /// <param name="maxValue">Value, Maximum specifies the maximum allowed value...</param>
        /// <param name="dftValue">
        /// Value, Default specifies the default value... If this argument specifies
        /// -<see cref="double.MaxValue"/>, there is no default value and the configuration
        /// specification must be present.</param>
        /// <param name="units">
        /// Unit Text specifies text for units (such as "cs" for "centiseconds")... If null or
        /// white space, this argument's value is not used.</param>
        /// <returns>fetched value</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="key"/> is null or empty, an exception is thrown.</exception>
        /// <exception cref="ConfigurationErrorsException">
        /// If any anomaly is detected, an exception, detailing the key and source texts is thrown.
        /// </exception>
        public static double PropertyConfigureReal(string key, double minValue, double maxValue, double dftValue = -double.MaxValue, string units = null)
        {
            double value;
            if (/*invalid?*/ string.IsNullOrWhiteSpace(key))
                throw new ArgumentOutOfRangeException(MethodBase.GetCurrentMethod().Name + ": key argument must not be null or empty/white space");
            key = key.Trim();
            if (/*invalid?*/ maxValue < minValue)
            {
                value = minValue;
                minValue = maxValue;
                maxValue = value;
            }
            string text = null;
            try
            {
                if (/*specified?*/ ConfigurationManager.AppSettings[key] != null)
                {
                    text = ConfigurationManager.AppSettings[key].Trim();
                    value = double.Parse(text, CultureInfo.InvariantCulture);
                }
                else /*not specified*/ if (/*default supplied?*/ dftValue != -double.MaxValue)
                    value = dftValue;
                else /*no default*/
                    throw new Exception();
                if (/*invalid?*/ (value < minValue) || (value > maxValue))
                    throw new Exception();
            }
            catch
            {
                text = string.IsNullOrWhiteSpace(text) ? string.Empty : text;
                key = new StackTrace(true).GetFrame(1).GetMethod().ReflectedType.Name + "/" + key;
                throw new ConfigurationErrorsException(key + " of \"" + text + "\" must specify a real value in the domain [" + minValue.ToString() + ", " + maxValue.ToString() + "]");
            }
            return value;
        }

        /// <summary>
        /// Text Tidy cleanses a string, replacing line ending sequences with carriage returns and
        /// ensuring that there are no repeating carriage return sequences.</summary>
        /// <param name="text">
        /// Text specifies the string to be cleansed... If this argument specifies null or white
        /// space, it is treated as <see cref="string.Empty"/>.</param>
        /// <param name="lineEnd">
        /// Line End specifies (optionally; default value is "\n") the line ending sequence... If
        /// this argument specifies null or <see cref="string.Empty"/>, an exception is
        /// thrown.</param>
        /// <param name="replacement">
        /// Replacement specifies (optionally; default value is "\r") the replacement sequence...
        /// If this argument specifies null or <see cref="string.Empty"/>, an exception is
        /// thrown.</param>
        /// <returns>cleansed string</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="lineEnd"/> or <paramref name="replacement"/> specifies null or
        /// <see cref="string.Empty"/>, an exception is thrown.
        /// </exception>
        public static string TextTidy(string text, string lineEnd = "\n", string replacement = "\r")
        {
            if (/*invalid?*/ string.IsNullOrEmpty(lineEnd) || string.IsNullOrEmpty(replacement))
                throw new ArgumentNullException();
            if (string.IsNullOrWhiteSpace(text))
                text = string.Empty;
            text = text.Trim();
            text = text.Replace(lineEnd, replacement);
            string duplicate = replacement + replacement;
            while (text.IndexOf(duplicate) >= 0)
                text = text.Replace(duplicate, replacement);
            return text;
        }

        /// <summary>Repeat forms a string of repeated phrases (strings).</summary>
        /// <param name="phrase">
        /// Phrase specifies the phrase to be repeated... If this argument's value is null or
        /// empty, no actions are performed.</param>
        /// <param name="count">
        /// Count specifies the number of times <paramref name="phrase"/> should be repeated...
        /// Values less than one (1) result in no action.</param>
        /// <returns>
        /// string with <paramref name="phrase"/> repeated <paramref name="count"/> times</returns>
        /// <example><code>
        /// string text = Repeat("---------+", 3);
        /// Debug.Assert(text == "---------+---------+---------+");
        /// </code></example>
        public static string Repeat(string phrase, int count = 1)
        {
            if (!string.IsNullOrEmpty(phrase))
            {
                for (string text = phrase; count >= 1; count--)
                    phrase += text;
            }
            return phrase;
        }

        /// <summary>Ten seconds in milliseconds</summary>
        public const int Time10SECONDS = /*10 seconds*/ 10 * TimeSECOND;
        /// <summary>One minute in milliseconds</summary>
        public const int TimeMINUTE = /*1 minute*/ 60 * TimeSECOND;
        /// <summary>One second in milliseconds</summary>
        public const int TimeSECOND = /*1s*/ 10 * TimeTENTH;
        /// <summary>A tenth of a second in milliseconds</summary>
        public const int TimeTENTH = /*1/10s*/ 100 /*ms*/;

        /// <summary>
        /// Time Text converts an interval, expressed in milliseconds, to text, showing days,
        /// hours:minutes:seconds and milliseconds.</summary>
        /// <param name="time">Time specifies, in milliseconds, the time interval...</param>
        /// <returns>text equivalent to <paramref name="time"/></returns>
        public static string TimeText(int time)
        {
            time = Math.Abs(time);
            TimeSpan span = new TimeSpan(0, 0, 0, 0, time);
            return span.ToString("c");
        }

        /// <summary>Within ensures that a value is within a domain.<summary>
        /// <param name="minimum">Minimum specifies the domain's minimum value...</param>
        /// <param name="value">Value specifies the candidate value...</param>
        /// <param name="maximum">Maximum specifies the domain's minimum value...</param>
        /// <returns>value, adjusted if necessary, within the specified domain</returns>
        public static double Within(double minimum, double value, double maximum)
        {
            if (/*adjust upward?*/ value < minimum)
                value = minimum;
            else if (/*adjust downward?*/ value > maximum)
                value = maximum;
            return value;
        }

        /// <summary>
        /// Is Within verifies whether or not a candidate value is within a domain.</summary>
        /// <param name="minimum">Minimum specifies the domain's minimum value...</param>
        /// <param name="value">Value specifies the candidate value...</param>
        /// <param name="maximum">Maximum specifies the domain's minimum value...</param>
        /// <returns>true if the candidate value is within the specified domain</returns>
        public static bool IsWithin(double minimum, double value, double maximum)
        { return (value >= minimum) && (value <= maximum); }
    }
}
