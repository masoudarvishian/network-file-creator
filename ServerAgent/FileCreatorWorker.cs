using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ServerAgent
{
    internal class FileCreatorWorker : IHostedService, IDisposable
    {
        private ILogger<FileCreatorWorker> _logger;

        private Timer _timer;

        private volatile bool _stopped = false;

        private static int _counter = 0;

        public FileCreatorWorker(ILogger<FileCreatorWorker> logger)
        {
            _logger = logger;

            // make sure we have the right directory
            if (!Directory.Exists("MyFiles"))
            {
                Directory.CreateDirectory("MyFiles");
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // call worker every 30 seconds
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _stopped = true;
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            if (_stopped)
            {
                return;
            }

            FileName.Current = Path.Combine("MyFiles", $"{++_counter:000}_{Path.GetRandomFileName().Replace(".", "")}.txt");
            _logger.LogInformation("new file name is genarated: " + FileName.Current);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
