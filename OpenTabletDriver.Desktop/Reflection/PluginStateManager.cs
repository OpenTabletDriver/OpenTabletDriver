using System.IO;
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
                foreach (var pendingPlugin in PluginUpdateDir.EnumerateFileSystemInfos())
                {
                    Delete(Path.Join(AppInfo.Current.PluginDirectory, pendingPlugin.Name));
                    Manager.InstallPlugin(pendingPlugin.FullName);
                    Delete(pendingPlugin.FullName);
                }
                PluginUpdateDir.Delete();
            }
        }

        private void ProcessUninstall()
        {
            if (PluginUninstallDir.Exists)
            {
                foreach (var pendingUninstall in PluginUninstallDir.GetFileSystemInfos())
                {
                    try
                    {
                        pendingUninstall.Delete();
                    }
                    catch
                    {
                        Log.Write("Plugin", $"Failed to delete '{Path.GetFileNameWithoutExtension(pendingUninstall.Name)}'");
                    }
                }
                PluginUninstallDir.Delete();
            }
        }

        public PluginStateResult QueueUpdate(string filePath)
        {
            var name = Path.GetFileName(filePath);
            if (!PluginUpdateDir.Exists)
                PluginUpdateDir.Create();
            File.Copy(filePath, Path.Join(PluginUpdateDir.FullName, name), true);
            return PluginStateResult.UpdateQueued;
        }

        public PluginStateResult QueueUninstall(PluginInfo plugin)
        {
            if (plugin.State == PluginState.PendingUninstall)
                return PluginStateResult.AlreadyQueued;

            var target = Path.Join(PluginUninstallDir.FullName, plugin.Form == PluginForm.File ? Path.GetFileName(plugin.Path) : Path.GetDirectoryName(plugin.Path));

            try
            {
                if (plugin.Form == PluginForm.File)
                    File.Move(plugin.Path, target);
                else
                    Directory.Move(plugin.Path, target);
                plugin.State = PluginState.PendingUninstall;
                return PluginStateResult.UninstallQueued;
            }
            catch
            {
                Log.Write("Plugin", $"Failed to enqueue '{plugin.Name}'");
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