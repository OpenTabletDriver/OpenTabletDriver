using System.Collections.Generic;

namespace OpenTabletDriver.Tests.Fakes
{
    public class FakeDirectory : FakeFileSystemEntry
    {
        public List<FakeFileSystemEntry> Entries { get; } = new();
    }
}
