using System;
using System.IO;
using System.Linq;
using System.Threading;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class PluginStateManager
    {
        public DirectoryInfo PluginUninstallDir = new DirectoryInfo(AppInfo.Current.PluginUninstallDir);
        public DirectoryInfo PluginUpdateDir = new DirectoryInfo(AppInfo.Current.PluginUpdateDir);

        private readonly DesktopPluginManager Manager;

        public PluginStateManager(DesktopPluginManager manager)
        {
            Manager = manager;
        }

        public void ProcessPendingStates()
        {
            using var mutex = new Mutex(false, @"Global\OpenTabletDriver.Plugin");
            mutex.WaitOne();
            ProcessUninstall();
            ProcessUpdate();
            mutex.ReleaseMutex();
        }

        private void ProcessUpdate()
        {
            if (PluginUpdateDir.Exists)
            {
                foreach (var pendingPlugin in PluginUpdateDir.EnumerateFileSystemInfos().OrderBy(p => p.Name))
                {
                    Delete(Path.Join(AppInfo.Current.PluginDirectory, pendingPlugin.Name));
                    Manager.InstallPlugin(pendingPlugin.FullName);
                    Delete(pendingPlugin.FullName);
                    var name = Path.GetFileNameWithoutExtension(pendingPlugin.Name);
                    Log.Write("Plugin", $"Plugin '{name}' updated");
                }
                PluginUpdateDir.Delete();
            }
        }

        private void ProcessUninstall()
        {
            if (PluginUninstallDir.Exists)
                PluginUninstallDir.Delete(true);
        }

        public PluginStateResult QueueUpdate(string filePath)
        {
            var name = Path.GetFileName(filePath);
            if (!PluginUpdateDir.Exists)
                PluginUpdateDir.Create();
            File.Copy(filePath, Path.Join(PluginUpdateDir.FullName, name), true);
            Log.Write("Plugin", $"Plugin '{Path.GetFileNameWithoutExtension(filePath)}");
            return PluginStateResult.UpdateQueued;
        }

        public PluginStateResult QueueUninstall(PluginInfo plugin)
        {
            if (plugin.State == PluginState.PendingUninstall)
                return PluginStateResult.AlreadyQueued;

            if (!PluginUninstallDir.Exists)
                PluginUninstallDir.Create();

            var target = Path.Join(PluginUninstallDir.FullName, Path.GetFileName(plugin.Path));

            try
            {
                if (plugin.Form == PluginForm.File)
                    File.Move(plugin.Path, target);
                else
                    Directory.Move(plugin.Path, target);
                plugin.State = PluginState.PendingUninstall;
                Log.Write("Plugin", $"Plugin '{Path.GetFileNameWithoutExtension(plugin.Name)}' enqueued for uninstall");
                return PluginStateResult.UninstallQueued;
            }
            catch (Exception e)
            {
                Log.Write("Plugin", $"Failed to enqueue uninstall for '{plugin.Name}' with error: {e.StackTrace}", LogLevel.Error);
                return PluginStateResult.Error;
            }
        }

        private static void Delete(string path)
        {
            var attrib = File.GetAttributes(path);
            if (attrib.HasFlag(FileAttributes.Directory))
                Directory.Delete(path, true);
            else
                File.Delete(path);
        }
    }
}