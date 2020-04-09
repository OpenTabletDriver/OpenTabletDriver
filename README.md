[![Actions Status](https://github.com/InfinityGhost/OpenTabletDriver/workflows/.NET%20Core/badge.svg)](https://github.com/InfinityGhost/OpenTabletDriver/actions) [![CodeFactor](https://www.codefactor.io/repository/github/infinityghost/opentabletdriver/badge/master)](https://www.codefactor.io/repository/github/infinityghost/opentabletdriver/overview/master)

# OpenTabletDriver

OpenTabletDriver is an open source tablet configurator. The goal of OpenTabletDriver is to be cross platform as possible with the highest compatibility in an easily configurable graphical user interface.

## Releases

You can grab the latest release below. Make sure to download the version for your platform.

- [Latest release](https://github.com/InfinityGhost/OpenTabletDriver/releases)

# Build Dependencies

The requirements to build OpenTabletDriver are consistent across all platforms. Running OpenTabletDriver on each platform requires different dependencies.

## All platforms
- .NET Core 3.1 SDK

### Windows

No special dependencies.

### Linux

- libx11
- libxrandr
- libxtst
- libevdev2

##### Debian / Ubuntu

- libx11-dev
- libxrandr-dev
- libxtst-dev
- libevdev-dev

### Mac OS X [Unsupported]
> Code is written for Mac OS X, but it isn't maintained at all. It will compile but most functions either won't work or cause crashes. There isn't any plan to support this platform at the moment but it certainly could be done.

No special dependencies.

# Features

- Absolute cursor positioning
  - Screen area and tablet area
  - Center-anchored offsets
  - Precise area rotation
- Relative cursor positioning
- Pen bindings
  - Tip by pressure bindings
  - Express key bindings
  - Pen button bindings
  - Mouse button bindings
  - Keyboard bindings
  - External plugin bindings
- Saving and loading settings
  - Auto-loads user settings via `settings.json` in the active directory
- Tablet Configuration Manager
  - Can convert [TabletDriver](https://github.com/hawku/TabletDriver) configuration files (.cfg)
- Plugins
  - Filters
  - Output modes

# Improving OpenTabletDriver

If you wish to help improve OpenTabletDriver, first [check out the pinned issues](https://github.com/InfinityGhost/OpenTabletDriver/issues).

# Tablet Support

All statuses of tablets that are supported, untested, and planned to be supported can be found here.

- [Tablet support project](https://github.com/InfinityGhost/OpenTabletDriver/projects/4)