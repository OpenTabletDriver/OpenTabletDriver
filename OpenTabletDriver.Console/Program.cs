using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;

namespace OpenTabletDriver.Console
{
    using static CommandTools;

    partial class Program
    {
        static async Task Main(string[] args)
        {
            var root = new RootCommand("OpenTabletDriver Console Client")
            {
                Name = "otd"
            };
            root.AddRange(GenerateIOCommands());
            root.AddRange(GenerateActionCommands());
            root.AddRange(GenerateDebugCommands());
            root.AddRange(GenerateModifyCommands());
            root.AddRange(GenerateRequestCommands());
            root.AddRange(GenerateListCommands());
            root.AddRange(GenerateScriptingCommands());

            await Driver.Connect();
            await root.InvokeAsync(args);
        }

        static IEnumerable<Command> GenerateIOCommands()
        {
            yield return CreateCommand<FileInfo>(LoadSettings, "Load settings from a file", "load");
            yield return CreateCommand<FileInfo>(SaveSettings, "Save settings to a file", "save");
        }

        static IEnumerable<Command> GenerateActionCommands()
        {
            yield return CreateCommand(Detect, "Detects tablets");
        }

        static IEnumerable<Command> GenerateDebugCommands()
        {
            yield return CreateCommand<int>(GetString, "Requests a device string");
        }

        static IEnumerable<Command> GenerateModifyCommands()
        {
            yield return CreateCommand<float, float, float, float>(SetDisplayArea, "Sets the display area");
            yield return CreateCommand<float, float, float, float, float>(SetTabletArea, "Sets the tablet area");
            yield return CreateCommand<float, float>(SetSensitivity, "Sets the relative sensitivity");
            yield return CreateCommand<string, string, float>(SetTipBinding, "Sets the current tip binding");
            yield return CreateCommand<string, string, int>(SetPenBinding, "Sets the current pen button bindings");
            yield return CreateCommand<string, string, int>(SetAuxBinding, "Sets the current express key bindings");
            yield return CreateCommand<int>(SetResetTime, "Sets the reset time in milliseconds");
            yield return CreateCommand<bool>(SetAutoHook, "Sets whether the driver should automatically enable on start");
            yield return CreateCommand<bool>(SetEnableClipping, "Sets whether inputs should be limited to the specified areas");
            yield return CreateCommand<bool>(SetLockAspectRatio, "Sets whether to lock tablet width/height to display width/height ratio");
        }

        static IEnumerable<Command> GenerateRequestCommands()
        {
            yield return CreateCommand(GetCurrentLog, "Gets the current log", "log");
            yield return CreateCommand(GetAllSettings, "Gets all current settings");
            yield return CreateCommand(GetAreas, "Gets the current display and tablet area");
            yield return CreateCommand(GetSensitivity, "Gets the current relative sensitivity");
            yield return CreateCommand(GetBindings, "Gets all current bindings");
            yield return CreateCommand(GetMiscSettings, "Gets other uncategorized settings");
            yield return CreateCommand(GetOutputMode, "Gets the current output mode");
            yield return CreateCommand(GetFilters, "Gets the currently enabled filters");
            yield return CreateCommand(GetTools, "Gets the currently enabled tools");
        }

        static IEnumerable<Command> GenerateListCommands()
        {
            yield return CreateCommand(ListOutputModes, "Lists all available output modes");
            yield return CreateCommand(ListFilters, "Lists all available filters");
            yield return CreateCommand(ListTools, "Lists all available tools");
            yield return CreateCommand(ListBindings, "Lists all available binding types");
        }

        static IEnumerable<Command> GenerateScriptingCommands()
        {
            Command[] commands = 
            {
                CreateCommand(GetAllSettingsJson, "Gets all current settings in JSON format"),
                CreateCommand(GetDiagnostics, "Gets diagnostic information in JSON format")
            };
            foreach (var command in commands)
                command.IsHidden = true;
            return commands;
        }
    }
}
