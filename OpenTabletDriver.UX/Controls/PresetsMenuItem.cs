using Eto.Forms;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Controls
{
    public class PresetsMenuItem : ButtonMenuItem
    {
        private readonly RpcClient<IDriverDaemon> _rpc;

        public PresetsMenuItem(RpcClient<IDriverDaemon> rpc)
        {
            _rpc = rpc;

            Text = "Apply preset...";

            Validate += (_, _) => UpdateItems().Run();
        }

        private async Task UpdateItems()
        {
            Items.Clear();

            foreach (var preset in await _rpc.Instance!.GetPresets())
            {
                Items.Add(GetPresetItem(preset));
            }

            Enabled = Items.Any();
        }

        private MenuItem GetPresetItem(string preset)
        {
            var command = new AppCommand(preset, () => _rpc.Instance!.ApplyPreset(preset));
            return new ButtonMenuItem(command);
        }
    }
}
