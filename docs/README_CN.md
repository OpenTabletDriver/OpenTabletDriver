[![Actions Status](https://github.com/OpenTabletDriver/OpenTabletDriver/workflows/.NET%20CI/badge.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/actions) [![总下载统计](https://img.shields.io/github/downloads/OpenTabletDriver/OpenTabletDriver/total.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest)

# OpenTabletDriver

[English](../README.md) | [Español](README_ES.md) | [Français](README_FR.md) | [Deutsch](README_DE.md) | [Português-BR](README_PTBR.md) | [Nederlands](README_NL.md) | [한국어](README_KO.md) | [Русский](README_RU.md) | 简体中文 | [繁體中文](README_TW.md) | [Ελληνικά](README_EL.md) | [Magyar](README_HU.md)

OpenTabletDriver是一个开源的，跨平台的数位板驱动。其目标是在拥有直观的可自定义的界面的同时能跨平台并兼容尽可能多的设备。

<p align="middle">
  <img src="https://i.imgur.com/XDYf62e.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/jBW8NpU.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/ZLCy6wz.png" width="410" align="middle"/>
</p>

# 支持的数位板

所有已经被支持的、未测试的、以及计划被支持的数位板都可以在这里被找到。如果您的数位板在您的平台上无法正常工作的话可以在Wiki之中寻找一些解决方法

- [数位板支持](https://opentabletdriver.net/Tablets)

# 安装方法

- [Windows](https://opentabletdriver.net/Wiki/Install/Windows)
- [Linux](https://opentabletdriver.net/Wiki/Install/Linux)
- [MacOS](https://opentabletdriver.net/Wiki/Install/MacOS)

# 运行OpenTabletDriver

OpenTabletDriver有两个独立的进程共同工作。主程序`OpenTabletDriver.Daemon`会处理所有的数位板的数据，而GUI前端则是由`OpenTabletDriver.UX.*`负责（这里的`*`取决于您的平台<sup>1</sup>）。主进程必须被启动，程序才能工作，不过GUI进程则是可选的。如果您已经配置好了设置，他们将会在主进程启动的时候自动生效。

> <sup>1</sup>Windows是`Wpf`，Linux是`Gtk`，而macOS则是`MacOS`。如果您不需要自己动手编译的话则可以忽略。

# 从源码编译OpenTabletDriver

编译OpenTabletDriver的要求在各个平台是一样的，在不同的平台运行本软件需要有不同的依赖：

### 所有平台

- .NET 8 SDK

#### Windows

没有其他的依赖

#### Linux

- libx11
- libxrandr
- libevdev2
- GTK+3

#### MacOS [试验性]

没有其他的依赖

# 功能

- 各个平台的GUI都是原生开发
  - Windows：`Windows Presentation Foundation`
  - Linux: `GTK+3`
  - macOS: `MonoMac`
- 完善的命令行工具
  - 快速地获得、更改、加载、以及保存您的设置
  - 支持脚本（JSON Output）
- 绝对准星定位
  - 相对屏幕区域或者平板区域
  - 以中心为锚点的偏移
  - 准确的区域旋转
- 相对准星定位
  - 以像素或者毫米为单位的纵向或者横向灵敏度
- 支持将笔尖压力、笔的快捷按钮和数位板上的快捷键映射为鼠标按键、键盘按键或插件提供的操作
- 保存以及加载设置
  - 自动加载当前用户的 `%localappdata%` 或者 `.config` 中 `settings.json` 保存的设置
- 插件
  - 过滤器
  - 输出模式
  - 工具

# 向OpenTabletDriver贡献

如果您希望对本项目进行贡献，可以查看[issue tracker](https://github.com/OpenTabletDriver/OpenTabletDriver/issues)。

如果你有任何的问题和建议，欢迎在[Issue](https://github.com/OpenTabletDriver/OpenTabletDriver/issues/new/choose)中提出。
