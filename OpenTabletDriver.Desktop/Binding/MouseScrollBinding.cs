using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Timers;

#nullable enable

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName(PLUGIN_NAME)]
    public class MouseScrollBinding : IStateBinding
    {
        private const string PLUGIN_NAME = "Mouse Scroll Binding";
        private ScrollDirection _direction;

        [Resolved]
        public IMouseScrollHandler? Pointer { set; get; }

        [Resolved]
        public ITimer? Timer
        {
            get;
            set
            {
                if (field != null)
                    field.Elapsed -= Scroll;

                field = value;

                if (field != null)
                {
                    field.Interval = Interval;
                    field.Elapsed += Scroll;
                }
            }
        }

        [OnDependencyLoad]
        public void VerifyInitialization()
        {
            if (Pointer == null)
                Log.Write(PLUGIN_NAME,
                    $"{nameof(IMouseScrollHandler)} unavailable. Your selected output mode is incompatible",
                    LogLevel.Error);
        }

        [Property("Direction"), DefaultPropertyValue("Vertical"), PropertyValidated(nameof(ValidDirections))]
        public string Direction
        {
            get => _direction.ToString();
            set
            {
                if (Enum.TryParse(value, out ScrollDirection direction))
                    _direction = direction;
                else
                    Log.Write("MouseScrollBinding", $"Invalid scroll direction '{value}', defaulting to 'Vertical'", LogLevel.Warning);
            }
        }

        [Property("Amount"),
         DefaultPropertyValue(120),
         ToolTip("The amount to scroll. A negative value will scroll up or left " +
                 "and a positive value will scroll down or right.\n\n" +
                 "Note: A tick equals to 120 on Windows & Linux.")]
        public int Amount
        {
            get;
            set => field = value != 0 ? value : 1;
        } = 120;

        [Property("Interval"),
         DefaultPropertyValue(300),
         Unit("ms"),
         ToolTip("The interval at which to scroll.")]
        public int Interval
        {
            get;
            set
            {
                field = Math.Max(1, value);
                if (Timer != null)
                    Timer.Interval = field;
            }
        } = 1;

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            Scroll();
            Timer?.Start();
        }

        public void Release(TabletReference tablet, IDeviceReport report) => Timer?.Stop();

        public void Scroll()
        {
            if (_direction == ScrollDirection.Vertical)
                Pointer?.ScrollVertically(-Amount);
            else
                Pointer?.ScrollHorizontally(-Amount);

            if (Pointer is ISynchronousPointer synchronousPointer)
                synchronousPointer.Flush();
        }

        public static IEnumerable<string> ValidDirections =>
            field ??= Enum.GetValues<ScrollDirection>().Select(Enum.GetName)!;

        public override string ToString() => $"{PLUGIN_NAME}: Direction: {Direction}, Amount: {Amount}, Interval: {Interval}";
    }
}
