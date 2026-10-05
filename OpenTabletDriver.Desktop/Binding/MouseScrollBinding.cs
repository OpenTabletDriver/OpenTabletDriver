using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Timers;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName(PLUGIN_NAME)]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public class MouseScrollBinding : IStateBinding
    {
        private const string PLUGIN_NAME = "Mouse Scroll Binding";

        private readonly IMouseScrollHandler _mouseScrollHandler;

        public MouseScrollBinding(IMouseScrollHandler mouseScrollHandler, ITimer timer)
        {
            _mouseScrollHandler = mouseScrollHandler;
            Timer = timer;
        }

        private ScrollDirection _direction;
        private int _interval = 1;

        public ITimer Timer
        {
            get;
            init
            {
                if (field != null)
                    field.Elapsed -= Scroll;

                field = value;

                if (field != null)
                {
                    field.Interval = _interval;
                    field.Elapsed += Scroll;
                }
            }
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

        [BooleanProperty("Invert", "Scroll Direction")]
        public bool Invert
        {
            get;
            set;
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
                Timer.Interval = _interval;
            }
        }

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            Scroll();
            Timer.Start();
        }

        public void Release(TabletReference tablet, IDeviceReport report) => Timer.Stop();

        public void Scroll()
        {
            int adjustedAmount = Invert ? Amount : Amount * -1;

            if (_direction == ScrollDirection.Vertical)
                _mouseScrollHandler.ScrollVertically(adjustedAmount);
            else
                _mouseScrollHandler.ScrollHorizontally(adjustedAmount);

            if (_mouseScrollHandler is ISynchronousPointer synchronousPointer)
                synchronousPointer.Flush();
        }

        private static IEnumerable<string>? validDirections;
        public static IEnumerable<string> ValidDirections =>
            validDirections ??= Enum.GetValues<ScrollDirection>().Select(Enum.GetName)!;

        public override string ToString() => $"{PLUGIN_NAME}: Direction: {Direction}, Amount: {Amount}, Interval: {Interval}";
    }
}
