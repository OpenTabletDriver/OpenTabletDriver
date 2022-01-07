using System;
using System.Threading;
using Eto.Forms;

namespace OpenTabletDriver.UX.Tools
{
    /// <summary>
    /// A UX composition scheduler that fires events at a maximum of <see cref="MAX_FRAMES_PER_SEC"/>
    /// and stops firing when there are no event handlers registered
    /// </summary>
    public static class CompositionScheduler
    {
        private static Timer scheduler = new Timer(_ => Application.Instance.AsyncInvoke(OnCompose));
        private const int MAX_FRAMES_PER_SEC = 60;

        public static void Register(EventHandler handler)
        {
            Compose += handler;
            Refresh();
        }

        public static void Unregister(EventHandler handler)
        {
            Compose -= handler;
            Refresh();
        }

        private static void Refresh()
        {
            if (Compose == null || Compose.GetInvocationList().Length == 0)
                Stop();
            else
                Start();
        }

        private static void Start()
        {
            if (!running)
            {
                running = true;
                scheduler.Change(0, 1000 / MAX_FRAMES_PER_SEC);
            }
        }

        private static void Stop()
        {
            if (running)
            {
                running = false;
                scheduler.Change(0, Timeout.Infinite);
            }
        }

        private static void OnCompose()
        {
            Compose?.Invoke(null, null);
        }

        private static bool running;
        private static event EventHandler Compose;
    }
}
