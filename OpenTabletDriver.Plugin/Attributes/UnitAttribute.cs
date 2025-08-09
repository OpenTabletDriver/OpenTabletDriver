namespace OpenTabletDriver.Plugin.Attributes
{
    /// <summary>
    /// Applies a unit suffix to a property on the client.
    /// </summary>
    public class UnitAttribute : ModifierAttribute
    {
        public UnitAttribute(string unit)
        {
            this.Unit = unit;
        }

        public string Unit { set; get; }
    }
}
