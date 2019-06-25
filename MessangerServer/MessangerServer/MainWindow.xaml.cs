using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MessangerServer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Thread serverThread;
        TcpListener server;

        List<TcpClient> clients = new List<TcpClient>();

        public MainWindow()
        {
            InitializeComponent();

            startStopButton.Tag = false;

            ipsComboBox.Items.Add("0.0.0.0");
            ipsComboBox.Items.Add("127.0.0.1");

            foreach(IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                ipsComboBox.Items.Add(ip.ToString());
            }

            ipsComboBox.SelectedIndex = 0;
        }

        delegate void DelegateAppendTextBox(TextBox tb, string str);

        private void AppendTextBox(TextBox tb, string str)
        {
            tb.Text += str + "\n";
        }

        private void PortTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if( (e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) )
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void StartStopButtonClick(object sender, RoutedEventArgs e)
        {
            if(!(bool)startStopButton.Tag)
            {
                startStopButton.Content = "Stop";
                startStopButton.Tag = true;

                server = new TcpListener(IPAddress.Parse(ipsComboBox.SelectedItem.ToString()), int.Parse(portTextBox.Text));
                server.Start(100);

                serverThread = new Thread(ServerThreadRoutine);
                serverThread.IsBackground = true;
                serverThread.Start(server);

                logTextBox.Text += "Server have started his work\n";
            }
            else
            {
                startStopButton.Content = "Start";
                startStopButton.Tag = false;

                server.Stop();
                clients.Clear();
            }
        }

        private void ServerThreadRoutine(object state)
        {
            TcpListener server = state as TcpListener;
            byte[] buf = new byte[4 * 1024];
            int recSize;

            try
            {
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();


                    recSize = client.Client.Receive(buf);
                    client.Client.Send(Encoding.UTF8.GetBytes("Hello " + Encoding.UTF8.GetString(buf, 0, recSize) + "\n"));
                    clients.Add(client);

                    Dispatcher.Invoke(new DelegateAppendTextBox(AppendTextBox), new object[] {logTextBox, $"Client {client.Client.LocalEndPoint} added\n" });

                    ThreadPool.QueueUserWorkItem(ClientThreadRoutine, client);
                }
            }
            catch(InvalidOperationException)
            {

            }
            catch(SocketException)
            {

            }
        }

        private void ClientThreadRoutine(object state)
        {
            TcpClient client = state as TcpClient;
            byte[] buf = new byte[4 * 1024];
            int recSize;

            while(client.Connected)
            {
                try
                {
                    recSize = client.Client.Receive(buf);

                    Dispatcher.Invoke(new DelegateAppendTextBox(AppendTextBox), new object[] { logTextBox, $"Client {client.Client.LocalEndPoint} wrote {Encoding.UTF8.GetString(buf, 0, recSize)}\n" });

                    foreach (TcpClient sendClient in clients)
                    {
                        sendClient.Client.Send(Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(buf, 0, recSize) + "\n"));
                    }
                }
                catch(SocketException)
                {

                }
            }

            clients.Remove(client);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
    }
}
