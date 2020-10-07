namespace OpenTabletDriver.Plugin.Attributes
{
    public class UnitAttribute : ModifierAttribute
    {
        public UnitAttribute(string unit)
        {
            this.Unit = unit;
        }
        
        public string Unit { set; get; }
    }
}