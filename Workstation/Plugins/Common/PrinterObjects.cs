using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using System.Collections.Generic;

namespace L3.Cargo.Workstation.Plugins.Common
{
    public class PrinterObjects : ObservableCollection<PrinterObject>
    {
        public PrinterObjects ()
        {
        }

        public Collection<List<PageContent>> PrintDocuments
        {
            get
            {
                Collection<List<PageContent>> printDocuments = new Collection<List<PageContent>>();

                foreach (PrinterObject printerObj in this.Items)
                {
                    if (printerObj.IsEnabled)
                    {
                        printDocuments.Add(printerObj.PrintDocument);
                    }
                }

                return printDocuments;
            }
        }
    }


    public class PrinterObject
    {
        private bool m_IsEnabled;

        private UserControl m_OptionsMenu;

        private TextBlock m_Name;

        public bool IsEnabled
        {
            get
            {
                return m_IsEnabled;
            }
            set
            {
                m_IsEnabled = value;
            }
        }

        public UserControl OptionsMenu
        {
            get
            {
                return m_OptionsMenu;
            }
        }

        public TextBlock Name
        {
            get
            {
                return m_Name;
            }
        }

        public List<PageContent> PrintDocument
        {
            get
            {
                if (OptionsMenu != null)
                {
                    return ((IPrintForm)OptionsMenu).PrintPage();
                }
                else
                {
                    return null;
                }
            }
        }

        public PrinterObject (string name, UserControl OptionMenu)
        {
            m_IsEnabled = false;
            m_Name = new TextBlock();
            m_Name.Text = name;

            m_OptionsMenu = OptionMenu;
        }
    }
}
