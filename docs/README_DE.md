[![Actions Status](https://github.com/OpenTabletDriver/OpenTabletDriver/workflows/.NET%20CI/badge.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/actions) [![Total Download Count](https://img.shields.io/github/downloads/OpenTabletDriver/OpenTabletDriver/total.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest)

# OpenTabletDriver

[English](../README.md) | [Español](README_ES.md) | [Français](README_FR.md) | Deutsch | [Português-BR](README_PTBR.md) | [Nederlands](README_NL.md) | [한국어](README_KO.md) | [Русский](README_RU.md) | [简体中文](README_CN.md) | [繁體中文](README_TW.md) | [Ελληνικά](README_EL.md) | [Magyar](README_HU.md)

OpenTabletDriver ist ein plattformübergreifender Open Source Benutzermodustreiber für Grafiktabletts. Das Ziel von OpenTabletDriver ist es, so Cross-Plattform wie möglich zu sein, mit der höchsten Kompatibilität und mit einer einfach zu konfigurierenden grafischen Benutzeroberfläche.

<p align="middle">
  <img src="https://i.imgur.com/XDYf62e.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/jBW8NpU.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/ZLCy6wz.png" width="410" align="middle"/>
</p>

# Unterstützte Tablets

Der Status aller unterstützten, ungetesteten und zur Unterstützung geplanten Tablets kann hier eingesehen werden. Häufige Fehler und deren Lösungen sind auf den Wikis der jeweiligen Plattform zu finden.

- [Unterstützte Tablets](https://opentabletdriver.net/Tablets)

# Installation

- [Windows](https://opentabletdriver.net/Wiki/Install/Windows)
- [Linux](https://opentabletdriver.net/Wiki/Install/Linux)
- [MacOS](https://opentabletdriver.net/Wiki/Install/MacOS)

# OpenTabletDriver ausführen

OpenTabletDriver besteht aus zwei unabhängigen Prozessen, welche nahtlos miteinander interagieren. Der aktive Prozess `OpenTabletDriver.Daemon` geht mit allen Tabletdaten um, während der `OpenTabletDriver.UX.*` Prozess die grafische Benutzeroberfläche darstellt, wobei `*` plattformabhängig ist <sup>1</sup>. Der Daemon muss laufen, damit OpenTabletDriver funktioniert, die Benutzeroberfläche allerdings nicht. Bestehende Einstellungen werden angewendet, wenn der Daemon startet.

> <sup>1</sup> `Wpf` für Windows, `Gtk` für Linux, und `MacOS` für MacOS. Dies kann jedoch bei nicht selbst kompilierten Versionen ignoriert werden, da nur die richtige Datei im Download enthalten ist.

## OpenTabletDriver selbst kompilieren

Die Voraussetzungen für OpenTabletDriver sind auf allen Plattformen gleich, die Abhängigkeiten sind jedoch unterschiedlich.

### Alle Plattformen

- .NET 8 SDK (kann [hier](https://dotnet.microsoft.com/download/dotnet/8.0) heruntergeladen werden - Die SDK für die jeweilige Plattform wird benötigt, Linux-Nutzer sollten die SDK nach Möglichkeit mithilfe eines Package-Managers installieren)

#### Windows

Führen Sie `build.ps1` aus, um die benötigten Dateien im 'bin' Ordner zu erstellen. Diese Builds laufen im Portable-Modus.

#### Linux

Benötigte Packages (manche Packages können bei Ihrer Distribution vorinstalliert sein)

- libx11
- libxrandr
- libevdev2
- GTK+3

Um auf Linux zu kompilieren, führen Sie 'build.sh' aus. Es werden die gleichen
'dotnet publish' Befehle ausgeführt, die auch für das AUR-Package verwendet werden.
Die ausführbaren Dateien werden dann in dem 'OpenTabletDriver/bin' Ordner erstellt.

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
  - Schnelles Ansehen, Ändern, Laden und Speichern von Einstellungen
  - Skriptunterstützung (json-Ausgabe)
- Absolute Mauszeigerpositionierung
  - Bildschirm- und Tabletarbeitsfläche
  - Zentrierte Verschiebungen
  - Präzise Arbeitsflächenrotation
- Relative Mauszeigerpositionierung
  - px/mm für horizontale und vertikale Empfindlichkeit
- Stifteinstellungen
  - Druckabhängige Einstellungen der Stiftspitze
  - Belegung der Zusatztasten
  - Belegung der Stifttasten
  - Belegung mit Maustasten
  - Belegung mit Tastaturtasten
  - Belegung mit pluginabhängigen Einstellungen
- Speichern und Laden von Einstellungen
  - Autoladen von Benutzereinstellungen aus `settings.json` in den momentan aktiven Benutzer `%localappdata%` oder `.config` in das Root-Verzeichnis.
- Plugins
  - Filter
  - Ausgabemodi
  - Werkzeuge

# Zu OpenTabletDriver beitragen

Wenn Sie zu OpenTabletDriver beitragen wollen, besuchen Sie den [Issue
tracker](https://github.com/OpenTabletDriver/OpenTabletDriver/issues). Wenn Sie
eine Pull-Request erstellen wollen, folgen Sie den Richtlinien unter [Beitragsrichtlinien](https://github.com/OpenTabletDriver/OpenTabletDriver/blob/master/CONTRIBUTING.md).

Wenn Sie Probleme oder Vorschläge haben, [Erstellen Sie ein Issue-Ticket](https://github.com/OpenTabletDriver/OpenTabletDriver/issues/new/choose)
und füllen Sie die Vorlage mit relevanten Informationen aus. Bug-Reports sowie
neue Tablets zum Unterstützen sind willkommen. In den meisten
Fällen ist es relativ einfach, neue Tablets zu unterstützen.

### Neue Tablets unterstützen

Wenn Sie wollen, dass wir ein neues Tablet unterstützen, erstellen Sie ein neues Issue-Ticket auf GitHub
oder treten Sie unserem [Discord Server](https://discord.gg/9bcMaPkVAR) bei und fragen Sie nach Hilfe.
*Wir bevorzugen es generell, Unterstützung neuer Tablets wegen der einfacheren Kommunikation auf Discord durchzuführen*.

Sie werden einige Dinge tun müssen, wie das Aufnehmen von gesendeten Tabletdaten mithilfe
des eingebauten Tablet-Debuggers, das Testen von Funktionen des Tablets
(Zusatztasten, Stifttasten, Stiftdruck, etc.) mit verschiedenen Konfigurationsdateien,
die wir Ihnen senden.

Sie können auch gerne eine Pull-Request erstellen und die Sache selbst durchführen,
wenn Sie ein gutes Verständnis der benötigten Grundlagen haben.

Normalerweise ist dieser Prozess ziemlich einfach, besonders wenn es
sich um ein Tablet eines bereits unterstützten Herstellers handelt.
