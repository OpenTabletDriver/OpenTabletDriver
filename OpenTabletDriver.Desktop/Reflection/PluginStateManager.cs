using System;
using System.IO;
using System.Linq;
using System.Threading;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class PluginStateManager
    {
        public FileInfo UninstallCommands = new FileInfo(Path.Join(AppInfo.Current.PendingPluginDirectory, "uninstallCommands.txt"));

        private DesktopPluginManager Manager;

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
            var update = new DirectoryInfo(AppInfo.Current.PendingPluginDirectory);
            if (update.Exists)
            {
                foreach (var pendingPlugin in update.EnumerateFileSystemInfos())
                {
                    Delete(Path.Join(AppInfo.Current.PluginDirectory, pendingPlugin.Name));
                    Manager.InstallPlugin(pendingPlugin.FullName);
                    Delete(pendingPlugin.FullName);
                }
            }
            update.Delete();
        }

        private void ProcessUninstall()
        {
            string[] pluginToUninstall = GetUninstallCommands();
            foreach (var plugin in pluginToUninstall)
            {
                var pluginDir = Path.Join(AppInfo.Current.PluginDirectory, plugin);
                var pluginDll = Path.Join(AppInfo.Current.PluginDirectory, plugin + ".dll");
                try
                {
                    if (Directory.Exists(pluginDir))
                        Directory.Delete(pluginDir, true);
                    else if (File.Exists(pluginDll))
                    {
                        File.Delete(pluginDll);
                    }
                }
                catch
                {
                    Log.Write("Plugin", $"Failed to uninstall '{plugin}'", LogLevel.Error);
                }
            }
            UninstallCommands.Delete();
        }

        public PluginProcessingResult QueueUpdate(string filePath)
        {
            var name = Path.GetFileName(filePath);
            if (!Directory.Exists(AppInfo.Current.PendingPluginDirectory))
                Directory.CreateDirectory(AppInfo.Current.PendingPluginDirectory);
            File.Copy(filePath, Path.Join(AppInfo.Current.PendingPluginDirectory, name), true);
            return PluginProcessingResult.UpdateQueued;
        }

        public PluginProcessingResult QueueUninstall(string pluginName)
        {
            string[] commands = GetUninstallCommands();

            if (!commands.Contains(pluginName))
            {
                using var outFile = new StreamWriter(UninstallCommands.Open(FileMode.Append));
                outFile.WriteLine(pluginName);
                return PluginProcessingResult.UninstallQueued;
            }
            else
            {
                return PluginProcessingResult.None;
            }
        }

        private string[] GetUninstallCommands()
        {
            string[] commands = Array.Empty<string>();
            if (UninstallCommands.Exists)
            {
                using var readFile = new StreamReader(UninstallCommands.FullName);
                commands = readFile.ReadToEnd().Split(Environment.NewLine);
            }
            else
            {
                UninstallCommands.Create().Dispose();
            }
            return commands;
        }

        private static void Delete(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
            else if (Directory.Exists(path))
                Directory.Delete(path);
        }
    }
}