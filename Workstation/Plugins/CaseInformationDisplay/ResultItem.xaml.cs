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
using L3.Cargo.Common;

namespace L3.Cargo.Workstation.Plugins.CaseInformationDisplay
{
    public partial class ResultItem : Border
    {
        /// <summary>
        /// The result this ResultItem is displaying
        /// </summary>
        private result result;

        public ResultItem()
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
        }

        public ResultItem(result res)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
            this.result = res;
            string decision = result.Decision.Replace(" ", "");
            if (!String.IsNullOrEmpty(L3.Cargo.Common.Resources.ResourceManager.GetString(decision)))
            {
                var binding = new Binding(decision);
                binding.Source = CultureResources.getDataProvider();
                BindingOperations.SetBinding(Decision, TextBlock.TextProperty, binding);
            }
            else
            {
                Decision.Text = result.Decision;
            }

            CreateTime.Text = (result.CreateTime == String.Empty) ? result.CreateTime : CultureResources.ConvertDateTimeToStringForDisplay(DateTime.Parse(result.CreateTime, CultureResources.getDefaultDisplayCulture()));
            Comment.Text = result.Comment;

            string reason = result.Reason.Replace(" ", "");
            if (!String.IsNullOrEmpty(L3.Cargo.Common.Resources.ResourceManager.GetString(reason)))
            {
                var binding = new Binding(reason);
                binding.Source = CultureResources.getDataProvider();
                BindingOperations.SetBinding(Reason, TextBlock.TextProperty, binding);
            }
            else
            {
                Reason.Text = result.Reason;
            }

            User.Text = result.User;
            StationType.Text = result.StationType;
            AnalysisTime.Text = result.AnalysisTime;

            CultureResources.getDataProvider().DataChanged += new EventHandler(CultureResources_DataChanged);

            this.Unloaded += new RoutedEventHandler(ResultItem_Unloaded);
        }

        void CultureResources_DataChanged(object sender, EventArgs e)
        {
            CreateTime.Text = (result.CreateTime == String.Empty) ? result.CreateTime : CultureResources.ConvertDateTimeToStringForDisplay(DateTime.Parse(result.CreateTime, CultureResources.getDefaultDisplayCulture()));
        }

        void ResultItem_Unloaded(object sender, RoutedEventArgs e)
        {
            CultureResources.getDataProvider().DataChanged -= this.CultureResources_DataChanged;
        }
    }
}
