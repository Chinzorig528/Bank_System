using System;
using System.Configuration;
using System.Linq;
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
            GetSetting("teller", "TELLER_ID", "TellerId", "AUTO");

        string socketHost =
            GetSetting("socket", "QUEUE_SOCKET_HOST", "SocketHost", "192.168.88.6");

        int socketPort =
            int.TryParse(
                GetSetting("socketPort", "QUEUE_SOCKET_PORT", "SocketPort", "5000"),
                out int configuredSocketPort)
                ? configuredSocketPort
                : 5000;

        public Form1()
        {
            InitializeComponent();

            lblCounter.Text =
                tellerId.Equals(
                    "AUTO",
                    StringComparison.OrdinalIgnoreCase)
                    ? "Connecting..."
                    : tellerId;

            ConnectToServer();
        }

        private async void ConnectToServer()
        {
            try
            {
                await client.ConnectAsync(
                    socketHost,
                    socketPort);

                NetworkStream stream =
                    client.GetStream();

                string registerMessage =
                    "DISPLAY|"
                    + tellerId
                    + "|"
                    + Environment.MachineName;

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

                if (queueNumber.StartsWith("ASSIGNED|"))
                {
                    tellerId =
                        queueNumber.Split('|')[1];

                    this.Invoke((MethodInvoker)delegate
                    {
                        lblCounter.Text =
                            tellerId;
                    });

                    continue;
                }

                this.Invoke((MethodInvoker)delegate
                {
                    lblQueue.Text =
                        queueNumber;

                    lblCounter.Text =
                        tellerId;
                });
            }
        }

        private static string GetSetting(
            string argumentName,
            string environmentName,
            string appSettingName,
            string fallback)
        {
            string argumentValue =
                Environment.GetCommandLineArgs()
                    .Skip(1)
                    .Select(arg => arg.Split(new[] { '=' }, 2))
                    .Where(parts => parts.Length == 2)
                    .Where(parts => parts[0].Equals(
                        "--" + argumentName,
                        StringComparison.OrdinalIgnoreCase))
                    .Select(parts => parts[1])
                    .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(argumentValue))
                return argumentValue;

            string environmentValue =
                Environment.GetEnvironmentVariable(environmentName);

            if (!string.IsNullOrWhiteSpace(environmentValue))
                return environmentValue;

            string appSettingValue =
                ConfigurationManager.AppSettings[appSettingName];

            if (!string.IsNullOrWhiteSpace(appSettingValue))
                return appSettingValue;

            return fallback;
        }
    }
}
