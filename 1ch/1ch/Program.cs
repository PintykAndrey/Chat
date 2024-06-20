using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace _1ch
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string serverIp = "127.0.0.1";
            int port = 8888;
            TcpClient client = new TcpClient();

            try
            {
                await client.ConnectAsync(serverIp, port);
                Console.WriteLine("Connected to server.");
                NetworkStream stream = client.GetStream();
                Console.Write("Enter your login: ");
                string login = Console.ReadLine();
                string loginMessage = $"LOGIN:{login}";
                byte[] loginData = Encoding.UTF8.GetBytes(loginMessage);
                await stream.WriteAsync(loginData, 0, loginData.Length);
                Console.WriteLine("Login sent to server.");
                Task readingTask = Task.Run(async () =>
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;
                    try
                    {
                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                        {
                            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);a
                            Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
                            Console.Write(message);
                            Console.Write("\nEnter message: ");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                });
                while (true)
                {
                    Console.Write("Enter message: ");
                    string message = Console.ReadLine();
                    byte[] messageData = Encoding.UTF8.GetBytes(message);
                    await stream.WriteAsync(messageData, 0, messageData.Length);
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
    }
}
