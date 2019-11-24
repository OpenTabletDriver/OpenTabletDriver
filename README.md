[![Actions Status](https://github.com/InfinityGhost/OpenTabletDriver/workflows/.NET%20Core/badge.svg)](https://github.com/InfinityGhost/OpenTabletDriver/actions) [![CodeFactor](https://www.codefactor.io/repository/github/infinityghost/opentabletdriver/badge/master)](https://www.codefactor.io/repository/github/infinityghost/opentabletdriver/overview/master)

# OpenTabletDriver
OpenTabletDriver is an open source tablet configurator. The goal of OpenTabletDriver is to be cross platform as possible with the highest compatability possible in an easily configurable graphical user interface.

## Releases

- None yet!

# Build Dependencies
The requirements to build OpenTabletDriver are consistent across all platforms. Running OpenTabletDriver on each platform requires different dependencies.

## All platforms
- .NET Core 3.0 SDK

### Linux
- Xlib

### Windows [Currently broken, [see issue #2](https://github.com/InfinityGhost/OpenTabletDriver/issues/2)]
> No special dependencies as of now.
> The program will still build and run on Windows, but you may not be able to hook device input.

### Mac OS X [Unsupported]
> Code is written for Mac OS X, but zero testing has been done to test if it works.
- Quartz

# Features
- Absolute cursor positioning
- Precise areas
  - Screen area and tablet area
  - Area offsets
- Pen bindings
  - Tip to Mouse Button binding
- Saving and loading settings
  - Auto-loads user settings via `settings.xml` in the active directory
- Tablet Configuration Manager
  - Can convert [TabletDriver](https://github.com/hawku/TabletDriver) configuration files (.cfg)

## Planned features
- Relative cursor positioning
- Precise area rotation
- Keyboard bindings
- Auxilary button bindings

# Improving OpenTabletDriver
If you wish to help improve OpenTabletDriver, first [check out the pinned issues](https://github.com/InfinityGhost/OpenTabletDriver/issues).

# Supported Tablets
These tablets are fully configured and confirmed functional.
- Wacom
  - CTL-480
- Gaomon
  - S620

## Configured Tablets
These tablets are configured but may not function as expected.
- Wacom
  - CTE-440
  - CTH-470
  - CTH-480
  - CTH-490
  - CTH-670
  - CTH-680
  - CTH-690
  - CTL-470
  - CTL-471
  - CTL-472
  - CTL-490
  - CTL-671
  - CTL-672
  - CTL-680
  - CTL-690
  - CTL-4100
  - CTL-4100 Bluetooth
  - CTL-6100
  - PTH-451
  - PTH-660
  - PTH-850