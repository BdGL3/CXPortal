using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.Threading;
using System.Windows;
using System.Xml.Linq;
using L3.Cargo.Communications.Client;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Interfaces;

namespace AWSCommClient
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private WSCommEndpoint m_AWSCommEndPoint1;

        private WSCommEndpoint m_AWSCommEndPoint2;

        private WSCommEndpoint m_AWSCommEndPoint3;

        private WSCommEndpoint m_AWSCommEndPoint4;

        private String m_AWSId;

        private String m_ActiveCase;

        public Window1()
        {
            InitializeComponent();

            m_AWSId = "AWS1_192.168.0.1";

            m_ActiveCase = String.Empty;

            m_AWSCommEndPoint1 = null;
            m_AWSCommEndPoint2 = null;
            m_AWSCommEndPoint3 = null;
            m_AWSCommEndPoint4 = null;

        }

        private void ListenForAWSComm()
        {
            Boolean createdPanel = false;
            HostDiscovery awsCommDiscovery = new HostDiscovery(typeof(IWSComm), new TimeSpan(0, 0, 0, 1, 0));

            while (!createdPanel)
            {

                Collection<EndpointDiscoveryMetadata> AWSCommEndpoints = awsCommDiscovery.GetAvailableConnections();

                if (AWSCommEndpoints.Count > 0)
                {
                    for (Int32 index = 0; index < AWSCommEndpoints.Count; index++)
                    {

                        foreach (XElement xElement in AWSCommEndpoints[index].Extensions)
                        {
                            try
                            {
                                String alias = xElement.Element("HostAlias").Value;
                                break;
                            }
                            catch
                            {
                                continue;
                            }
                        }

                        InstanceContext awsCommCallback = new InstanceContext(new WSCommCallback());

                        TCPBinding tcpbinding = new TCPBinding();

                        ServiceEndpoint HostEndPoint = new ServiceEndpoint(ContractDescription.GetContract(typeof(IWSComm)), tcpbinding, AWSCommEndpoints[index].Address);

                        if (index == 0)
                        {
                            // Connect and setup our endpoint
                            m_AWSCommEndPoint1 = new WSCommEndpoint(awsCommCallback, HostEndPoint);
                            tabItem1.Visibility = System.Windows.Visibility.Visible;
                            Tab1Grid.Visibility = System.Windows.Visibility.Visible;

                            Thread Conn1Ping = new Thread(new ThreadStart(PingHost));
                            Conn1Ping.Start();
                        }
                        else if (index == 1)
                        {
                            m_AWSCommEndPoint2 = new WSCommEndpoint(awsCommCallback, HostEndPoint);
                            tabItem2.Visibility = System.Windows.Visibility.Visible;
                            Tab2Grid.Visibility = System.Windows.Visibility.Visible;
                        }
                        else if (index == 2)
                        {
                            m_AWSCommEndPoint3 = new WSCommEndpoint(awsCommCallback, HostEndPoint);
                            tabItem3.Visibility = System.Windows.Visibility.Visible;
                            Tab3Grid.Visibility = System.Windows.Visibility.Visible;
                        }
                        else if (index == 3)
                        {
                            m_AWSCommEndPoint4 = new WSCommEndpoint(awsCommCallback, HostEndPoint);
                            tabItem4.Visibility = System.Windows.Visibility.Visible;
                            Tab4Grid.Visibility = System.Windows.Visibility.Visible;
                        }

                        //Show the Tab Page
                    }
                    createdPanel = true;
                }
                else
                {
                    Thread.Sleep(10000);
                }
            }
        }

        private void Tab1Login_Click(object sender, RoutedEventArgs e)
        {
            WorkstationInfo awsInfo = new WorkstationInfo(m_AWSId);
            awsInfo.userInfo.UserName = Tab1UsernameTextBox.Text;
            awsInfo.userInfo.Password = Tab1PasswordTextBox.Text;

            try
            {
                LoginResponse response = m_AWSCommEndPoint1.Login(awsInfo);
                Tab1LogListBox.Items.Add("Result: Success | AuthLevel: "
                    + response.UserAuthenticationLevel.ToString());
            }
            catch (Exception ex)
            {
                Tab1LogListBox.Items.Add("Result: Failed | ErrorMessage: " + ex.Message
                    + " | AuthLevel: None");
            }
        }

        private void Tab1GetCaseListButton_Click(object sender, RoutedEventArgs e)
        {
            CaseListDataSet caseListDataSet = m_AWSCommEndPoint1.RequestCaseList(m_AWSId);

            Tab1CaseListBox.Items.Clear();

            foreach (DataRow row in caseListDataSet.Tables[0].Rows)
            {
                Tab1CaseListBox.Items.Add(row[caseListDataSet.CaseListTable.CaseIdColumn].ToString());
            }
        }

        private void Tab1DecisionButton_Click(object sender, RoutedEventArgs e)
        {
            WorkstationResult awsResult = new WorkstationResult();
           
            
            awsResult.Comment = Tab1CommentTextBox.Text;

            if (Tab1DecisionComboBox.SelectedItem.ToString().Contains("Clear"))
            {
                awsResult.Decision = WorkstationDecision.Clear;
            }
            else if (Tab1DecisionComboBox.SelectedItem.ToString().Contains("Reject"))
            {
                awsResult.Decision = WorkstationDecision.Reject;
            }
            else if (Tab1DecisionComboBox.SelectedItem.ToString().Contains("Unknown"))
            {
                awsResult.Decision = WorkstationDecision.Unknown;
            }
            else
            {
                awsResult.Decision = WorkstationDecision.Caution;
            }

            if (Tab1ReasonComboBox.SelectedItem.ToString().Contains("Not Applicable"))
            {
                awsResult.Reason = WorkstationReason.NotApplicable;
            }
            else if (Tab1ReasonComboBox.SelectedItem.ToString().Contains("Too Complex"))
            {
                awsResult.Reason = WorkstationReason.TooComplex;
            }
            else if (Tab1ReasonComboBox.SelectedItem.ToString().Contains("Too Dense"))
            {
                awsResult.Reason = WorkstationReason.TooDense;
            }
            else
            {
                awsResult.Reason = WorkstationReason.AnomalyIdentified;
            }

            awsResult.UserName = Tab1UsernameTextBox.Text;

            awsResult.WorkstationId = m_AWSId;

            awsResult.CaseId = m_ActiveCase;

            try
            {
                //m_AWSCommEndPoint1.Decision(awsResult);
                Tab1LogListBox.Items.Add("Result: Successed");
            }
            catch (Exception ex)
            {
                Tab1LogListBox.Items.Add("Result: Failed | ErrorMessage: " + ex.Message);
            }
        }

        private void Tab1RequestCaseButton_Click(object sender, RoutedEventArgs e)
        {

            String caseId = String.Empty;

            try
            {
                caseId = Tab1CaseListBox.SelectedItem.ToString();
            }
            catch
            {
            }

            CaseMessage message = new CaseMessage();
            message.CaseId = caseId;
            message.WorkstationId = m_AWSId;

            //make case directory
            if (!Directory.Exists("C:\\Reena"))
                Directory.CreateDirectory("C:\\Reena");

            String DirName = "C:\\Reena\\" + caseId;
            if (!Directory.Exists(DirName))
                Directory.CreateDirectory(DirName);

            try
            {
                //get case xml file
                CaseRequestMessageResponse response = m_AWSCommEndPoint1.RequestCase(message);

                FileStream casexml = new FileStream(DirName + "\\case.xml", FileMode.Create);
                response.file.CopyTo(casexml);
                casexml.Close();
                response.file.Close();
                //GetCaseFiles(DirName, caseid);
                m_ActiveCase = caseId;
            }
            catch (FaultException exp)
            {
                Tab1LogListBox.Items.Add(exp.Message);
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            ListenForAWSComm();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void PingHost()
        {
            while (m_AWSCommEndPoint1 != null)
            {
                try
                {
                    m_AWSCommEndPoint1.Ping(m_AWSId);
                }
                catch
                {
                    m_AWSCommEndPoint1 = null;

                    tabItem1.Dispatcher.BeginInvoke(new Action(
                        delegate()
                        {
                            tabItem1.Visibility = Visibility.Collapsed;
                        }));
                    Tab1Grid.Dispatcher.BeginInvoke(new Action(
                        delegate()
                        {
                            Tab1Grid.Visibility = Visibility.Collapsed;
                        }));
                }

                Thread.Sleep(1000);
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LogOutInfo logOutInfo = new LogOutInfo(m_AWSId);
                m_AWSCommEndPoint1.Logout(logOutInfo);

                Tab1LogListBox.Items.Add("Result: Successful");
            }
            catch (Exception ex)
            {
                Tab1LogListBox.Items.Add(ex.Message.ToString());
            }
        }
    }
}
