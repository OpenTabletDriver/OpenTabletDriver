using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;

namespace OpenTabletDriver.Tools.LibinputQuirks
{
    class Program
    {
        static readonly bool DEFAULT_SMOOTHING = false;
        static readonly uint DEFAULT_TIPDOWN_PRESSURE_PERMILLE = 10;
        static readonly uint DEFAULT_TIPUP_PRESSURE_PERMILLE = 9;
        static readonly string LIBINPUT_HEADER = "[OpenTabletDriver]";
        static readonly string CONFIG_KEY_NAME_STRING = "MatchName";
        static readonly string CONFIG_KEY_TABLET_SMOOTHING = "AttrTabletSmoothing";
        static readonly string CONFIG_KEY_PRESSURE_RANGE = "AttrPressureRange";

        static void Main(string[] args)
        {
            var root = new RootCommand("OpenTabletDriver libinput quirks generator")
            {
                new Option<uint>(new[] { "--tip-down-pressure-permille", "-pd" }, () => DEFAULT_TIPDOWN_PRESSURE_PERMILLE, "Pressure threshold for tip down in permille (1/1000)"),
                new Option<uint>(new[] { "--tip-up-pressure-permille", "-pu" }, () => DEFAULT_TIPUP_PRESSURE_PERMILLE, "Pressure threshold for tip up in permille (1/1000)"),
                new Option<bool>(new[] { "--smoothing", "-s" }, () => DEFAULT_SMOOTHING, "Allow libinput tablet smoothing"),
                new Option(new[] { "--no-pressure", "-np" }, $"Do not include '{CONFIG_KEY_PRESSURE_RANGE}'"),
                new Argument<FileInfo>("output", "Resulting .quirks file"),
                new Option(new[] { "--verbose", "-v" }, "Verbose output")
            };

            root.Handler = CommandHandler.Create<uint, uint, bool, bool, FileInfo, bool>(WriteQuirksAsync);
            root.Invoke(args);
        }

        static async Task WriteQuirksAsync(uint tipDownPressurePermille, uint tipUpPressurePermille, bool smoothing, bool skipPressure, FileInfo output, bool verbose)
        {
            if (tipUpPressurePermille >= tipDownPressurePermille) throw new ArgumentOutOfRangeException("Tip-up pressure must be less than tip-down pressure");

            var filenameDoesntContainQuirks = !output.Name.EndsWith(".quirks");
            if (filenameDoesntContainQuirks)
            {
                Console.WriteLine("INFO: File name does not end with '.quirks' - this has been added!");
                output = new FileInfo(output.FullName + ".quirks");
            }
            var path = output.FullName.Contains(Directory.GetCurrentDirectory()) ? output.Name : output.FullName;
            Console.WriteLine($"Writing quirks to '{path}'");

            if (output.Exists)
                output.Delete();
            if (!output.Directory.Exists)
                output.Directory.Create();

            using (var sw = output.AppendText())
            {
                if (verbose) Console.WriteLine("Adding header");
                await sw.WriteLineAsync(LIBINPUT_HEADER);
                foreach (var line in CreateQuirks(smoothing, skipPressure, tipDownPressurePermille, tipUpPressurePermille))
                {
                    if (verbose) Console.WriteLine($"Adding {line.Substring(0,line.IndexOf("="))}");
                    await sw.WriteLineAsync(line);
                }
            }
        }

        static IEnumerable<string> CreateQuirks(bool smoothing, bool skipPressure, uint tipDownPressurePermille, uint tipUpPressurePermille)
        {
            yield return FormatKeyValue(CONFIG_KEY_NAME_STRING, EvdevVirtualTablet.TABLET_NAME);
            yield return FormatKeyValue(CONFIG_KEY_TABLET_SMOOTHING, Convert.ToInt32(smoothing).ToString());

            if (!skipPressure)
            {
                yield return FormatKeyValue(CONFIG_KEY_PRESSURE_RANGE,
                        FormatPressureRange(
                            PressureConvertPermilleToUnits(tipDownPressurePermille),
                            PressureConvertPermilleToUnits(tipUpPressurePermille)
                        ));
            }
        }

        static string FormatKeyValue(string left, string right) => left + "=" + right;

        static string FormatPressureRange(uint tipDownPressure, uint tipUpPressure) => tipDownPressure + ":" + tipUpPressure;

        static uint PressureConvertPermilleToUnits(uint permille) => (uint)(EvdevVirtualTablet.MAX_PRESSURE * ((double)permille / 1000));
    }
}
