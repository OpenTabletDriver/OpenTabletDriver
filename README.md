[![Actions Status](https://github.com/InfinityGhost/OpenTabletDriver/workflows/.NET%20Core/badge.svg)](https://github.com/InfinityGhost/OpenTabletDriver/actions) [![CodeFactor](https://www.codefactor.io/repository/github/infinityghost/opentabletdriver/badge/master)](https://www.codefactor.io/repository/github/infinityghost/opentabletdriver/overview/master)

# OpenTabletDriver

OpenTabletDriver is an open source, cross platform, user mode tablet driver. The goal of OpenTabletDriver is to be cross platform as possible with the highest compatibility in an easily configurable graphical user interface.

<p align="middle">
  <img src="https://i.imgur.com/hxEVlMa.png" height="350"/>
  <img src="https://i.imgur.com/Pdbd4b7.png" height="350"/>
</p>

# Supported Tablets

All statuses of tablets that are supported, untested, and planned to be supported can be found here. Common issue workarounds can be found in the wiki for your platform.

- [Supported Tablets](https://github.com/InfinityGhost/OpenTabletDriver/projects/4)

# Installation

- [Installation guide](https://github.com/InfinityGhost/OpenTabletDriver/wiki/Installation-Guide)

# Running OpenTabletDriver binaries

OpenTabletDriver functions as two separate processes that interact with each other seamlessly. The active program that does all of the tablet data handling is `OpenTabletDriver.Daemon`, while the GUI frontend is `OpenTabletDriver.UX.*`, where `*` depends on your platform<sup>1</sup>. The daemon must be started in order for anything to work, however the GUI is unnecessary. If you have existing settings, they should apply when the daemon starts.

> <sup>1</sup>Windows uses `Wpf`, Linux uses `Gtk`, and MacOS uses `MacOS` respectively. This for the most part can be ignored if you don't build it from source as only the correct version will be provided.

## Building OpenTabletDriver from source

The requirements to build OpenTabletDriver are consistent across all platforms. Running OpenTabletDriver on each platform requires different dependencies.

### All platforms

- .NET 5 SDK

#### Windows

No other dependencies.

#### Linux

- libx11
- libxrandr
- libevdev2
- GTK+3

#### MacOS [Experimental]

No other dependencies.

# Features

- Fully platform-native GUI
  - Windows: `Windows Presentation Foundation`
  - Linux: `GTK+3`
  - MacOS: `MonoMac`
- Fully fledged console tool
  - Quickly acquire, change, load, or save settings
  - Scripting support (json output)
- Absolute cursor positioning
  - Screen area and tablet area
  - Center-anchored offsets
  - Precise area rotation
- Relative cursor positioning
  - px/mm horizontal and vertical sensitivity
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
  - Tools

# Contributing to OpenTabletDriver

If you wish to contribute to OpenTabletDriver, check out the [issue tracker](https://github.com/InfinityGhost/OpenTabletDriver/issues).

If you have any suggestions, [open an issue ticket](https://github.com/InfinityGhost/OpenTabletDriver/issues/new?assignees=&labels=enhancement&template=feature_request.md&title=).