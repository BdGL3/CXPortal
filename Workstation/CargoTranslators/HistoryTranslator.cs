using System;
using System.IO;
using System.Xml.Serialization;
using L3.Cargo.Common;
using L3.Cargo.Common.Xml.History_1_0;

namespace L3.Cargo.Translators
{
    public class HistoryTranslator
    {
        public static Histories Translate (DataAttachment dataAttachment)
        {
            Histories histories;

            XmlSerializer mySerializer = new XmlSerializer(typeof(Histories));

            try
            {
                histories = (Histories)mySerializer.Deserialize(dataAttachment.attachmentData);
            }
            catch (Exception ex)
            {
                //TODO: Must be an old AnalysisHistory File, throw away and recreate a new one.
                histories = new Histories();
            }

            return histories;
        }

        public static MemoryStream Translate (Histories histories)
        {
            XmlSerializer mySerializer = new XmlSerializer(typeof(Histories));
            MemoryStream strm = new MemoryStream();

            mySerializer.Serialize(strm, histories);
            strm.Seek(0, SeekOrigin.Begin);

            return strm;
        }
    }
}
