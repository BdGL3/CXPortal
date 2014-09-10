using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Xml.Serialization;
using L3.Cargo.Common;

namespace L3.Cargo.Workstation.Plugins.RadiationDetectorData
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    /// 

    public class XRayData
    {
        public Double G_RSP1_Data { get; set; }
        public Double G_RSP2_Data { get; set; }
        public Double N_RSP1_Data { get; set; }
        public Double N_RSP2_Data { get; set; }

        public Double G_RSP1_Data_Bkg { get; set; }
        public Double G_RSP2_Data_Bkg { get; set; }
        public Double N_RSP1_Data_Bkg { get; set; }
        public Double N_RSP2_Data_Bkg { get; set; }

        public Int32 G_RSP1_Alarm { get; set; }
        public Int32 G_RSP2_Alarm { get; set; }
        public Int32 N_RSP1_Alarm { get; set; }
        public Int32 N_RSP2_Alarm { get; set; }
    }
    

    public class XRAyDataCollection : Collection<XRayData>
    {
        public XRAyDataCollection()
        {
        }
    } 


    public partial class UserControl1 : UserControl, IDisposable
    {
        private CaseObject m_CaseObject;
        XRAyDataCollection dt= new XRAyDataCollection();

        public UserControl1(CaseObject caseObj)
        {
            m_CaseObject = caseObj;
            InitializeComponent();
            CultureResources.getDataProvider().DataChanged += new EventHandler(CultureResources_DataChanged);
            // fire an event to simulate a culture change so the language is displayed correctly
            CultureResources_DataChanged(null, null);

            GetCaseMeasurementsData();
        }

        private void CultureResources_DataChanged(object sender, EventArgs e)
        {
            // handle culture change
            // update the legend names
            Chart_Series_Warnings.Name = L3.Cargo.Common.Resources.Warnings;
            Chart_Series_Alarms.Name = L3.Cargo.Common.Resources.Alarms;
            Chart_Series_NORMAlarms.Name = L3.Cargo.Common.Resources.NORMAlarms;
            // the other legend items do not appear to be translatable.

            Radiation_Chart.Invalidate();
        }

        private void GetCaseMeasurementsData()
        {
            if (m_CaseObject.attachments.CountofType(AttachmentType.SNM) > 0)
            {
                foreach (DataAttachment attachment in m_CaseObject.attachments.GetSNMAttachments())
                {
                    try
                    {
                        //Attachment information in the two elements below  //attachment.attachmentData;
                        string s = attachment.attachmentId;
                        XmlSerializer serializer = new XmlSerializer(typeof(Measurements));
                        Measurements snmData = (Measurements)serializer.Deserialize(attachment.attachmentData);
                        PrepareChartData(snmData, dt);

                        for (int k = 0; k < dt.Count; k++)
                        {
                            if (dt[k].G_RSP1_Alarm > 0)
                            {
                                if (dt[k].G_RSP1_Alarm == 1)
                                {
                                    Chart_Series_Warnings.Points.AddXY(k, dt[k].G_RSP1_Data);
                                }
                                else if (dt[k].G_RSP1_Alarm == 2)
                                {
                                    Chart_Series_Alarms.Points.AddXY(k, dt[k].G_RSP1_Data);
                                }
                                else if (dt[k].G_RSP1_Alarm == 3)
                                {
                                    Chart_Series_NORMAlarms.Points.AddXY(k, dt[k].G_RSP1_Data);
                                }
                            }

                            if (dt[k].G_RSP2_Alarm > 0)
                            {
                                if (dt[k].G_RSP2_Alarm == 1)
                                {
                                    Chart_Series_Warnings.Points.AddXY(k, dt[k].G_RSP2_Data);
                                }
                                else if (dt[k].G_RSP2_Alarm == 2)
                                {
                                    Chart_Series_Alarms.Points.AddXY(k, dt[k].G_RSP2_Data);
                                }
                                else if (dt[k].G_RSP2_Alarm == 3)
                                {
                                    Chart_Series_NORMAlarms.Points.AddXY(k, dt[k].G_RSP2_Data);
                                }
                            }

                            if (dt[k].N_RSP1_Alarm > 0)
                            {
                                if (dt[k].N_RSP1_Alarm == 1)
                                {
                                    Chart_Series_Warnings.Points.AddXY(k, dt[k].N_RSP1_Data);
                                }
                                else if (dt[k].N_RSP1_Alarm == 2)
                                {
                                    Chart_Series_Alarms.Points.AddXY(k, dt[k].N_RSP1_Data);
                                }
                                else if (dt[k].N_RSP1_Alarm == 3)
                                {
                                    Chart_Series_NORMAlarms.Points.AddXY(k, dt[k].N_RSP1_Data);
                                }
                            }

                            if (dt[k].N_RSP2_Alarm > 0)
                            {
                                if (dt[k].N_RSP2_Alarm == 1)
                                {
                                    Chart_Series_Warnings.Points.AddXY(k, dt[k].N_RSP2_Data);
                                }
                                else if (dt[k].N_RSP2_Alarm == 2)
                                {
                                    Chart_Series_Alarms.Points.AddXY(k, dt[k].N_RSP2_Data);
                                }
                                else if (dt[k].N_RSP2_Alarm == 3)
                                {
                                    Chart_Series_NORMAlarms.Points.AddXY(k, dt[k].N_RSP2_Data);
                                }
                            }

                            Chart_Series1.Points.AddXY(k, dt[k].G_RSP1_Data);
                            Chart_Series2.Points.AddXY(k, dt[k].G_RSP2_Data);
                            Chart_Series3.Points.AddXY(k, dt[k].N_RSP1_Data);
                            Chart_Series4.Points.AddXY(k, dt[k].N_RSP2_Data);
                        }

                        Chart_ChartArea.AxisX.Maximum = dt.Count - 1;
                    }
                    catch (Exception ex)
                    {
                        //TODO: Log Bad File
                    }
                }
            }
        }

        private void PrepareChartData(Measurements snmData, XRAyDataCollection dt)
        {
            int cnt = snmData.Sample.Length;
            if (cnt > 0)
            {
                for (int j = 0; j < cnt; j++)
                {
                    XRayData xrd = new XRayData();

                    xrd.G_RSP1_Data = ConvertToDouble(snmData.Sample[j].G_RSP1.Data);
                    xrd.G_RSP2_Data = ConvertToDouble(snmData.Sample[j].G_RSP2.Data);
                    xrd.N_RSP1_Data = ConvertToDouble(snmData.Sample[j].N_RSP1.Data);
                    xrd.N_RSP2_Data = ConvertToDouble(snmData.Sample[j].N_RSP2.Data);
                    xrd.G_RSP1_Data_Bkg = ConvertToDouble(snmData.Sample[j].G_RSP1.Bkg);
                    xrd.G_RSP2_Data_Bkg = ConvertToDouble(snmData.Sample[j].G_RSP2.Bkg);
                    xrd.N_RSP1_Data_Bkg = ConvertToDouble(snmData.Sample[j].N_RSP1.Bkg);
                    xrd.N_RSP2_Data_Bkg = ConvertToDouble(snmData.Sample[j].N_RSP2.Bkg);

                    xrd.G_RSP1_Alarm = Convert.ToInt32(snmData.Sample[j].G_RSP1.Alarm);
                    xrd.G_RSP2_Alarm = Convert.ToInt32(snmData.Sample[j].G_RSP2.Alarm);
                    xrd.N_RSP1_Alarm = Convert.ToInt32(snmData.Sample[j].N_RSP1.Alarm);
                    xrd.N_RSP2_Alarm = Convert.ToInt32(snmData.Sample[j].N_RSP2.Alarm);

                    xrd.G_RSP1_Data -= xrd.G_RSP1_Data_Bkg;
                    xrd.G_RSP2_Data -= xrd.G_RSP2_Data_Bkg;
                    xrd.N_RSP1_Data -= xrd.N_RSP1_Data_Bkg;
                    xrd.N_RSP2_Data -= xrd.N_RSP2_Data_Bkg;

                    if (xrd.G_RSP1_Data < 0)
                    {
                        xrd.G_RSP1_Data *= -1;
                    }

                    if (xrd.G_RSP2_Data < 0)
                    {
                        xrd.G_RSP2_Data *= -1;
                    }

                    if (xrd.N_RSP1_Data < 0)
                    {
                        xrd.N_RSP1_Data *= -1;
                    }

                    if (xrd.N_RSP2_Data < 0)
                    {
                        xrd.N_RSP2_Data *= -1;
                    }

                    dt.Add(xrd);
                }
            }
        }

        private Double ConvertToDouble(String data)
        {
            return Convert.ToDouble(data.Replace(",", "."));
        }


        public void Dispose ()
        {
            CultureResources.getDataProvider().DataChanged -= this.CultureResources_DataChanged;
            Radiation_Chart.Dispose();
            ChartHost.Dispose();
            ChartHost = null;
            MainPanel.Children.Clear();
        }
    }
}
