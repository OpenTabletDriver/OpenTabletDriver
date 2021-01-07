using System;
using System.Threading.Tasks;

namespace OpenTabletDriver.Plugin
{
    public static class Info
    {
        public static IDriver Driver => GetDriverInstance();

        public static Func<Task<IUXDriver>> GetUXInstance { set; get; }
        public static Func<IDriver> GetDriverInstance { set; get; }
    }
}