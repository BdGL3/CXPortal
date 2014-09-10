using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using L3.Cargo.Common;

namespace L3.Cargo.Workstation.Plugins.ManifestImages
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl, IDisposable
    {
        private CaseObject m_CaseObj;

        private String TempDir = AppDomain.CurrentDomain.BaseDirectory + "\\Manifest\\";

        public UserControl1(CaseObject caseObj)
        {
            m_CaseObj = caseObj;

            InitializeComponent();

            foreach (DataAttachment attachment in m_CaseObj.attachments.GetManifestAttachments())
            {
                try
                {
                    if (!Directory.Exists(TempDir))
                    {
                        Directory.CreateDirectory(TempDir);
                    }

                    String filename = TempDir + attachment.attachmentId;

                    if (File.Exists(filename))
                    {
                        File.Delete(filename);
                    }

                    using (MemoryStream ms = new MemoryStream(attachment.attachmentData.GetBuffer()))
                    {
                        FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                        fs.Write(ms.ToArray(), 0, (int)ms.Length);
                        fs.Close();
                    }
                    
                    PDFHolder pf = new PDFHolder();
                    pf.LoadPDF(filename);

                    WindowsFormsHost wfh = new WindowsFormsHost();
                    wfh.Child = pf;
                    TabItem ti1 = new TabItem();
                    ti1.Header = attachment.attachmentId;
                    ti1.Content = wfh;
                    
                    ManifestTab.Items.Add(ti1);
                }
                catch(Exception ex)
                {
                    //TODO: Log Message here.
                }
            }
        }

        public void Dispose ()
        {
            if (Directory.Exists(TempDir))
            {
                foreach (string file in Directory.GetFiles(TempDir))
                {
                    File.Delete(file);
                }
            }

            foreach (TabItem tabItem in ManifestTab.Items)
            {
                WindowsFormsHost wfh = tabItem.Content as WindowsFormsHost;

                if (wfh != null)
                {
                    wfh.Child.Dispose();
                    wfh.Dispose();
                }
            }

            ManifestTab.Items.Clear();

            MainPanel.Children.Clear();

            ManifestTab = null;

            m_CaseObj = null;
        }
    }
}
