using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using L3.Cargo.Common.Dashboard;

namespace L3.Cargo.Safety.Display.Common
{
    /// <summary>
    /// Interaction logic for WarningStatus.xaml
    /// </summary>
    public partial class WarningStatus : UserControl
    {
        private TagCollection _warningTags;

        public WarningStatus ()
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
            CultureResources.getDataProvider().DataChanged += new EventHandler(CultureResources_DataChanged);

            _warningTags = (TagCollection)FindResource("warningCollection");
        }

        public WarningStatus (TagUpdate tagupdate)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
            CultureResources.getDataProvider().DataChanged += new EventHandler(CultureResources_DataChanged);
        }

        private void CultureResources_DataChanged(object sender, EventArgs e)
        {
            // refresh the item source to translate
            var dataContext = WarningListView.ItemsSource;
            WarningListView.ItemsSource = null;
            WarningListView.ItemsSource = dataContext;
        }

        public void UpdateTagsCollection(string tagDisplayName, string tagValue, Dispatcher dispatcher)
        {
            bool tagFound = false;

            dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                foreach (TagInfo estop in _warningTags)
                {
                    if (estop.TagName.Equals(tagDisplayName))
                    {
                        int index = _warningTags.IndexOf(estop);

                        _warningTags.RemoveAt(index);

                        estop.TagName = tagDisplayName;
                        estop.TagValue = tagValue;
                        _warningTags.Insert(index, estop);
                        tagFound = true;
                        break;
                    }
                }

                if (!tagFound)
                {
                    TagInfo estop = new TagInfo();
                    estop.TagValue = tagValue;
                    estop.TagName = tagDisplayName;
                    _warningTags.Add(estop);
                }
            }));
        }
    }
}
