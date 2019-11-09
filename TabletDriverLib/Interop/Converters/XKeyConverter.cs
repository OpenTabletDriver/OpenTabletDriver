using System;
using System.Linq;
using NativeLib.Linux.Input;
using TabletDriverLib.Interop.Input;

namespace TabletDriverLib.Interop.Converters
{
    public class XKeyConverter : IConverter<Key, XKeySymDef>
    {
        public Key Convert(XKeySymDef obj)
        {
            var name = obj.GetDescription().Description;
            return ConversionTools.GetEnumValues<Key>().FirstOrDefault(
                key => key.GetDescription().Description == name
            );
        }

        public XKeySymDef Convert(Key obj)
        {
            var name = Enum.GetName(typeof(Key), obj);
            return ConversionTools.GetEnumValues<XKeySymDef>().FirstOrDefault(
                key => key.GetDescription().Description == name
            );
        }
    }
}