using System.IO;
using TabletDriverLib.Interface;

namespace TabletDriverLib.Class
{
    public class Logger : ILogger
    {
        public Stream Output { private set; get; } = new MemoryStream();
        private StreamWriter LogInput => new StreamWriter(Output);

        public async void Write(string text)
        {
            using (LogInput)
                await LogInput.WriteAsync(text);
        }

        public async void WriteLine(string text)
        {
            using (LogInput)
                await LogInput.WriteLineAsync(text);
        }

        public async void WriteLine(string prefix, string text)
        {
            using (LogInput)
                await LogInput.WriteLineAsync($"{prefix}: {text}");
        }
    }
}