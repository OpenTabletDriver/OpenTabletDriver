[![Actions Status](https://github.com/OpenTabletDriver/OpenTabletDriver/workflows/.NET%20CI/badge.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/actions) [![Total Download Count](https://img.shields.io/github/downloads/OpenTabletDriver/OpenTabletDriver/total.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest)

# OpenTabletDriver

[English](../README.md) | [Español](README_ES.md) | [Français](README_FR.md) | [Deutsch](README_DE.md) | [Português-BR](README_PTBR.md) | [Nederlands](README_NL.md) | [한국어](README_KO.md) | [Русский](README_RU.md) | [简体中文](README_CN.md) | [繁體中文](README_TW.md) | [Ελληνικά](README_EL.md) | Magyar

Az OpenTabletDriver egy nyílt forráskódú, cross-platform, felhasználóbarát tablet driver. A program célja, hogy a lehető legtöbb ember elérhesse a lehető legoptimalizáltabb kompatibilitásával, mindezt egy egyszerűen konfigurálható GUI-val.

<p align="middle">
  <img src="https://i.imgur.com/XDYf62e.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/jBW8NpU.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/ZLCy6wz.png" width="410" align="middle"/>
</p>

# Támogatott tabletek

Minden támogatott, nem tesztelt, és támogatásra váró tabletet megtalálhatsz itt. Gyakori problémák megoldását a Wiki-n találhatod meg a te platformodra.

- [Támogatott Tabletek](https://opentabletdriver.net/Tablets)

# Installation

- [Windows](https://opentabletdriver.net/Wiki/Install/Windows)
- [Linux](https://opentabletdriver.net/Wiki/Install/Linux)
- [MacOS](https://opentabletdriver.net/Wiki/Install/MacOS)

# Az OpenTabletDriver futtatása

Az OpenTabletDriver két, külön folyamatként fut, amelyek a háttérben együttműködnek. Az aktív program - ami a tablet adatait kezeli - az `OpenTabletDriver.Daemon`, míg a program megjelenítését az `OpenTabletDriver.UX.*` program végzi, ahol a `*` a te platformodat jelzi<sup>1</sup>. A daemont mindig el kell indítani, hogy működjön a program, de a GUI hanyagolható. Ha már vannak beállításaid, a daemon azokkal fog elindulni.

> <sup>1</sup>Windows uses `Wpf`, Linux uses `Gtk`, and MacOS uses `MacOS` respectively. This for the most part can be ignored if you don't build it from source as only the correct version will be provided.
> <sup>1</sup>A Windowst a `Wpf` utótag jelzi, a Linuxot a `Gtk`, a MacOS-t pedig a `MacOS`. Ez nem olyan fontos, ha nem a forráskódból építed az appot.

## Az OpenTabletDriver építése forráskódból

A követelmények az OpenTabletDriver építéséhez minden platformon ugyan azok. Az OpenTabletDriver futtatásának követelményei viszont változhatnak platformról platformra.

### Mindegyik platform

- .NET 6 SDK (töltsd le [innen](https://dotnet.microsoft.com/download/dotnet/6.0) - A te platformodra készült SDK-t töltsd le, Linux felhasználók a package managerrel töltsék le)

#### Windows

Futtasd le a `build.ps1` fájlt, hogy bináris build-eket készíts a 'bin' maooába. Ezek a build-ek portable módban indulnak el alapértelmezetten.

#### Linux

Kötelező csomagok (néhány csomag már lehet, hogy le van töltve - ez disztrótól függ):

- libx11
- libxrandr
- libevdev2
- GTK+3

Futtasd a `./eng/linux/package.sh` fájlt. Ha a "package" build-et szeretnéd,
a következő csomagformátumok hivatalosan támogatva vannak:

| Csomagformátum | Parancs |
| --- | --- |
| Generic binary tarball (`.tar.gz`) | `./eng/linux/package.sh --package BinaryTarBall` |
| Debian package (`.deb`) | `./eng/linux/package.sh --package Debian` |
| Red Hat package (`.rpm`) | `./eng/linux/package.sh --package RedHat` |

Az általános bináris tarball-t a gyökérmappából kell kicsomagolni.

#### MacOS [Kisérleti]

Futtasd a `./eng/macos/package.sh --package true` fájlt (az argumentummal együtt).

# Funkciók

- Teljesen platform-natív GUI
  - Windows: `Windows Presentation Foundation`
  - Linux: `GTK+3`
  - MacOS: `MonoMac`
- Teljesen kidolgozott konzolos eszköz
  - Gyorsan szerezz be, változtass meg, importálj, vagy ments beállításokat
  - Scriptelési lehetőség (json-nel)
- Abszolút kurzorelhelyezés
  - Kijelző- és tabletterületre
  - Középponthoz igazított irányzás
  - Precíz területforgatás
- Relatív kurzorelhelyezés
  - px/mm függőleges és vízszintes érzékenység
- Toll beállítások
  - Hegy nyomás alapján beállítás
  - Expresszgomb beállítás
  - Toll gomb beállítás
  - Egérgomb beállítás
  - Billentyűzet beállítás
  - Külső plugin beállítás
- Beállítások betöltése és mentése
  - Automatikusan betölti a felhasználói beállításokat a `settings.json` segítségével, vagy az aktív felhasználó `%localappdata%` mappájába, vagy a gyökérmappa `.config` mappájába
- Konfiguráció szerkesztő
  - Készíts, szerkessz, törölj konfigurációkat egyszerűen
  - Generálj konfigurációkat a látható HID eszközökből
- Pluginok
  - Filterek
  - Kimeneti módok
  - Eszközök

# Járulj hozzá az OpenTabletDriver fejlesztéséhez

Ha szeretnél hozzájárulni az OpenTabletDriver-höz, nézz rá a [hibakövetőre](https://github.com/OpenTabletDriver/OpenTabletDriver/issues).
Ha pull request-et készítesz, kövesd a [hozzájárulási útmutatóban](https://github.com/OpenTabletDriver/OpenTabletDriver/blob/master/CONTRIBUTING.md)
meghatározott szabályokat.

Ha problémáid adódtak, vagy ötleteid vannak, [nyiss
egy hibajegyet](https://github.com/OpenTabletDriver/OpenTabletDriver/issues/new/choose)
és töltsd ki a sablont a szükséges információkkal. Tárt karokkal várjuk a
hibabejelentéseket, és az új tableteket is, amikhez támogatást szeretnétek.
Általában nagyon egyszerűen lehet új tableteket támogatni.

A hibajegyeket és az OpenTabletDriver [weboldalához](https://opentabletdriver.net) kapcsolódó PR-eket [ebben](https://github.com/OpenTabletDriver/opentabletdriver.github.io) a repository-ban találhatod meg.

### Új tablet támogatása

Ha szeretnéd, hogy új tableteket támogassunk, nyiss hibajegyet, vagy
csatlakozz a [discordunkra](https://discord.gg/9bcMaPkVAR). *Általában
a discordos kéréseket jobban szeretjük, az egyszerűség kedvéért.*

Majd megkérünk kis dolgokra, pl. arra, hogy vedd fel a tableted által küldött
adatokat a beépített tablet-debugoló eszközünkkel, vagy hogy teszteld a tablet
funkcióit (tabletes gombok, toll gombok, toll nyomás, stb.) különböző
konfigurációkkal, amiket mi majd elküldünk.

Nyilván szívesen látjuk az általad nyitott PR-eket is, ha konyítasz az
ilyesfajta témákhoz.

Általában ez a folyamat egyszerű, főleg, ha a tablet készítőjétől már van
másik tabletre támogatás.
