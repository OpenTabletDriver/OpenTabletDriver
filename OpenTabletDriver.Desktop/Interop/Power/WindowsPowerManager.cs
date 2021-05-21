using System;
using Microsoft.Win32;
namespace OpenTabletDriver.Desktop.Interop.Power 
{
    public class WindowsPowerManager : IPowerManager
    {
        public event EventHandler<PowerEventArgs> PowerEvent;

        public WindowsPowerManager() 
        {
            SystemEvents.PowerModeChanged += OnPowerEvent;
        }

        void OnPowerEvent(object sender, PowerModeChangedEventArgs e)
        {
            PowerEvent?.Invoke(this, ConvertArgs(e));
        }

        private PowerEventArgs ConvertArgs(PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    return new PowerEventArgs(PowerEventType.Suspend);

                case PowerModes.Resume:
                    return new PowerEventArgs(PowerEventType.Resume);
            }

            return new PowerEventArgs(PowerEventType.Unknown);
        }

        public void Dispose()
        {
            // detach static event handler to prevent memory leaks
            // https://docs.microsoft.com/en-us/dotnet/api/microsoft.win32.systemevents.powermodechanged?view=net-5.0#remarks
        
            SystemEvents.PowerModeChanged -= OnPowerEvent;
        }
    }
}