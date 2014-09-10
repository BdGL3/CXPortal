using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3.Cargo.Translators
{
    public class ProfileTranslator
    {
        public static L3.Cargo.Common.ProfileObject Translate (L3.Cargo.Common.Xml.Profile_1_0.Profile commProfile, int capacity)
        {
            L3.Cargo.Common.ProfileObject profile = new L3.Cargo.Common.ProfileObject(capacity);

            try
            {
                if (commProfile != null)
                {
                    if (commProfile.Macro != null)
                    {
                        foreach (L3.Cargo.Common.Xml.Profile_1_0.ProfileMacro commMacro in commProfile.Macro)
                        {
                            L3.Cargo.Common.Macro macro = new L3.Cargo.Common.Macro(commMacro.id);

                            macro.Buffer.name = commMacro.Buffer.parameter;

                            macro.PseudoColor.name = commMacro.PseudoColor.parameter;

                            L3.Cargo.Common.Xml.History_1_0.HistoryHistogram histogram = new L3.Cargo.Common.Xml.History_1_0.HistoryHistogram();
                            histogram.effecttype = commMacro.Histogram.effectType;
                            histogram.start = commMacro.Histogram.start;
                            histogram.end = commMacro.Histogram.end;
                            macro.Histogram = histogram;

                            foreach (L3.Cargo.Common.Xml.Profile_1_0.ProfileMacroFilter filter in commMacro.Filters)
                            {
                                L3.Cargo.Common.Xml.History_1_0.HistoryFilter filterHistory = new L3.Cargo.Common.Xml.History_1_0.HistoryFilter();
                                filterHistory.name = filter.id;
                                filterHistory.parameter = filter.parameter;

                                macro.Filter.Add(filterHistory);
                            }

                            profile.UserMacros.Add(macro);
                        }
                    }
                    if (commProfile.DensityAlarm != null)
                    {
                        profile.DensityAlarmValue = commProfile.DensityAlarm.value;
                    }
                }

            }
            catch (Exception ex)
            {
                //TODO: Log exception here
            }

            return profile;
        }

        public static L3.Cargo.Common.Xml.Profile_1_0.Profile Translate (L3.Cargo.Common.ProfileObject profileObj)
        {
            L3.Cargo.Common.Xml.Profile_1_0.Profile profile = new L3.Cargo.Common.Xml.Profile_1_0.Profile();
            profile.Macro = new L3.Cargo.Common.Xml.Profile_1_0.ProfileMacro[profileObj.UserMacros.Count];

            try
            {

                if (profileObj != null)
                {
                    if (profileObj.UserMacros != null)
                    {
                        int macroCount = 0;
                        foreach (L3.Cargo.Common.Macro macro in profileObj.UserMacros)
                        {
                            L3.Cargo.Common.Xml.Profile_1_0.ProfileMacro profileMacro = new L3.Cargo.Common.Xml.Profile_1_0.ProfileMacro();

                            profileMacro.id = macro.Name;

                            profileMacro.Buffer = new L3.Cargo.Common.Xml.Profile_1_0.ProfileMacroBuffer();
                            profileMacro.Buffer.parameter = macro.Buffer.name;

                            profileMacro.PseudoColor = new L3.Cargo.Common.Xml.Profile_1_0.ProfileMacroPseudoColor();
                            profileMacro.PseudoColor.parameter = macro.PseudoColor.name;

                            profileMacro.Histogram = new L3.Cargo.Common.Xml.Profile_1_0.ProfileMacroHistogram();
                            profileMacro.Histogram.effectType = macro.Histogram.effecttype;
                            profileMacro.Histogram.start = macro.Histogram.start;
                            profileMacro.Histogram.end = macro.Histogram.end;

                            profileMacro.Filters = new L3.Cargo.Common.Xml.Profile_1_0.ProfileMacroFilter[macro.Filter.Count];
                            int count = 0;

                            foreach (L3.Cargo.Common.Xml.History_1_0.HistoryFilter filterHistory in macro.Filter)
                            {
                                L3.Cargo.Common.Xml.Profile_1_0.ProfileMacroFilter filter = new L3.Cargo.Common.Xml.Profile_1_0.ProfileMacroFilter();
                                filter.id = filterHistory.name;
                                filter.parameter = filterHistory.parameter;

                                profileMacro.Filters[count] = filter;
                                count++;
                            }

                            profile.Macro[macroCount] = profileMacro;

                            macroCount++;
                        }
                    }

                    profile.DensityAlarm = new L3.Cargo.Common.Xml.Profile_1_0.ProfileDensityAlarm();
                    if (profileObj.DensityAlarmValue == 0.0)
                    {
                        // clear the element if the value is 0
                        profile.DensityAlarm = null;
                    }
                    else
                    {
                        profile.DensityAlarm.value = profileObj.DensityAlarmValue;
                    }
                }

            }
            catch (Exception ex)
            {
                //TODO: Log exception here
            }

            return profile;
        }
    }
}
