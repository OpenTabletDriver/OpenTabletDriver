using System;
public enum PowerEventType
{
    Unknown,
    Suspend,
    Resume
}

public class PowerEventArgs : EventArgs 
{
    public PowerEventType EventType;

    public PowerEventArgs(PowerEventType type) => EventType = type;
}