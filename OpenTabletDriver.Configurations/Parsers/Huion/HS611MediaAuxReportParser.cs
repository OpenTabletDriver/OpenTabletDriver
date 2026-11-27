using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Huion
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class HS611MediaAuxReportParser : IReportParser<IDeviceReport>
    {
        // data[0] report IDs emitted by the touch buttons on the media aux interface.
        private const byte CONSUMER_REPORT = 0x01;
        private const byte KEYBOARD_REPORT = 0x03;

        private static readonly Dictionary<ushort, int> ConsumerButtonMap = new()
        {
            [0x00E2] = 10, // Mute
            [0x00EA] = 11, // Volume down
            [0x00E9] = 12, // Volume up
            [0x00B6] = 13, // Previous track
            [0x00CD] = 14, // Play/pause
            [0x00B5] = 15  // Next track
        };

        private static readonly Dictionary<(byte Modifier, byte KeyCode), int> KeyboardButtonMap = new()
        {
            [(0x08, 0x2B)] = 16, // Left GUI + Tab
            [(0x08, 0x07)] = 17  // Left GUI + D
        };

        private int? _activeConsumerButton;
        private int? _activeKeyboardButton;

        public IDeviceReport Parse(byte[] data)
        {
            if (data.Length < 3)
                return new DeviceReport(data);

            return data[0] switch
            {
                CONSUMER_REPORT => ParseConsumerReport(data),
                KEYBOARD_REPORT => ParseKeyboardReport(data),
                _ => new DeviceReport(data)
            };
        }

        private HS611TouchAuxReport ParseConsumerReport(byte[] data)
        {
            if (_activeConsumerButton is int previousButton)
                HS611AuxState.SetTouchButton(previousButton, false);

            _activeConsumerButton = null;

            // HID consumer usage is reported little-endian in bytes 1-2.
            ushort usage = BitConverter.ToUInt16(data, 1);
            if (usage != 0 && ConsumerButtonMap.TryGetValue(usage, out int buttonIndex))
            {
                _activeConsumerButton = buttonIndex;
                return new HS611TouchAuxReport(data, HS611AuxState.SetTouchButton(buttonIndex, true));
            }

            return new HS611TouchAuxReport(data, HS611AuxState.Snapshot());
        }

        private HS611TouchAuxReport ParseKeyboardReport(byte[] data)
        {
            if (_activeKeyboardButton is int previousButton)
                HS611AuxState.SetTouchButton(previousButton, false);

            _activeKeyboardButton = null;

            byte modifier = data[1];
            byte keyCode = data[2];
            if (modifier != 0 || keyCode != 0)
            {
                var combo = (modifier, keyCode);
                if (!KeyboardButtonMap.TryGetValue(combo, out int buttonIndex))
                    return new HS611TouchAuxReport(data, HS611AuxState.Snapshot());

                _activeKeyboardButton = buttonIndex;
                return new HS611TouchAuxReport(data, HS611AuxState.SetTouchButton(buttonIndex, true));
            }

            return new HS611TouchAuxReport(data, HS611AuxState.Snapshot());
        }
    }

    public struct HS611TouchAuxReport : IAuxReport
    {
        public HS611TouchAuxReport(byte[] raw, bool[] auxButtons)
        {
            Raw = raw;
            AuxButtons = auxButtons;
        }

        public bool[] AuxButtons { get; set; }
        public byte[] Raw { get; set; }
    }
}
