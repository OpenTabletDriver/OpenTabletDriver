using Eto.Forms;
using OpenTabletDriver.Platform.Keyboard;

namespace OpenTabletDriver.UX.Services
{
    public class EtoKeysProvider : IKeysProvider
    {
        public EtoKeysProvider()
        {
            var keys = Enum.GetNames<Keys>();
            var pairs = keys.Select(k => new KeyValuePair<string, object>(k, k));
            EtoToNative = new Dictionary<string, object>(pairs);
        }

        public IDictionary<string, object> EtoToNative { get; }
    }
}
