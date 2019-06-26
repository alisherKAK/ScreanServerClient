using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace ScreanServer
{
    class Program
    {
        static Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        static Thread serverThread;

        static void Main(string[] args)
        {
            server.Bind(new IPEndPoint(IPAddress.Any, 12345));
            serverThread = new Thread(ListenClients);
            serverThread.IsBackground = true;
            serverThread.Start(server);

            Console.ReadLine();
        }

        private static void ListenClients(object state)
        {
            Socket server = state as Socket;
            EndPoint clientEndPoint = new IPEndPoint(0, 0);
            byte[] buf = new byte[64 * 1024];
            int recSize;

            while (true)
            {
                recSize = server.ReceiveFrom(buf, ref clientEndPoint);

                if (Encoding.UTF8.GetString(buf, 0, recSize).ToLower() == "get sreen")
                {
                    using (Bitmap BM = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height))
                    using (Graphics GH = Graphics.FromImage(BM as Image))
                    {
                        var bytes = server.SendTo(ImageToByte(BM), clientEndPoint);
                        GH.CopyFromScreen(0, 0, 0, 0, BM.Size);
                    }
                }
            }
        }

        private static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
    }
}
