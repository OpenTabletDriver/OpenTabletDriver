using System;

public interface IPowerManager : IDisposable
{
    event EventHandler<PowerEventArgs> PowerEvent;
}