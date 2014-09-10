using System.Windows;
using System.Windows.Controls;
using L3.Cargo.Common.Dashboard;
using System.Windows.Data;
using System.Globalization;
using System.Diagnostics;

namespace L3.Cargo.Dashboard.PresentationCore
{
    /// <summary>
    /// Interaction logic for LogoutButton.xaml
    /// </summary>
    public partial class LanguageChangeControl : UserControl
    {
        // flag so we don't set the culture before initializing is finished
        private bool initialized;
        private ComboBox cbLanguages;

        public LanguageChangeControl()
        {
            initialized = false;
            InitializeComponent();
            CultureResources.getDataProvider().DataChanged += new System.EventHandler(LanguageChangeControl_DataChanged);
            CultureResources.registerDataProvider(this);
        }

        private void updateComboBox()
        {
            cbLanguages.SelectedItem = L3.Cargo.Common.Dashboard.Resources.Culture;
        }

        private void cbLanguages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CultureInfo selected_culture = (sender as ComboBox).SelectedItem as CultureInfo;

            //if not current language
            //could check here whether the culture we want to change to is available in order to provide feedback / action
            if (initialized && selected_culture != null && !selected_culture.Equals(L3.Cargo.Common.Dashboard.Resources.Culture))
            {
                Debug.WriteLine(string.Format("Change Current Culture to [{0}]", selected_culture));

                //save language in settings
                //Properties.Settings.Default.CultureDefault = selected_culture;
                //Properties.Settings.Default.Save();

                //change resources to new culture
                CultureResources.ChangeCulture(selected_culture);

                //could apply a theme tied to this culture if desired
            }
        }

        private void cbLanguages_Initialized(object sender, System.EventArgs e)
        {
            initialized = true;
            cbLanguages = (sender as ComboBox);
            updateComboBox();
        }

        private void LanguageChangeControl_DataChanged(object sender, System.EventArgs e)
        {
            if (cbLanguages != null)
            {
                updateComboBox();
            }
        }

        private void Grid_MouseDown(object sender, System.Windows.Input.InputEventArgs e)
        {
            cbLanguages.IsDropDownOpen = true;
        }

        private void ComboBox_TouchDown(object sender, System.Windows.Input.TouchEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                comboBox.IsDropDownOpen = true;
            }
        }
    }
}
