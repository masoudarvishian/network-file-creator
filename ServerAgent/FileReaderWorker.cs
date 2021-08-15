using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ServerAgent
{
    internal class FileReaderWorker : IHostedService, IDisposable
    {
        private ILogger<FileReaderWorker> _logger;

        private Timer _timer;

        private volatile bool _stopped = false;

        public FileReaderWorker(ILogger<FileReaderWorker> logger)
        {
            _logger = logger;
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

        private async void DoWork(object state)
        {
            if (_stopped)
            {
                return;
            }

            // making sure file is created
            await Task.Delay(1000);

            while (FileName.Collection.Count > 0)
            {
                string path = FileName.Collection.Peek();
                if (File.Exists(path))
                {
                    // create info file
                    var fileName = $"{Path.GetFileNameWithoutExtension(path)}-info.txt";
                    var finalPath = Path.Combine("MyFiles", fileName);
                    using StreamWriter writer = File.AppendText(finalPath);

                    // number of lines
                    string[] lines = await File.ReadAllLinesAsync(path);

                    Regex reg_exp = new Regex("[^a-zA-Z0-9]");

                    string text = await File.ReadAllTextAsync(path);
                    text = reg_exp.Replace(text, " ");

                    // number of words
                    string[] words = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    // find most used words
                    var mostUsedWord = words
                      .GroupBy(x => x)
                      .Select(x => new 
                      {
                          Word = x.Key,
                          Count = x.Count()
                      })
                      .OrderByDescending(x => x.Count)
                      .FirstOrDefault();

                    // find most used words line number
                    var mostUsedLines = lines
                       .Select((text, index) => new { text, lineNumber = index + 1 })
                       .Where(x => x.text.Contains(mostUsedWord.Word))
                       .ToArray();

                    // prepare line numbers string
                    var strLineNums = "";
                    foreach (var item in mostUsedLines)
                    {
                        strLineNums += $"{item.lineNumber}, ";
                    }

                    // write to info file
                    await writer.WriteLineAsync($"Line Count: {lines.Length}\n" +
                                           $"Word Count: {words.Length}\n" +
                                           $"Most Used Word: {mostUsedWord}\n" +
                                           $"Most Used Word lines: {strLineNums}");

                    // remove from list
                    FileName.Collection.Dequeue();
                }
                else
                {
                    _logger.LogError($"path not found: {path}");
                }
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}