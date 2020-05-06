using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using HidSharp;
using TabletDriverLib;
using TabletDriverLib.Binding;
using TabletDriverLib.Contracts;
using TabletDriverLib.Plugins;
using TabletDriverPlugin;
using TabletDriverPlugin.Tablet;

namespace OpenTabletDriverDaemon
{
    public class DriverDaemon : IDriverDaemon
    {
        public DriverDaemon()
        {
            Driver = new Driver();
        }

        public Driver Driver { private set; get; }
        private Settings Settings { set; get; }

        public bool SetTablet(TabletProperties tablet)
        {
            return Driver.Open(tablet);
        }

        public TabletProperties GetTablet()
        {
            return Driver.TabletProperties;
        }

        public void SetSettings(Settings settings)
        {
            Settings = settings;
            
            Driver.OutputMode = new PluginReference(Settings.OutputMode).Construct<IOutputMode>();

            if (Driver.OutputMode != null)
            {
                Log.Write("Settings", $"Output mode: {Driver.OutputMode.GetType().FullName}");
            }

            if (Driver.OutputMode is IOutputMode outputMode)
            {                
                outputMode.Filters = from filter in Settings.Filters
                    select new PluginReference(filter).Construct<IFilter>();

                if (outputMode.Filters != null)
                    Log.Write("Settings", $"Filters: {string.Join(", ", outputMode.Filters)}");
                
                outputMode.TabletProperties = Driver.TabletProperties;
            }
            
            if (Driver.OutputMode is IAbsoluteMode absoluteMode)
            {
                absoluteMode.Output = new Area
                {
                    Width = Settings.DisplayWidth,
                    Height = Settings.DisplayHeight,
                    Position = new Point
                    {
                        X = Settings.DisplayX,
                        Y = Settings.DisplayY
                    }
                };
                Log.Write("Settings", $"Display area: {absoluteMode.Output}");

                absoluteMode.Input = new Area
                {
                    Width = Settings.TabletWidth,
                    Height = Settings.TabletHeight,
                    Position = new Point
                    {
                        X = Settings.TabletX,
                        Y = Settings.TabletY
                    }
                };
                Log.Write("Settings", $"Tablet area: {absoluteMode.Input}");

                absoluteMode.AreaClipping = Settings.EnableClipping;   
                Log.Write("Settings", $"Clipping: {(absoluteMode.AreaClipping ? "Enabled" : "Disabled")}");
            }

            if (Driver.OutputMode is IRelativeMode relativeMode)
            {
                relativeMode.XSensitivity = Settings.XSensitivity;
                Log.Write("Settings", $"Horizontal Sensitivity: {relativeMode.XSensitivity}");

                relativeMode.YSensitivity = Settings.YSensitivity;
                Log.Write("Settings", $"Vertical Sensitivity: {relativeMode.YSensitivity}");

                relativeMode.ResetTime = Settings.ResetTime;
                Log.Write("Settings", $"Reset time: {relativeMode.ResetTime}");
            }

            if (Driver.OutputMode is IBindingHandler<IBinding> bindingHandler)
            {
                bindingHandler.TipBinding = BindingTools.GetBinding(Settings.TipButton);
                bindingHandler.TipActivationPressure = Settings.TipActivationPressure;
                Log.Write("Settings", $"Tip Binding: '{bindingHandler.TipBinding?.Name ?? "None"}'@{bindingHandler.TipActivationPressure}%");

                if (Settings.PenButtons != null)
                {
                    for (int index = 0; index < Settings.PenButtons.Count; index++)
                        bindingHandler.PenButtonBindings[index] = BindingTools.GetBinding(Settings.PenButtons[index]);

                    Log.Write("Settings", $"Pen Bindings: " + string.Join(", ", bindingHandler.PenButtonBindings));
                }

                if (Settings.AuxButtons != null)
                {
                    for (int index = 0; index < Settings.AuxButtons.Count; index++)
                        bindingHandler.AuxButtonBindings[index] = BindingTools.GetBinding(Settings.AuxButtons[index]);

                    Log.Write("Settings", $"Express Key Bindings: " + string.Join(", ", bindingHandler.AuxButtonBindings));
                }
            }

            if (Settings.AutoHook)
            {
                Driver.BindingEnabled = true;
                Log.Write("Settings", "Driver is auto-enabled.");
            }
        }

        public Settings GetSettings()
        {
            return Settings;
        }

        public void SetOutputMode(PluginReference outputMode)
        {
            Driver.OutputMode = outputMode.Construct<IOutputMode>();
        }

        public IOutputMode GetOutputMode()
        {
            return Driver.OutputMode;
        }

        public async Task<bool> ImportPlugin(FileInfo plugin)
        {
            return await PluginManager.AddPlugin(plugin);
        }

        public void SetInputHook(bool isHooked)
        {
            Driver.BindingEnabled = isHooked;
        }
    }
}