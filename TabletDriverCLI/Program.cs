using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TabletDriverLib;
using TabletDriverLib.Class;

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
        private static TabletProperties Tablet;

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
                    foreach (var str in Driver.InputManager.GetAllDeviceIdentifiers())
                        Log("DEVICE", str);
                    return true;
                case "tablet":
                    Driver.InputManager.OpenTablet(tokens.Remainder(1));
                    return true;
                case "tabletinfo":
                    var tabDevice = Driver.InputManager.Tablet;
                    WriteDeviceInfo("DevicePath", tabDevice.DevicePath);
                    WriteDeviceInfo("FriendlyName", TryGetResult(tabDevice.GetFriendlyName));
                    WriteDeviceInfo("Manufacturer", TryGetResult(tabDevice.GetManufacturer));
                    WriteDeviceInfo("ProductName", TryGetResult(tabDevice.GetProductName));
                    WriteDeviceInfo("FeatureReportLength", TryGetResult(tabDevice.GetMaxFeatureReportLength));
                    WriteDeviceInfo("InputReportLength", TryGetResult(tabDevice.GetMaxInputReportLength));
                    WriteDeviceInfo("OutputReportLength", TryGetResult(tabDevice.GetMaxOutputReportLength));
                    WriteDeviceInfo("SerialNumber", TryGetResult(tabDevice.GetSerialNumber));
                    return true;
                case "identifiers":
                    tabDevice = Driver.InputManager.Tablet;
                    WriteDeviceInfo("VendorId", tabDevice.VendorID.ToString("X"));
                    WriteDeviceInfo("ProductId", tabDevice.ProductID.ToString("X"));
                    WriteDeviceInfo("FileSystemName", TryGetResult(tabDevice.GetFileSystemName));
                    WriteDeviceInfo("ReleaseNumber", tabDevice.ReleaseNumber);
                    return true;
                case "newtablet":
                    Tablet = new TabletProperties();
                    Log("INFO", "Created new tablet properties.");
                    return true;
                case "detect":
                    Driver.InputManager.OpenTablet(Tablet);
                    return true;
                case "set.name":
                    Tablet.TabletName = tokens.Remainder(1);
                    return true;
                case "set.vid":
                    Tablet.VendorID = Convert.ToInt32(tokens[1]);
                    return true;
                case "set.pid":
                    Tablet.ProductID = Convert.ToInt32(tokens[1]);
                    return true;
                case "set.width":
                    Tablet.Width = Convert.ToInt32(tokens[1]);
                    return true;
                case "set.height":
                    Tablet.Height = Convert.ToInt32(tokens[1]);
                    return true;
                case "set.x":
                    Tablet.MaxX = Convert.ToInt32(tokens[1]);
                    return true;
                case "set.y":
                    Tablet.MaxY = Convert.ToInt32(tokens[1]);
                    return true;
                case "set.p":
                case "set.pressure":
                    Tablet.MaxPressure = Convert.ToUInt32(tokens[1]);
                    return true;
                case "tablet.save":
                    var savePath = tokens[1];
                    Tablet.Write(new FileInfo(savePath));
                    return true;
                case "tablet.read":
                    var readPath = tokens[1];
                    Tablet = TabletProperties.Read(new FileInfo(readPath));
                    return true;
                case "hook":
                    var hook = Convert.ToBoolean(tokens[1]);
                    if (hook)
                    {
                        Log("INFO", "Hooking input positions");
                        Driver.InputManager.BindPositions(hook);
                    }
                    else
                    {
                        Log("INFO", "Unhooking input positions");
                        Driver.InputManager.BindPositions(hook);
                    }
                    return true;
                case "tabletarea":
                    Driver.InputManager.TabletArea = new Area
                    {
                        Width = Convert.ToSingle(tokens[1]),
                        Height = Convert.ToSingle(tokens[2]),
                    };
                    if (tokens.Length > 3)
                    {
                        Driver.InputManager.TabletArea.Position = new Point(Convert.ToSingle(tokens[3]), Convert.ToSingle(tokens[4]));
                    }
                    return true;
                case "displayarea":
                    Driver.InputManager.DisplayArea = new Area
                    {
                        Width = Convert.ToSingle(tokens[1]),
                        Height = Convert.ToSingle(tokens[2]),
                    };
                    if (tokens.Length > 3)
                    {
                        Driver.InputManager.DisplayArea.Position = new Point(Convert.ToSingle(tokens[3]), Convert.ToSingle(tokens[4]));
                    }
                    return true;
                default:
                    return false;
            }
        }

        private static void WriteDeviceInfo(string property, object value) => Log("DEVICE", $"{property}:\t{value}");
    }
}
