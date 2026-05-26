using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TellerApp.Services
{
    /// <summary>
    /// Teller app-аас queue display socket server руу дуудагдсан дугаарыг илгээдэг service.
    /// </summary>
    public class SocketSenderService
    {
        /// <summary>
        /// Teller-ийн дуудсан queue дугаарыг socket server руу TCP мессежээр илгээнэ.
        /// </summary>
        /// <param name="tellerId">Дуудлага хийж буй teller-ийн ID.</param>
        /// <param name="queueNumber">Дэлгэц дээр харуулах queue дугаар.</param>
        /// <param name="socketHost">Socket server-ийн host.</param>
        /// <param name="socketPort">Socket server-ийн port.</param>
        public async Task SendQueueAsync(
            string tellerId,
            string queueNumber,
            string socketHost,
            int socketPort)
        {
            TcpClient client =
                new TcpClient();

            await client.ConnectAsync(
                socketHost,
                socketPort);

            NetworkStream stream =
                client.GetStream();

            string message =
                "CALL|"
                + tellerId
                + "|"
                + System.Environment.MachineName
                + "|"
                + queueNumber;

            byte[] data =
                Encoding.UTF8.GetBytes(
                    message);

            await stream.WriteAsync(
                data,
                0,
                data.Length);

            client.Close();
        }
    }
}
