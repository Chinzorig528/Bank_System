using System;
using System.Buffers;
using System.Configuration;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace QueueDisplayWinForms
{
    /// <summary>
    /// Queue display-ийн WinForms цонх. Socket server-оос teller assignment болон queue дугаар хүлээн авна.
    /// </summary>
    public partial class Form1 : Form
    {
        TcpClient client =
            new TcpClient();

        string tellerId =
            GetSetting("teller", "TELLER_ID", "TellerId", "AUTO");

        string socketHost =
            GetSetting("socket", "QUEUE_SOCKET_HOST", "SocketHost", "172.20.13.121");

        int socketPort =
            int.TryParse(
                GetSetting("socketPort", "QUEUE_SOCKET_PORT", "SocketPort", "5000"),
                out int configuredSocketPort)
                ? configuredSocketPort
                : 5000;

        /// <summary>
        /// Display цонхыг үүсгэж teller ID-г харуулаад socket server-тэй холбогдоно.
        /// </summary>
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

        /// <summary>
        /// Socket server-тэй холбогдож display төхөөрөмжөө бүртгүүлнэ.
        /// </summary>
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
                    + Environment.MachineName
                    + "\n";

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

        /// <summary>
        /// Socket server-оос ирэх teller assignment болон queue дугаарын мессежүүдийг сонсоно.
        /// </summary>
        private async void ReceiveMessages()
        {
            PipeReader reader =
                PipeReader.Create(client.GetStream());

            while (true)
            {
                ReadResult result =
                    await reader.ReadAsync();

                ReadOnlySequence<byte> buffer =
                    result.Buffer;

                SequencePosition? position;

                while ((position = buffer.PositionOf((byte)'\n')) != null)
                {
                    string message =
                        Encoding.UTF8.GetString(
                            buffer.Slice(0, position.Value).ToArray())
                            .Trim();

                    buffer =
                        buffer.Slice(
                            buffer.GetPosition(1, position.Value));

                    HandleMessage(message);
                }

                reader.AdvanceTo(
                    buffer.Start,
                    buffer.End);

                if (result.IsCompleted)
                    break;
            }

            await reader.CompleteAsync();
        }

        /// <summary>
        /// Нэг мөр мессежийг боловсруулна: teller оноолт эсвэл queue дугаар.
        /// </summary>
        /// <param name="message">Socket server-ээс ирсэн мөр.</param>
        private void HandleMessage(string message)
        {
            if (message.Length == 0)
                return;

            if (message.StartsWith("ASSIGNED|"))
            {
                tellerId =
                    message.Split('|')[1];

                this.Invoke((MethodInvoker)delegate
                {
                    lblCounter.Text =
                        tellerId;
                });

                return;
            }

            this.Invoke((MethodInvoker)delegate
            {
                lblQueue.Text =
                    message;

                lblCounter.Text =
                    tellerId;
            });
        }

        /// <summary>
        /// Тохиргоог command line argument, environment variable, app.config гэсэн дарааллаар уншина.
        /// </summary>
        /// <param name="argumentName">Command line argument-ийн нэр.</param>
        /// <param name="environmentName">Environment variable-ийн нэр.</param>
        /// <param name="appSettingName">App.config доторх key нэр.</param>
        /// <param name="fallback">Утга олдохгүй үед ашиглах default.</param>
        /// <returns>Олдсон тохиргооны утга.</returns>
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
