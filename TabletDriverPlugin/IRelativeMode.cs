namespace TabletDriverPlugin
{
    public interface IRelativeMode : IOutputMode
    {
        float XSensitivity { set; get; }
        float YSensitivity { set; get; }
    }
}