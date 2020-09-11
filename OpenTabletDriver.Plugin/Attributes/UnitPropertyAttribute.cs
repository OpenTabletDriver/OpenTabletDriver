namespace OpenTabletDriver.Plugin.Attributes
{
    public class UnitPropertyAttribute : PropertyAttribute
    {
        public UnitPropertyAttribute(string displayName, string unit) : base(displayName)
        {
            Unit = unit;
        }
        
        public string Unit { set; get; }
    }
}