using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace QueueSocketServer
{
    /// <summary>
    /// Queue display болон teller app хооронд TCP socket мессеж дамжуулах console server.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Teller ID бүрт холбогдсон display client-ийг хадгална.
        /// </summary>
        static Dictionary<string, TcpClient> displays =
            new Dictionary<string, TcpClient>();

        /// <summary>
        /// Компьютер бүрт автоматаар оноосон teller ID-г хадгална.
        /// </summary>
        static Dictionary<string, string> tellerIdsByMachine =
            new Dictionary<string, string>();

        /// <summary>
        /// Дараагийн автоматаар оноох teller дугаар.
        /// </summary>
        static int nextTellerNumber =
            1;

        /// <summary>
        /// Socket server-ийг эхлүүлж teller/display client-үүдийг тасралтгүй хүлээн авна.
        /// </summary>
        /// <param name="args">Console application-д дамжсан параметрүүд.</param>
        static async Task Main(string[] args)
        {
            TcpListener listener =
                new TcpListener(IPAddress.Any, 5000);

            listener.Start();

            Console.WriteLine("Socket server started...");

            while (true)
            {
                TcpClient client =
                    await listener.AcceptTcpClientAsync();

                HandleClient(client);
            }
        }

        /// <summary>
        /// Нэг TCP client-ээс ирэх DISPLAY болон CALL мессежүүдийг боловсруулна.
        /// </summary>
        /// <param name="client">Холбогдсон teller эсвэл display client.</param>
        static async void HandleClient(TcpClient client)
        {
            NetworkStream stream =
                client.GetStream();

            byte[] buffer = new byte[1024];

            while (true)
            {
                int byteCount;

                try
                {
                    byteCount =
                        await stream.ReadAsync(
                            buffer,
                            0,
                            buffer.Length);

                    if (byteCount == 0)
                        break;
                }
                catch
                {
                    break;
                }

                string message =
                    Encoding.UTF8.GetString(
                        buffer,
                        0,
                        byteCount);

                Console.WriteLine(message);

                string[] parts =
                    message.Split('|');

                // DISPLAY REGISTER
                if (parts[0] == "DISPLAY")
                {
                    string tellerId =
                        parts[1];

                    bool isAutoTeller =
                        tellerId.Equals(
                            "AUTO",
                            StringComparison.OrdinalIgnoreCase);

                    if (isAutoTeller)
                    {
                        string machineKey =
                            GetMachineKey(
                                client,
                                parts.Length > 2 ? parts[2] : "");

                        tellerId =
                            GetOrCreateTellerId(machineKey);

                        byte[] assignedData =
                            Encoding.UTF8.GetBytes(
                                "ASSIGNED|" + tellerId);

                        await stream.WriteAsync(
                            assignedData,
                            0,
                            assignedData.Length);
                    }

                    displays[tellerId] =
                        client;

                    Console.WriteLine(
                        "Display registered: "
                        + tellerId);
                }

                // TELLER CALL
                if (parts[0] == "CALL")
                {
                    string tellerId =
                        parts[1];

                    bool isAutoTeller =
                        tellerId.Equals(
                            "AUTO",
                            StringComparison.OrdinalIgnoreCase);

                    if (isAutoTeller)
                    {
                        string machineKey =
                            GetMachineKey(
                                client,
                                parts.Length > 3 ? parts[2] : "");

                        tellerId =
                            GetOrCreateTellerId(machineKey);
                    }

                    string queueNumber =
                        isAutoTeller && parts.Length > 3
                            ? parts[3]
                            : parts[2];

                    if (displays.ContainsKey(tellerId))
                    {
                        TcpClient displayClient =
                            displays[tellerId];

                        NetworkStream displayStream =
                            displayClient.GetStream();

                        byte[] data =
                            Encoding.UTF8.GetBytes(
                                queueNumber);

                        await displayStream.WriteAsync(
                            data,
                            0,
                            data.Length);

                        Console.WriteLine(
                            "Sent "
                            + queueNumber
                            + " to "
                            + tellerId);
                    }
                }
            }
        }

        /// <summary>
        /// Auto teller онооход ашиглах машины түлхүүрийг machine name эсвэл IP хаягаар тодорхойлно.
        /// </summary>
        /// <param name="client">Холбогдсон TCP client.</param>
        /// <param name="machineName">Client-ээс илгээсэн computer name.</param>
        /// <returns>Тухайн компьютерт тогтвортой ашиглах key.</returns>
        static string GetMachineKey(
            TcpClient client,
            string machineName)
        {
            if (!string.IsNullOrWhiteSpace(machineName))
                return machineName;

            IPEndPoint remoteEndPoint =
                client.Client.RemoteEndPoint as IPEndPoint;

            return remoteEndPoint != null
                ? remoteEndPoint.Address.ToString()
                : Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Өмнө нь оноогдсон teller ID байвал буцаана, байхгүй бол шинэ TELLER дугаар үүсгэнэ.
        /// </summary>
        /// <param name="machineKey">Компьютерийг ялгах key.</param>
        /// <returns>Тухайн компьютерт оноосон teller ID.</returns>
        static string GetOrCreateTellerId(string machineKey)
        {
            if (tellerIdsByMachine.ContainsKey(machineKey))
                return tellerIdsByMachine[machineKey];

            string tellerId =
                "TELLER" + nextTellerNumber;

            nextTellerNumber++;

            tellerIdsByMachine[machineKey] =
                tellerId;

            return tellerId;
        }
    }
}
