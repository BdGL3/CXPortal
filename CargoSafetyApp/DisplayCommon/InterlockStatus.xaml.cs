using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using L3.Cargo.Common.Dashboard;

namespace L3.Cargo.Safety.Display.Common
{
    /// <summary>
    /// Interaction logic for InterlockStatus.xaml
    /// </summary>
    public partial class InterlockStatus : UserControl
    {
        private TagCollection _interlockTags;

        public InterlockStatus ()
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
            CultureResources.getDataProvider().DataChanged += new EventHandler(CultureResources_DataChanged);

            _interlockTags = (TagCollection)FindResource("interlockCollection");
        }

        public InterlockStatus (TagUpdate tagupdate)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
            CultureResources.getDataProvider().DataChanged += new EventHandler(CultureResources_DataChanged);
        }

        private void CultureResources_DataChanged(object sender, EventArgs e)
        {
            // refresh the item source to translate
            var dataContext = InterlockListView.ItemsSource;
            InterlockListView.ItemsSource = null;
            InterlockListView.ItemsSource = dataContext;
        }

        public void UpdateTagsCollection(string tagDisplayName, string tagValue, Dispatcher dispatcher)
        {
            bool tagFound = false;

            dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                foreach (TagInfo estop in _interlockTags)
                {
                    if (estop.TagName.Equals(tagDisplayName))
                    {
                        int index = _interlockTags.IndexOf(estop);

                        _interlockTags.RemoveAt(index);

                        estop.TagName = tagDisplayName;
                        estop.TagValue = tagValue;
                        _interlockTags.Insert(index, estop);
                        tagFound = true;
                        break;
                    }
                }

                if (!tagFound)
                {
                    TagInfo estop = new TagInfo();
                    estop.TagValue = tagValue;
                    estop.TagName = tagDisplayName;
                    _interlockTags.Add(estop);
                }
            }));
        }
    }
}
