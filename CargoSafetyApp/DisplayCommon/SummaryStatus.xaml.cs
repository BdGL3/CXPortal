using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace L3.Cargo.Safety.Display.Common
{
    /// <summary>
    /// Interaction logic for SummaryStatus.xaml
    /// </summary>
    public partial class SummaryStatus : UserControl
    {
        private TagCollection _summaryStatusTags;

        public SummaryStatus()
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
            CultureResources.getDataProvider().DataChanged += new EventHandler(CultureResources_DataChanged);

            _summaryStatusTags = (TagCollection)FindResource("summaryCollection");
        }

        public SummaryStatus(TagUpdate tagupdate)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
            CultureResources.getDataProvider().DataChanged += new EventHandler(CultureResources_DataChanged);
        }

        private void CultureResources_DataChanged(object sender, EventArgs e)
        {
            // refresh the item source to translate
            var dataContext = SummaryListView.ItemsSource;
            SummaryListView.ItemsSource = null;
            SummaryListView.ItemsSource = dataContext;
        }

        public void UpdateTagsCollection(string tagDisplayName, string tagValue, Dispatcher dispatcher)
        {
            bool tagFound = false;

            dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                foreach (TagInfo summarySignal in _summaryStatusTags)
                {
                    if (summarySignal.TagName.Equals(tagDisplayName))
                    {
                        int index = _summaryStatusTags.IndexOf(summarySignal);

                        _summaryStatusTags.RemoveAt(index);

                        summarySignal.TagName = tagDisplayName;
                        summarySignal.TagValue = tagValue;
                        _summaryStatusTags.Insert(index, summarySignal);
                        tagFound = true;
                        break;
                    }
                }

                if (!tagFound)
                {
                    TagInfo summarySignal = new TagInfo();
                    summarySignal.TagValue = tagValue;
                    summarySignal.TagName = tagDisplayName;
                    _summaryStatusTags.Add(summarySignal);
                }
            }));
        }
    }
}
