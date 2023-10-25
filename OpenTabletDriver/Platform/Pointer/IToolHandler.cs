namespace OpenTabletDriver.Platform.Pointer
{
    public interface IToolHandler
    {
        void RegisterTool(uint toolID, ulong toolSerial);
    }
}
