using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.RPC;

namespace OpenTabletDriver.Daemon
{
    partial class Program
    {
        static async Task Main(string[] args)
        {
            using (var instance = new Instance("OpenTabletDriver.Daemon"))
            {
                if (instance.AlreadyExists)
                {
                    Console.WriteLine("OpenTabletDriver Daemon is already running.");
                    Thread.Sleep(1000);
                    return;
                }

                AppDomain.CurrentDomain.UnhandledException += (sender, e) => 
                {
                    var exception = (Exception)e.ExceptionObject;
                    File.WriteAllLines(Path.Join(AppInfo.Current.AppDataDirectory, "daemon.log"),
                        new string[]
                        {
                            DateTime.Now.ToString(),
                            exception.GetType().FullName,
                            exception.Message,
                            exception.Source,
                            exception.StackTrace,
                            exception.TargetSite.Name
                        }
                    );
                };

                var rootCommand = new RootCommand("OpenTabletDriver")
                {
                    new Option(new string[] { "--appdata", "-a" }, "Application data directory")
                    {
                        Argument = new Argument<DirectoryInfo>("appdata")
                    },
                    new Option(new string[] { "--config", "-c" }, "Configuration directory")
                    {
                        Argument = new Argument<DirectoryInfo> ("config")
                    }
                };
                rootCommand.Handler = CommandHandler.Create<DirectoryInfo, DirectoryInfo>((appdata, config) => 
                {
                    AppInfo.Current.AppDataDirectory = appdata?.FullName;
                    AppInfo.Current.ConfigurationDirectory = config?.FullName;
                });
                rootCommand.Invoke(args);

                var host = new RpcHost<DriverDaemon>("OpenTabletDriver.Daemon");
                host.ConnectionStateChanged += (sender, state) => 
                    Log.Write("IPC", $"{(state ? "Connected to" : "Disconnected from")} a client.", LogLevel.Debug);
                await Task.Delay(-1);
            }
        }
    }
}
