using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Desktop.Updater;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Components;

namespace OpenTabletDriver.Daemon
{
    class CommandLineOptions
    {
        public object SpecialCommand { get; set; }
        public DirectoryInfo AppDataDirectory { get; set; }
        public DirectoryInfo ConfigurationDirectory { get; set; }
        public bool ConsoleHidden { get; set; }
    }

    class UpdateCommandOptions
    {
        public List<DirectoryInfo> Sources { get; set; }
        public DirectoryInfo Destination { get; set; }
    }

    partial class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern void FreeConsole();

        static async Task Main(string[] args)
        {
            var cmdLineOptions = ParseCmdLineOptions(args);

            if (!string.IsNullOrWhiteSpace(cmdLineOptions?.AppDataDirectory?.FullName))
                AppInfo.Current.AppDataDirectory = cmdLineOptions.AppDataDirectory.FullName;
            if (!string.IsNullOrWhiteSpace(cmdLineOptions?.ConfigurationDirectory?.FullName))
                AppInfo.Current.ConfigurationDirectory = cmdLineOptions.ConfigurationDirectory.FullName;
            if (cmdLineOptions.ConsoleHidden)
            {
                if (OperatingSystem.IsWindows())
                    FreeConsole();
                else
                    Console.WriteLine("Console hiding is not yet supported on this OS.");
            }

            if (cmdLineOptions.SpecialCommand is UpdateCommandOptions updateCommandOptions)
            {
                var sources = updateCommandOptions.Sources.Select(x => x.FullName).ToArray();
                var destination = updateCommandOptions.Destination.FullName;
                var ret = 0;

                using (var file = File.Open(Path.Join(AppInfo.Current.AppDataDirectory, "daemon-update.log"), FileMode.Create))
                {
                    var logger = new StreamWriter(file) { AutoFlush = true };

                    await logger.WriteLineAsync("Starting update using following files...");
                    foreach (var source in sources)
                        await logger.WriteLineAsync($" {source}");
                    await logger.WriteLineAsync($"Destination: {destination}");

                    try
                    {
                        await DesktopInterop.Updater.Install(
                            // bypass CheckForUpdate
                            new Update(
                                new Version(0, 0, 0, 0),
                                sources.ToImmutableArray(),
                                destination
                            )
                        );
                        await logger.WriteLineAsync("Update complete.");
                    }
                    catch (Exception ex)
                    {
                        await logger.WriteLineAsync(ex.ToString());
                        await logger.WriteLineAsync("Update failed!");
                        ret = 1;
                    }
                }

                Environment.Exit(ret);
            }

            using (var instance = new Instance("OpenTabletDriver.Daemon"))
            {
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

                var host = new RpcHost<DriverDaemon>("OpenTabletDriver.Daemon");
                host.ConnectionStateChanged += (sender, state) =>
                    Log.Write("IPC", $"{(state ? "Connected to" : "Disconnected from")} a client.", LogLevel.Debug);

                try
                {
                    await host.Run(BuildDaemon());
                }
                catch (Exception e)
                {
                    Log.Exception(e, LogLevel.Fatal);
                    Environment.Exit(1);
                }
            }
        }

        static CommandLineOptions ParseCmdLineOptions(string[] args)
        {
            var cmdLineOptions = new CommandLineOptions();

            var rootCommand = new RootCommand("OpenTabletDriver")
            {
                TreatUnmatchedTokensAsErrors = true
            };

            var updateCommand = new Command("update") { IsHidden = true };

            rootCommand.AddCommand(updateCommand);

            var appDataOption = new Option<DirectoryInfo>(
                aliases: new[] { "--appdata", "-a" },
                description: "Application data directory"
            );

            var configOption = new Option<DirectoryInfo>(
                aliases: new[] { "--config", "-c" },
                description: "Configuration directory"
            );

            var hiddenOption = new Option<bool>(
                aliases: new[] { "--hidden", "-h" },
                description: "Console hidden"
            );

            rootCommand.AddGlobalOption(appDataOption);
            rootCommand.AddGlobalOption(configOption);
            rootCommand.AddGlobalOption(hiddenOption);

            rootCommand.SetHandler(setupGlobalOptions, appDataOption, configOption, hiddenOption);

            var sourcesArg = new Option<List<DirectoryInfo>>("--sources")
            {
                IsRequired = true,
                AllowMultipleArgumentsPerToken = true
            };
            var destArg = new Option<DirectoryInfo>("--destination")
            {
                IsRequired = true
            };

            updateCommand.AddOption(sourcesArg);
            updateCommand.AddOption(destArg);

            updateCommand.SetHandler((appData, config, hidden, src, dest) =>
            {
                setupGlobalOptions(appData, config, hidden);
                cmdLineOptions.SpecialCommand = new UpdateCommandOptions
                {
                    Sources = src,
                    Destination = dest
                };
            }, appDataOption, configOption, hiddenOption, sourcesArg, destArg);

            rootCommand.Invoke(args);

            return cmdLineOptions;

            void setupGlobalOptions(DirectoryInfo appData, DirectoryInfo config, bool hidden)
            {
                cmdLineOptions.AppDataDirectory = appData;
                cmdLineOptions.ConfigurationDirectory = config;
                cmdLineOptions.ConsoleHidden = hidden;
            }
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
