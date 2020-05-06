using TabletDriverLib.Interop.Converters;

namespace OpenTabletDriverUX.Converters
{
    public class SafeFloatConverter : IConverter<string, float>
    {
        public string Convert(float obj)
        {
            return obj.ToString();
        }

        public float Convert(string obj)
        {
            return float.TryParse(obj, out var result) ? result : 0f;
        }
    }
}