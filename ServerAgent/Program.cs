using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ServerAgent
{
    class Program
    {
        public static void Main()
        {
        ConnectPoint:
            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 13000.
                int port = 4200;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                byte[] bytes = new byte[256];
                string data = null;

                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a UTF8 string.
                        data = System.Text.Encoding.UTF8.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);

                        // write in a file
                        WriteToFile(data);

                        // Process the data sent by the client.
                        data = data.ToUpper();

                        byte[] msg = System.Text.Encoding.UTF8.GetBytes(data);

                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent: {0}", data);
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e.Message);
                goto ConnectPoint;
            }
            catch (IOException e)
            {
                Console.WriteLine("IOException catched: " + e.Message);
                goto ConnectPoint;
            }
            catch(Exception e)
            {
                Console.WriteLine("Unhandled exception: " + e.Message);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }

        private static string GetFileName()
        {
            var fileName = "this_is_a_test.txt";
            return fileName;
        }

        private static void WriteToFile(string data)
        {
            var path = GetFileName();

            // It will create the file if it doesn't exist and open the file for appending.
            using StreamWriter w = File.AppendText(path);

            w.WriteLine(data);
        }
    }
}
