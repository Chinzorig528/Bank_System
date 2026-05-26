using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace QueueDisplayWinForms
{
    public partial class Form1 : Form
    {
        TcpClient client =
            new TcpClient();

        string tellerId =
            "TELLER1";

        public Form1()
        {
            InitializeComponent();

            ConnectToServer();
        }

        private async void ConnectToServer()
        {
            try
            {
                await client.ConnectAsync(
                    "192.168.88.6",
                    5000);

                NetworkStream stream =
                    client.GetStream();

                string registerMessage =
                    "DISPLAY|" + tellerId;

                byte[] registerData =
                    Encoding.UTF8.GetBytes(
                        registerMessage);

                await stream.WriteAsync(
                    registerData,
                    0,
                    registerData.Length);

                ReceiveMessages();
            }
            catch
            {
                MessageBox.Show(
                    "Cannot connect to server");
            }
        }

        private async void ReceiveMessages()
        {
            NetworkStream stream =
                client.GetStream();

            byte[] buffer =
                new byte[1024];

            while (true)
            {
                int byteCount =
                    await stream.ReadAsync(
                        buffer,
                        0,
                        buffer.Length);

                if (byteCount == 0)
                    break;

                string queueNumber =
                    Encoding.UTF8.GetString(
                        buffer,
                        0,
                        byteCount);

                this.Invoke((MethodInvoker)delegate
                {
                    lblQueue.Text =
                        queueNumber;

                    lblCounter.Text =
                        tellerId;
                });
            }
        }
    }
}