using System;
using System.IO;
using System.Net.Sockets;

namespace ClientAgent
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Create a TcpClient.
                int port = 4200;
                string ip = "127.0.0.1";
                TcpClient client = new TcpClient(ip, port);

                NetworkStream stream = client.GetStream();

                // infinitely enter message
                while (true)
                {
                    // get message from user
                    Console.Write("Enter message or press q to quit: ");
                    var message = Console.ReadLine();

                    if (message == "q" || message == "Q")
                    {
                        break; // in order to quit...
                    }

                    if (string.IsNullOrWhiteSpace(message))
                    {
                        Console.WriteLine("enter a valid message!");
                        continue;
                    }

                    // Translate the passed message into UTF8 and store it as a Byte array.
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(message);

                    // Send the message to the connected TcpServer.
                    stream.Write(data, 0, data.Length);

                    Console.WriteLine("Sent: {0}", message);

                    // Receive the TcpServer.response.

                    // Buffer to store the response bytes.
                    data = new Byte[256];

                    // String to store the response UTF8 representation.
                    string responseData = string.Empty;

                    // Read the first batch of the TcpServer response bytes.
                    int bytes = stream.Read(data, 0, data.Length);
                    responseData = System.Text.Encoding.UTF8.GetString(data, 0, bytes);
                    Console.WriteLine("Received: {0}", responseData);
                }

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e.Message);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e.Message);
            }
            catch (IOException e)
            {
                Console.WriteLine("IOException: {0}", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unhandled exception: " + e.Message);
            }
            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }
    }
}
