using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace L3.Cargo.Common
{
    public partial class PDFHolder : UserControl
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint GetShortPathName(
           [MarshalAs(UnmanagedType.LPTStr)] string lpszLongPath,
           [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszShortPath,
           uint cchBuffer);

        public PDFHolder()
        {
            InitializeComponent();
        }

        public void LoadPDF(string fileName)
        {
            if (String.IsNullOrWhiteSpace(fileName))
            {
                //Throw an exception here
            }
            else if (fileName.StartsWith("http"))
            {
                pdfWindowLeft.src = fileName;
            }
            else
            {
                try
                {
                    StringBuilder shortFileName = new StringBuilder(4096);
                    uint shortFileNameSize = (uint)shortFileName.Capacity;
                    if (GetShortPathName(fileName, shortFileName, shortFileNameSize) != 0)
                    {
                        pdfWindowLeft.LoadFile(shortFileName.ToString());
                    }
                    else
                    {
                        //throw exception
                    }
                }
                catch (Exception)
                {
                    //throw exception
                    //MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
