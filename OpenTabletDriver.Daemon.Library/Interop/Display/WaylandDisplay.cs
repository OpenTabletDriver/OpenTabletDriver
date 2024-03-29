using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Platform.Display;
using WaylandNET.Client;
using WaylandNET.Client.Protocol;

namespace OpenTabletDriver.Daemon.Interop.Display
{
    public class WaylandDisplay : IVirtualScreen
    {
        private readonly List<WaylandOutput> _outputs;

        public WaylandDisplay()
        {
            _outputs = new List<WaylandOutput>();
            using (var connection = new WaylandClientConnection())
            {
                ZxdgOutputManagerV1? outputManager = null;
                var registry = connection.Display.GetRegistry();
                registry.Global += (wlRegistry, name, @interface, version) =>
                {
                    switch (@interface)
                    {
                        case "wl_output":
                            var output = new WaylandOutput
                            {
                                Index = _outputs.Count + 1,
                                WlOutput = wlRegistry.Bind<WlOutput>(name, @interface, 1)
                            };
                            output.WlOutput.Geometry += (wlOutput, x, y, physicalWidth, physicalHeight, subpixel, make, model, transform) =>
                            {
                                if (output.XdgOutput == null || output.XdgOutput.Version < 2)
                                {
                                    output.Position = new Vector2(x, y);
                                    output.Name = make;
                                    output.Description = model;
                                }
                            };
                            output.WlOutput.Mode += (wlOutput, flags, width, height, refresh) =>
                            {
                                if (output.XdgOutput == null && flags.HasFlag(WlOutput.ModeEnum.Current))
                                {
                                    output.Width = width;
                                    output.Height = height;
                                }
                            };
                            _outputs.Add(output);
                            break;
                        case "zxdg_output_manager_v1":
                            outputManager = wlRegistry.Bind<ZxdgOutputManagerV1>(name, @interface, Math.Min(version, 2));
                            break;
                    }
                };
                connection.Roundtrip();
                if (outputManager != null)
                {
                    foreach (var output in _outputs)
                    {
                        output.XdgOutput = outputManager.GetXdgOutput(output.WlOutput);
                        output.XdgOutput.LogicalPosition += (xdgOutput, x, y) =>
                        {
                            output.Position = new Vector2(x, y);
                        };
                        output.XdgOutput.LogicalSize += (xdgOutput, width, height) =>
                        {
                            output.Width = width;
                            output.Height = height;
                        };
                        output.XdgOutput.Name += (xdgOutput, name) =>
                        {
                            output.Name = name;
                        };
                        output.XdgOutput.Description += (xdgOutput, description) =>
                        {
                            output.Description = description;
                        };
                    }
                }
                connection.Roundtrip();
            }
        }

        public IEnumerable<IDisplay> Displays => _outputs.Append<IDisplay>(this);

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

        public Vector2 Position => new Vector2(0, 0);

        public override string ToString() => $"Virtual Display ({Width}x{Height}@{Position})";
    }
}
