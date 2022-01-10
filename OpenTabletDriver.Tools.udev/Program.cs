using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Tools.udev
{
    class Program
    {
        static void Main(string[] args)
        {
            var root = new RootCommand("OpenTabletDriver udev rule tool")
            {
                new Argument<DirectoryInfo>("directory"),
                new Argument<FileInfo>("output"),
                new Option(new string[] { "--verbose", "-v" }, "Verbose output")
            };

            root.Handler = CommandHandler.Create<DirectoryInfo, FileInfo, bool>(WriteRules);
            root.Invoke(args);
        }

        static async Task WriteRules(DirectoryInfo directory, FileInfo output, bool verbose = false)
        {
            if (output.Exists)
                output.Delete();
            if (!output.Directory.Exists)
                output.Directory.Create();

            var path = output.FullName.Replace(Directory.GetCurrentDirectory(), string.Empty);
            Console.WriteLine($"Writing all rules to '{path}'...");
            using (var sw = output.AppendText())
            {
                await sw.WriteLineAsync(
                    "# Dynamically generated with the OpenTabletDriver.udev tool. " +
                    "https://github.com/OpenTabletDriver/OpenTabletDriver"
                );
                foreach (var rule in CreateRules(directory))
                {
                    await sw.WriteLineAsync(rule);
                    if (verbose)
                        Console.WriteLine(rule);
                }
            }
            Console.WriteLine("Finished writing all rules.");
        }

        static IEnumerable<string> CreateRules(DirectoryInfo directory)
        {
            yield return RuleGenerator.CreateAccessRule("uinput", "misc");
            foreach (var tablet in GetAllConfigurations(directory))
            {
                if (string.IsNullOrWhiteSpace(tablet.Name))
                    continue;
                yield return string.Format("# {0}", tablet.Name);

                foreach (var rule in RuleGenerator.CreateAccessRules(tablet, "hidraw", "0666"))
                    yield return rule;

                foreach (var rule in RuleGenerator.CreateAccessRules(tablet, "usb", "0666"))
                    yield return rule;

                if (tablet.Attributes.TryGetValue("libinputoverride", out var value) && (value == "1" || value.ToLower() == "true"))
                    foreach (var rule in RuleGenerator.CreateOverrideRules(tablet))
                        yield return rule;
            }
        }

        static IEnumerable<TabletConfiguration> GetAllConfigurations(DirectoryInfo directory)
        {
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

        static readonly JsonSerializer jsonSerializer = new JsonSerializer();
    }
}
