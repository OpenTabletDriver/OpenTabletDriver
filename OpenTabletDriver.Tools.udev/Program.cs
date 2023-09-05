using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using OpenTabletDriver.Plugin.Tablet;
using udev.NET.Rules;
using udev.NET.Rules.Names;

namespace OpenTabletDriver.Tools.udev
{
    class Program
    {
        static void Main(string[] args)
        {
            var root = new RootCommand("OpenTabletDriver udev rule tool")
            {
                new Argument<DirectoryInfo>("directory")
            };

            root.Handler = CommandHandler.Create<DirectoryInfo>(WriteRules);
            root.Invoke(args);
        }

        static void WriteRules(DirectoryInfo directory)
        {
            Console.WriteLine(
                "# Dynamically generated with the OpenTabletDriver.udev tool. " +
                "https://github.com/OpenTabletDriver/OpenTabletDriver"
            );

            Console.WriteLine(
                new Rule(
                    new Token("KERNEL", Operator.Equal, "uinput"),
                    new Token("SUBSYSTEM", Operator.Equal, "misc"),
                    new Token("OPTIONS", Operator.Add, "static_node=uinput")
                )
            );

            Console.WriteLine(
                new Rule(
                    new Token("KERNEL", Operator.Equal, "uinput"),
                    new Token("SUBSYSTEM", Operator.Equal, "misc"),
                    new Token("TAG", Operator.Add, "uaccess")
                )
            );

            var identifiers = GetAllIdentifiers(directory)
                .OrderBy(x => x.Key.VendorId)
                .ThenBy(x => x.Key.ProductId);

            foreach (var (identifier, data) in identifiers)
            {
                foreach (var name in data.Names.OrderBy(name => name))
                    Console.WriteLine($"# {name}");

                Console.WriteLine(
                    new Rule(
                        new Token("SUBSYSTEM", Operator.Equal, "hidraw"),
                        new ATTRS("idVendor", Operator.Equal, identifier.VendorId.ToHexFormat()),
                        new ATTRS("idProduct", Operator.Equal, identifier.ProductId.ToHexFormat()),
                        new Token("MODE", Operator.Assign, "0666")
                    )
                );

                Console.WriteLine(
                    new Rule(
                        new Token("SUBSYSTEM", Operator.Equal, "usb"),
                        new ATTRS("idVendor", Operator.Equal, identifier.VendorId.ToHexFormat()),
                        new ATTRS("idProduct", Operator.Equal, identifier.ProductId.ToHexFormat()),
                        new Token("MODE", Operator.Assign, "0666")
                    )
                );

                if (data.Flags.HasFlag(UdevDeviceFlags.LibInputOverride))
                {
                    Console.WriteLine(
                        new Rule(
                            new Token("SUBSYSTEM", Operator.Equal, "input"),
                            new ATTRS("idVendor", Operator.Equal, identifier.VendorId.ToHexFormat()),
                            new ATTRS("idProduct", Operator.Equal, identifier.ProductId.ToHexFormat()),
                            new ENV("LIBINPUT_IGNORE_DEVICE", Operator.Assign, "1")
                        )
                    );
                }
            }
        }

        static IEnumerable<TabletConfiguration> GetAllConfigurations(DirectoryInfo directory)
        {
            var jsonSerializer = new JsonSerializer();
            var files = Directory.GetFiles(directory.FullName, "*.json", SearchOption.AllDirectories);
            foreach (var path in files)
            {
                var file = new FileInfo(path);
                using (var fs = file.OpenRead())
                using (var sr = new StreamReader(fs))
                using (var jr = new JsonTextReader(sr))
                    yield return jsonSerializer.Deserialize<TabletConfiguration>(jr);
            }
        }

        static Dictionary<UdevDeviceIdentifier, UdevDeviceData> GetAllIdentifiers(DirectoryInfo directory)
        {
            var identifiers = new Dictionary<UdevDeviceIdentifier, UdevDeviceData>();
            foreach (var tabletConfiguration in GetAllConfigurations(directory))
            {
                foreach (var identifier in tabletConfiguration.DigitizerIdentifiers)
                    addIdentifier(tabletConfiguration, identifier);
                foreach (var identifier in tabletConfiguration.AuxilaryDeviceIdentifiers)
                    addIdentifier(tabletConfiguration, identifier);
            }

            return identifiers;

            void addIdentifier(TabletConfiguration config, DeviceIdentifier identifier)
            {
                var name = config.Name;
                if (string.IsNullOrWhiteSpace(name))
                    return;

                var udevDeviceIdentifier = new UdevDeviceIdentifier(
                    identifier.VendorID,
                    identifier.ProductID
                );

                ref var data = ref CollectionsMarshal.GetValueRefOrAddDefault(identifiers, udevDeviceIdentifier, out _);
                data ??= new UdevDeviceData();
                data.Names.Add(name);

                if (config.Attributes.TryGetValue("libinputoverride", out var value) && (value == "1" || value.ToLower() == "true"))
                    data.Flags |= UdevDeviceFlags.LibInputOverride;
            }
        }
    }
}
