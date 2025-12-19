namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IToolHandler
    {
        /// <summary>
        /// Register a tool entering range.
        /// Changing tool is expected to be done via <see cref="ISynchronousPointer.Reset()"/> followed by registering a tool again.
        /// </summary>
        /// <param name="toolID">The ID of the tool provided by the parser. This usually defines a specific type of tool, and does not guarantee tool uniqueness.</param>
        /// <param name="toolSerial">The serial number of the tool provided by the parser. Use this along with <paramref name="toolID"/> to define uniqueness</param>
        void RegisterTool(uint toolID, ulong toolSerial);
    }
}
