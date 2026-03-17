using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Timers;
using System;
using System.Numerics;

#nullable enable

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName(PLUGIN_NAME)]
    public class PenScrollingBinding : IStatePositionBinding
    {
        private const string PLUGIN_NAME = "Pen Scrolling Binding";

        private Vector2 _initialPosition;
        private int _scrollAmountHorizontal, _scrollAmountVertical;

        [Resolved]
        public IMouseScrollHandler? Pointer { set; get; }

        private ITimer? _scrollingTimer;

        [Resolved]
        public ITimer? ScrollingTimer
        {
            get => _scrollingTimer;
            set
            {
                if (value is null)
                    return;

                if (_scrollingTimer != null && !ReferenceEquals(_scrollingTimer, value))
                {
                    if (_scrollingTimer.Enabled)
                        _scrollingTimer.Stop();
                    _scrollingTimer.Elapsed -= Scroll;
                    _scrollingTimer.Dispose();
                }

                _scrollingTimer = value;
                _scrollingTimer.Elapsed += Scroll;
            }
        }

        [BooleanPropertyAttribute("Scroll horizontally", "pen movement scrolls horizontally"), DefaultPropertyValue(true)]
        public bool HorizontalScrollingEnabled { get; set; }

        [BooleanPropertyAttribute("Scroll vertically", "pen movement scrolls vertically"), DefaultPropertyValue(true)]
        public bool VerticalScrollingEnabled { get; set; }

        [Property("Sensitivity"), ToolTip("The sensitivity of scrolling with pen movement."), DefaultPropertyValue(50f)]
        public float Sensitivity { get; set; }

        [SliderProperty("Refresh rate", 1f, 320f, 60f), ToolTip("How often scrolling event gets sent (lower for better performance, higher for smoother scrolling)."), DefaultPropertyValue(60f)]
        public float RefreshRate { get; set; }

        [BooleanPropertyAttribute("Invert horizontal axis", "invert horizontal scrolling"), DefaultPropertyValue(false)]
        public bool InvertHorizontalAxis { get; set; }

        [BooleanPropertyAttribute("Invert vertical axis", "invert vertical scrolling (MacOS like)"), DefaultPropertyValue(false)]
        public bool InvertVerticalAxis { get; set; }

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            if (report is not IAbsolutePositionReport absolutePositionReport)
            {
                throw new InvalidOperationException("PenScrollingBinding not supported on this device");
            }

            ResetScrollingTimer();

            _initialPosition = absolutePositionReport.Position;
            _scrollAmountHorizontal = 0;
            _scrollAmountVertical = 0;

            _scrollingTimer?.Start();
        }

        protected void ResetScrollingTimer()
        {
            if (_scrollingTimer == null)
                throw new InvalidOperationException("Timer was not injected, can not scroll");

            if (_scrollingTimer.Enabled)
                _scrollingTimer.Stop();

            _scrollingTimer.Interval = 1000f / RefreshRate;
        }

        public void Release(TabletReference tablet, IDeviceReport report)
        {
            if (_scrollingTimer != null)
                _scrollingTimer.Stop();
        }

        public void SetPosition(Vector2 pos)
        {
            if (_scrollingTimer is null || !_scrollingTimer.Enabled)
                return;

            if (InvertHorizontalAxis)
                _scrollAmountHorizontal = (int)Math.Ceiling((_initialPosition.X - pos.X) * (Sensitivity / 100));
            else
                _scrollAmountHorizontal = (int)Math.Ceiling((pos.X - _initialPosition.X) * (Sensitivity / 100));

            if (InvertVerticalAxis)
                _scrollAmountVertical = (int)Math.Ceiling((pos.Y - _initialPosition.Y) * (Sensitivity / 100));
            else
                _scrollAmountVertical = (int)Math.Ceiling((_initialPosition.Y - pos.Y) * (Sensitivity / 100));
        }

        public void Scroll()
        {
            if (Pointer == null)
                throw new InvalidOperationException("Pointer was not injected, can not scroll");

            if (HorizontalScrollingEnabled)
                Pointer.ScrollHorizontally(_scrollAmountHorizontal);

            if (VerticalScrollingEnabled)
                Pointer.ScrollVertically(_scrollAmountVertical);

            if (Pointer is ISynchronousPointer synchronousPointer)
                synchronousPointer.Flush();
        }
    }
}
