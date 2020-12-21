using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenTabletDriver.Tests
{
    [TestClass]
    public class TestBase
    {
        protected static string TestDirectory = Environment.GetEnvironmentVariable("OPENTABLETDRIVER_TEST") ?? Environment.CurrentDirectory;
    }
}