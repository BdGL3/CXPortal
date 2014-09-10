using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using L3.Cargo.Common;
using Microsoft.Win32;
using System.Windows.Data;
using System.Globalization;

namespace L3.Cargo.Workstation.Plugins.CaseInformationDisplay
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl, IDisposable
    {
        private CaseObject m_CaseObj;

        private String m_AddAttachmentFileType;

        private String m_AddAttachmentFileNameWithPath;

        private List<CaseObject.CaseEventRecord> m_EventRecords;

        private List<StatusBarItem> m_statusBarItems;

        private SaveFileDialog m_saveDlg;
        
        public UserControl1(CaseObject caseObj)
        {
            m_CaseObj = caseObj;

            InitializeComponent();
            CultureResources.registerDataProvider(this);

            this.Loaded += new RoutedEventHandler(UserControl1_Loaded);

            InfoDisplayArea.DataContext = m_CaseObj;

            m_EventRecords = new List<CaseObject.CaseEventRecord>();

            listView2.DataContext = m_CaseObj.NewAttachments;            
            m_statusBarItems = new List<StatusBarItem>();                   

            TextBlock caseIdTextBlck = new TextBlock();

            // Bind the case id text to the resource file
            var binding = new Binding("CaseId_Colon");
            binding.Source = CultureResources.getDataProvider();
            BindingOperations.SetBinding(caseIdTextBlck, TextBlock.TextProperty, binding);

            TextBlock textBlck = new TextBlock();
            textBlck.Text = " " + m_CaseObj.CaseId;

            // Put the label and the text next side by side in a stack panel
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.Children.Add(caseIdTextBlck);
            stackPanel.Children.Add(textBlck);

            StatusBarItem item = new StatusBarItem();            
            item.Content = stackPanel;
            item.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            item.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
            m_statusBarItems.Add(item);

            CultureResources.getDataProvider().DataChanged += new EventHandler(CultureResources_DataChanged);

            if (caseObj.ResultsList == null || caseObj.ResultsList.Count == 0)
            {
                ResultsSectionBorder.Visibility = System.Windows.Visibility.Collapsed;
                ResultsView.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                foreach (result result in caseObj.ResultsList)
                {
                    ResultItem resultItem = new ResultItem(result);

                    ResultsView.Children.Add(resultItem);
                }
            }
        }

        private void CultureResources_DataChanged(object sender, EventArgs e)
        {
            CaseInfo_CreateTime.Content = m_CaseObj.createTime.ToString();

            var data = InfoDisplayArea.DataContext;
            InfoDisplayArea.DataContext = null;
            InfoDisplayArea.DataContext = data;

            data = listView2.DataContext;
            listView2.DataContext = null;
            listView2.DataContext = data;
        }

        private void UserControl1_Loaded(object sender, RoutedEventArgs e)
        {            
        }

        public List<StatusBarItem> StatusDisplay
        {
            get { return m_statusBarItems; }
        }

        private void AddAttchBtn_Click(object sender, RoutedEventArgs e)
        {
            TextBox1.Text = String.Empty;
            AddAttachmentPopup.IsOpen = true;            
        }
       

        private void RemoveAttachBtn_Click(object sender, RoutedEventArgs e)
        {
            //remove selected listview item
            int idx = listView2.SelectedIndex;
            DataAttachment attach = listView2.SelectedItem as DataAttachment;
            if (idx >= 0)
            {
                m_CaseObj.NewAttachments.RemoveAt(idx); 
                //m_CaseObj.attachments.RemoveAt(idx);

                if (m_CaseObj.attachments.Count == 0)
                    m_CaseObj.AttachmentsModified = false;
                
                int indx = m_EventRecords.FindIndex(
                //int indx = m_CaseObj.EventRecords.FindIndex(
                    delegate(CaseObject.CaseEventRecord rec)
                    {                        
                        if (rec.description == "manually added attachment - " +
                attach.attachmentType.ToString() + ", " + attach.attachmentId)                        
                            return true;                        
                        else
                            return false;
                    });

                m_EventRecords.RemoveAt(indx);
                //m_CaseObj.EventRecords.RemoveAt(indx);

                if (m_EventRecords.Count == 0)
                //if(m_CaseObj.EventRecords.Count == 0)
                {
                    RemoveAttachBtn.Visibility = System.Windows.Visibility.Hidden;
                    m_CaseObj.EventRecordsModified = false;
                }
            }
            else
                MessageBox.Show(L3.Cargo.Common.Resources.SelectAnAttachmentToRemove);
        }

        private void AddAttachmentBrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (m_saveDlg == null)
            {
                //open browse dialog
                m_saveDlg = new SaveFileDialog();
                m_saveDlg.Filter = "All Files|*.*";
                m_saveDlg.OverwritePrompt = false;

                AddAttachmentPopup.StaysOpen = true;

                if ((Boolean)m_saveDlg.ShowDialog())
                {
                    string path = BindingOperations.GetBindingExpression((ComboBoxItem)comboBox1.SelectedItem, ComboBoxItem.ContentProperty).ParentBinding.Path.Path;
                    m_AddAttachmentFileType = path;
                    m_AddAttachmentFileNameWithPath = m_saveDlg.FileName;
                    TextBox1.Text = m_saveDlg.FileName.Substring(m_saveDlg.FileName.LastIndexOf("\\") + 1);
                }
                m_saveDlg = null;
                AddAttachmentPopup.StaysOpen = true;
            }            
        }

        private void AddAttachmentOKBtn_Click(object sender, RoutedEventArgs e)
        {
            if (m_AddAttachmentFileNameWithPath != null)
            {
                DataAttachment attachment = new DataAttachment();

                if (m_AddAttachmentFileType == "NuclearIsotopeAnalysis")
                    attachment.attachmentType = AttachmentType.NUC;
                else if (m_AddAttachmentFileType == "GammaNeutronProfile")
                    attachment.attachmentType = AttachmentType.SNM;
                else if (m_AddAttachmentFileType == "OCRImage")
                    attachment.attachmentType = AttachmentType.OCR;
                else
                    attachment.attachmentType = AttachmentType.Unknown;

                attachment.attachmentId = m_AddAttachmentFileNameWithPath.Substring(m_AddAttachmentFileNameWithPath.LastIndexOf("\\") + 1);
                FileStream strm = File.OpenRead(m_AddAttachmentFileNameWithPath);
                attachment.attachmentData = new MemoryStream();
                attachment.CreateTime = CultureResources.ConvertDateTimeToStringForData(DateTime.Now);
                strm.CopyTo(attachment.attachmentData);
                attachment.attachmentData.Seek(0, SeekOrigin.Begin);
                strm.Close();
                attachment.IsNew = true;

                m_CaseObj.NewAttachments.Add(attachment);
                //m_CaseObj.attachments.Add(attachment);
                CaseObject.CaseEventRecord eventRc = new CaseObject.CaseEventRecord(DateTime.Now, "manually added attachment - " +
                    attachment.attachmentType.ToString() + ", " + attachment.attachmentId, true);
                m_EventRecords.Add(eventRc);
                //m_CaseObj.EventRecords.Add(eventRc);
                //m_CaseObj.EventRecordsModified = true;
                AddAttachmentPopup.IsOpen = false;
                RemoveAttachBtn.Visibility = System.Windows.Visibility.Visible;
                AddAttachmentPopup.IsOpen = false;
                m_saveDlg = null;
                m_AddAttachmentFileNameWithPath = null;
            }
                
        }

        private void AddAttachmentCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (m_saveDlg == null)
            {
                AddAttachmentPopup.IsOpen = false;
                m_saveDlg = null;
                m_AddAttachmentFileNameWithPath = null;
            }            
        }

        #region IDisposable
        public void Dispose()
        {
            CultureResources.getDataProvider().DataChanged -= this.CultureResources_DataChanged;
            foreach (CaseObject.CaseEventRecord record in m_EventRecords)
            {
                m_CaseObj.EventRecords.Add(record);               
            }

            if (m_EventRecords.Count > 0)
                m_CaseObj.EventRecordsModified = true;

            m_CaseObj = null;
        }
        #endregion        

        private void listView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //open the selected item if its type is unknown.
            try
            {
                ListView view = sender as ListView;
                DataAttachment attachment = view.SelectedItem as DataAttachment;

                if (attachment.attachmentType == AttachmentType.Unknown)
                {
                    //open unknown file using default shell program.
                    FileStream fs = new FileStream(attachment.attachmentId, FileMode.Create, FileAccess.Write);
                    attachment.attachmentData.Seek(0, SeekOrigin.Begin);
                    attachment.attachmentData.CopyTo(fs);
                    fs.Close();

                    System.Diagnostics.Process.Start(attachment.attachmentId);
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }

            return;
        }

        private void ContainerIdEditBtn_Click(object sender, RoutedEventArgs e)
        {
            EditContainerIdPopup.IsOpen = true;
        }

        private void EditContainerIdCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            EditContainerIdPopup.IsOpen = false;
        }

        private void EditContainerIdOKBtn_Click(object sender, RoutedEventArgs e)
        {
            if (m_CaseObj.scanInfo == null)
            {
                m_CaseObj.scanInfo = new ScanInfo();
            }

            if (m_CaseObj.scanInfo.container == null)
            {
                m_CaseObj.scanInfo.container = new Container();
            }

            m_CaseObj.scanInfo.container.Id = ContainerIdTextBox.Text;
            m_CaseObj.ScanContainerIdModified = true;
            //m_CaseObj.ScanContainerIdModified = true;
            EditContainerIdPopup.IsOpen = false;
        }

        private void EditContainerIdPopup_Closed(object sender, EventArgs e)
        {

        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

        }
    }
}


