using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TellerApp.Services
{
    public class SocketSenderService
    {
        public async Task SendQueueAsync(
            string tellerId,
            string queueNumber)
        {
            TcpClient client =
                new TcpClient();

            await client.ConnectAsync(
                "127.0.0.1",
                5000);

            NetworkStream stream =
                client.GetStream();

            string message =
                "CALL|"
                + tellerId
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