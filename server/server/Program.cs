using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Program
    {
        private static Dictionary<string, TcpClient> clients = new Dictionary<string, TcpClient>();
        private static List<string> messages = new List<string>();

        static async Task Main(string[] args)
        {
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            int port = 8888;

            TcpListener listener = new TcpListener(ipAddress, port);
            listener.Start();
            Console.WriteLine("The server is running. Waiting for connections...");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("Client conected.");

                _ = HandleClient(client);
            }
        }

        static async Task HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;
            string clientTag = Guid.NewGuid().ToString();

            try
            {
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Received: " + dataReceived);

                    if (dataReceived.StartsWith("LOGIN:"))
                    {
                        string login = dataReceived.Substring(6);
                        string clientKey = $"{clientTag}:{login}";
                        clients.Add(clientKey, client);
                        Console.WriteLine($"Client '{login}' add with tag '{clientTag}'.");
                    }
                    else
                    {
                        string senderClientKey = clients.FirstOrDefault(x => x.Value == client).Key;
                        string senderLogin = senderClientKey.Split(':')[1];
                        string messageWithSender = $"{senderLogin}: {dataReceived}";
                        messages.Add(messageWithSender);
                        BroadcastMessage(messageWithSender, senderClientKey);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                client.Close();
            }
        }

        static void BroadcastMessage(string message, string senderClientKey)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            foreach (var entry in clients)
            {
                if (entry.Key != senderClientKey)
                {
                    TcpClient targetClient = entry.Value;
                    SendMessageToClient(targetClient, message);
                }
            }
        }

        static async void SendMessageToClient(TcpClient client, string message)
        {
            NetworkStream stream = client.GetStream();
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
        }
    }
}
