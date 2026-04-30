using System;

namespace OpenTabletDriver.Native.OSX.Input;

/*
 * see https://github.com/Wacom-Developer/wacom-device-kit-macos-scribble-demo/blob/754c72c0c76e23e57d97f9a773cab522fb502f81/ScribbleDemo/Wacom.h#L24-L37
 */
[Flags]
public enum WacomCapabilityMask : long
{
    deviceIdBitMask = 0x001,
    absXBitMask = 0x002,
    absYBitMask = 0x004,
    buttonsBitMask = 0x040,
    tiltXBitMask = 0x080,
    tiltYBitMask = 0x100,
    pressureBitMask = 0x400,
}
