using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eto.Forms;
using TabletDriverPlugin;
using TabletDriverPlugin.Logging;

namespace OpenTabletDriver.UX.Controls
{
    public class LogView : ListBox
    {
        public LogView()
        {
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            foreach (var message in await App.DriverDaemon.InvokeAsync(d => d.GetCurrentLog()))
                this.Items.Add(Log.GetStringFormat(message));
            
            ServerID = await App.DriverDaemon.InvokeAsync(d => d.SetLogOutput(true));
            if (ServerID != Guid.Empty)
            {
                PipeClient = new NamedPipeClientStream(ServerID.ToString());
                await PipeClient.ConnectAsync();
                
                MainThread = new Thread(Main);
                MainThread.Start();
            }
        }

        public Guid ServerID { private set; get; }
        
        private NamedPipeClientStream PipeClient { set; get; }
        private Thread MainThread;

        private async void Main()
        {
            using (var sr = new StreamReader(PipeClient))
            {
                while (PipeClient.CanRead)
                {
                    var line = await sr.ReadLineAsync();
                    await Application.Instance.InvokeAsync(() => this.Items.Add(line));
                }
            }
        }
    }
}