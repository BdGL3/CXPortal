using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows;
using L3.Cargo.Common;
using System.Windows.Data;
using System.Threading;
using System.Globalization;

namespace L3.Cargo.Communications.Interfaces
{
    public class CaseListSearch
    {
        public ObservableCollection<SearchCaseListCriteriaItem> SearchCaseListCriteriaItemList;
        public ObservableCollection<SearchCaseListCriteriaItem> AddSearchCriteriaCaseList;
        public ObservableCollection<SearchCaseListCriteriaItem> DeleteSearchCriteriaCaseList;

        public delegate void RefreshCaseListHandler(object sender, RoutedEventArgs e);

        public CaseListSearch(String[] criteria, RefreshCaseListHandler refereshList)
        {
            SearchCaseListCriteriaItemList = new ObservableCollection<SearchCaseListCriteriaItem>();
            AddSearchCriteriaCaseList = new ObservableCollection<SearchCaseListCriteriaItem>();
            DeleteSearchCriteriaCaseList = new ObservableCollection<SearchCaseListCriteriaItem>();

            foreach (String val in criteria)
            {
                SearchCaseListCriteriaItem newItem = CreateCaseListCriteriaItem(val, refereshList);
                SearchCaseListCriteriaItemList.Add(newItem);
                AddSearchCriteriaCaseList.Add(newItem);
            }
        }

        public SearchCaseListCriteriaItem GetListCriteriaItem(String item)
        {
            foreach (SearchCaseListCriteriaItem listItem in SearchCaseListCriteriaItemList)
            {
                if (listItem.Item == item)
                {
                    return listItem;
                }
            }
            return null;
        }

        public SearchCaseListCriteriaItem GetListCriteriaItem(String item, string initialVal)
        {
            SearchCaseListCriteriaItem returnItem = GetListCriteriaItem(item);
            if (returnItem != null)
            {
                UIElement element = returnItem.Element;
                if (element is TextBox)
                {
                    TextBox textBox = (TextBox)element;
                    if (initialVal == "*")
                    {
                        initialVal = "";
                    }
                    textBox.Text = initialVal;
                }
                else if (element is StackPanel)
                {
                    StackPanel stackPanel = (StackPanel)element;

                    DatePicker startDate = (DatePicker)stackPanel.Children[0];
                    DatePicker endDate = (DatePicker)stackPanel.Children[1];

                    startDate.SelectedDate = DateTime.Now.AddDays(-int.Parse(initialVal));
                    endDate.SelectedDate = DateTime.Now;
                }
                else if (element is ComboBox)
                {
                    ComboBox comboBox = (ComboBox)element;
                    string[] names = Enum.GetNames(typeof(WorkstationDecision));
                    if (names.Contains(initialVal))
                    {
                        int index = Array.IndexOf(names, initialVal);
                        comboBox.SelectedIndex = index;
                    }
                }
            }
            return returnItem;
        }

        public static SearchCaseListCriteriaItem CreateCaseListCriteriaItem(String item, RefreshCaseListHandler refereshList)
        {
            SearchCaseListCriteriaItem critriaItem;
            UIElement element;

            TextBox textbx = new TextBox();
            textbx.Width = 125;
            textbx.Height = 25;
            textbx.TextChanged += new TextChangedEventHandler(refereshList);
            element = textbx;

            switch (item)
            {
                case "CaseID":
                    {
                        break;
                    }
                case "Analyst":
                    {
                        break;
                    }
                case "AnalystComment":
                    {
                        break;
                    }
                case "FlightNumber":
                    {
                        break;
                    }
                case "ObjectID":
                    {
                        break;
                    }
                case "Area":
                    {
                        break;
                    }
                case "Result":
                    {
                        ComboBox cmbBx = new ComboBox();
                        int[] vals = (int[])Enum.GetValues(typeof(WorkstationDecision));
                        String[] ResultList = new String[vals.Length];
                        int i = 0;
                        foreach (int v in vals)
                        {
                            ResultList[i] = v.ToString(CultureResources.getDefaultDisplayCulture());
                            i++;
                        }
                        DataTemplate dataTemplate = new DataTemplate(typeof(String));
                        var binding = new Binding();
                        binding.Converter = new IntToDecisionConverter();
                        var elemFactory = new FrameworkElementFactory(typeof(ContentPresenter));
                        elemFactory.SetBinding(ContentPresenter.ContentProperty, binding);

                        dataTemplate.VisualTree = elemFactory;

                        cmbBx.ItemTemplate = dataTemplate;
                        cmbBx.ItemsSource = ResultList;
                        cmbBx.Height = 25;
                        cmbBx.Width = 125;
                        cmbBx.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                        cmbBx.SelectionChanged += new SelectionChangedEventHandler(refereshList);


                        CultureResources.getDataProvider().DataChanged += new EventHandler((object sender, EventArgs e) =>
                        {
                            // reset the items source to refresh the display
                            int index = cmbBx.SelectedIndex;
                            var source = cmbBx.ItemsSource;
                            cmbBx.ItemsSource = null;
                            cmbBx.ItemsSource = source;
                            cmbBx.SelectedIndex = index;
                        });

                        element = cmbBx;
                        break;
                    }
                case "UpdateTime":
                    {
                        StackPanel panel = new StackPanel();

                        DatePicker datePicker = new DatePicker();
                        datePicker.IsTodayHighlighted = true;
                        datePicker.SelectedDate = DateTime.Now.AddDays(-7);
                        datePicker.SelectedDateChanged += new EventHandler<SelectionChangedEventArgs>(refereshList);
                        panel.Children.Add(datePicker);

                        datePicker = new DatePicker();
                        datePicker.IsTodayHighlighted = true;
                        datePicker.SelectedDate = DateTime.Now;
                        datePicker.SelectedDateChanged += new EventHandler<SelectionChangedEventArgs>(refereshList);

                        // catch culture changes and refresh the date picker
                        CultureResources.getDataProvider().DataChanged += new EventHandler((object sender, EventArgs e) =>
                        {   
                            datePicker.InvalidateVisual();
                        });

                        panel.Children.Add(datePicker);

                        element = panel;
                        break;
                    }
                default:
                    {
                        element = null;
                        break;
                    }
            }

            if (element != null)
            {
                critriaItem = new SearchCaseListCriteriaItem(element, item);
                return critriaItem;
            }

            return null;
        }

