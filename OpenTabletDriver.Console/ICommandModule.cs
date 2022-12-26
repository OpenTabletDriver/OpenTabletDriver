using System.CommandLine;

namespace OpenTabletDriver.Console;

public interface ICommandModule
{
    RootCommand Build(string name);
}
