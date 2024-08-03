[![Actions Status](https://github.com/OpenTabletDriver/OpenTabletDriver/workflows/.NET%20CI/badge.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/actions) [![Total Download Count](https://img.shields.io/github/downloads/OpenTabletDriver/OpenTabletDriver/total.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest)

# OpenTabletDriver

[English](../README.md) | [Español](README_ES.md) | [Français](README_FR.md) | [Deutsch](README_DE.md) | [Português-BR](README_PTBR.md) | [Nederlands](README_NL.md) | [한국어](README_KO.md) | [Русский](README_RU.md) | [简体中文](README_CN.md) | 繁體中文 | [Ελληνικά](README_EL.md) | [Magyar](README_HU.md)

OpenTabletDriver是一個開源、跨平台的電繪板驅動程式。其目標是在擁有直覺與可自訂的介面的同時也能跨平台並與盡可能支援多數的設備。

<p align="middle">
   <img src="https://i.imgur.com/XDYf62e.png" width="410" align="middle"/>
   <img src="https://i.imgur.com/jBW8NpU.png" width="410" align="middle"/>
   <img src="https://i.imgur.com/ZLCy6wz.png" width="410" align="middle"/>
</p>

# 支援的電繪板

所有已經被支援的、未測試的、以及計劃被支援的電繪板都可以在下方連結找到。如果你的電繪板在你的平台上無法正常運作的話可以在Wiki之中尋找一些解決方法

- [電繪板支援](https://opentabletdriver.net/Tablets)

# 安裝方法

- [Windows](https://opentabletdriver.net/Wiki/Install/Windows)
- [Linux](https://opentabletdriver.net/Wiki/Install/Linux)
- [MacOS](https://opentabletdriver.net/Wiki/Install/MacOS)

# 運行OpenTabletDriver二進位檔案

OpenTabletDriver有兩個獨立的程序共同運作。主程式 `OpenTabletDriver.Daemon` 會處理所有電繪板的數據，而GUI前端則是由 `OpenTabletDriver.UX.*` 負責（這裡的`*`取決於你的平台<sup>1</sup>）。主進程必須啟動，程式才能運作，不過GUI進程則是可選的。如果你已經配置好了設定，他們將會在主進程啟動的時候自動生效。

> <sup>1</sup>Windows是`Wpf`，Linux是`Gtk`，而macOS則是`MacOS`。如果你不需要自己動手編譯的話則可以忽略。

# 從原始碼編譯OpenTabletDriver

在各平台編譯OpenTabletDriver的需求是一樣的，但不同平台執行本軟體需要有不同的依附性元件（dependencies）：

### 所有平台

- .NET 8 SDK（請參閱[這裡](https://dotnet.microsoft.com/download/dotnet/8.0) - 需要與平台相容的SDK，在Linux建議使用package manager安裝）

#### Windows

執行 `build.ps1` 為了在'bin'資料夾中產生二進位版本。預設情況下，這些建置將在便攜式模式下運作。

#### Linux

所需的package（某些package可能在你的distro已預先安裝）：

- libx11
- libxrandr
- libevdev2
- GTK+3

執行 `./eng/linux/package.sh`。 如果需要一個package build，官方支援以下打包格式：

| package格式 | 指令 |
| ---| ---|
| 通用二進位tarball (`.tar.gz`) | `./eng/linux/package.sh --package BinaryTarBall` |
| Debian package (`.deb`) | `./eng/linux/package.sh --package Debian` |
| Red Hat package (`.rpm`) | `./eng/linux/package.sh --package RedHat` |

通用二進位tarball旨在從根目錄中取出。

#### MacOS [試驗性]

請執行 `./eng/macos/package.sh --package true`。

# 功能

- GUI都是各平台的原生開發
  - Windows：`Windows Presentation Foundation`
  - Linux：`GTK+3`
  - macOS：`MonoMac`
- 齊全的命令列工具
  - 快速地獲得、更改、加載、以及保存你的設置
  - 支援腳本（JSON Output）
- 絕對游標定位
  - 相對螢幕區域或平板區域
  - 以中心為錨點的偏移
  - 準確的區域旋轉
- 相對游標定位
  - 以像素或毫米為單位的縱向或橫向靈敏度
- 支援
  - 筆尖壓力
  - 筆的快捷鍵
  - 電繪板上的快捷鍵
  - 滑鼠按鍵
  - 鍵盤按鍵
  - 另外還支援外掛程式
- 儲存以及載入設定
  - 自動載入目前使用者的 `%localappdata%` 或 `.config` 從 `settings.json` 中儲存的設定
- 設定檔編輯器
  - 你可建立、修改、以及刪除設定文件
  - 從已偵測到的HID設備中產生設定檔
- 外掛
  - 過濾器
  - 輸出模式
  - 工具

# 向OpenTabletDriver貢獻

如果你想為OpenTabletDriver做出貢獻，請查看 [issue追蹤器](https://github.com/OpenTabletDriver/OpenTabletDriver/issues)。當建立PR(pull requests)時，請遵循我們的[貢獻指南](https://github.com/OpenTabletDriver/OpenTabletDriver/blob/master/CONTRIBUTING.md)。

如果你有任何問題或建議，[開啟問題票](https://github.com/OpenTabletDriver/OpenTabletDriver/issues/new/choose)並在範本中填寫相關資訊。我們歡迎bugs報告以及新增其他電繪板的支援。在許多情況支援一台新的電繪板是非常容易的。

有關OpenTabletDriver[網頁](https://opentabletdriver.net)相關的問題與PR，請參閱[此處](https://github.com/OpenTabletDriver/opentabletdriver.github.io)的儲存庫。

### 支援一台新的電繪板

如果你希望我們添加對新平板電腦的支持，請提出問題或加入我們的[discord](https://discord.gg/9bcMaPkVAR)尋求支援。*我們比較歡迎透過Discord添加對平板電腦的支持*。

我們會讓你做一些事情，例如記錄你發送的數據平板電腦使用我們內建的平板電腦調試工具，測試平板電腦的功能（平板電腦按鈕、筆按鈕、筆壓力等）具有不同的配置，我們將送你去試試。

當然也歡迎你自己創建PR來添加支持，如果你對所涉及領域很有把握。

一般來說，這個過程相對簡單，特別是我們已經在同一個製造商支援其他的電繪板了。
