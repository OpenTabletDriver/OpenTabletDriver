using System;
using System.Diagnostics;
using OpenTabletDriver.Platform.Environment;

namespace OpenTabletDriver.Daemon.Interop.Environment
{
    public abstract class EnvironmentHandler : IEnvironmentHandler
    {
        public abstract void Open(string path);
        public virtual void OpenFolder(string path) => Open(path);

        protected void Exec(string executable, string args)
        {
            try
            {
                Process.Start(executable, args);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        protected void Exec(ProcessStartInfo startInfo)
        {
            try
            {
                Process.Start(startInfo);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }
    }
}
