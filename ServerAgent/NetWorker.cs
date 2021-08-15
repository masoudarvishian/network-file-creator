using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ServerAgent
{
    internal class NetWorker : IHostedService
    {
        private ILogger<NetWorker> _logger;

        private volatile bool _stopped = false;

        public NetWorker(ILogger<NetWorker> logger)
        {
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await DoWork();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _stopped = true;

            return Task.CompletedTask;
        }

        private async Task DoWork()
        {
            if (_stopped)
            {
                return;
            }

            await Connect();
        }

        private async Task Connect()
        {
            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 13000.
                int port = 4200;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                byte[] bytes = new byte[256];
                string data = null;

                // Enter the listening loop.
                while (true)
                {
                    _logger.LogInformation("Waiting for a connection... ");

                    TcpClient client = await server.AcceptTcpClientAsync();
                    _logger.LogInformation("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = await stream.ReadAsync(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a UTF8 string.
                        data = System.Text.Encoding.UTF8.GetString(bytes, 0, i);
                        _logger.LogInformation($"Received: {data}");

                        // write in a file
                        await WriteToFileAsync(data);

                        // Process the data sent by the client.
                        data = data.ToUpper();

                        byte[] msg = System.Text.Encoding.UTF8.GetBytes(data);

                        // Send back a response.
                        await stream.WriteAsync(msg, 0, msg.Length);
                        //_logger.LogInformation($"Sent: {data}");
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                _logger.LogError("SocketException: {0}", e.Message);
            }
            catch (IOException e)
            {
                _logger.LogError("IOException catched: " + e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError("Unhandled exception: " + e.Message);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
        }

        private static async Task WriteToFileAsync(string data)
        {
            // It will create the file if it doesn't exist and open the file for appending.
            using StreamWriter writer = File.AppendText(FileName.Current);

            await writer.WriteLineAsync(data);
        }
    }
}
