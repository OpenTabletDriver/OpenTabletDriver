namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IPenActionHandler
    {
        /// <summary>
        /// Activate (button press) the appropriate action for the output mode
        /// </summary>
        /// <param name="action"></param>
        void Activate(PenAction action);

        /// <summary>
        /// Deactivate (button release) the appropriate action for the output mode
        /// </summary>
        /// <param name="action"></param>
        void Deactivate(PenAction action);
    }
}
