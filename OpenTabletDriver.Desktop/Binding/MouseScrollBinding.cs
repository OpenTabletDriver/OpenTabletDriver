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

        private ITimer? _timer;
        private ScrollDirection _direction;
        private int _interval = 1;

        [Resolved]
        public IMouseScrollHandler? Pointer { set; get; }

        [Resolved]
        public ITimer? Timer
        {
            get => _timer;
            set
            {
                if (_timer != null)
                    _timer.Elapsed -= Scroll;

                _timer = value;

                if (_timer != null)
                {
                    _timer.Interval = _interval;
                    _timer.Elapsed += Scroll;
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

        private int _amount = 120;

        [Property("Amount"),
         DefaultPropertyValue(120),
         ToolTip("The amount to scroll. A negative value will scroll up or left " +
                 "and a positive value will scroll down or right.\n\n" +
                 "Note: A tick equals to 120 on Windows & Linux.")]
        public int Amount
        {
            get => _amount;
            set => _amount = value != 0 ? value : 1;
        }

        [Property("Interval"),
         DefaultPropertyValue(300),
         Unit("ms"),
         ToolTip("The interval at which to scroll.")]
        public int Interval
        {
            get => _interval;
            set
            {
                _interval = Math.Max(1, value);
                if (_timer != null)
                    _timer.Interval = _interval;
            }
        }

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

        private static IEnumerable<string>? validDirections;
        public static IEnumerable<string> ValidDirections =>
            validDirections ??= Enum.GetValues<ScrollDirection>().Select(Enum.GetName)!;

        public override string ToString() => $"{PLUGIN_NAME}: Direction: {Direction}, Amount: {Amount}, Interval: {Interval}";
    }
}
