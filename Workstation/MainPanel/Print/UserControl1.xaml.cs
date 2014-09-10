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
using L3.Cargo.Workstation.Plugins.Common;
using L3.Cargo.Common;

namespace L3.Cargo.Workstation.MainPanel.Print
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl, IDisposable
    {
        #region Constructor

        public UserControl1(PrinterObjects printerObjs, Window MainFrameworkWindow)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            PrintingPopup.PlacementTarget = MainFrameworkWindow;

            try
            {
                PrintingDisplay.ParentPopup = PrintingPopup;                
                PrintingDisplay.PrinterObjects = printerObjs;
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        #endregion

        #region public Members        

        #endregion

        #region Private Methods

        private void Print_Button_Click(object sender, RoutedEventArgs e)
        {            
            PrintDialog tmpPrintDlg = new PrintDialog();

            try
            {
                if (tmpPrintDlg.PrintableAreaWidth > 0.0 && tmpPrintDlg.PrintableAreaHeight > 0.0)
                {
                    PrintingPopup.IsOpen = !PrintingPopup.IsOpen;
                }
            }
            catch
            {
                MessageBox.Show("No default printer selected, please select a default printer");  
            }        
        }

        private void PrintingPopup_Closed(object sender, EventArgs e)
        {
            PrintingDisplay.Plugin_ListBox.SelectedIndex = -1;
        }

        private void PrintingPopup_Opened(object sender, EventArgs e)
        {
            PrintingDisplay.Plugin_ListBox.SelectedIndex = PrintingDisplay.Plugin_ListBox.Items.Count - 1;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            PrintingPopup.IsOpen = false;
            PrintingDisplay.Plugin_ListBox.Items.Clear();
            PrintButton.Visibility = Visibility.Collapsed;
        }
        #endregion
    }
}
