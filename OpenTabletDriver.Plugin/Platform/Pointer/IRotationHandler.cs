namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IRotationHandler
    {
        /// <summary>
        /// Set the rotation of the pen using absolute barrel rotation data.
        /// </summary>
        /// <param name="percentage">Rotation value normalized to between 0 and 1</param>
        void SetRotation(float percentage);
    }
}
