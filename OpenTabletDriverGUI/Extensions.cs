using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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

        public static ObservableCollection<T> ToObservableCollection<T>(this ICollection<T> collection)
        {
            return new ObservableCollection<T>(collection);
        }
    }
}