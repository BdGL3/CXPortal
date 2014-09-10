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
using System.Windows.Shapes;

namespace L3.Cargo.DetectorPlot
{
    /// <summary>
    /// Interaction logic for Comment.xaml
    /// </summary>
    /// 
  
    public partial class Comment : Window
    {
       public Comment()
        {
            InitializeComponent();
        }

        public string CommentsText
        {
            get
            {
                return textBox1.Text;
            }
            set
            {
                textBox1.Text = value;

            }

        }
        private void AnAddDetBtn_Click(object sender, RoutedEventArgs e)
        {
            //ok
            DialogResult = true;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {//cancel
            DialogResult = false;
        }
    }
}
