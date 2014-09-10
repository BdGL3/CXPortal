using System;
using System.Configuration;
using System.Linq;
using System.Windows;

namespace L3.Cargo.WSCommunications
{
    /// <summary>
    /// Interaction logic for NetworkConfig.xaml
    /// </summary>
    public partial class NetworkConfig : Window
    {

        public NetworkConfig()
        {
            InitializeComponent();

            String allowedIPAddress = (String)ConfigurationManager.AppSettings["AllowedIPList"];
            String availableIPAddress = (String)ConfigurationManager.AppSettings["AvailableIPs"];

            String [] allowedIPAddresses = allowedIPAddress.Split(new String[] { ";" },
                                                                  StringSplitOptions.RemoveEmptyEntries);

            String[] availableIPAddresses = availableIPAddress.Split(new String[] { ";" },
                                                                     StringSplitOptions.RemoveEmptyEntries);

            foreach (String ipAddress in availableIPAddresses)
            {
                if (allowedIPAddresses.Contains(ipAddress))
                {
                    listBox2.Items.Add(ipAddress);
                }
                else
                {
                    listBox1.Items.Add(ipAddress);
                }
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            while (listBox2.SelectedItem != null)
            {
                listBox1.Items.Add(listBox2.SelectedItem);
                listBox2.Items.Remove(listBox2.SelectedItem);
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            while (listBox1.SelectedItem != null)
            {
                listBox2.Items.Add(listBox1.SelectedItem);
                listBox1.Items.Remove(listBox1.SelectedItem);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            String allowedIPAddress = String.Empty;

            for (int index = 0; index < listBox2.Items.Count; index++)
            {
                allowedIPAddress += (String)listBox2.Items[index];

                if (listBox2.Items.Count != index + 1)
                {
                    allowedIPAddress += ";";
                }
            }

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);


            config.AppSettings.Settings.Remove("AllowedIPList");
            config.AppSettings.Settings.Add("AllowedIPList", allowedIPAddress);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
