using System;
using System.Collections.Generic;
using OpenTabletDriver.Plugin.Tablet.Wheel.Actions;

namespace OpenTabletDriver.Plugin.Tablet.Wheel
{
    public class WheelModeManager
    {
        private readonly WheelModeSlot[] modes;
        private readonly Dictionary<WheelActionType, IWheelAction> actionMap;

        private int currentMode = 0;
        private bool[] previousAux = new bool[20];

        private int wheelDirection = 0;
        private DateTime lastWheelEvent = DateTime.MinValue;
        private readonly TimeSpan repeatInterval = TimeSpan.FromMilliseconds(50);

        public Action<IWheelAction, int>? OnWheelAction;

        public WheelModeManager(WheelModeSlot[] config, IWheelActionFactory factory)
        {
            modes = config ?? throw new ArgumentNullException(nameof(config));

            actionMap = new Dictionary<WheelActionType, IWheelAction>
            {
                { WheelActionType.Scroll, factory.CreateScroll() },
                { WheelActionType.CanvasZoom, factory.CreateZoom() },
                { WheelActionType.BrushSize, factory.CreateBrush() },
                { WheelActionType.UndoRedo, factory.CreateUndoRedo() },
                { WheelActionType.LayerUpDown, factory.CreateLayer() },
                { WheelActionType.Custom, factory.CreateCustom() }
            };
        }

        public WheelModeSlot CurrentMode => modes[currentMode];

        public void SwitchMode()
        {
            int start = currentMode;

            do
            {
                currentMode = (currentMode + 1) % modes.Length;
            }
            while (!modes[currentMode].Enabled && currentMode != start);

            Log.Write(nameof(WheelModeManager), $"Switched to mode: {modes[currentMode].Name}", LogLevel.Debug);
        }

        public void HandleDelta(int delta)
        {
            var mode = modes[currentMode];

            if (actionMap.TryGetValue(mode.ActionType, out var action))
            {
                int direction = delta > 0 ? +1 : -1;
                OnWheelAction?.Invoke(action, direction);
            }
        }

        private bool IsRising(bool now, bool before)
        {
            return !before && now;
        }

        public void UpdateAuxButtons(bool[] current)
        {
            if (IsRising(current[3], previousAux[3]))
            {
                wheelDirection = 0;
                SwitchMode();
                Array.Copy(current, previousAux, current.Length);
                return;
            }
        
            if (current[3])
            {
                Array.Copy(current, previousAux, current.Length);
                return;
            }
        
            if (IsRising(current[8], previousAux[8]))
            {
                wheelDirection = +1;
                lastWheelEvent = DateTime.Now;
                HandleDelta(+1);
            }
        
            if (IsRising(current[9], previousAux[9]))
            {
                wheelDirection = -1;
                lastWheelEvent = DateTime.Now;
                HandleDelta(-1);
            }
        
            Array.Copy(current, previousAux, current.Length);
        }

        public void Tick()
        {
            if (wheelDirection != 0 &&
                DateTime.Now - lastWheelEvent > repeatInterval)
            {
                lastWheelEvent = DateTime.Now;
                HandleDelta(wheelDirection);
            }
        }
    }
}
