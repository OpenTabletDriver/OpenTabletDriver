namespace OpenTabletDriver.Plugin.Tablet
{
    /// <summary>
    /// Defines a tablet report parser that is dynamically looked up when
    /// defined in <see cref="TabletConfiguration"/> in <see cref="DeviceIdentifier.ReportParser"/>
    /// </summary>
    /// <typeparam name="T"><see cref="IDeviceReport"/></typeparam>
    /// <remarks>
    /// Make sure you add the following attribute to the inheriting class:
    /// <c>[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]</c>
    /// </remarks>
    public interface IReportParser<T> where T : IDeviceReport
    {
        T Parse(byte[] report);
    }
}
