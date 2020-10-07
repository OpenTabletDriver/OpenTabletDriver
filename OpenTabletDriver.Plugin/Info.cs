using System;

namespace OpenTabletDriver.Plugin
{
    public static class Info
    {
        public static IDriver Driver => GetDriverInstance();
        
        public static Func<IDriver> GetDriverInstance { set; get; }
    }
}