[![Actions Status](https://github.com/OpenTabletDriver/OpenTabletDriver/workflows/.NET%20Core/badge.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/actions) [![CodeFactor](https://www.codefactor.io/repository/github/OpenTabletDriver/OpenTabletDriver/badge/master)](https://www.codefactor.io/repository/github/OpenTabletDriver/OpenTabletDriver/overview/master) [![Total Download Count](https://img.shields.io/github/downloads/OpenTabletDriver/OpenTabletDriver/total.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest)

# OpenTabletDriver

[English](README.md) | 简体中文

OpenTabletDriver是一个开源的，跨平台的数位板驱动。OpenTabletDriver的目标是在拥有直观的可自定义的界面的同时做到跨平台兼容经可能多的设备。

<p align="middle">
  <img src="https://i.imgur.com/XDYf62e.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/jBW8NpU.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/ZLCy6wz.png" width="410" align="middle"/>
</p>

# 支持的数位板

所有已经被支持的、未测试的、以及计划被支持的数位板都可以在这里被找到。您可以在Wiki找到一些您的平台的解决方法。

- [Supported Tablets](https://github.com/OpenTabletDriver/OpenTabletDriver/blob/master/TABLETS.md)

# 安装方法

- [安装指导](https://github.com/OpenTabletDriver/OpenTabletDriver/wiki/Installation-Guide)

# 运行OpenTabletDriver已编译的二进制程序

OpenTabletDriver有两个独立的进程共同工作。主程序`OpenTabletDriver.Daemon`会处理所有的数位板的数据，而GUI前端则是由`OpenTabletDriver.UX.*`负责（这里的`*`取决于您的平台<sup>1</sup>）。主进程必须被启动程序才能工作，可是GUI进程则是可选的。如果您已经配置好了，他们则会在主进程启动的时候自动生效。

> <sup>1</sup>Windows是`Wpf`，Linux是`Gtk`，而macOS则是`MacOS`。如果您不需要自己动手编译的话则可以忽略。

# 自己编译OpenTabletDriver

OpenTabletDriver的要求在各个平台是一样的。而本软件在不同平台则是会有不同的依赖：

### 全平台

- .NET 5 SDK

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

- 在各个平台都是完全原生的GUI
  - Windows：`Windows Presentation Foundation`
  - Linux: `GTK+3`
  - macOS: `MonoMac`
- 齐全的控制台工具
  - 快速地获得、更改、加载、以及保存您的设置
  - 支持脚本（JSON Output）
- 绝对准星定位
  - 相对屏幕区域或者平板区域
  - 以中心为锚点的偏移
  - 准确的区域旋转
- 相对准星定位
  - 以像素或者毫米为单位的纵向或者横向灵敏度
- 笔上的按键自定义
  - 压力自定义
  - 笔上的按钮自定义
  - 鼠标按键自定义
  - 键盘按键自定义
  - 外接设备的自定义
- 保存以及加载设置
  - 通过`%localappdata%`或者`.config`里的`settings.json`自动加载用户的设置
- 配置文件编辑器
  - 让您创建、修改、以及删除配置文件
  - 从可见的HID设备中生成配置文件
- 插件
  - Filters
  - Output Modes
  - Tools

# 向OpenTabletDriver贡献

如果您希望对本项目进行贡献，可以查看[issue tracker](https://github.com/OpenTabletDriver/OpenTabletDriver/issues)。

如果你有任何的问题和建议，欢迎在[Issue中提出](https://github.com/OpenTabletDriver/OpenTabletDriver/issues/new/choose)。