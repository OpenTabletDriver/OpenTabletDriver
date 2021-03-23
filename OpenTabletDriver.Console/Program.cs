using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;

namespace OpenTabletDriver.Console
{
    using static CommandTools;

    partial class Program
    {
        public static async Task Main(string[] args)
        {
            await Driver.Connect();
            await Root.InvokeAsync(args);
        }

        private static readonly Lazy<RootCommand> root = new Lazy<RootCommand>(GenerateRoot);
        public static RootCommand Root => root.Value;

        private static RootCommand GenerateRoot()
        {
            var root = new RootCommand("OpenTabletDriver Console Client")
            {
                Name = "otd"
            };

            root.AddRange(IOCommands);
            root.AddRange(ActionCommands);
            root.AddRange(DebugCommands);
            root.AddRange(ModifyCommands);
            root.AddRange(RequestCommands);
            root.AddRange(ListCommands);
            root.AddRange(ScriptingCommands);

            return root;
        }

        private static readonly IEnumerable<Command> IOCommands = new Command[]
        {
            CreateCommand<FileInfo>(LoadSettings, "Load settings from a file", "load"),
            CreateCommand<FileInfo>(SaveSettings, "Save settings to a file", "save")
        };

        private static readonly IEnumerable<Command> ActionCommands = new Command[]
        {
            CreateCommand(Detect, "Detects tablets")
        };

        private static readonly IEnumerable<Command> DebugCommands = new Command[]
        {
            CreateCommand<int>(GetString, "Requests a device string")
        };

        private static readonly IEnumerable<Command> ModifyCommands = new Command[]
        {
            CreateCommand<string>(SetOutputMode, "Sets the output mode"),
            CreateCommand<string[]>(SetFilters, "Sets the filters applied to the current output mode"),
            CreateCommand<string[]>(SetInterpolators, "Sets the active interpolators for the current output mode"),
            CreateCommand<string[]>(SetTools, "Sets the active tools"),
            CreateCommand<float, float, float, float>(SetDisplayArea, "Sets the display area"),
            CreateCommand<float, float, float, float, float>(SetTabletArea, "Sets the tablet area"),
            CreateCommand<float, float, float>(SetSensitivity, "Sets the relative sensitivity"),
            CreateCommand<string, string, float>(SetTipBinding, "Sets the current tip binding"),
            CreateCommand<string, string, int>(SetPenBinding, "Sets the current pen button bindings"),
            CreateCommand<string, string, int>(SetAuxBinding, "Sets the current express key bindings"),
            CreateCommand<int>(SetResetTime, "Sets the reset time in milliseconds"),
            CreateCommand<bool>(SetAutoHook, "Sets whether the driver should automatically enable on start"),
            CreateCommand<bool>(SetEnableClipping, "Sets whether inputs should be limited to the specified areas"),
            CreateCommand<bool>(SetEnableAreaLimiting, "Sets whether inputs outside of the tablet area should be ignored"),
            CreateCommand<bool>(SetLockAspectRatio, "Sets whether to lock tablet width/height to display width/height ratio")
        };

        private static readonly IEnumerable<Command> RequestCommands = new Command[]
        {
            CreateCommand(GetCurrentLog, "Gets the current log", "log"),
            CreateCommand(GetAllSettings, "Gets all current settings"),
            CreateCommand(GetOutputMode, "Gets the current output mode"),
            CreateCommand(GetAreas, "Gets the current display and tablet area"),
            CreateCommand(GetSensitivity, "Gets the current relative sensitivity"),
            CreateCommand(GetBindings, "Gets all current bindings"),
            CreateCommand(GetMiscSettings, "Gets other uncategorized settings"),
            CreateCommand(GetFilters, "Gets the currently enabled filters"),
            CreateCommand(GetInterpolators, "Gets the currently enabled interpolators"),
            CreateCommand(GetTools, "Gets the currently enabled tools")
        };

        private static IEnumerable<Command> ListCommands = new Command[]
        {
            CreateCommand(ListOutputModes, "Lists all available output modes"),
            CreateCommand(ListFilters, "Lists all available filters"),
            CreateCommand(ListTools, "Lists all available tools"),
            CreateCommand(ListBindings, "Lists all available binding types")
        };

        private static readonly IEnumerable<Command> ScriptingCommands = new Command[]
        {
            CreateCommand(GetDiagnostics, "Gets diagnostic information"),
            CreateCommand(STDIO, "Open with standard input and output", "stdio")
        };
    }
}
