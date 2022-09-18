using Eto.Forms;

namespace OpenTabletDriver.UX.Components
{
    public class AppCommand : Command
    {
        public AppCommand(string text)
        {
            MenuText = text;
            ToolBarText = text;
        }

        public AppCommand(string text, Keys shortcut) : this(text)
        {
            Shortcut = shortcut;
        }

        public AppCommand(string text, Action handler) : this(text)
        {
            AddActionHandler(handler);
        }

        public AppCommand(string text, Func<Task> handler) : this(text)
        {
            AddTaskHandler(handler);
        }

        public AppCommand(string text, Action handler, Keys shortcut) : this(text, shortcut)
        {
            AddActionHandler(handler);
        }

        public AppCommand(string text, Func<Task> handler, Keys shortcut) : this(text, shortcut)
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
