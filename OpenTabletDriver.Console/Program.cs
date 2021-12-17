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
            root.AddRange(UpdateCommands);
            root.AddRange(ListCommands);
            root.AddRange(ScriptingCommands);

            return root;
        }

        private static readonly IEnumerable<Command> IOCommands = new Command[]
        {
            CreateCommand<FileInfo>(LoadSettings, "Load settings from a file", "load"),
            CreateCommand<FileInfo>(SaveSettings, "Save settings to a file", "save"),
            CreateCommand<string>(ApplyPreset, "Apply a preset from the Presets directory", "preset"),
            CreateCommand<string>(SavePreset, "Save the current settings to the Presets directory")
        };

        private static readonly IEnumerable<Command> ActionCommands = new Command[]
        {
            CreateCommand(Detect, "Detects tablets")
        };

        private static readonly IEnumerable<Command> DebugCommands = new Command[]
        {
            CreateCommand<int, int, int>(GetString, "Requests a device string")
        };

        private static readonly IEnumerable<Command> ModifyCommands = new Command[]
        {
            CreateCommand<string, string>(SetOutputMode, "Sets the output mode"),
            CreateCommand<string, string[]>(SetFilters, "Sets the filters applied to the current output mode"),
            CreateCommand<string[]>(SetTools, "Sets the active tools"),
            CreateCommand<string, float, float, float, float>(SetDisplayArea, "Sets the display area"),
            CreateCommand<string, float, float, float, float, float>(SetTabletArea, "Sets the tablet area"),
            CreateCommand<string, float, float, float>(SetSensitivity, "Sets the relative sensitivity"),
            CreateCommand<string, string, float>(SetTipBinding, "Sets the current tip binding"),
            CreateCommand<string, string, int>(SetPenBinding, "Sets the current pen button bindings"),
            CreateCommand<string, string, int>(SetAuxBinding, "Sets the current express key bindings"),
            CreateCommand<string, int>(SetResetTime, "Sets the reset time in milliseconds"),
            CreateCommand<string, bool>(SetEnableClipping, "Sets whether inputs should be limited to the specified areas"),
            CreateCommand<string, bool>(SetEnableAreaLimiting, "Sets whether inputs outside of the tablet area should be ignored"),
            CreateCommand<string, bool>(SetLockAspectRatio, "Sets whether to lock tablet width/height to display width/height ratio")
        };

        private static readonly IEnumerable<Command> RequestCommands = new Command[]
        {
            CreateCommand(GetCurrentLog, "Gets the current log", "log"),
            CreateCommand(GetAllSettings, "Gets all current settings"),
            CreateCommand<string>(GetOutputMode, "Gets the current output mode"),
            CreateCommand<string>(GetAreas, "Gets the current display and tablet area"),
            CreateCommand<string>(GetSensitivity, "Gets the current relative sensitivity"),
            CreateCommand<string>(GetBindings, "Gets all current bindings"),
            CreateCommand<string>(GetMiscSettings, "Gets other uncategorized settings"),
            CreateCommand<string>(GetFilters, "Gets the currently enabled filters"),
            CreateCommand(GetTools, "Gets the currently enabled tools")
        };

        private static readonly IEnumerable<Command> UpdateCommands = new Command[]
        {
            CreateCommand(HasUpdate, "Check for any updates"),
            CreateCommand(InstallUpdate, "Install update")
        };

        private static readonly IEnumerable<Command> ListCommands = new Command[]
        {
            CreateCommand(ListOutputModes, "Lists all available output modes"),
            CreateCommand(ListFilters, "Lists all available filters"),
            CreateCommand(ListTools, "Lists all available tools"),
            CreateCommand(ListBindings, "Lists all available binding types")
        };

        private static readonly IEnumerable<Command> ScriptingCommands = new Command[]
        {
            CreateCommand(GetDiagnostics, "Gets diagnostic information"),
            CreateCommand(STDIO, "Open with standard input and output", "stdio"),
            CreateCommand(EditSettings, "Opens the settings file with the editor defined in the EDITOR environment variable.", "edit")
        };
    }
}
