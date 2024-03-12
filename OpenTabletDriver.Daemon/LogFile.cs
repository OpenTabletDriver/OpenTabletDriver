using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTabletDriver.Logging;
using SysDirectory = System.IO.Directory;

namespace OpenTabletDriver.Daemon
{
    public sealed class LogFile : IDisposable
    {
        private const int MAX_LOG_FILES = 20;
        private readonly string _fileName;
        private readonly FileStream _stream;
        private readonly StreamWriter _writer;
        private readonly Channel<LogMessage> _channel = Channel.CreateUnbounded<LogMessage>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private bool _disposed = false;

        public string Directory { get; }

        public LogFile(string logDirectory)
        {
            Directory = logDirectory;
            PrepareDirectory(logDirectory);

            int? i = null;
            var currentDate = DateTime.Now;
            string? logFile;

            while (File.Exists(logFile = GetFileName(Directory, currentDate, i)))
                i = (i + 1) ?? 1;

            _fileName = logFile;
            _stream = File.Open(_fileName, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
            _writer = new StreamWriter(_stream);

            Task.Factory.StartNew(async () =>
            {
                var token = _cts.Token;
                var writer = _writer;
                await writer.WriteLineAsync("[");
                await foreach (var logMessage in _channel.Reader.ReadAllAsync(token))
                {
                    if (token.IsCancellationRequested)
                        break;

                    var log = JsonConvert.SerializeObject(logMessage, Formatting.Indented) + ",";
                    await writer.WriteLineAsync(log);
                    await writer.FlushAsync();
                }
                await writer.WriteLineAsync("]");
            }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void Write(LogMessage message)
        {
            while (!_channel.Writer.TryWrite(message)) ;
        }

        public IEnumerable<LogMessage> Read()
        {
            using var file = File.Open(_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(file);
            using var jsonReader = new JsonTextReader(reader);

            while (jsonReader.Read())
            {
                if (jsonReader.TokenType == JsonToken.StartObject)
                {
                    LogMessage? logMessage = null;
                    try
                    {
                        logMessage = JsonConvert.DeserializeObject<LogMessage>(JObject.Load(jsonReader).ToString());
                    }
                    catch
                    {
                        // ignored
                    }

                    if (logMessage != null)
                        yield return logMessage;
                }
            }
        }

        private static string GetFileName(string directory, DateTime date, int? index = null)
        {
            var fileName = date.ToUniversalTime().ToString("u").Replace(":", "_");
            if (index.HasValue)
                fileName += $"-{index.Value}";
            fileName += ".log.json";
            return Path.Combine(directory, fileName);
        }

        private static void PrepareDirectory(string directory)
        {
            if (!SysDirectory.Exists(directory))
                SysDirectory.CreateDirectory(directory);

            var files = SysDirectory.GetFiles(directory, "*.log.json");
            if (files.Length < MAX_LOG_FILES)
                return;

            Array.Sort(files);

            for (var i = 0; i <= files.Length - MAX_LOG_FILES; i++)
            {
                try
                {
                    File.Delete(files[i]);
                }
                catch
                {
                    // ignored
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _cts.Cancel();
            _writer.Flush();
            _writer.Dispose();
            _stream.Dispose();
        }

        ~LogFile()
        {
            Dispose();
        }
    }
}
