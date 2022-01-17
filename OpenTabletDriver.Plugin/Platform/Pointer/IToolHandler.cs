namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IToolHandler
    {
        void RegisterTool(uint toolID, ulong toolSerial);
    }
}
