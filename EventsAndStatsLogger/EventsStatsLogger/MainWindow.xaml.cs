using System;
using System.IO;
using System.Data;
using System.Windows;
using System.Threading;
using System.Messaging;
using System.Configuration;
using System.Windows.Controls;
using System.Windows.Threading;
using L3.Cargo.Communications.EventsLogger.Common;

namespace EventAndStatsLogger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Members

        private int _maxDisplayLines;

        private EventsLogger _logger;

        #endregion Private Members


        #region Constructors

        public MainWindow()
        {
            InitializeComponent();

            string uri = (String)ConfigurationManager.AppSettings["ConnectionUri"];
            string MSMQname = (String)ConfigurationManager.AppSettings["MSMQ_Name"];
            string MSMQAddress = (String)ConfigurationManager.AppSettings["MSMQ_Address"];
            string MSMQServiceNamespace = (String)ConfigurationManager.AppSettings["MSMQ_ServiceNamespace"];

            _maxDisplayLines = int.Parse(ConfigurationManager.AppSettings["MaxDisplayedMessages"]);

            try
            {
                if (!MessageQueue.Exists(MSMQname))
                {
                    MessageQueue mq = MessageQueue.Create(MSMQname, true);
                }
            }
            catch (Exception)
            {
                Thread.Sleep(5000);
                Application.Current.Shutdown();
            }

            _logger = new EventsLogger(new Uri(uri), this, MSMQAddress, MSMQServiceNamespace);
        }

        #endregion Constructors


        #region Private Methods

        private void Window_Loaded (object sender, RoutedEventArgs e)
        {
            _logger.Start();
        }

        private void Window_Closed (object sender, EventArgs e)
        {
            _logger.Stop();
        }

        private void DisplayMessage(string message)
        {
            LogDisplayArea.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate()
            {
                if (LogDisplayArea.LineCount > _maxDisplayLines)
                {
                    int last = LogDisplayArea.Text.IndexOf("\r");
                    LogDisplayArea.Text = LogDisplayArea.Text.Substring(last + 1);
                }

                LogDisplayArea.AppendText(message);
                LogDisplayArea.ScrollToEnd();
            }));
        }

        private DataSet GetFilteredDataSet ()
        {
            string selectedType = ((ComboBoxItem)TypeSelection.SelectedItem).Content.ToString();
            selectedType = (String.IsNullOrWhiteSpace(selectedType)) ? string.Empty : selectedType;

            ReportFilter filter = new ReportFilter(selectedType, (DateTime?)ReportdatePickerStart.SelectedDate, (DateTime?)ReportdatePickerEnd.SelectedDate,
                ComputerTextBox.Text, ApplicationTextBox.Text, DescriptionTextBox.Text, UserTextBox.Text);

            return _logger.GetReport(filter);
        }

        private void GenerateButton_Click (object sender, RoutedEventArgs e)
        {
            DataSet ds = GetFilteredDataSet();
            ReportDataGrid.DataContext = (ds != null) ? ds.Tables[0] : null;
        }

        private void ExportButton_Click (object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    DataSet ds = GetFilteredDataSet();

                    using (StreamWriter sw = new StreamWriter(Path.Combine(dialog.SelectedPath, "Export.csv")))
                    {
                        for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
                        {
                            sw.Write(ds.Tables[0].Columns[i].ColumnName);
                            string nextChar = i != ds.Tables[0].Columns.Count ? "," : " ";
                            sw.Write(nextChar);
                        }

                        sw.Write(sw.NewLine);

                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
                            {
                                sw.Write(row[ds.Tables[0].Columns[i]].ToString());
                                string nextChar = i != ds.Tables[0].Columns.Count ? "," : " ";
                                sw.Write(nextChar);
                            }
                            sw.Write(sw.NewLine);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage(ex.Message);
                }
            }
        }

        #endregion Private Methods


        #region Public Methods

        public void LogMessage(String message)
        {
            DisplayMessage(message.Insert(0, DateTime.Now.ToString() + ": ") + "\r");
        }

        public void LogMessage(Event info)
        {
            DisplayMessage(info.DateAndTime + ", " + info.ComputerName + ", " +
                info.Type + ", " + info.Application + ", " + info.Object + ", " + info.Line + ", " + info.Description + "\r");
        }

        #endregion Public Methods
    }
}