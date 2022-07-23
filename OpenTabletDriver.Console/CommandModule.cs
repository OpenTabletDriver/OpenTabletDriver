using System.CommandLine;

namespace OpenTabletDriver.Console
{
    public abstract class CommandModule : ICommandModule
    {
        public RootCommand Build(string name)
        {
            var root = new RootCommand("OpenTabletDriver Console Client")
            {
                Name = name
            };

            foreach (var command in new CommandCollection(this))
                root.AddCommand(command);

            return root;
        }
    }
}
