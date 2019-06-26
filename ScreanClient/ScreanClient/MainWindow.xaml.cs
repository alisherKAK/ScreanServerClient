using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScreanClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<byte> imageBytes = new List<byte>();
        Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void GetSreenButtonClick(object sender, RoutedEventArgs e)
        {
            IPAddress serverIp;
            byte[] buf = new byte[64 * 1024];
            int recSize;

            if (IPAddress.TryParse(serverIpTextBox.Text, out serverIp))
            {
                EndPoint endPoint = new IPEndPoint(serverIp, int.Parse(serverPortTextBox.Text));
                client.SendTo(Encoding.UTF8.GetBytes("get sreen"), endPoint);
                recSize = client.ReceiveFrom(buf, ref endPoint);
                imageBytes.AddRange(buf);
            }
        }
    }
}
