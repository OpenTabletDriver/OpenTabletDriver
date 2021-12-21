[![Actions Status](https://github.com/OpenTabletDriver/OpenTabletDriver/workflows/.NET%20Core/badge.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/actions) [![CodeFactor](https://www.codefactor.io/repository/github/OpenTabletDriver/OpenTabletDriver/badge/master)](https://www.codefactor.io/repository/github/OpenTabletDriver/OpenTabletDriver/overview/master) [![Total Download Count](https://img.shields.io/github/downloads/OpenTabletDriver/OpenTabletDriver/total.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest)

# OpenTabletDriver

[English](README.md) | [한국어](README_KO.md) | [Español](README_ES.md) | [Русский](README_RU.md) | [简体中文](README_CN.md) | [Français](README_FR.md) | Deutsch

OpenTabletDriver ist ein open source, platformübergreifender, Benutzermodus Tablet Driver. Das Ziel von OpenTabletDriver ist es so cross platform wie möglich zu sein mit der höchsten Kompatibilität und einer einfach konfigurierbaren grafsichen Benutzeroberfläche.

<p align="middle">
  <img src="https://i.imgur.com/XDYf62e.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/jBW8NpU.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/ZLCy6wz.png" width="410" align="middle"/>
</p>

# Unterstützte Tablets

Der Status aller unterstützten, ungestesteten und zur Ünterstützung geplanten Tablets kann hier eingesehen werden. Häufige Fehler und deren Lösungen sind auf den wikis der jeweiligen Platform zu finden.

- [Unterstützte Tablets](https://opentabletdriver.net/Tablets)

# Installation

- [Windows](https://opentabletdriver.net/Wiki/Install/Windows)
- [Linux](https://opentabletdriver.net/Wiki/Install/Linux)
- [MacOS](https://opentabletdriver.net/Wiki/Install/MacOS)

# OpenTabletDriver ausführen

OpenTabletDriver arbeitet als zwei unabhängige Prozesse, welche nahtlos miteinender interagieren. Der aktive Prozess `OpenTabletDriver.Daemon` geht mit allen Tabetdaten um, während `OpenTabletDriver.UX.*` das die grafische Benutzeroberfläche ist, wobei `*` platformabhängig ist <sup>1</sup>. Der daemon muss laufen, damit OpenTabletDriver funktioniert, die Benutzeroberfläche allerdings nicht. Bestehende Einstellungen werden angewendet, wenn der daemon startet.

> <sup>1</sup> `Wpf` für Windows, `Gtk` für Linux, und `MacOS` für MacOS. Dies kann jedoch bei nicht selbst kompilierten Versionen ignoriert werden, da nur die richtige Datei im Download enthalten ist.

## OpenTabletDriver selbst kompilieren

Die Voraussetzungen für OpenTabletDriver sind auf allen Platformen gleich. Abhängigkeiten sind jedoch unterschiedlich.

### Alle Platformen

- .NET 6 SDK (kann [hier](https://dotnet.microsoft.com/download/dotnet/6.0) heruntergeladen werden - Die SDK für die jeweilige Platform wird benötigt, Linuxnutzer sollten die SDK nach möglichkeit mithilfe eines package managers installieren)

#### Windows

Keine weiteren Abhängigkeiten.

#### Linux

Benötigte packages (manche packages können bei Ihrer Distribution vorinstalliert sein)

- libx11
- libxrandr
- libevdev2
- GTK+3


Um auf Linux zu kompilieren, führen Sie 'build.sh' aus. Es werden die gleichen 
'dotnet publish' Befehle wie für das AUR package benötigt werden ausgeführt 
und ausführbare Dateien werden in 'OpenTabletDriver/bin' erstellt.

Um auf Linux für ARM zu kompilieren, führen Sie 'build.sh' aus und geben 
die richtige Version als Argument an. Z.B. für ARM64 ist das 'linux-arm64'.

Hinweis: Falls Sie OpenTabletDriver zum ersten Mal kompilieren, führen Sie das 
enthaltene generate-rules.sh Skript aus. Damit werden einige udev Regeln in 
OpenTabletDriver/bin generiert (99-opentabletdriver.rules). 
Diese Datei sollte dann in `/etc/udev/rules.d/` verschoben werden:

```
sudo mv ./bin/99-opentabletdriver.rules /etc/udev/rules.d/
```

#### MacOS [Experimentell]

Keine weiteren Abhängigkeiten.

# Funktionen

- Komplett plattformnative Benutzeroberfläche
  - Windows: `Windows Presentation Foundation`
  - Linux: `GTK+3`
  - MacOS: `MonoMac`
- Vollwertiges Konsolenwerkzeug
  - Schnelles ändern, laden und speichern von Einstellungen
  - Skriptunterstützung (json Ausgabe)
- Absolute Mauszeigerpositionierung
  - Bildschirm- und Tabletarbeitsfläche
  - Zentrierte Verschiebungen
  - Präzise Arbeitsflächenrotation
- Relative Mauszeigerpositionierung
  - px/mm für horizontale und vertikale Empfindlichkeit
- Stifteinstellugen
  - Druckabhängige Einstellungen der Stiftspitze
  - Belegung der Zusatztasten
  - Belegung der Stifttasten
  - Belegung mit Maustasten
  - Belegung mit Tastaturtasten
  - Belegung mit pluginabhängigen Einstellungen
- Speichern und Laden von Einstellungen
  - Autoladen von Benutzereinstellungen aus `settings.json` in den momentan aktiven Benutzer `%localappdata%` oder `.config` für das Root-Verzeichnis.
- Konfigurationseditor
  - Erlaubt es, Konfigurationsdateien zu erstellen, ändern und löschen.
  - Gerneriert Konfigurationen für sichtbare HID geräte.
- Plugins
  - Filter
  - Ausgabemodi
  - Werkzeuge

# zu OpenTabletDriver beitragen

Wenn Sie zu OpenTabletDriver beitragen wollen, besuchen Sie den [issue
tracker](https://github.com/OpenTabletDriver/OpenTabletDriver/issues). Wenn Sie 
ein pull request erstellen wollen, folgen Sie den Richtlinien unter [Beitragsrichtlinien](https://github.com/OpenTabletDriver/OpenTabletDriver/blob/master/CONTRIBUTING.md).

Wenn Sie Probleme oder Vorschläge haben, [Erstellen Sie ein issue
ticket](https://github.com/OpenTabletDriver/OpenTabletDriver/issues/new/choose)
und füllen Sie die Vorlage mit relevanten Informationen aus. Bug reports sowie 
neue Tablets zum unterstützen sind wilkommen. In den meisten 
Fällen ist er relativ einfach, neue Tablets zu unterstützen.

### neue Tablets unterstützen

Wenn sie wollen, dass wir ein neues Tablet unterstützen, erstellen Sie eine neue issue auf GitHub 
oder treten Sie unserem [Discord Server](https://discord.gg/9bcMaPkVAR) bei und fragen Sie nach Hilfe. 
*Wir bevorzugen es generell, Unterstützng neuer Tablets wegen der einfacheren Kommunikation auf Discord durchzuführen*.

Sie werden einige Dinge tun müssen, wie das Aufnehmen von gesendeten Tabetdaten mithilfe 
des eingebauten Tablet-Debuggers, testen von Funktionen des Tablets 
(Zusatztasten, Stifttasten, Stiftdruck, etc) mit verschiedenen Konfigurationsdateien, 
die wir Ihnen senden.

Sie können auch gerne ein pull request erstellen und Überstützung selbst hinzufügen,
wenn Sie ein gutes Verständnis der benötigten Grundlagen haben.

Normalerweise ist dieser Prozess ziemlich einfach, besonders wenn es 
sich um ein Tablet eines bereits unterstützten Herstellers handelt.
