using Eto.Forms;

namespace OpenTabletDriver.UX.Components
{
    public class AppCommand : Command
    {
        public AppCommand(string menuText)
        {
            MenuText = menuText;
        }

        public AppCommand(string menuText, Keys shortcut) : this(menuText)
        {
            Shortcut = shortcut;
        }

        public AppCommand(string menuText, Action handler) : this(menuText)
        {
            AddActionHandler(handler);
        }

        public AppCommand(string menuText, Func<Task> handler) : this(menuText)
        {
            AddTaskHandler(handler);
        }

        public AppCommand(string menuText, Action handler, Keys shortcut) : this(menuText, shortcut)
        {
            AddActionHandler(handler);
        }

        public AppCommand(string menuText, Func<Task> handler, Keys shortcut) : this(menuText, shortcut)
        {
            AddTaskHandler(handler);
        }

        private void AddActionHandler(Action handler)
        {
            Executed += (_, _) => handler();
        }

        private void AddTaskHandler(Func<Task> handler)
        {
            Executed += (_, _) => handler().Run();
        }
    }
}
