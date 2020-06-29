[![Actions Status](https://github.com/InfinityGhost/OpenTabletDriver/workflows/.NET%20Core/badge.svg)](https://github.com/InfinityGhost/OpenTabletDriver/actions) [![CodeFactor](https://www.codefactor.io/repository/github/infinityghost/opentabletdriver/badge/master)](https://www.codefactor.io/repository/github/infinityghost/opentabletdriver/overview/master)

# OpenTabletDriver

OpenTabletDriver is an open source tablet configurator. The goal of OpenTabletDriver is to be cross platform as possible with the highest compatibility in an easily configurable graphical user interface.

## Releases

You can grab the latest release below. Make sure to download the right version for your platform.

- [Latest Release](https://github.com/InfinityGhost/OpenTabletDriver/releases)
- [Arch User Repository (opentabletdriver-git)](https://aur.archlinux.org/packages/opentabletdriver-git)

## Running OpenTabletDriver

OpenTabletDriver functions as two separate processes that interact with each other seamlessly. The active program that does all of the tablet data handling is `OpenTabletDriver.Daemon`, while the GUI frontend is `OpenTabletDriver.UX.*`, where `*` depends on your platform<sup>1</sup>. The daemon must be started in order for anything to work, however the GUI is unnecessary. If you have existing settings, they should apply when the daemon starts.

<sup>1</sup>Windows uses `Wpf`, Linux uses `Gtk`, and MacOS uses `MacOS` respectively. This for the most part can be ignored if you don't build it from source as only the correct version will be provided.

## Building OpenTabletDriver

The requirements to build OpenTabletDriver are consistent across all platforms. Running OpenTabletDriver on each platform requires different dependencies.

### All platforms
- .NET Core 3.1 SDK

#### Windows

No special dependencies.

#### Linux

- libx11
- libxrandr
- libevdev2
- GTK+3

#### MacOS [Experimental]

No special dependencies.

# Features

- Fully platform-native GUI
  - Windows uses WPF
  - Linux uses GTK+3
  - MacOS uses its native GUI
- Fully fledged console tool
  - Quickly acquire, change, load, or save settings
  - Scripting support (json output)
- Absolute cursor positioning
  - Screen area and tablet area
  - Center-anchored offsets
  - Precise area rotation
- Relative cursor positioning
  - mm/px horizontal and vertical sensitivity
- Pen bindings
  - Tip by pressure bindings
  - Express key bindings
  - Pen button bindings
  - Mouse button bindings
  - Keyboard bindings
  - External plugin bindings
- Saving and loading settings
  - Auto-loads user settings via `settings.json` in the active user `%localappdata%` or `.config` settings root directory.
- Configuration Editor
  - Allows you to create, modify, and delete configurations.
  - Generate configurations from visible HID devices
- Plugins
  - Filters
  - Output modes
  - Run-and-stay-resident tools

# Improving OpenTabletDriver

If you wish to help improve OpenTabletDriver, please [check out the pinned issues](https://github.com/InfinityGhost/OpenTabletDriver/issues).

# Tablet Support

All statuses of tablets that are supported, untested, and planned to be supported can be found here. Common issue workarounds can be found in the wiki for your platform.

- [Tablet support project](https://github.com/InfinityGhost/OpenTabletDriver/projects/4)