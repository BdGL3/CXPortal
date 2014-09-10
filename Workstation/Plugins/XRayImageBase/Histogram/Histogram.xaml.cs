using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using L3.Cargo.Common.Xml.History_1_0;
using L3.Cargo.Workstation.Common;
using System.Windows.Data;
using L3.Cargo.Common;

namespace L3.Cargo.Workstation.Plugins.XRayImageBase
{
    public partial class Histogram : UserControl, IDisposable
    {
        #region Private Members

        private bool m_RangeMouseClicked;

        private bool m_IsColorEnabled;

        private long m_PreviousStart;

        private long m_PreviousEnd;

        private int[] m_DataPoints;

        private History m_History;

        private TimeSpan m_ScanTimeSpan;

        private DispatcherTimer m_DispatchTimer;

        private XrayImageEffect _Effect;

        private ImageBrush m_LowerBoundBrush;

        private ImageBrush m_UpperBoundBrush;

        #endregion Private Members


        #region Constructors

        public Histogram ()
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            PositionA_TextBox.Text = rangeSlider.RangeStart.ToString();
            PositionB_TextBox.Text = rangeSlider.RangeStop.ToString();

            m_PreviousStart = rangeSlider.RangeStart;
            m_PreviousEnd = rangeSlider.RangeStop;

            m_DataPoints = new int[rangeSlider.RangeStop + 1];
            m_ScanTimeSpan = new TimeSpan(0, 0, 0, 0, 50);
            m_RangeMouseClicked = false;

