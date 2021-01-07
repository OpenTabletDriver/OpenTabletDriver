using System;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName("UX Binding")]
    public class UXBinding : IBinding, IValidateBinding
    {
        public string[] ValidProperties => new string[]
        {
            "Show Window"
        };

        [Property("Property")]
        public string Property { get; set; }

        public Action Press => async () =>
        {
            switch (Property)
            {
                case "Show Window":
                    try
                    {
                        var ux = await Info.GetUXInstance();
                        await ux.ShowClient();
                    }
                    catch { }
                    break;

                default:
                    break;
            }
        };

        public Action Release => () =>
        {

        };
    }
}