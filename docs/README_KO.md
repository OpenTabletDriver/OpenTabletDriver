[![Actions Status](https://github.com/OpenTabletDriver/OpenTabletDriver/workflows/.NET%20CI/badge.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/actions) [![Total Download Count](https://img.shields.io/github/downloads/OpenTabletDriver/OpenTabletDriver/total.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest)

# OpenTabletDriver

[English](../README.md) | [Español](README_ES.md) | [Français](README_FR.md) | [Deutsch](README_DE.md) | [Português-BR](README_PTBR.md) | [Nederlands](README_NL.md) | 한국어 | [Русский](README_RU.md) | [简体中文](README_CN.md) | [繁體中文](README_TW.md) | [Ελληνικά](README_EL.md) | [Magyar](README_HU.md)

OpenTabletDriver는 관리자 권한 없이 여러 플랫폼에서 작동하는 오픈 소스 타블렛 드라이버입니다. OpenTabletDriver는 쉽게 사용 가능한 유저 인터페이스를 제공함과 동시에 가능한 한 여러 플랫폼에서 무리 없이 작동하도록 하는 것을 목표로 하고 있습니다.

<p align="middle">
  <img src="https://i.imgur.com/XDYf62e.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/jBW8NpU.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/ZLCy6wz.png" width="410" align="middle"/>
</p>

# 지원하는 타블렛

OpenTabletDriver에서 지원하거나, 아직 테스트가 부족하거나, 아니면 지원 예정인 타블렛들의 목록은 여기서 확인하실 수 있습니다. 각 플랫폼에 대해 자주 발생하는 문제에 대한 해결책은 위키를 찾아보세요.

- [지원하는 타블렛](https://opentabletdriver.net/Tablets)

# 설치

- [Windows](https://opentabletdriver.net/Wiki/Install/Windows)
- [Linux](https://opentabletdriver.net/Wiki/Install/Linux)
- [MacOS](https://opentabletdriver.net/Wiki/Install/MacOS)

# OpenTabletDriver 실행하기

OpenTabletDriver는 두 개의 서로 다른 프로세스로 이루어져, 그 두 프로세스가 서로 소통하면서 작동합니다. 타블렛 데이터를 다루는 프로그램은 `OpenTabletDriver.Daemon`이며, GUI는 `OpenTabletDriver.UX.*`가 담당하는데, 여기서 `*`는 플랫폼에 따라 다릅니다<sup>1</sup>. 작동 그 자체에 있어서 UX는 불필요하고, 데몬만 있어도 작동합니다. UX 등으로 이미 설정을 마치셨다면, 데몬이 그 설정을 가져와서 적용합니다.

> <sup>1</sup>각각 Windows는 `Wpf`, Linux는 `Gtk`, 그리고 MacOS는 `MacOS`입니다. 이 부분은 소스로 직접 빌드할 때 필요한 부분이지만, 실제 사용에 있어서는 필요한 버전만 제공되므로 중요하지는 않습니다.

## OpenTabletDriver를 소스로부터 빌드하기

OpenTabletDriver를 빌드하기 위해 필요한 것들은 모든 플랫폼에서 동일합니다. 하지만 OpenTabletDriver를 실행할 때에는 각 플랫폼에서 미리 설치해야 할 것들이 존재합니다.

### 모든 플랫폼

- .NET 8 SDK

#### Windows

특별히 없습니다.

#### Linux

- libx11
- libxrandr
- libevdev2
- GTK+3

#### MacOS [실험적]

특별히 없습니다.

# 기능

- 플랫폼 친화적인 GUI
  - Windows: `Windows Presentation Foundation`
  - Linux: `GTK+3`
  - MacOS: `MonoMac`
- 완전한 콘솔 도구
  - 빠르게 설정을 관리
  - 스크립팅 지원 (json 출력)
- 절대 좌표 커서 이동
  - 화면 영역과 타블렛 영역
  - 중앙 기준의 오프셋
  - 정확한 영역 회전
- 상대 좌표 커서 이동
  - px/mm 가로 세로 감도
- 펜 할당
  - 펜압 팁 할당
  - 익스프레스 키 할당
  - 펜 버튼 할당
  - 마우스 버튼 할당
  - 키보드 할당
  - 외부 플러그인 할당
- 설정 저장 및 불러오기
  - 사용자 설정을 `%localappdata%`나 `.config`에 있는 `settings.json`을 통해 자동으로 유지
- 플러그인
  - 필터
  - 출력 모드
  - 도구

# OpenTabletDriver에 기여하기

OpenTabletDriver에 기여하고 싶으시다면, [이슈 트래커](https://github.com/OpenTabletDriver/OpenTabletDriver/issues)를 확인해보세요.

문제가 있으시거나 제안하실 게 있으시면, [이슈 티켓을 열어주세요](https://github.com/OpenTabletDriver/OpenTabletDriver/issues/new/choose).
