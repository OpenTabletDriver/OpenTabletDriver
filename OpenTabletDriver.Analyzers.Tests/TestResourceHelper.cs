using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenTabletDriver.Analyzers.Tests
{
    public static class TestResourceHelper
    {
        public static string TestProjectDir { get; } = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

        public static IEnumerable<IGrouping<string, (string file, string content)>> GetGroupedTestResourcesContent(string resourceName)
        {
            var resourceDir = Path.Combine(TestProjectDir, "TestResources", resourceName);

            return Directory.Exists(resourceDir)
                ? Directory.EnumerateDirectories(resourceDir)
                    .SelectMany(Directory.EnumerateFiles)
                    .Select(f => (f!, File.ReadAllText(f)))
                    .GroupBy(
                        k => Path.GetFileName(Path.GetDirectoryName(k.Item1))!,
                        v => (Path.GetFileName(v.Item1), v.Item2))
                : throw new DirectoryNotFoundException($"Test resource directory not found: {resourceDir}");
        }
    }
}
