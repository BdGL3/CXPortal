using System;
using System.IO;
using System.Windows.Controls;
using System.Xml.Serialization;
using L3.Cargo.Common;

namespace L3.Cargo.Workstation.Plugins.AnalysisHistoryDisplay
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    /// 

    public partial class UserControl1 : UserControl, IDisposable
    {
        private CaseObject m_CaseObject;
        public AnalysisHistory m_AnalysisHistory;
        Stream reader;

        public UserControl1(CaseObject caseObj)
        {
            m_CaseObject = caseObj;
            InitializeComponent();

            reader = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\AnalysisHistory.xml", FileMode.Open);

            XmlSerializer serializer = new XmlSerializer(typeof(AnalysisHistory));

            m_AnalysisHistory = (AnalysisHistory)serializer.Deserialize(reader);

        }

        public void Dispose ()
        {
            reader.Close();
        }
    }
}
