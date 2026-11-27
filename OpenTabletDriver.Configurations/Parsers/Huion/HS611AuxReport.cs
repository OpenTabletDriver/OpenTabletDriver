using OpenTabletDriver.Configurations.Parsers.UCLogic;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Huion
{
    public struct HS611AuxReport : IAuxReport
    {
        public HS611AuxReport(byte[] report)
        {
            Raw = report;
            var auxReport = new UCLogicAuxReport(report);
            AuxButtons = HS611AuxState.UpdatePadButtons(auxReport.AuxButtons);
        }

        public bool[] AuxButtons { get; set; }
        public byte[] Raw { get; set; }
    }

    internal static class HS611AuxState
    {
        // Pad buttons (indices 0-9) arrive on the digitizer interface, touch buttons
        // (indices 10-17) on the media aux interface; both are merged into Buttons.
        private const int PAD_BUTTON_COUNT = 10;
        private const int TOTAL_BUTTON_COUNT = 18;

        private static readonly object Lock = new();
        private static readonly bool[] Buttons = new bool[TOTAL_BUTTON_COUNT];

        public static bool[] UpdatePadButtons(bool[] auxButtons)
        {
            lock (Lock)
            {
                for (int i = 0; i < PAD_BUTTON_COUNT; i++)
                    Buttons[i] = i < auxButtons.Length && auxButtons[i];

                return (bool[])Buttons.Clone();
            }
        }

        public static bool[] SetTouchButton(int buttonIndex, bool pressed)
        {
            lock (Lock)
            {
                Buttons[buttonIndex] = pressed;
                return (bool[])Buttons.Clone();
            }
        }

        public static bool[] Snapshot()
        {
            lock (Lock)
                return (bool[])Buttons.Clone();
        }
    }
}
