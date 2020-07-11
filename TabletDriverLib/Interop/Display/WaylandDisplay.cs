using System;
using System.Collections.Generic;
using System.Linq;
using TabletDriverPlugin;
using TabletDriverPlugin.Platform.Display;
using WaylandNET.Client;
using WaylandNET.Client.Protocol;

namespace TabletDriverLib.Interop.Display
{
    public class WaylandDisplay : IVirtualScreen, WlRegistry.IListener
    {
        private List<WaylandOutput> _outputs;
        private ZxdgOutputManagerV1 _outputManager;

        public WaylandDisplay()
        {
            _outputs = new List<WaylandOutput>();
            using (var connection = new WaylandClientConnection())
            {
                var registry = connection.Display.GetRegistry();
                registry.Listener = this;
                connection.Roundtrip();
                if (_outputManager != null)
                {
                    foreach (var output in _outputs)
                    {
                        output.XdgOutput = _outputManager.GetXdgOutput(output.WlOutput);
                        output.XdgOutput.Listener = output;
                    }
                }
                connection.Roundtrip();
            }
        }

        public IEnumerable<IDisplay> Displays => new IDisplay[] { this }.Concat(_outputs);

        public int Index => 0;

        public float Width
        {
            get
            {
                var left = _outputs.Min(d => d.Position.X);
                var right = _outputs.Max(d => d.Position.X + d.Width);
                return right - left;
            }
        }

        public float Height
        {
            get
            {
                var top = _outputs.Min(d => d.Position.Y);
                var bottom = _outputs.Max(d => d.Position.Y + d.Height);
                return bottom - top;
            }
        }

        public Point Position => new Point(0, 0);

        public override string ToString()
        {
            return $"Virtual Display ({Width}x{Height}@{Position})";
        }

        void WlRegistry.IListener.Global(WlRegistry wlRegistry, uint name, string @interface, uint version)
        {
            switch (@interface)
            {
                case "wl_output":
                    var wlOutput = wlRegistry.Bind<WlOutput>(name, @interface, 3);
                    var output = new WaylandOutput(wlOutput, _outputs.Count + 1);
                    output.WlOutput.Listener = output;
                    _outputs.Add(output);
                    break;
                case "zxdg_output_manager_v1":
                    _outputManager = wlRegistry.Bind<ZxdgOutputManagerV1>(name, @interface, 3);
                    break;
            }
        }

        void WlRegistry.IListener.GlobalRemove(WlRegistry wlRegistry, uint name)
        {
        }
    }
}
