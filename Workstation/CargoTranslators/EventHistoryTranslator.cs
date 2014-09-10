using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using L3.Cargo.Common;
using L3.Cargo.Common.Xml.XCase_2_0;

namespace L3.Cargo.Translators
{
    public class EventHistoryTranslator
    {
        public static List<CaseObject.CaseEventRecord> Translate(Stream EventHistoryXML)
        {            

            XmlSerializer serializer = new XmlSerializer(typeof(EventHistory));
            EventHistory eventHistory = (EventHistory)serializer.Deserialize(EventHistoryXML);

            List<CaseObject.CaseEventRecord> list = null;

            if (eventHistory != null)
            {
                list = new List<CaseObject.CaseEventRecord>();

                foreach (EventRecord record in eventHistory.EventRecord)
                {
                    CaseObject.CaseEventRecord rec = new CaseObject.CaseEventRecord(record.createTime, record.description, false);
                    list.Add(rec);
                }
            }

            return list;
        }

        public static Stream Translate(List<CaseObject.CaseEventRecord> records, Stream EventHis)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(EventHistory));
            EventHistory eventHistory = new EventHistory();

            EventHis.Seek(0, SeekOrigin.Begin);

            List<EventRecord> eventRecList = new List<EventRecord>();

            foreach (CaseObject.CaseEventRecord rec in records)
            {
                EventRecord r = new EventRecord();
                r.createTime = rec.createTime;
                r.description = rec.description;
                eventRecList.Add(r);
            }

            eventHistory.EventRecord = eventRecList.ToArray();

            serializer.Serialize(EventHis, eventHistory);
            EventHis.Seek(0, SeekOrigin.Begin);
            return EventHis;

        }
    }
}
