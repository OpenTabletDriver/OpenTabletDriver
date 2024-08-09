using System.Collections;
using Avalonia.Data.Converters;

namespace OpenTabletDriver.UI.Converters;

public static class CollectionConverters
{
    public static readonly IValueConverter IsEmpty =
        new FuncValueConverter<object, bool>(obj => obj is ICollection collection && collection.Count == 0);

    public static readonly IValueConverter IsNotEmpty =
        new FuncValueConverter<object, bool>(obj => obj is ICollection collection && collection.Count > 0);
}
