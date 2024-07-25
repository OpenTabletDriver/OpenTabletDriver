[![Actions Status](https://github.com/OpenTabletDriver/OpenTabletDriver/workflows/.NET%20CI/badge.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/actions) [![Total Download Count](https://img.shields.io/github/downloads/OpenTabletDriver/OpenTabletDriver/total.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest)

# OpenTabletDriver

[English](../README.md) | [Español](README_ES.md) | [Français](README_FR.md) | [Deutsch](README_DE.md) | [Português-BR](README_PTBR.md) | Nederlands | [한국어](README_KO.md) | [Русский](README_RU.md) | [简体中文](README_CN.md) | [繁體中文](README_TW.md) | [Ελληνικά](README_EL.md) | [Magyar](README_HU.md)

OpenTabletDriver is een open source, gebruikersmodus tablet-stuurprogramma voor meerdere platformen. Het doel van OpenTabletDriver is om zo veel mogelijk platformen te ondersteunen met maximale compatibiliteit in een makkelijk configureerbare grafische gebruikersinterface.

<p align="middle">
  <img src="https://i.imgur.com/XDYf62e.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/jBW8NpU.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/ZLCy6wz.png" width="410" align="middle"/>
</p>

# Ondersteunde Tablets

Alle statussen van tablets die ondersteund, ongetest of waarvan ondersteuning gepland is kunnen hier gevonden worden. Omwegen voor vaak voorkomende problemen kunnen gevonden worden in de wiki van het platform dat u gebruikt.

- [Ondersteunde Tablets](https://opentabletdriver.net/Tablets)

# Installatie

- [Windows](https://opentabletdriver.net/Wiki/Install/Windows)
- [Linux](https://opentabletdriver.net/Wiki/Install/Linux)
- [MacOS](https://opentabletdriver.net/Wiki/Install/MacOS)

# OpenTabletDriver uitvoeren

OpenTabletDriver functioneert als twee aparte processen die onmerkbaar met elkaar communiceren. Het actieve programma dat de data van de tablet verwerkt is `OpenTabletDriver.Daemon`, de GUI is `OpenTabletDriver.UX.*`, waar `*` afhankelijk is van uw platform<sup>1</sup>. De daemon moet aan staan om het programma te laten werken, de GUI is optioneel. Als u bestaande instellingen heeft, zouden ze moeten worden toegepast wanneer de daemon start.

> <sup>1</sup>Windows gebruikt `Wpf`, Linux gebruikt `Gtk` en MacOS gebruikt `MacOS`. Dit kan voor het grootste deel genegeerd worden als u het niet van de broncode bouwt, aangezien de juiste versie zal worden gegeven.

## OpenTabletDriver bouwen van de broncode

De benodigdheden om OpenTabletDriver te bouwen zijn hetzelfde voor alle platformen. OpenTabletDriver gebruiken vereist verschillende afhankelijkheden voor elk platform.

### Alle platformen

- .NET 8 SDK (kan [hier](https://dotnet.microsoft.com/download/dotnet/8.0) verkregen worden - U heeft de SDK voor uw platform nodig, Linux gebruikers kunnen het via hun package manager installeren waar mogelijk)

#### Windows

Start `build.ps1` om een build te maken in het 'bin' mapje. Deze builds zullen standaard in portable modus opereren.

#### Linux

Vereiste packages (sommige zijn mogelijk voorgeïnstalleerd met uw distributie)

- libx11
- libxrandr
- libevdev2
- GTK+3

Start `./eng/linux/package.sh`. Indien u liever een "package" build heeft, zijn de volgende package formaten officieel ondersteund:

| Package Formaat | Opdracht |
| --- | --- |
| Generieke binaire tarball (`.tar.gz`) | `./eng/linux/package.sh --package BinaryTarBall` |
| Debian package (`.deb`) | `./eng/linux/package.sh --package Debian` |
| Red Hat package (`.rpm`) | `./eng/linux/package.sh --package RedHat` |

De generieke binaire tarball is ontworpen om uitgepakt te worden vanuit de rootmap

#### MacOS [Experimenteel]

Voer `./eng/macos/package.sh --package true` uit.

# Functies

- Volledige platform-specifieke GUI
  - Windows: `Windows Presentation Foundation`
  - Linux: `GTK+3`
  - MacOS: `MonoMac`
- Complete console tool
  - Verkrijg, verander, laad en sla instellingen snel op
  - Ondersteuning voor scripten (json uitvoer)
- Absolute cursorpositie
  - Scherm- en tabletgebied
  - Gecentreerde verplaatsingen
  - Precieze gebiedsdraaiingen
- Relatieve cursorpositie
  - px/mm horizontale en verticale gevoeligheid
- Pen instellingen
  - Drukafhankelijke toewijzing van de penpunt
  - ExpressKeys toewijzen
  - Pen knoppen toewijzen
  - Muis knoppen toewijzen
  - Toetsenbordtoetsen toewijzen
  - Toewijzingen voor externe plugins
- Instellingen opslaan en laden
  - OpenTabletDriver laadt automatisch de gebruikersinstellingen via `settings.json` in de `%localappdata%` van de actieve gebruiker of uit de `.config` map van de gebruiker.
- Plugins
  - Filters
  - Uitvoer modi
  - Hulpmiddelen

# Bijdragen aan OpenTabletDriver

Bekijk de [issue tracker](https://github.com/OpenTabletDriver/OpenTabletDriver/issues) als u wilt bijdragen aan OpenTabletDriver. Volg de richtlijnen zoals benoemd in onze [richtlijnen voor bijdragen](https://github.com/OpenTabletDriver/OpenTabletDriver/blob/master/CONTRIBUTING.md) wanneer u een pull request maakt.

[Maak een issue ticket](https://github.com/OpenTabletDriver/OpenTabletDriver/issues/new/choose) als u problemen of suggesties heeft. Vul de template in met relevante informatie. Zowel bug meldingen als tablets om te ondersteunen zijn welkom. Meestal is het ondersteunen van een tablet vrij eenvoudig.

Voor problemen en PRs met betrekking tot de [website](https://opentabletdriver.net) van OpenTabletDriver, bekijk alstublieft [hier](https://github.com/OpenTabletDriver/opentabletdriver.github.io) de repository.

### Een nieuwe tablet ondersteunen

Als u wilt dat wij ondersteuning toevoegen voor een nieuwe tablet, open dan een issue of sluit u aan bij onze [Discord](https://discord.gg/9bcMaPkVAR) om ondersteuning te vragen. *Over het algemeen is Discord handiger voor het ondersteunen van een tablet, om communicatie makkelijker te maken.*

Wij zullen een aantal dingen van u vragen, zoals het opnemen van de data die door uw tablet wordt verstuurd met behulp van het ingebouwde tablet debugging programma en wij zullen u vragen om de functies van de tablet (knoppen op de tablet en pen, druk van de pen, etc.) te testen met verschillende configuraties die wij u sturen om te proberen.

U mag natuurlijk ook zelf een pull request openen om ondersteuning toe te voegen, indien u een goed begrip heeft over de werking ervan.

Over het algemeen is het proces relatief eenvoudig, vooral als het voor een tablet fabrikant is waar al ondersteuning is voor andere tablets.
