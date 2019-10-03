using System;
using System.Collections.Generic;
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
                    Log(ex);
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
                    foreach (var str in Driver.DeviceManager.GetAllDeviceIdentifiers())
                        Log("DEVICE", str);
                    return true;
                case "tablet":
                    Driver.DeviceManager.OpenTablet(tokens.Remainder(1));
                    return true;
                case "tabletinfo":
                    var tablet = Driver.DeviceManager.Tablet;
                    WriteDeviceInfo("DevicePath", tablet.DevicePath);
                    WriteDeviceInfo("FriendlyName", TryGetResult(tablet.GetFriendlyName));
                    WriteDeviceInfo("Manufacturer", TryGetResult(tablet.GetManufacturer));
                    WriteDeviceInfo("ProductName", TryGetResult(tablet.GetProductName));
                    WriteDeviceInfo("FeatureReportLength", TryGetResult(tablet.GetMaxFeatureReportLength));
                    WriteDeviceInfo("InputReportLength", TryGetResult(tablet.GetMaxInputReportLength));
                    WriteDeviceInfo("OutputReportLength", TryGetResult(tablet.GetMaxOutputReportLength));
                    WriteDeviceInfo("SerialNumber", TryGetResult(tablet.GetSerialNumber));
                    return true;
                default:
                    return false;
            }
        }

        private static void WriteDeviceInfo(string property, object value) => Log("DEVICE", $"{property}:\t{value}");
    }
}
