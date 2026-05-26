using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TellerApp.Services
{
    public class SocketSenderService
    {
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
