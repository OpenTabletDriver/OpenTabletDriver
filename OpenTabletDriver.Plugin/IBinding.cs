using System;

namespace OpenTabletDriver.Plugin
{
    public interface IBinding
    {
        string Property { set; get; }
        Action Press { get; }
        Action Release { get; }
    }
}