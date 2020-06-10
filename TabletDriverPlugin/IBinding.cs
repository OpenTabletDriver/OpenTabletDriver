using System;

namespace TabletDriverPlugin
{
    public interface IBinding
    {
        string Property { set; get; }
        Action Press { get; }
        Action Release { get; }
    }
}