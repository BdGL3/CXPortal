using System;
using System.Collections.Generic;
using System.Windows.Controls;
using L3.Cargo.Workstation.Plugins.XRayImageBase;
using System.Windows;

namespace L3.Cargo.Workstation.Plugins.EAFB
{
    /// <summary>
    /// Interaction logic for HistogramDisplay.xaml
    /// </summary>
    public partial class HistogramDisplay : UserControl, IDisposable
    {
        public HistogramDisplay ()
        {
            InitializeComponent();
        }

        public void AddHistogram (Histogram histogram)
        {
            GroupBox groupBox = new GroupBox();
            MainPanel.Children.Add(groupBox);

            groupBox.Header = "Xray Image " + MainPanel.Children.Count.ToString();
            groupBox.Content = histogram;
        }

        public void Dispose ()
        {
            for (int index = MainPanel.Children.Count - 1; index >= 0; index--)
            {
                GroupBox groupBox = MainPanel.Children[index] as GroupBox;

                if (groupBox != null)
                {
                    Histogram histogram = groupBox.Content as Histogram;

                    if (histogram != null)
                    {
                        histogram.Dispose();
                        histogram = null;
                    }
                }

                MainPanel.Children.RemoveAt(index);
            }
        }
    }
}
