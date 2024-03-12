using System;

namespace OpenTabletDriver.Tests.Fakes
{
    public class FakeFile : FakeFileSystemEntry
    {
        public byte[] Data { get; init; } = Array.Empty<byte>();
    }
}
