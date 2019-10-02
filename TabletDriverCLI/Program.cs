using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TabletDriverLib;

namespace TabletDriverCLI
{
    using static Extensions;

    class Program
    {
        static void Main(string[] args)
        {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Console.WriteLine("TabletDriver CLI".CenterInDivider());
            
            MainTask(args).GetAwaiter().GetResult();
        }

        private static Task MainTask(string[] args)
        {
            Log("INFO", "Creating TabletDriver instance...");
            Driver = new Driver();
            Driver.Debugging = Debugger.IsAttached;
            
            Log("INFO", "Starting driver...");
            Driver.Start();
            
            while (Driver.IsRunning)
            {
                try
                {
                    if (!CLICommand(Console.ReadLine()))
                        Log("ERROR", "Invalid command.");
                }
                catch (Exception ex)
                {
                    Log(ex.GetType().Name.ToUpper(), ex.Message);
                }
            }
            return Task.CompletedTask;
        }

        private static Driver Driver { set; get; }

        private static bool CLICommand(string command)
        {
            var tokens = command.Split(' ');
            switch(tokens[0].ToLower())
            {
                case "stop":
                case "exit":
                case "kill":
                    Driver.Stop();
                    return true;
                case "debug":
                    if (tokens.Length > 1)
                    {
                        Driver.Debugging = Convert.ToBoolean(tokens[1]);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "clear":
                    Console.Clear();
                    return true;
                case "devices":
                    foreach (var str in Driver.DeviceManager.GetDeviceIdentifiers())
                        Log("DEVICE", str);
                    return true;
                case "tablet":
                    Driver.DeviceManager.OpenTablet(tokens.Remainder(1));
                    return true;
                default:
                    return false;
            }
        }
    }
}
