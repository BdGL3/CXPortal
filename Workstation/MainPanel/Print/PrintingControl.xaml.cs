using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using L3.Cargo.Workstation.Plugins.Common;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System;
using L3.Cargo.Common;
using System.Windows.Data;
using System.Windows.Media;

namespace L3.Cargo.Workstation.MainPanel.Print
{
    public partial class PrintingControl : UserControl
    {
        #region Private Members

        private PrinterObjects myPrinterObjects;
        private bool printingButtonBindingsSet;

        #endregion Private Members


        #region Public Members

        public PrinterObjects PrinterObjects
        {
            set
            {
                myPrinterObjects = value;
                DataContext = myPrinterObjects;
                UserList.ItemsSource = myPrinterObjects;

                foreach (PrinterObject po in myPrinterObjects)
                {
                    Plugin_ListBox.Items.Add(po.Name);

                    po.OptionsMenu.Visibility = Visibility.Collapsed;
                    ((IPrintForm)po.OptionsMenu).PageUpdated +=new PageUpdatedEventHandler(PageUpdated);
                    ((IPrintForm)po.OptionsMenu).InitializePages();
                }

                PrintPreviewDocView.FitToMaxPagesAcross(1);
                DocumentViewer.FitToMaxPagesAcrossCommand.Execute("1", PrintPreviewDocView);
            }
            get
            {
                return myPrinterObjects;
            }
        }

        public Popup ParentPopup { get; set; }

        #endregion Public Members


        #region Constructors

        public PrintingControl ()
            : base()
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
            printingButtonBindingsSet = false;

            PrintPreviewDocView.Document = new FixedDocument();
            ContentControl cc = PrintPreviewDocView.Template.FindName("PART_FindToolBarHost", PrintPreviewDocView) as ContentControl;
            cc.Visibility = Visibility.Collapsed;

            var contentHost = PrintPreviewDocView.Template.FindName("PART_ContentHost", PrintPreviewDocView) as ScrollViewer;
            var grid = contentHost.Parent as Grid;
            var child = grid.Children[0];
            ContentControl toolbar = grid.Children[0] as ContentControl;
            toolbar.Loaded += new RoutedEventHandler(toolbar_Loaded);
            
            PrintPreviewDocView.Document = null;
        }

        void toolbar_Loaded(object sender, RoutedEventArgs e)
        {
            var toolbar = sender as ContentControl;

            if (VisualTreeHelper.GetChildrenCount(toolbar) > 0)
            {
                var buttonToolbar = VisualTreeHelper.GetChild(toolbar, 0) as ToolBar;
                var button = buttonToolbar.FindName("PrintButton") as Button;
                var binding = new Binding("Print_CtrlP");
                binding.Source = CultureResources.getDataProvider();
                BindingOperations.SetBinding(button, Button.ToolTipProperty, binding);

                button = buttonToolbar.FindName("CopyButton") as Button;
                binding = new Binding("Copy_CtrlC");
                binding.Source = CultureResources.getDataProvider();
                BindingOperations.SetBinding(button, Button.ToolTipProperty, binding);

                button = buttonToolbar.FindName("ZoomInButton") as Button;
                binding = new Binding("IncreaseTheSizeOfTheContent_CtrlPlus");
                binding.Source = CultureResources.getDataProvider();
                BindingOperations.SetBinding(button, Button.ToolTipProperty, binding);

                button = buttonToolbar.FindName("ZoomOutButton") as Button;
                binding = new Binding("DecreaseTheSizeOfTheContent_CtrlMinus");
                binding.Source = CultureResources.getDataProvider();
                BindingOperations.SetBinding(button, Button.ToolTipProperty, binding);

                button = buttonToolbar.FindName("ActualSizeButton") as Button;
                binding = new Binding("OneHundredPercent_Ctrl1");
                binding.Source = CultureResources.getDataProvider();
                BindingOperations.SetBinding(button, Button.ToolTipProperty, binding);

                button = buttonToolbar.FindName("PageWidthButton") as Button;
                binding = new Binding("PageWidth_Ctrl2");
                binding.Source = CultureResources.getDataProvider();
                BindingOperations.SetBinding(button, Button.ToolTipProperty, binding);

                button = buttonToolbar.FindName("WholePageButton") as Button;
                binding = new Binding("WholePage_Ctrl3");
                binding.Source = CultureResources.getDataProvider();
                BindingOperations.SetBinding(button, Button.ToolTipProperty, binding);

                button = buttonToolbar.FindName("TwoPagesButton") as Button;
                binding = new Binding("TwoPages_Ctrl4");
                binding.Source = CultureResources.getDataProvider();
                BindingOperations.SetBinding(button, Button.ToolTipProperty, binding);

                printingButtonBindingsSet = true;
            }
        }

        #endregion Constructors


        #region Private Methods

        private void PrintBtn_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog pd = new PrintDialog();
            ParentPopup.IsOpen = false;

            try
            {
                pd.PrintDocument(PrintPreviewDocView.Document.DocumentPaginator, "CasePrintOut");
                PrintPreviewDocView.Document = null;
            }
            catch { }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            ParentPopup.IsOpen = false;
        }

        private void SelectAllBtn_Click (object sender, RoutedEventArgs e)
        {
            Plugin_ListBox.SelectAll();
        }

        private void SelectNoneBtn_Click (object sender, RoutedEventArgs e)
        {
            Plugin_ListBox.SelectedItems.Clear();
        }

        private void PluginListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox != null && listBox.IsVisible)
            {
                foreach (PrinterObject po in myPrinterObjects)
                {
                    if (listBox.SelectedItems.Contains(po.Name))
                    {
                        po.IsEnabled = true;
                        po.OptionsMenu.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        po.IsEnabled = false;
                        po.OptionsMenu.Visibility = Visibility.Collapsed;
                    }
                }

                UpdatePrintPreview();
            }
        }

        private void UpdatePrintPreview ()
        {
            // set the document to null to free up memory.
            PrintPreviewDocView.Document = null;
            FixedDocument document = new FixedDocument();

            PrintDialog printDialog = new PrintDialog();

            document.DocumentPaginator.PageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);

            foreach (List<PageContent> pages in myPrinterObjects.PrintDocuments)
            {
                foreach (PageContent pageContent in pages)
                {
                    document.Pages.Add(pageContent);
                }
            }

            PrintPreviewDocView.Document = document;
        }

        #endregion Private Methods


        #region Public Methods

        public void PageUpdated ()
        {
            UpdatePrintPreview();
        }

        #endregion Public Methods
    }
}
