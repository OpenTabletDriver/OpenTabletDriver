using Eto.Forms;
using OpenTabletDriver.Platform.Keyboard;

namespace OpenTabletDriver.UX.Services
{
    public class EtoKeysProvider : IKeysProvider
    {
        public EtoKeysProvider()
        {
            var pairs = from key in Enum.GetNames<Keys>()
                where key != nameof(Keys.None)
                select new KeyValuePair<string, object>(key, key);

            EtoToNative = new Dictionary<string, object>(pairs);
        }

        public IDictionary<string, object> EtoToNative { get; }
    }
}