            m_LowerBoundBrush = new ImageBrush();
            m_UpperBoundBrush = new ImageBrush();
            m_LowerBoundBrush.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"Plugins\Histogram\GrayLower.lut", UriKind.Relative));
            m_UpperBoundBrush.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"Plugins\Histogram\GrayUpper.lut", UriKind.Relative));
            m_IsColorEnabled = false;

            m_DispatchTimer = new DispatcherTimer();
            m_DispatchTimer.Tick += new EventHandler(DispatchTimer_Tick);
            m_DispatchTimer.Interval = m_ScanTimeSpan;
        }

        #endregion Constructors


        #region Private Methods

        private void SetDataPoints (Image image, float[] data)
        {
            int width = (int)image.Source.Width;
            int height = (int)image.Source.Height;

            if (width > 0 && height > 0)
            {
                BitmapSource bitmapSource = image.Source as BitmapSource;

                if (bitmapSource != null)
                {
                    int dataPointMax = Convert.ToInt32(rangeSlider.RangeStop);
                    int chartMax = Convert.ToInt32(Chart_ChartArea.AxisX.Maximum);

                    float[] m_Data = data;

                    int[] chartData = new int[chartMax + 1];
                    int value;

                    for (int i = 0; i < m_Data.Length; i++)
                    {
                        try
                        {
                            value = Convert.ToInt32(m_Data[i] * dataPointMax);

                            if (value > dataPointMax)
                            {
                                value = dataPointMax;
                            }

                            if (value < 0.0F)
                            {
                                value = 0;
                            }

                            m_DataPoints[value]++;

                            value = Convert.ToInt32(m_Data[i] * chartMax);

                            if (value > chartMax)
                            {
                                value = chartMax;
                            }

                            if (value < 0.0F)
                            {
                                value = 0;
                            }

                            chartData[value]++;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }

                    m_Data = null;

                    int maxYAxis = 0;

                    for (int k = 0; k < chartData.Length; k++)
                    {
                        Chart_Series.Points.Add(new DataPoint(k, chartData[k]));

                        if (chartData[k] > maxYAxis)
                        {
                            maxYAxis = chartData[k];
                        }
                    }

                    chartData = null;

                    Chart_ChartArea.AxisY.Maximum = maxYAxis + (maxYAxis * 0.1);
                }
            }
        }

        private int GetFirstIndex ()
        {
            int ret = 0;

            for (int i = 0; i < m_DataPoints.Length; i++)
            {
                if (m_DataPoints[i] > 0)
                {
                    ret = i;
                    break;
                }
            }

            return ret;
        }

        private int GetLastNonzeroIndex ()
        {
            int ret = 0;

            for (int i = 0; i < m_DataPoints.Length - 1; i++)
            {
                if (m_DataPoints[i] > 50)
                {
                    ret = i;
                }
            }

            return ret;
        }

        private void UpdateDisplay ()
        {
            ColorButton.IsChecked = m_IsColorEnabled;
            GrayButton.IsChecked = !m_IsColorEnabled;

            if (m_IsColorEnabled)
            {
                _Effect.UpperBoundColor = Colors.Green;
                _Effect.LowerBoundColor = Colors.Blue;
            }
            else
            {
                _Effect.UpperBoundColor = Colors.White;
                _Effect.LowerBoundColor = Colors.Black;
            }

        }

        private void LogHistory ()
        {
            if (m_PreviousStart != rangeSlider.RangeStartSelected || m_PreviousEnd != rangeSlider.RangeStopSelected)
            {
                m_PreviousStart = rangeSlider.RangeStartSelected;
                m_PreviousEnd = rangeSlider.RangeStopSelected;

                if (m_History != null)
                {
                    HistoryHistogram histogram = new HistoryHistogram();
                    histogram.effecttype = m_IsColorEnabled ? "Color" : "Gray";
                    histogram.start = rangeSlider.RangeStartSelected;
                    histogram.end = rangeSlider.RangeStopSelected;
                    m_History.AddStep(histogram);
                }
            }
        }

        private void ApplyHistory (HistoryStep step)
        {
            m_IsColorEnabled = (string.Compare(step.Histogram.effecttype, "Color", true) == 0) ? true : false;

            UpdateDisplay();

            rangeSlider.SetSelectedRange(step.Histogram.start, step.Histogram.end);
        }

        private void Color_Click (object sender, RoutedEventArgs e)
        {
            m_IsColorEnabled = true;

            UpdateDisplay();

            rangeSlider.SetSelectedRange(rangeSlider.RangeStartSelected, rangeSlider.RangeStopSelected);

            LogHistory();
        }

        private void Gray_Click (object sender, RoutedEventArgs e)
        {
            m_IsColorEnabled = false;

            UpdateDisplay();

            rangeSlider.SetSelectedRange(rangeSlider.RangeStartSelected, rangeSlider.RangeStopSelected);

            LogHistory();
        }

        private void RemoveBackground_Click (object sender, RoutedEventArgs e)
        {
            int first = GetFirstIndex();
            int last = GetLastNonzeroIndex();

            rangeSlider.SetSelectedRange(first, last);

            LogHistory();
        }

        private void StartButton_Click (object sender, RoutedEventArgs e)
        {
            m_DispatchTimer.IsEnabled = true;
            m_DispatchTimer.Start();
        }

        private void StopScanButton_Click (object sender, RoutedEventArgs e)
        {
            m_DispatchTimer.Stop();
            m_DispatchTimer.IsEnabled = false;
        }

        private void PositionSet_Button_Click (object sender, RoutedEventArgs e)
        {
            long positionA = -1;
            if (!Int64.TryParse(PositionA_TextBox.Text, out positionA))
            {
                //Error print out
            }

            long positionB = -1;
            if (!Int64.TryParse(PositionB_TextBox.Text, out positionB))
            {
                //Error print out
            }

            if (positionA < positionB && positionA >= rangeSlider.RangeStart && positionB <= rangeSlider.RangeStop)
            {
                rangeSlider.SetSelectedRange(positionA, positionB);
                LogHistory();
            }
        }

        private void Reset_Click (object sender, RoutedEventArgs e)
        {
            rangeSlider.SetSelectedRange(rangeSlider.RangeStart, rangeSlider.RangeStop);

            LogHistory();
        }

        private void rangeSlider_RangeSelectionChanged(object sender, RangeSelectionChangedEventArgs e)
        {
            if (PositionA_TextBox != null && PositionB_TextBox != null)
            {
                PositionA_TextBox.Text = e.NewRangeStart.ToString();
                PositionB_TextBox.Text = e.NewRangeStop.ToString();
            }

            if (_Effect != null)
            {
                _Effect.LowerBound = (double)(e.NewRangeStart) / (double)(rangeSlider.RangeStop);
                _Effect.UpperBound = (double)(e.NewRangeStop) / (double)(rangeSlider.RangeStop);
            }
        }

        private void rangeSlider_PreviewMouseLeftButtonUp (object sender, MouseButtonEventArgs e)
        {
            m_RangeMouseClicked = true;
        }

        private void rangeSlider_MouseLeave (object sender, MouseEventArgs e)
        {
            if (m_RangeMouseClicked)
            {
                LogHistory();
            }

            m_RangeMouseClicked = false;
        }

        private void DispatchTimer_Tick (object sender, EventArgs e)
        {
            long newStart = rangeSlider.RangeStartSelected;
            long newStop = rangeSlider.RangeStopSelected;

            long newStep = (long)(Math.Ceiling(newStop * 0.035));

            if ((rangeSlider.RangeStopSelected != rangeSlider.RangeStartSelected) && (newStep > 0))
            {
                newStop -= newStep;

                if (newStop <= rangeSlider.RangeStartSelected)
                {
                    newStop = rangeSlider.RangeStartSelected;
                }

                rangeSlider.SetSelectedRange(newStart, newStop);
            }
            else
            {
                m_DispatchTimer.Stop();
                m_DispatchTimer.IsEnabled = false;
            }
        }

        #endregion Private Methods


        #region Public Methods

        public void Setup (XrayImageEffect effect, Image image, History history, float[] data)
        {
            _Effect = effect;

            m_History = history;
            m_History.CurrentHistoryChangedEvent += new CurrentHistoryChanged(ApplyHistory);

            HistoryHistogram histogram = new HistoryHistogram();
            histogram.effecttype = "Gray";
            histogram.start = rangeSlider.RangeStart;
            histogram.end = rangeSlider.RangeStop;
            m_History.SetFirstStep(histogram);

            SetDataPoints(image, data);

            m_History.ApplyStep();
        }

        public void Dispose ()
        {
            m_DataPoints = null;
            m_History = null;
            m_LowerBoundBrush = null;
            m_UpperBoundBrush = null;

            rangeSlider.Dispose();
            ChartHost.Dispose();
        }

        #endregion Public Methods
    }
}