        static void CaseListSearch_DataChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts from the data item key to a resource key for translation.
        /// </summary>
        /// <param name="item">The search criteria item key.</param>
        /// <returns>The language resource key.</returns>
        public static string ItemToResourceKey(string item)
        {
            switch (item)
            {
                case "CaseID":
                    {
                        return "CaseId";
                    }
                case "Analyst":
                    {
                        return "Analyst";
                    }
                case "AnalystComment":
                    {
                        return "AnalystComment";
                    }
                case "FlightNumber":
                    {
                        return "FlightNumber";
                    }
                case "ObjectID":
                    {
                        return "ObjectId";
                    }
                case "Area":
                    {
                        return "Area";
                    }
                case "Result":
                    {
                        return "Result";
                    }
                case "UpdateTime":
                    {
                        return "UpdateTime";
                    }
                default:
                    {
                        return "";
                    }
            }
        }

    }

    public class SearchCaseListCriteriaItem
    {
        String m_Item;
        Label m_ItemLabel;
        UIElement m_ItemElement;

        public SearchCaseListCriteriaItem(UIElement element, String item)
        {
            m_ItemLabel = new Label();
            // Bind the label to the resources
            var binding = new Binding(CaseListSearch.ItemToResourceKey(item));
            binding.Source = CultureResources.getDataProvider();
            BindingOperations.SetBinding(m_ItemLabel, Label.ContentProperty, binding);

            m_ItemLabel.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;

            m_ItemElement = element;
            m_Item = item;
        }

        public Label ItemLabel
        {
            get
            {
                return m_ItemLabel;
            }
            set
            {
                m_ItemLabel = value;
            }
        }

        public string Item
        {
            get
            {
                return m_Item;
            }
        }

        public UIElement Element
        {
            get
            {
                return m_ItemElement;
            }
            set
            {
                m_ItemElement = value;
            }
        }

        public override string ToString()
        {
            return (string)m_ItemLabel.Content;
        }

        public bool MachesCriteria(String srt)
        {
            try
            {
                String ItemContent = m_Item;

                if ((ItemContent == "CaseID") ||
                    (ItemContent == "ObjectID") ||
                    (ItemContent == "FlightNumber") ||
                    (ItemContent == "AnalystComment") ||
                    (ItemContent == "Analyst") ||
                    (ItemContent == "Area" ))
                {
                    TextBox txtbx = (TextBox)m_ItemElement;
                    if (srt.Contains(txtbx.Text))
                        return true;
                    else
                        return false;
                }
                else if (ItemContent == "UpdateTime")
                {
                    StackPanel panel = (StackPanel)m_ItemElement;
                    DatePicker startDate = (DatePicker)panel.Children[0];
                    DatePicker endDate = (DatePicker)panel.Children[1];

                    DateTime time = System.Convert.ToDateTime(srt);

                    if ((time.Date >= startDate.SelectedDate) && (time.Date <= endDate.SelectedDate))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (ItemContent == "Result")
                {
                    ComboBox cmbBox = (ComboBox)m_ItemElement;
                    
                    if (cmbBox.SelectedItem != null)
                    {
                        if (srt.Equals(cmbBox.SelectedItem))
                            return true;
                        else
                            return false;
                    }
                    return true;
                }
                return true;
            }
            catch (Exception exp)
            {
                throw;
            }
        }
    }

    /// <summary>
    /// Class for converting enum of results combo box to the display value.
    /// </summary>
    public class IntToDecisionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (String.IsNullOrEmpty((String)value))
            {
                return value;
            }
            WorkstationDecision decision = (WorkstationDecision)Enum.Parse(typeof(WorkstationDecision), (string)value);
            string translated = L3.Cargo.Common.Resources.ResourceManager.GetString(decision.ToString(), CultureResources.getCultureSetting());
            if (String.IsNullOrEmpty(translated) )
            {
                return decision.ToString();
            }
            else
            {
                return translated;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((int)value).ToString(CultureResources.getDefaultDisplayCulture());
        }
    }
}
