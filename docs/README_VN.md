[![GitHub Actions Status](https://github.com/OpenTabletDriver/OpenTabletDriver/actions/workflows/dotnet.yml/badge.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/actions/workflows/dotnet.yml) [![Total Download Count](https://img.shields.io/github/downloads/OpenTabletDriver/OpenTabletDriver/total.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest)

# OpenTabletDriver

[English](../README.md) | [한국어](README_KO.md) | [Español](README_ES.md) | [Русский](README_RU.md) | [简体中文](README_CN.md) | [Français](README_FR.md) | [Deutsch](README_DE.md) | Tiếng Việt

OpenTabletDriver là trình điều khiển máy tính bảng mã nguồn mở, đa nền tảng, chạy ở chế độ người dùng. Mục tiêu của OpenTabletDriver là hỗ trợ càng nhiều nền tảng càng tốt với khả năng tương thích cao nhất thông qua giao diện đồ họa dễ cấu hình.

<p align="middle">
  <img src="https://i.imgur.com/XDYf62e.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/jBW8NpU.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/ZLCy6wz.png" width="410" align="middle"/>
</p>

# Máy tính bảng được hỗ trợ

Bạn có thể xem trạng thái của tất cả máy tính bảng được hỗ trợ tại đây.

- [Máy tính bảng được hỗ trợ](https://opentabletdriver.net/Tablets)

# Cài đặt

- [Windows](https://opentabletdriver.net/Wiki/Install/Windows)
- [Linux](https://opentabletdriver.net/Wiki/Install/Linux)
- [MacOS](https://opentabletdriver.net/Wiki/Install/MacOS)

# Khắc phục sự cố

Vui lòng xem [wiki trên trang web](https://opentabletdriver.net/Wiki) của chúng tôi để biết các vấn đề thường gặp và những lưu ý khác.

# Chạy các tệp nhị phân OpenTabletDriver

OpenTabletDriver hoạt động dưới dạng hai tiến trình riêng biệt tương tác liền mạch với nhau. Chương trình chính xử lý toàn bộ dữ liệu từ máy tính bảng là `OpenTabletDriver.Daemon`, còn giao diện đồ họa là `OpenTabletDriver.UX.*`, trong đó `*` phụ thuộc vào nền tảng của bạn<sup>1</sup>. Daemon phải được khởi động để mọi thứ hoạt động, tuy nhiên giao diện đồ họa là không bắt buộc. Nếu bạn đã có thiết lập sẵn, chúng sẽ được áp dụng khi daemon khởi động.

> <sup>1</sup>Windows sử dụng `Wpf`, Linux sử dụng `Gtk`, và MacOS sử dụng `MacOS`. Phần này hầu như có thể bỏ qua nếu bạn không tự build từ mã nguồn vì chỉ phiên bản phù hợp sẽ được cung cấp.

## Build OpenTabletDriver từ mã nguồn

Yêu cầu để build OpenTabletDriver là nhất quán trên mọi nền tảng. Việc chạy OpenTabletDriver trên từng nền tảng sẽ cần các phụ thuộc khác nhau.

### Mọi nền tảng

- .NET 10 SDK (có thể tải [tại đây](https://dotnet.microsoft.com/download/dotnet/10.0) - Hãy chọn SDK cho nền tảng của bạn; người dùng Linux nên cài qua trình quản lý gói nếu có thể)

#### Windows

Chạy `build.sh windows` để tạo các bản build nhị phân trong thư mục `bin`. Các bản build này mặc định sẽ chạy ở chế độ portable.

Nếu bạn không có WSL hoặc cách khác để dùng BASH với cài đặt dotnet hoạt động, script build Windows đã ngừng khuyến nghị vẫn còn trong `build.ps1`.

#### Linux

Các gói bắt buộc (một số gói có thể đã được cài sẵn trong bản phân phối của bạn):

- libx11
- libxrandr
- libevdev2
- GTK+3

Chạy `./eng/bash/package.sh`. Nếu muốn build dạng "package",
dự án hỗ trợ chính thức các định dạng đóng gói sau:

| Định dạng gói | Lệnh |
| --- | --- |
| Tarball nhị phân chung (`.tar.gz`) | `./eng/bash/package.sh --package BinaryTarBall` |
| [Gói nhị phân đơn giản](./eng/bash/Simple/README-SimplePackage.md) (`.tar.gz`) | `./eng/bash/package.sh --package Simple` |
| Gói Debian (`.deb`) | `./eng/bash/package.sh --package Debian` |
| Gói Red Hat (`.rpm`) | `./eng/bash/package.sh --package RedHat` |
| Gói chung (dành cho người duy trì gói) | `./eng/bash/package.sh --package Generic` |

Tarball nhị phân chung được thiết kế để giải nén từ thư mục gốc.

Gói đơn giản chỉ nên dùng để thử nghiệm tính năng mới trên các bản cài đặt hiện có, vì nó không cài các tệp hệ thống cần thiết.

Bạn cũng có thể chạy `./build.sh linux` để tạo tệp vào `bin/`, nhưng cách này không bao gồm các tệp hệ thống.

#### MacOS

Cần phiên bản Bash và Coreutils mới hơn để build OpenTabletDriver. Bạn có thể cài chúng bằng Homebrew.
Chạy `PATH="$(brew --prefix coreutils)/libexec/gnubin:$PATH" $(brew --prefix)/bin/bash ./eng/bash/package.sh -r osx-x64`.

| Định dạng gói | Lệnh |
| --- | --- |
| Gói x64 chưa ký | `./eng/bash/package.sh --runtime osx-x64 --package macos` |
| Gói x64 đã ký | `./eng/bash/package.sh --signed true --runtime osx-x64 --package macos` |
| Gói arm64 đã ký | `./eng/bash/package.sh --signed true --runtime osx-arm64 --package macos` |

Đóng gói các bản build MacOS đã ký trên Linux hoặc Windows yêu cầu `rcodesign`.

Kể từ [MacOS 11](https://developer.apple.com/documentation/macos-release-notes/macos-big-sur-11_0_1-universal-apps-release-notes/#Code-Signing), bạn **phải** ký các gói arm64.

# Tính năng

- Giao diện đồ họa hoàn toàn gốc theo nền tảng
  - Windows: `Windows Presentation Foundation`
  - Linux: `GTK+3`
  - MacOS: `MonoMac`
- Hỗ trợ nhiều máy tính bảng
  - Xử lý nhiều máy tính bảng từ nhiều mẫu và nhà sản xuất, mỗi thiết bị có pipeline plugin và thiết lập riêng
- Thông số máy tính bảng được xác thực, giúp chuyển đổi máy tính bảng mượt mà nhất
- Công cụ dòng lệnh đầy đủ
  - Nhanh chóng lấy, thay đổi, tải hoặc lưu thiết lập
  - Hỗ trợ scripting (đầu ra json)
- Định vị con trỏ tuyệt đối
  - Vùng màn hình và vùng máy tính bảng
  - Độ lệch neo ở tâm
  - Xoay vùng chính xác
- Định vị con trỏ tương đối
  - Độ nhạy ngang và dọc theo px/mm
- Khả năng tương thích tính năng máy tính bảng nâng cao
  - Xuất áp lực bút
    - Windows: Cần plugin Windows Ink OpenTabletDriver cho chế độ xuất Windows Ink và driver hệ thống VMulti
    - Linux: Được hỗ trợ sẵn với chế độ xuất "Linux Artist Mode"
    - MacOS: Được hỗ trợ sẵn trong mọi chế độ xuất
  - Độ nghiêng bút
  - Vòng xoay/núm xoay trên tablet pad
  - Phím phụ/phím Express
- Gán thao tác cho bút
  - Gán đầu bút theo áp lực
  - Gán phím Express ("Aux")
  - Gán nút bút
  - Gán nút chuột
  - Gán vòng xoay
  - Gán phím bàn phím
  - Gán preset
  - Gán plugin bên ngoài
- Lưu và tải thiết lập
  - Thiết lập được lưu bền vững
  - Preset để truy cập nhanh các thiết lập đã lưu trước đó
- Plugin
  - Plugin Manager (thông qua
    [Plugin-Repository](https://github.com/OpenTabletDriver/Plugin-Repository))
  - Bộ lọc, bao gồm bộ lọc bất đồng bộ (interpolator)
  - Chế độ xuất
- Công cụ gỡ lỗi thiết bị
  - Trình phân tích dữ liệu máy tính bảng ("Tablet Debugger")
  - Đọc chuỗi thiết bị USB
- Thông báo cập nhật phiên bản tự động
  - Có thể tắt bằng cờ dòng lệnh `--skipupdate`
- Chuyển đổi vùng từ driver của nhà sản xuất
  - Hỗ trợ chuyển đổi vùng Wacom / XP-Pen / Huion / Gaomon / VEIKK của bạn
- Daemon độc lập, dành cho hệ thống cấu hình thấp hoặc không có giao diện.

# Đóng góp cho OpenTabletDriver

Nếu bạn muốn đóng góp cho OpenTabletDriver, hãy xem [trình theo dõi issue](https://github.com/OpenTabletDriver/OpenTabletDriver/issues). Khi tạo pull request, hãy làm theo các hướng dẫn trong [quy định đóng góp](../CONTRIBUTING.md).

Nếu bạn gặp vấn đề hoặc có đề xuất, hãy [mở issue](https://github.com/OpenTabletDriver/OpenTabletDriver/issues/new/choose) và điền mẫu với thông tin liên quan. Chúng tôi hoan nghênh cả báo cáo lỗi lẫn các máy tính bảng mới cần thêm hỗ trợ. Trong nhiều trường hợp, việc thêm hỗ trợ cho máy tính bảng mới khá dễ dàng.

Đối với issue và PR liên quan đến [trang web](https://opentabletdriver.net) của OpenTabletDriver, hãy xem repository [tại đây](https://github.com/OpenTabletDriver/opentabletdriver.github.io).

### Hỗ trợ máy tính bảng mới

Nếu bạn muốn chúng tôi thêm hỗ trợ cho một máy tính bảng mới, hãy mở issue hoặc tham gia [discord](https://discord.gg/9bcMaPkVAR) của chúng tôi để yêu cầu hỗ trợ. *Chúng tôi thường ưu tiên việc thêm hỗ trợ cho máy tính bảng được thực hiện qua discord, do cần trao đổi qua lại*.

Chúng tôi sẽ yêu cầu bạn thực hiện một vài việc như ghi lại dữ liệu do máy tính bảng gửi bằng công cụ gỡ lỗi máy tính bảng tích hợp, kiểm thử các tính năng của máy tính bảng (nút trên máy tính bảng, nút bút, áp lực bút, v.v.) với các cấu hình khác nhau mà chúng tôi gửi để bạn thử.

Tất nhiên bạn cũng có thể tự mở PR thêm hỗ trợ nếu bạn hiểu rõ những gì cần làm.

Nhìn chung quy trình này tương đối dễ, đặc biệt nếu đó là máy tính bảng từ nhà sản xuất mà chúng tôi đã hỗ trợ trên các mẫu khác.