/*
 *     <Grid Name="grid1">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="0*" x:Name="Col0"/>
        </Grid.ColumnDefinitions>        
        <Grid.RowDefinitions>
            <RowDefinition  Height ="10" x:Name="Row1"/>
            <RowDefinition Height ="100" x:Name="Row2"/>
            <RowDefinition Height ="Auto" x:Name="Row3"/>
        </Grid.RowDefinitions>
        <Canvas Grid.Row="0" Margin="0,0,0,0" Name="canvas1" HorizontalAlignment="Center"  Grid.Column="0">
            <Label Content="Available images" Height="Auto" HorizontalAlignment="Center" Margin="0,0,0,0" Name="label1" VerticalAlignment="Top" Width="Auto" />
        </Canvas>
        <Canvas Grid.Row="1" Margin="0,32,0,0" Name="canvas2" Height="105" Grid.Column="0">
            <ListBox Width="Auto" Height="100" HorizontalAlignment="Center"  Margin="0,5,0,0" Name="listBox1" VerticalAlignment="Top" SelectionChanged="listBox1_SelectionChanged">
                <ListBox.ItemsPanel>
                    <ItemsPanelTemplate>
                        <StackPanel Orientation="Horizontal"/>
                    </ItemsPanelTemplate>
                </ListBox.ItemsPanel>
            </ListBox>
        </Canvas>
        <Canvas Grid.Row="2" Margin="0,0,0,0" Name="canvas3" Width ="Auto" Height="Auto" Grid.ColumnSpan="2" HorizontalAlignment="Center" VerticalAlignment="Center">
            <Image Name="ViewImage" StretchDirection="Both" Stretch="Fill" UseLayoutRounding="True" />
        </Canvas>

    </Grid>

 
     <Canvas Name="stackPanel" DataContext="{StaticResource CaseObj}">
        <!--StackPanel.Resources>
            <local:CaseObject x:Key="CaseObj"/>
        </StackPanel.Resources-->
        <Label Name="lblCaseId" Content="{Binding Path=CaseId}"  FontSize="20" FontWeight="Bold" Height="57" Width="591" />

        <Label Name="lblSiteId0" Content="Site ID"  FontSize="12" FontWeight="Normal" Canvas.Left="91" Canvas.Top="31" />
        <Label Name="lblSiteId" Content="{Binding Path=SiteId}"  FontSize="12" FontWeight="Normal" Canvas.Left="142" Canvas.Top="31" Height="26" />

        <Label Name="lblAttachments" Content="Case Attachments"  FontSize="12" FontWeight="Normal" Canvas.Left="0" Canvas.Top="259" />
        <ListView DockPanel.Dock="Top" Name="listView1" ItemsSource="{Binding Path=attachments}" Canvas.Left="0" Canvas.Top="279">
            <ListView.View>
                <GridView>
                    <GridViewColumn Header="ID" HeaderStringFormat="Case" Width="150" DisplayMemberBinding="{Binding Path=attachmentId}"/>
                    <GridViewColumn Header="Type" HeaderStringFormat="AWS" Width="125" DisplayMemberBinding="{Binding Path=attachmentType}" />
                </GridView>
            </ListView.View>
        </ListView>
        <Label Name="lblSourceAlias0" Content="Case Source Name"  FontSize="12" FontWeight="Normal" Canvas.Left="26" Canvas.Top="63" />
        <Label Name="lblSourceAlias" Content="{Binding Path=SourceAlias}"  FontSize="12" FontWeight="Normal" Canvas.Left="142" Canvas.Top="63" Height="26" />

        <Label Name="lblcreateTime0" Content="Create Time"  FontSize="12" FontWeight="Normal" Canvas.Left="62" Canvas.Top="95" />
        <Label Name="lblcreateTime" Content="{Binding Path=createTime}"  FontSize="12" FontWeight="Normal" Canvas.Left="142" Canvas.Top="95" />

        <Label Name="lblVehicleId0" Content="VehicleId"  FontSize="12" FontWeight="Normal" Canvas.Left="77" Canvas.Top="127" />
        <Label Name="lblVehicleId" Content="{Binding Path=VehicleId}"  FontSize="12" FontWeight="Normal" Canvas.Left="142" Canvas.Top="127" Height="25" />

        <Label Name="lblWeight0" Content="Weight"  FontSize="12" FontWeight="Normal" Canvas.Left="87" Canvas.Top="159" />
        <Label Name="lblWeight" Content="{Binding Path=Weight}"  FontSize="12" FontWeight="Normal" Canvas.Left="142" Canvas.Top="159" />

        <Label Name="lblWeightSpecified0" Content="Weight Specified"  FontSize="12" FontWeight="Normal" Canvas.Left="36" Canvas.Top="191" />
        <Label Name="lblWeightSpecified" Content="{Binding Path=WeightSpecified}"  FontSize="12" FontWeight="Normal" Canvas.Left="142" Canvas.Top="191" />

        <Label Name="lblMaxManifest0" Content="Max Manifests"  FontSize="12" FontWeight="Normal" Canvas.Left="49" Canvas.Top="223" />
        <Label Name="lblMaxManifest" Content="{Binding Path=MaxManifests}"  FontSize="12" FontWeight="Normal" Canvas.Left="142" Canvas.Top="223" />

        <TextBlock Text="{Binding Path=Caption, diagnostics:PresentationTraceSources.TraceLevel=High}" />
    </Canvas>
    
 */