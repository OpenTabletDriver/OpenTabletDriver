using System;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Components;

namespace OpenTabletDriver.Daemon
{
    class CommandLineOptions
    {
        public DirectoryInfo? AppDataDirectory { get; set; }
        public DirectoryInfo? ConfigurationDirectory { get; set; }
    }

    partial class Program
    {
        static async Task Main(string[] args)
        {
            Log.Output += (sender, message) =>
            {
                Console.WriteLine(Log.GetStringFormat(message));
            };

            var cmdLineOptions = ParseCmdLineOptions(args);

            if (!string.IsNullOrWhiteSpace(cmdLineOptions.AppDataDirectory?.FullName))
                AppInfo.Current.CommandLineAppDataDirectory = cmdLineOptions.AppDataDirectory.FullName;
            if (!string.IsNullOrWhiteSpace(cmdLineOptions.ConfigurationDirectory?.FullName))
                AppInfo.Current.CommandLineConfigurationDirectory = cmdLineOptions.ConfigurationDirectory.FullName;

            await StartDaemon();
        }

        static async Task StartDaemon()
        {
            using var instance = new Instance("OpenTabletDriver.Daemon");
            if (instance.AlreadyExists)
            {
                Console.WriteLine("OpenTabletDriver Daemon is already running.");
                Thread.Sleep(1000);
                return;
            }

            TaskScheduler.UnobservedTaskException += (_, e) =>
            {
                Log.Exception(e.Exception);
                e.SetObserved();
            };

            var host = GetRpcHost();

            var cts = new CancellationTokenSource();
            bool tokenCancelled = false;
            bool daemonRunning = false;

            // Handle SIGINT
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true; // must set to true otherwise we can't wait gracefully
                Log.Debug("CancelKeyPressHandler", "Handling Ctrl+C/SIGINT");
                CloseDaemon();
            };

            // Handle SIGHUP
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                PosixSignal[] trappedSignals = [PosixSignal.SIGHUP];

                foreach (var signal in trappedSignals)
                    PosixSignalRegistration.Create(signal,
                        [SuppressMessage("ReSharper", "AccessToModifiedClosure")] (_) =>
                        {
                            Log.Debug("signal", Enum.GetName(signal) ?? "<unknown signal>");
                            CloseDaemon();
                        });
            }

            // Handle other termination operations (SIGTERM, etc.)
            AssemblyLoadContext.Default.Unloading += _ =>
            {
                Log.Write("AssemblyLoadContext.Unloading", "Terminating daemon..");
                CloseDaemon();
            };

            try
            {
                daemonRunning = true;
                await host.Run(BuildDaemon(), cts.Token);
                daemonRunning = false;
                Log.Write("ProgramMain", "Daemon gracefully stopped");
            }
            catch (Exception e)
            {
                daemonRunning = false;
                Log.Exception(e, LogLevel.Fatal);
                Environment.Exit(1);
            }

            return;

            void CloseDaemon()
            {
                if (tokenCancelled) // don't cancel twice
                    return;

                tokenCancelled = true;
                cts.CancelAsync();

                Log.Write("closeDaemon", "Waiting for daemon to terminate");

                // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
                // ReSharper disable once AccessToModifiedClosure
                while (daemonRunning)
                    Thread.Sleep(100);

                Log.Write("closeDaemon", "Waiting for daemon finished");
            }
        }

        private static RpcHost<DriverDaemon> GetRpcHost()
        {
            var host = new RpcHost<DriverDaemon>("OpenTabletDriver.Daemon");
            host.ConnectionStateChanged += (sender, state) =>
                Log.Write("IPC", $"{(state ? "Connected to" : "Disconnected from")} a client.", LogLevel.Debug);
            return host;
        }

        static CommandLineOptions ParseCmdLineOptions(string[] args)
        {
            var cmdLineOptions = new CommandLineOptions();

            var rootCommand = new RootCommand("OpenTabletDriver")
            {
                TreatUnmatchedTokensAsErrors = true
            };

            var appDataOption = new Option<DirectoryInfo>("--appdata", "-a")
            {
                Description = "Application data directory",
            }.AcceptLegalFilePathsOnly();

            var configOption = new Option<DirectoryInfo>("--config", "-c")
            {
                Description = "Configuration directory",
            }.AcceptExistingOnly();

            rootCommand.Options.Add(appDataOption);
            rootCommand.Options.Add(configOption);

            var parseResult = rootCommand.Parse(args);
            if (parseResult.Errors.Any())
            {
                Log.Write(nameof(ParseCmdLineOptions), $"Command line parsing errors encountered: {string.Join(",", parseResult.Errors.Select(x => x.Message))}", LogLevel.Error);
                Environment.Exit(1);
            }

            cmdLineOptions.AppDataDirectory = parseResult.RootCommandResult.GetValue(appDataOption);
            cmdLineOptions.ConfigurationDirectory = parseResult.RootCommandResult.GetValue(configOption);

            if (parseResult.Action is not null)
            {
                parseResult.Invoke();
                Environment.Exit(0);
            }

            return cmdLineOptions;
        }

        static DriverDaemon BuildDaemon()
        {
            return new DriverDaemon(new DriverBuilder()
                .ConfigureServices(serviceCollection =>
                {
                    serviceCollection.AddSingleton<IDeviceConfigurationProvider, DesktopDeviceConfigurationProvider>();
                    serviceCollection.AddSingleton<IReportParserProvider, DesktopReportParserProvider>();
                })
                .Build<Driver>(out _)
            );
        }
    }
}
