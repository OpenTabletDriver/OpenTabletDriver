using System.Collections.Generic;

namespace OpenTabletDriver.Daemon.Contracts;

public interface IEnvironmentDictionary
{
    Dictionary<string, string> Variables { get; }
}
