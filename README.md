[![Actions Status](https://github.com/OpenTabletDriver/OpenTabletDriver/workflows/.NET%20CI/badge.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/actions) [![CodeFactor](https://www.codefactor.io/repository/github/OpenTabletDriver/OpenTabletDriver/badge/master)](https://www.codefactor.io/repository/github/OpenTabletDriver/OpenTabletDriver/overview/master) [![Total Download Count](https://img.shields.io/github/downloads/OpenTabletDriver/OpenTabletDriver/total.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest)

# OpenTabletDriver

English | [한국어](docs/README_KO.md) | [Español](docs/README_ES.md) | [Русский](docs/README_RU.md) | [简体中文](docs/README_CN.md) | [Français](docs/README_FR.md) | [Deutsch](docs/README_DE.md) | [Português-BR](docs/README_PTBR.md)

OpenTabletDriver is an open source, cross platform, user mode tablet driver. The goal of OpenTabletDriver is to be cross platform as possible with the highest compatibility in an easily configurable graphical user interface.

<p align="middle">
  <img src="https://i.imgur.com/XDYf62e.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/jBW8NpU.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/ZLCy6wz.png" width="410" align="middle"/>
</p>

# Supported Tablets

All statuses of tablets that are supported, untested, and planned to be supported can be found here. Common issue workarounds can be found in the wiki for your platform.

- [Supported Tablets](https://opentabletdriver.net/Tablets)

# Installation

- [Windows](https://opentabletdriver.net/Wiki/Install/Windows)
- [Linux](https://opentabletdriver.net/Wiki/Install/Linux)
- [MacOS](https://opentabletdriver.net/Wiki/Install/MacOS)

# Running OpenTabletDriver binaries

OpenTabletDriver functions as two separate processes that interact with each other seamlessly. The active program that does all of the tablet data handling is `OpenTabletDriver.Daemon`, while the GUI frontend is `OpenTabletDriver.UX.*`, where `*` depends on your platform<sup>1</sup>. The daemon must be started in order for anything to work, however the GUI is unnecessary. If you have existing settings, they should apply when the daemon starts.

> <sup>1</sup>Windows uses `Wpf`, Linux uses `Gtk`, and MacOS uses `MacOS` respectively. This for the most part can be ignored if you don't build it from source as only the correct version will be provided.

## Building OpenTabletDriver from source

The requirements to build OpenTabletDriver are consistent across all platforms. Running OpenTabletDriver on each platform requires different dependencies.

### All platforms

- .NET 6 SDK (can be obtained from [here](https://dotnet.microsoft.com/download/dotnet/6.0) - You want the SDK for your platform, Linux users should install via package manager where possible)

#### Windows

Run `build.ps1` to produce binary builds to 'bin' folder. These builds will run in portable mode by default.

#### Linux

Required packages (some packages may be pre-installed for your distribution):

- libx11
- libxrandr
- libevdev2
- GTK+3

Run `./eng/linux/package.sh`. If a "package" build is desired,
there are official support for the following packaging formats:

| Package Format | Command |
| --- | --- |
| Generic binary tarball (`.tar.gz`) | `./eng/linux/package.sh --package BinaryTarBall` |
| Debian package (`.deb`) | `./eng/linux/package.sh --package Debian` |
| Red Hat package (`.rpm`) | `./eng/linux/package.sh --package RedHat` |

The generic binary tarball is designed to be extracted from the root directory.

#### MacOS [Experimental]

Run `./eng/macos/package.sh --package true`.

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

If you wish to contribute to OpenTabletDriver, check out the [issue
tracker](https://github.com/OpenTabletDriver/OpenTabletDriver/issues). When
creating pull requests, follow the guidelines outlined in our [contribution
guidelines](https://github.com/OpenTabletDriver/OpenTabletDriver/blob/master/CONTRIBUTING.md).

If you have any issues or suggestions, [open an issue
ticket](https://github.com/OpenTabletDriver/OpenTabletDriver/issues/new/choose)
and fill out the template with relevant information. We welcome both bug
reports, as well as new tablets to add support for. In many cases adding support
for a new tablet is quite easy.

For issues and PRs related to OpenTabletDriver's packaging, please see the repository [here](https://github.com/OpenTabletDriver/OpenTabletDriver.Packaging).

For issues and PRs related to OpenTabletDriver's [web page](https://opentabletdriver.net), see the repository [here](https://github.com/OpenTabletDriver/OpenTabletDriver.Web).

### Supporting a new tablet

If you'd like us to add support for a new tablet, open an issue or join our
[discord](https://discord.gg/9bcMaPkVAR) asking for support. *We generally
prefer that adding support for a tablet be done through discord, due to the
back-and-forth involved*.

We'll have you do a few things like making a recording of the data sent by your
tablet using our built-in tablet debugging tool, testing features of the tablet
(on-tablet buttons, pen buttons, pen pressure, etc) with different configs we'll
send you to try.

You're also of course welcome to open a PR adding support for it yourself, if
you have a good grasp on what's involved.

Generally this process is relatively easy, especially if it's for a tablet
manufacturer we already have support for on other tablets.
