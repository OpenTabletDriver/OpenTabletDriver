namespace OpenTabletDriver.Plugin.Tablet
{
    public interface IRotationReport : IDeviceReport
    {
        /// <summary>
        /// Raw rotation data from the tablet.
        /// </summary>
        /// <remarks>
        /// The rotation range is stored in the tablet's config as <see cref="PenSpecifications.MinRotation"/> and <see cref="PenSpecifications.MaxRotation"/>.
        /// </remarks>
        int Rotation { set; get; }
    }
}
