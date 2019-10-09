using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenTabletDriverGUI
{
    internal static class Extensions
    {
        public static void CopyPropertiesTo<T, TU>(this T source, TU dest)
        {
            var sourceProperties = typeof (T).GetProperties().Where(x => x.CanRead).ToList();
            var destinationProperties = typeof(TU).GetProperties().Where(x => x.CanWrite).ToList();
            foreach (var sourceProp in sourceProperties)
            {
                if (destinationProperties.Any(x => x.Name == sourceProp.Name))
                {
                    var property = destinationProperties.First(prop => prop.Name == sourceProp.Name);
                    if (property.CanWrite)
                        property.SetValue(dest, sourceProp.GetValue(source, null), null);
                }
            }
        }
    }
}