using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using L3.Cargo.Communications.Dashboard.Display.Host;
using L3.Cargo.Common.Dashboard.Display;
using System.Configuration;
using L3.Cargo.Common.Configurations;
using System.Windows.Threading;
using System.ComponentModel;

namespace L3.Cargo.Safety.Display.Common
{
    public class TagUpdate
    {
        #region private members

        private EStopStatus _estopStatus;

        private Dispatcher _dispatcher;

        private InterlockStatus _interlockStatus;

        private WarningStatus _warningStatus;

        private SummaryStatus _summaryStatus;

        #endregion


        #region public members

        #endregion


        #region Constructors
        public TagUpdate(EStopStatus estop, InterlockStatus interlock, WarningStatus warning, SummaryStatus summaryStatus, Dispatcher dispatcher, WidgetStatusHost host)
        {
            _estopStatus = estop;
            _interlockStatus = interlock;
            _warningStatus = warning;
            _summaryStatus = summaryStatus;
            _dispatcher = dispatcher;
            host.WidgetUpdateEvent += new WidgetUpdateHandler(WidgetUpdate);
        }

        #endregion


        #region Private Methods

        private void WidgetUpdate(string name, int value)
        {
            string tagDisplayName = (name + "_TITLE");

            System.Console.WriteLine("Widget Update: " + tagDisplayName + " with value " + value);

            if (!String.IsNullOrWhiteSpace(tagDisplayName))
            {
                string tagValue = string.Empty;

                if (name.Contains("ESTOP") && name != "ESTOP_CLICKER")
                {
                    tagValue = ("ESTOP_STATE" + "_" + value.ToString());
                    _estopStatus.UpdateTagsCollection(tagDisplayName, tagValue, _dispatcher);
                }
                else if ((name.Contains("INTERLOCK") || name == "ESTOP_CLICKER" || name == "RADIATION_MONITOR") && !name.Equals(OpcTags.INTERLOCK_BYPASS.Name))
                {
                    tagValue = ("INTERLOCK_STATE" + "_" + value.ToString());
                    _interlockStatus.UpdateTagsCollection(tagDisplayName, tagValue, _dispatcher);
                }
                else if (name.Contains("RADIATION_MONITOR"))
                {
                    tagValue = ("RADIATION_MONITOR_STATE" + "_" + value.ToString());
                    _interlockStatus.UpdateTagsCollection(tagDisplayName, tagValue, _dispatcher);
                }
                else if (name.Contains("TRAFFIC_LIGHT_STATUS"))
                {
                    // Don't display the traffic lights in the status
                }
                else if (name.Contains("WARNING") || name.Contains("BCN"))
                {
                    tagValue = ("WARNING_STATE" + "_" + value.ToString());
                    _warningStatus.UpdateTagsCollection(tagDisplayName, tagValue, _dispatcher);
                }
                else if (name.Contains("VEHICLE_SENSOR"))
                {
                    // In Summary Status
                    tagValue = ("VEHICLE_NOT_DETECTED" + "_" + value.ToString());
                    _summaryStatus.UpdateTagsCollection(tagDisplayName, tagValue, _dispatcher);
                }
                else if (name.Contains("DOPPLER_RADAR_SENSOR"))
                {
                    tagValue = value.ToString();
                    _warningStatus.UpdateTagsCollection(tagDisplayName, tagValue, _dispatcher);
                }
                else if (name.Contains("DISTANCE_MEASUREMENT_SENSOR"))
                {
                    tagValue = ("DISTANCE_MEASUREMENT" + "_" + value.ToString());
                    _warningStatus.UpdateTagsCollection(tagDisplayName, tagValue, _dispatcher);
                }
                else if (name.Contains("IN_MDS"))
                {
                    // In Summary Status
                    tagValue = ("IN_MDS_VEHICLE_MOTION_DET_SAFETY_STATE" + "_" + value.ToString());
                    _summaryStatus.UpdateTagsCollection(tagDisplayName, tagValue, _dispatcher);
                }
                else if (name.Contains("VEHICLE_TYPE"))
                {
                    tagValue = ("VEHICLE_TYPE_STATE" + "_" + value.ToString());
                    _summaryStatus.UpdateTagsCollection(tagDisplayName, tagValue, _dispatcher);
                }
                else if (name.Contains("BARCODE_READ"))
                {
                    tagValue = value.ToString();
                    _summaryStatus.UpdateTagsCollection(tagDisplayName, tagValue, _dispatcher);
                }
            }
        }

        private string GetTextFromResource (string resourcename)
        {
            return (!String.IsNullOrWhiteSpace(Resources.ResourceManager.GetString(resourcename))) ?
                    Resources.ResourceManager.GetString(resourcename) : Resources.UNKNOWN_RESOURCE;
        }

        private string convertTagToString(int value)
        {
            return value.ToString();
        }
        #endregion Private Methods
    }
}
