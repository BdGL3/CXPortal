using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using L3.Cargo.Common;
using System.Windows.Data;

namespace L3.Cargo.Workstation.Plugins.OCRImages
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl, IDisposable
    {
        private List<ImageInfo> imageFiles;

        private CaseObject m_CaseObject;
        //private Boolean m_ContainerIDChanged = false;
        public UserControl1 (CaseObject caseObj)
        {
            imageFiles = new List<ImageInfo>();

            InitializeComponent();
            CultureResources.registerDataProvider(this);

            foreach (DataAttachment attachment in caseObj.attachments.GetOCRAttachments())
            {
                try
                {
                    ImageInfo imgInfo = new ImageInfo();
                    MemoryStream ms = new MemoryStream(attachment.attachmentData.GetBuffer());
                    BitmapImage imageSource = new BitmapImage();

                    imageSource.BeginInit();
                    imageSource.StreamSource = ms;
                    imageSource.CacheOption = BitmapCacheOption.None;
                    imageSource.CreateOptions = BitmapCreateOptions.None;
                    imageSource.EndInit();

                    imageSource.Freeze();

                    imgInfo.imageSource = imageSource;
                    imgInfo.FullName = attachment.attachmentId;

                    imageFiles.Add(imgInfo);
                }
                catch (Exception ex)
                {
                    //TODO: Log Error that this is a bad file.
                }
            }
            m_CaseObject = caseObj;
            ContainerInfoArea.DataContext = m_CaseObject;
            if (!caseObj.IsCaseEditable)
                ContainerIDUpdateBtn.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void UserControl_Loaded (object sender, RoutedEventArgs e)
        {
            listBox1.Items.Clear();

            for (int i = 0; i < imageFiles.Count; i++)
            {
                ListBoxItem lbi = new ListBoxItem();
                Image im = new Image();

                im.Source = ((ImageInfo)imageFiles[i]).imageSource;
                double ratio = im.Source.Height / im.Source.Width;

                lbi.Height = ratio * listBox1.Width;
                im.Width = 0.9 * listBox1.Width;
                im.Height = lbi.Height * 0.9;

                lbi.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;

                im.Stretch = System.Windows.Media.Stretch.Fill;
                lbi.Content = im;
                listBox1.Items.Add(lbi);
            }


            if (listBox1.Items.Count > 0)
                listBox1.SelectedIndex = 0;
        }

        private void listBox1_SelectionChanged (object sender, SelectionChangedEventArgs e)
        {
            int i = listBox1.SelectedIndex;

            if (i != -1)
                ViewImage.Source = imageFiles[i].imageSource;
        }

        public void Dispose ()
        {
            m_CaseObject = null;
            listBox1.Items.Clear();
            imageFiles.Clear();
            imageFiles = null;
        }
        private void ContainerIDUpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            UpdateContainerIDPopup.IsOpen = true;
        }
        private void UpdateContainerIDPopup_Closed(object sender, EventArgs e)
        {
        }
        private void UpdateContainerIDOKBtn_Click(object sender, RoutedEventArgs e)
        {
            if (m_CaseObject.scanInfo == null)
                m_CaseObject.scanInfo = new ScanInfo(null, new Location(String.Empty, String.Empty), new Conveyance(String.Empty, String.Empty, String.Empty),
                    new Container(ContainerIDTextBox.Text, String.Empty, String.Empty, String.Empty));
            else
                m_CaseObject.scanInfo.container.Id = ContainerIDTextBox.Text;
            //m_ContainerIDChanged = true;
            m_CaseObject.ScanContainerIdModified = true;
            UpdateContainerIDPopup.IsOpen = false;
        }
        private void UpdateContainerIDCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            UpdateContainerIDPopup.IsOpen = false;
        }
    }
}
