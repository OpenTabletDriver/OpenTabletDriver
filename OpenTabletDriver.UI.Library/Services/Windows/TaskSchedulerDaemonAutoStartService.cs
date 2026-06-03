using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace OpenTabletDriver.UI.Services.Windows;

/// <summary>
/// Creates a per-user scheduled task that starts the daemon when the current user logs on.
/// </summary>
[SupportedOSPlatform("windows")]
public class TaskSchedulerDaemonAutoStartService : IDriverDaemonAutoStartService
{
    private const string TaskName = "OpenTabletDriver Daemon";
    private const int TaskTriggerLogon = 9;
    private const int TaskActionExecute = 0;
    private const int TaskCreateOrUpdate = 6;
    private const int TaskLogonInteractiveToken = 3;
    private const int TaskRunLevelLua = 0;
    private const int TaskInstancesIgnoreNew = 2;

    public bool AutoStartSupported => true;
    public bool AutoStart => TryGetTask(out _, out var task) && IsTaskForCurrentDaemon(task);
    public bool HideWindowSupported => true;
    public bool HideWindow
    {
        get
        {
            if (!TryGetTask(out _, out var task) || !IsTaskForCurrentDaemon(task))
                return true;

            try
            {
                return GetTaskAction(task)!.HideAppWindow;
            }
            catch
            {
                return true;
            }
        }
    }

    public string? BackendName => "Windows Task Scheduler";

    public bool TrySetAutoStart(bool autoStart, bool hideWindow)
    {
        if (!autoStart)
        {
            if (!TryGetTask(out var rootFolder))
                return true;

            try
            {
                rootFolder!.DeleteTask(TaskName, 0);
                return true;
            }
            catch
            {
                return false;
            }
        }

        var daemonPath = GetCurrentDaemonPath();
        if (!File.Exists(daemonPath))
            return false;

        try
        {
            var service = CreateTaskService();
            var rootFolder = service.GetFolder("\\");
            var taskDefinition = service.NewTask(0);
            var currentUser = WindowsIdentity.GetCurrent().Name;

            taskDefinition.RegistrationInfo.Description = "Launch OpenTabletDriver daemon when the current user logs on.";
            taskDefinition.Settings.Enabled = true;
            taskDefinition.Settings.Hidden = false;
            taskDefinition.Settings.AllowDemandStart = true;
            taskDefinition.Settings.StartWhenAvailable = true;
            taskDefinition.Settings.DisallowStartIfOnBatteries = false;
            taskDefinition.Settings.StopIfGoingOnBatteries = false;
            taskDefinition.Settings.ExecutionTimeLimit = "PT0S";
            taskDefinition.Settings.MultipleInstances = TaskInstancesIgnoreNew;

            var principal = taskDefinition.Principal;
            principal.UserId = currentUser;
            principal.LogonType = TaskLogonInteractiveToken;
            principal.RunLevel = TaskRunLevelLua;

            var trigger = taskDefinition.Triggers.Create(TaskTriggerLogon);
            trigger.Id = "CurrentUserLogon";
            trigger.UserId = currentUser;

            var action = taskDefinition.Actions.Create(TaskActionExecute);
            action.Id = "StartDaemon";
            action.Path = daemonPath;
            action.HideAppWindow = hideWindow;
            action.WorkingDirectory = GetCurrentWorkingDirectory();

            rootFolder.RegisterTaskDefinition(TaskName, taskDefinition, TaskCreateOrUpdate, null, null, TaskLogonInteractiveToken, null);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string GetCurrentDaemonPath()
    {
        return Path.Join(GetCurrentWorkingDirectory(), "OpenTabletDriver.Daemon.exe");
    }

    private static string GetCurrentWorkingDirectory()
    {
        return AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static bool IsTaskForCurrentDaemon(dynamic? task)
    {
        try
        {
            var action = GetTaskAction(task);
            if (action is null)
                return false;

            return PathsEqual(action.Path, GetCurrentDaemonPath())
                && PathsEqual(action.WorkingDirectory, GetCurrentWorkingDirectory());
        }
        catch
        {
            return false;
        }
    }

    private static dynamic? GetTaskAction(dynamic? task)
    {
        if (task is null)
            return null;

        var taskValue = task;
        if (taskValue.Definition.Actions.Count < 1)
            return null;

        return taskValue.Definition.Actions.Item(1);
    }

    private static bool PathsEqual(string? left, string? right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            return false;

        return string.Equals(NormalizePath(left), NormalizePath(right), StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string path)
    {
        return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static bool TryGetTask(out dynamic? rootFolder)
    {
        return TryGetTask(out rootFolder, out _);
    }

    private static bool TryGetTask(out dynamic? rootFolder, out dynamic? task)
    {
        rootFolder = null;
        task = null;
        try
        {
            var service = CreateTaskService();
            rootFolder = service.GetFolder("\\");
            task = rootFolder.GetTask(TaskName);
            return true;
        }
        catch (COMException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }

    private static dynamic CreateTaskService()
    {
        var taskServiceType = Type.GetTypeFromProgID("Schedule.Service")
            ?? throw new InvalidOperationException("Task Scheduler COM service is not available.");
        dynamic service = Activator.CreateInstance(taskServiceType)
            ?? throw new InvalidOperationException("Failed to create Task Scheduler COM service.");
        service.Connect();
        return service;
    }
}
