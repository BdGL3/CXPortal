using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using L3.Cargo.Common.Dashboard;

namespace L3.Cargo.Safety.Display.Common
{
    /// <summary>
    /// Interaction logic for EStopStatus.xaml
    /// </summary>
    public partial class EStopStatus : UserControl
    {
        private TagCollection _estopTags;

        public EStopStatus ()
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
            CultureResources.getDataProvider().DataChanged += new EventHandler(CultureResources_DataChanged);

            _estopTags = new TagCollection();

            _estopTags = (TagCollection)FindResource("estopCollection");
        }

        public EStopStatus (TagUpdate tagupdate)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
            CultureResources.getDataProvider().DataChanged += new EventHandler(CultureResources_DataChanged);
        }

        private void CultureResources_DataChanged(object sender, EventArgs e)
        {
            // refresh the item source to translate
            var dataContext = EStopListView.ItemsSource;
            EStopListView.ItemsSource = null;
            EStopListView.ItemsSource = dataContext;
        }

        public void UpdateTagsCollection(string tagDisplayName, string tagValue, Dispatcher dispatcher)
        {
            bool tagFound = false;

            dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                foreach (TagInfo estop in _estopTags)
                {
                    if (estop.TagName.Equals(tagDisplayName))
                    {
                        int index = _estopTags.IndexOf(estop);

                        _estopTags.RemoveAt(index);

                        estop.TagName = tagDisplayName;
                        estop.TagValue = tagValue;
                        _estopTags.Insert(index, estop);
                        tagFound = true;
                        break;
                    }
                }

                if (!tagFound)
                {
                    TagInfo estop = new TagInfo();
                    estop.TagValue = tagValue;
                    estop.TagName = tagDisplayName;
                    _estopTags.Add(estop);
                }
            }));
        }
    }
}
