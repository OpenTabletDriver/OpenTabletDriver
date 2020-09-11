using System;
using System.IO;
using System.Threading.Tasks;
using JKang.IpcServiceFramework;
using JKang.IpcServiceFramework.Client;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Contracts;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Logging;

namespace OpenTabletDriver.Daemon
{
    public class LogServer : IDisposable
    {
        public LogServer()
        {
            Server = CreateClient(Identifier);
            Log.Output += HandleMessage;
            Log.Debug("Daemon", $"Started log server {{{Identifier}}}");
        }

        private IIpcClient<ILogServer> CreateClient(Guid guid)
        {
            var name = guid.ToString();

            ServiceProvider serviceProvider = new ServiceCollection()
                .AddNamedPipeIpcClient<ILogServer>(name, name)
                .BuildServiceProvider();

            IIpcClientFactory<ILogServer> clientFactory = serviceProvider
                .GetRequiredService<IIpcClientFactory<ILogServer>>();

            return clientFactory.CreateClient(name);
        }
        
        private async void HandleMessage(object sender, LogMessage message)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    // Connect to server
                    await Server?.InvokeAsync(d => d.Post(message));
                    break;
                }
                catch (IOException)
                {
                    // Delay to wait for the client to be ready
                    await Task.Delay(1000);
                    continue;
                }
                catch (NullReferenceException)
                {
                    // This should have been unsubscribed by the point the server disconnected
                    break;
                }
                catch (IpcCommunicationException ipcex)
                {
                    // Something broke in the IPC framework, we should ignore this message to save the operation of the driver.
                    Log.Exception(ipcex);
                    break;
                }
            }
        }

        public readonly Guid Identifier = Guid.NewGuid();
        private IIpcClient<ILogServer> Server { set; get; }

        public void Dispose()
        {
            Log.Output -= HandleMessage;
            Server = null;

            Log.Debug("Daemon", $"Stopped log server {{{Identifier}}}");
        }
    }
}