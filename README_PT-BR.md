[![Actions Status](https://github.com/OpenTabletDriver/OpenTabletDriver/workflows/.NET%20Core/badge.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/actions) [![CodeFactor](https://www.codefactor.io/repository/github/OpenTabletDriver/OpenTabletDriver/badge/master)](https://www.codefactor.io/repository/github/OpenTabletDriver/OpenTabletDriver/overview/master) [![Total Download Count](https://img.shields.io/github/downloads/OpenTabletDriver/OpenTabletDriver/total.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest)

# OpenTabletDriver

[English](README.md)| [한국어](README_KO.md) | [Español](README_ES.md) | [Русский](README_RU.md) | [简体中文](README_CN.md) | Português 

O OpenTabletDriver é um driver em modo de usuário, código-aberto e multi plataforma. O objetivo de OpenTabletDriver é ser multi-plataforma com a maior compatibilidade em uma interface gráfica configurável fácil de usar. 
<p align="middle">
  <img src="https://i.imgur.com/XDYf62e.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/jBW8NpU.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/ZLCy6wz.png" width="410" align="middle"/>
</p>

# Mesas [Tablets] Suportadas

Todos os estados das mesas serão suportado, não testado, e planejado á ter suporte podem ser encontrados aqui. Soluções para problemas comuns podem ser encontrados na wiki para sua plataforma. 
- [Mesas Suportadas](https://opentabletdriver.net/Tablets)

# Instalação

- [Windows](https://opentabletdriver.net/Wiki/Install/Windows)
- [Linux](https://opentabletdriver.net/Wiki/Install/Linux)
- [MacOS](https://opentabletdriver.net/Wiki/Install/MacOS)

# Executando os binários do OpenTabletDriver

O OpenTabletDriver funciona como dois processos separados que se interagem entre si perfeitamente. O programa ativo que cuida de toda a informação da mesa é o `OpenTabletDriver.Daemon`, enquanto a interface é `OpenTabletDriver.UX.*`, onde `*` depende de sua plataforma<sup>1</sup>. O Daemon deve ser iniciado para alguma coisa funcionar, mas a interface é desnecessária. Se você tem configurações existentes, elas devem aplicar quando o daemon começa.

> <sup>1</sup>Windows usa o `Wpf`, Linux usa o `Gtk`, e MacOS usa  `MacOS` respectivamente. Isso por maior parte pode ser ignorada se você não o compilar do código fonte, como a versão correta será fornecida.

## Compilando o OpenTabletDriver do código fonte

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

If you wish to contribute to OpenTabletDriver, check out the [issue tracker](https://github.com/OpenTabletDriver/OpenTabletDriver/issues).

If you have any issues or suggestions, [open an issue ticket](https://github.com/OpenTabletDriver/OpenTabletDriver/issues/new/choose).
