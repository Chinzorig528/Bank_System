using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace QueueSocketServer
{
    class Program
    {
        static Dictionary<string, TcpClient> displays =
            new Dictionary<string, TcpClient>();

        static Dictionary<string, string> tellerIdsByMachine =
            new Dictionary<string, string>();

        static int nextTellerNumber =
            1;

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
