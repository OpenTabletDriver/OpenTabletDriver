[![GitHub Actions Status](https://github.com/OpenTabletDriver/OpenTabletDriver/actions/workflows/dotnet.yml/badge.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/actions/workflows/dotnet.yml) [![Total Download Count](https://img.shields.io/github/downloads/OpenTabletDriver/OpenTabletDriver/total.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest)

# OpenTabletDriver

[English](../README.md) | [한국어](README_KO.md) | [Español](README_ES.md) | [Русский](README_RU.md) | 简体中文 | [Français](README_FR.md) | [Deutsch](README_DE.md)

OpenTabletDriver 是一款开源、跨平台、工作在用户模式（用户态）的数位板驱动程序。
其目标是尽可能实现跨平台兼容，并提供一个易于配置的图形用户界面。

<p align="middle">
  <img src="https://i.imgur.com/XDYf62e.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/jBW8NpU.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/ZLCy6wz.png" width="410" align="middle"/>
</p>

# 支持的数位板

所有受支持的数位板都可以在这里看到。

- [数位板支持](https://opentabletdriver.net/Tablets)

# 安装方法

- [Windows](https://opentabletdriver.net/Wiki/Install/Windows)
- [Linux](https://opentabletdriver.net/Wiki/Install/Linux)
- [MacOS](https://opentabletdriver.net/Wiki/Install/MacOS)

# 运行 OpenTabletDriver

OpenTabletDriver 由两个独立进程协同工作：守护进程`OpenTabletDriver.Daemon`负责处理所有数位板数据，而图形前端`OpenTabletDriver.UX.*`（这里的`*`取决于您的平台<sup>1</sup>）则提供用户界面。
守护进程必须启动，驱动程序才能正常工作，但图形界面并非必需。若您已有保存的设置，它们将在守护进程启动时自动加载并生效。

> <sup>1</sup>Windows是`Wpf`，Linux是`Gtk`，而macOS则是`MacOS`。
> 如果您不需要自己动手编译的话则可以忽略。

# 从源码编译 OpenTabletDriver

在各个平台编译 OpenTabletDriver 的依赖都是一样的，但在不同平台运行需要不同的依赖：

### 所有平台

- .NET 8 SDK (点击[这里](https://dotnet.microsoft.com/download/dotnet/8.0)获取 - 需要对应平台的SDK， Linux建议通过包管理器安装)

#### Windows

运行 `build.sh windows` 将生成二进制文件至 'bin' 文件夹。
产生的构建默认将运行在便携版模式上。

如果您没有 WSL 或其它带有 .NET 的 BASH，已弃用的 Windows 构建脚本 `build.ps1` 依然可以。

#### Linux

所需依赖包（某些包可能已预装进您所使用的发行版中）

- libx11
- libxrandr
- libevdev2
- GTK+3

运行 `./eng/bash/package.sh`

如果需要一个“包”构建的话，以下是官方支持的包格式：

| 包格式                                                                 | 命令                                            |
| ---------------------------------------------------------------------- | ----------------------------------------------- |
| 二进制归档文件 （`.tar.gz`）                                           | `./eng/bash/package.sh --package BinaryTarBall` |
| [简易二进制包](../eng/bash/Simple/README-SimplePackage.md) (`.tar.gz`) | `./eng/bash/package.sh --package Simple`        |
| Debian 包 （`.deb`）                                                   | `./eng/bash/package.sh --package Debian`        |
| Red Hat 包 （`.rpm`）                                                  | `./eng/bash/package.sh --package RedHat`        |
| 通用包 （适用于软件包维护人员）                                        | `./eng/bash/package.sh --package Generic`       |

二进制归档文件 被设计为从根目录中提取。
简易二进制包只能用于在已存在安装的基础上测试新功能，它并不会安装必要系统文件。
您也可以直接运行 `./build.sh linux` 将文件生成至 `bin/`，但这并不包含系统文件。

#### MacOS

构建 OpenTabletDriver 需要较新版本的 Bash 和 Coreutils，您可以使用 Homebrew 来安装
运行 `PATH="$(brew --prefix coreutils)/libexec/gnubin:$PATH" $(brew --prefix)/bin/bash ./eng/bash/package.sh -r osx-x64`

# 功能

- 各平台均提供原生图形界面
  - Windows：`Windows Presentation Foundation`
  - Linux：`GTK+3`
  - macOS：`MonoMac`
- 多数位板支持
  - 支持来自众多型号和厂商的多款数位板，每款设备均拥有独立的插件处理流程和配置设置
  - 已验证的数位板规格，确保在更换数位板时实现最顺畅的过渡
- 完善的命令行工具
  - 快速获取、更改、加载和保存设置
  - 支持脚本（JSON格式）
- 绝对定位模式
  - 可映射至屏幕区域或数位板有效区域
  - 以中心为锚点的偏移
  - 准确的区域旋转
- 相对定位模式
  - 可独立设置水平与垂直方向的灵敏度（像素/毫米）
- 数位板高级功能兼容性
  - 压感输出
    - Windows： 需要 Windows Ink OpenTabletDriver 插件来支持 Windows Ink 输出模式，以及 VMulti 系统驱动
    - Linux： 原生支持带有 “Linux Artist Mode” 输出模式
    - MacOS： 原生支持所有输出模式
  - 笔尖倾斜
  - 数位板滚轮 / 旋钮
  - 辅助按键 / 快捷键
- 笔绑定
  - 含压感的笔尖绑定
  - 快捷键（“辅助键”）绑定
  - 笔身按钮绑定
  - 鼠标按键绑定
  - 滚轮绑定
  - 键盘按键绑定
  - 预设绑定
  - 扩展插件绑定
- 保存以及加载设置
  - 持久性设置
  - 预设可快速访问之前保存的设置
- 配置文件编辑器
  - 允许您创建、修改、以及删除配置文件
  - 从可见的 HID 设备中生成配置文件
- 插件
  - 插件管理器 （通过
    [插件仓库](https://github.com/OpenTabletDriver/Plugin-Repository)）
  - 过滤器，包括异步过滤器 （如插值器）
  - 输出模式
- 设备调试工具
  - 数位板数据分析器 （“Tablet Debugger”）
  - USB设备字符串读取
- 自动更新提示
  - 您可以通过添加 `--skipupdate` 命令行标志来禁用
- 供应商驱动区域转换
  - 支持转换您的 Wacom / XP-Pen / Huion / Gaomon / VEIKK 区域
- 为低规格或无头系统提供独立的守护进程

# 向 OpenTabletDriver 贡献

若您希望为 OpenTabletDriver 做出贡献，请查看[议题追踪器](https://github.com/OpenTabletDriver/OpenTabletDriver/issues)。
创建拉取请求（PR）时，请遵循我们的 [贡献指南](../CONTRIBUTING.md)。
如果您有任何**问题或建议**，请[创建新议题](https://github.com/OpenTabletDriver/OpenTabletDriver/issues/new/choose)，填写模板并附上相关信息。
我们欢迎bug报告以及添加对新数位板的支持。
通常，添加对新数位板的支持相当简单。

有关 OpenTabletDriver 包的议题（issue）与拉取请求（PR），请查看[此仓库](https://github.com/OpenTabletDriver/OpenTabletDriver.Packaging)。
有关 OpenTabletDriver [网页](https://opentabletdriver.net)的议题（issue）与拉取请求（PR），请查看[此仓库](https://github.com/OpenTabletDriver/opentabletdriver.github.io)。

### 添加对新数位板的支持

如果您想要添加对新数位板的支持，创建一个议题（issue）或加入我们的
[discord](https://discord.gg/9bcMaPkVAR) 来寻求帮助。（*我们更倾向通过 discord 来添加对新数位板的支持*.）

通常需要您协助完成一些步骤。
例如：使用内置数位板调试工具（Tablet Debugger），来测试数位板的功能（数位板快捷键，笔身按钮，压感笔尖，等等）。
我们会发送给您不同的配置文件来进行尝试。

当然也欢迎您提交拉取请求（PR）来自行适配，如果您对其所涉及的领域很有把握的话。
通常来讲，这一过程相对简单，尤其是如果我们已经适配相同制造商的其它型号数位板时。
