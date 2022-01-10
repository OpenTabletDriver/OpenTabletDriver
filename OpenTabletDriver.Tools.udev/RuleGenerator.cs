using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tools.udev.Comparers;
using udev.NET.Rules;
using udev.NET.Rules.Names;

namespace OpenTabletDriver.Tools.udev
{
    internal static class RuleGenerator
    {
        public static Rule CreateAccessRule(string module, string subsystem)
        {
            return new Rule(
                new Token("KERNEL", Operator.Equal, module),
                new Token("SUBSYSTEM", Operator.Equal, subsystem),
                new Token("TAG", Operator.Add, "uaccess"),
                new Token("OPTIONS", Operator.Add, $"static_node={module}")
            );
        }

        private static IEnumerable<DeviceIdentifier> GetDistinctIdentifiers(TabletConfiguration config)
        {
            var allIdentifiers = config.DigitizerIdentifiers.Concat(config.AuxilaryDeviceIdentifiers);
            return allIdentifiers.Distinct(new IdentifierComparer());
        }

        public static IEnumerable<Rule> CreateAccessRules(TabletConfiguration tablet, string subsystem, string mode)
        {
            foreach (var id in GetDistinctIdentifiers(tablet))
            {
                yield return new Rule(
                    new Token("SUBSYSTEM", Operator.Equal, subsystem),
                    new ATTRS("idVendor", Operator.Equal, id.VendorID.ToHexFormat()),
                    new ATTRS("idProduct", Operator.Equal, id.ProductID.ToHexFormat()),
                    new Token("MODE", Operator.Assign, mode)
                );
            }
        }

        public static IEnumerable<Rule> CreateOverrideRules(TabletConfiguration tablet)
        {
            foreach (var id in GetDistinctIdentifiers(tablet))
            {
                yield return new Rule(
                    new Token("SUBSYSTEM", Operator.Equal, "input"),
                    new ATTRS("idVendor", Operator.Equal, id.VendorID.ToHexFormat()),
                    new ATTRS("idProduct", Operator.Equal, id.ProductID.ToHexFormat()),
                    new ENV("LIBINPUT_IGNORE_DEVICE", Operator.Assign, "1")
                );
            }
        }
    }
}
