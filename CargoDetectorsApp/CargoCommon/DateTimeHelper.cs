using System;
using System.Globalization;

namespace L3.Cargo.Common
{
    public class DateTimeHelper
    {
        private static readonly string[] _Formats = {"M/d/yyyy h:mm:ss tt", "M-d-yyyy h:mm:ss tt",
                                                     "M/d/yyyy h:mm tt",    "M-d-yyyy h:mm:ss tt",
                                                     "MM/dd/yyyy hh:mm:ss", "MM-dd-yyyy hh:mm:ss",
                                                     "MM/dd/yyyy hh:mm:ss", "MM-dd-yyyy hh:mm:ss",
                                                     "M/d/yyyy h:mm:ss",    "M-d-yyyy h:mm:ss",
                                                     "M/d/yyyy hh:mm tt",   "M-d-yyyy hh:mm tt",
                                                     "M/d/yyyy hh tt",      "M-d-yyyy hh tt",
                                                     "M/d/yyyy h:mm",       "M-d-yyyy h:mm",
                                                     "M/d/yyyy h:mm",       "M-d-yyyy h:mm",
                                                     "MM/dd/yyyy hh:mm",    "MM-dd-yyyy hh:mm",
                                                     "M/dd/yyyy hh:mm",     "M-dd-yyyy hh:mm",

                                                     "d/M/yyyy h:mm:ss tt", "d-M-yyyy h:mm:ss tt",
                                                     "d/M/yyyy h:mm tt",    "d-M-yyyy h:mm:ss tt",
                                                     "dd/MM/yyyy hh:mm:ss", "dd-MM-yyyy hh:mm:ss",
                                                     "dd/MM/yyyy hh:mm:ss", "dd-MM-yyyy hh:mm:ss",
                                                     "d/M/yyyy h:mm:ss",    "d-M-yyyy h:mm:ss",
                                                     "d/M/yyyy hh:mm tt",   "d-M-yyyy hh:mm tt",
                                                     "d/M/yyyy hh tt",      "d-M-yyyy hh tt",
                                                     "d/M/yyyy h:mm",       "d-M-yyyy h:mm",
                                                     "d/M/yyyy h:mm",       "d-M-yyyy h:mm",
                                                     "dd/MM/yyyy hh:mm",    "dd-MM-yyyy hh:mm",
                                                     "dd/M/yyyy hh:mm",     "dd-M-yyyy hh:mm",

                                                     "M/d/yyyy HH:mm:ss tt","M-d-yyyy HH:mm:ss tt",
                                                     "M/d/yyyy HH:mm tt",   "M-d-yyyy HH:mm:ss tt",
                                                     "MM/dd/yyyy HH:mm:ss", "MM-dd-yyyy HH:mm:ss",
                                                     "M/d/yyyy HH:mm:ss",   "M-d-yyyy HH:mm:ss",
                                                     "M/d/yyyy HH:mm tt",   "M-d-yyyy HH:mm tt",
                                                     "M/d/yyyy HH tt",      "M-d-yyyy HH tt",
                                                     "M/d/yyyy HH:mm",      "M-d-yyyy HH:mm",
                                                     "MM/dd/yyyy HH:mm",    "MM-dd-yyyy HH:mm",
                                                     "M/dd/yyyy HH:mm",     "M-dd-yyyy HH:mm",

                                                     "d/M/yyyy HH:mm:ss tt","d-M-yyyy HH:mm:ss tt",
                                                     "d/M/yyyy HH:mm tt",   "d-M-yyyy HH:mm:ss tt",
                                                     "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss",
                                                     "d/M/yyyy HH:mm:ss",   "d-M-yyyy HH:mm:ss",
                                                     "d/M/yyyy HH:mm tt",   "d-M-yyyy HH:mm tt",
                                                     "d/M/yyyy HH tt",      "d-M-yyyy HH tt",
                                                     "d/M/yyyy HH:mm",      "d-M-yyyy HH:mm",
                                                     "dd/MM/yyyy HH:mm",    "dd-MM-yyyy HH:mm",
                                                     "dd/M/yyyy HH:mm",     "dd-M-yyyy HH:mm"
                                                     };

        public static DateTime Parse(string dateTime)
        {
            return DateTime.ParseExact(dateTime, _Formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
        }
    }
}
