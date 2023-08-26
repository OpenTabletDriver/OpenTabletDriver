[![Actions Status](https://github.com/OpenTabletDriver/OpenTabletDriver/workflows/.NET%20CI/badge.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/actions) [![CodeFactor](https://www.codefactor.io/repository/github/OpenTabletDriver/OpenTabletDriver/badge/master)](https://www.codefactor.io/repository/github/OpenTabletDriver/OpenTabletDriver/overview/master) [![Total Download Count](https://img.shields.io/github/downloads/OpenTabletDriver/OpenTabletDriver/total.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest)

# OpenTabletDriver

[English](../README.md) | [한국어](README_KO.md) | [Español](README_ES.md) | [Русский](README_RU.md) | [简体中文](README_CN.md) | [Français](README_FR.md) | [Deutsch](README_DE.md) | [Português-BR](README_PTBR.md) | Nederlands

OpenTabletDriver is een open source, gebruikersmodus tablet-stuurprogramma voor meerdere platformen. Het doel van OpenTabletDriver is om zo veel mogelijk platformen te ondersteunem met de hoogste compatibiliteit in een makkelijk configureerbare grafische gebruikersinterface.

<p align="middle">
  <img src="https://i.imgur.com/XDYf62e.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/jBW8NpU.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/ZLCy6wz.png" width="410" align="middle"/>
</p>

# Ondersteunde Tablets

Alle statussen van tablets die ondersteund, ongetest of waarvan ondersteuning gepland is kunnen hier gevonden worden. Oplossingen voor vaak voorkomende problemen kunnen gevonden worden in de wiki voor uw platform.

- [Ondersteunde Tablets](https://opentabletdriver.net/Tablets)

# Installatie

- [Windows](https://opentabletdriver.net/Wiki/Install/Windows)
- [Linux](https://opentabletdriver.net/Wiki/Install/Linux)
- [MacOS](https://opentabletdriver.net/Wiki/Install/MacOS)

# OpenTabletDriver uitvoeren

OpenTabletDriver functioneert als twee aparte processen die naadloos met elkaar communiceren. Het actieve programma dat de gegevens van de tablet verwerkt is `OpenTabletDriver.Daemon`, de GUI is `OpenTabletDriver.UX.*`, waar `*` afhankelijk is van uw platform<sup>1</sup>. De daemon moet aan staan om het programma te laten werken, de GUI is niet nodig. Als u bestaande instellingen heeft, worden ze als het goed is toegepast wanneer de daemon start.

> <sup>1</sup>Windows gebruikt `Wpf`, Linux gebruikt `Gtk` en MacOS gebruikt respectievelijk `MacOS`. Dit kan voor het grootste deel genegeerd worden als u het niet van de broncode bouwt, aangezien de juiste versie zal worden gegeven.

## OpenTabletDriver bouwen van de broncode

De benodigdheden om OpenTabletDriver te bouwen zijn hetzelfde voor alle platformen. OpenTabletDriver gebruiken vereist verschillende afhankelijkheden voor elk platform.

### Alle platformen

- .NET 6 SDK (kan [hier](https://dotnet.microsoft.com/download/dotnet/6.0) verkregen worden - U heeft de SDK voor uw platform nodig, Linux gebruikers kunnen het via hun package manager installeren indien mogelijk)

#### Windows

Start `build.ps1` om een build te maken in het 'bin' mapje. Deze builds zullen standaard in mobiele stand opereren.

#### Linux

Vereiste packages (sommige zijn mogelijk voorgeïnstalleerd met uw distributie)

- libx11
- libxrandr
- libevdev2
- GTK+3

Start het 'build.sh' bestand om OpenTabletDriver te bouwen voor Linux. Dit zal dezelfde 'dotnet publish' commando's uitvoeren die gebruikt worden om het AUR-package te bouwen en zal de bruikbare binaire bestanden in 'OpenTabletDriver/bin' zetten.

Om een build te maken op ARM Linux, kunt u het meegeleverde 'build.sh' bestand gebruiken met de gepaste runtime als een argument. Voor arm64 is dit 'linux-arm64'.

Let op: als u OpenTabletDriver voor de eerste keer bouwt, gebruik dan eerst het inbegrepen 'generate-rules.sh' script. Dit zal een aantal udev regels in 'OpenTabletDriver/bin' genereren, genaamd '99-opentabletdriver.rules'. Dit bestand moet u vervolgens verplaatsen naar `/etc/udev/rules.d/`:

```
sudo mv ./bin/99-opentabletdriver.rules /etc/udev/rules.d/
```

#### MacOS [Experimenteel]

Geen andere afhankelijkheden.

# Functies

- Volledige platform-specifieke GUI
  - Windows: `Windows Presentation Foundation`
  - Linux: `GTK+3`
  - MacOS: `MonoMac`
- Volledige console tool
  - Verkrijg, verander, laad en sla instellingen snel op
  - Ondersteuning voor scripting (json uitvoer)
- Absolute cursorpositie
  - Scherm- en tabletgebied
  - Gecentreerde verplaatsingen
  - Precieze gebiedsdraaiing
- Relatieve cursorpositie
  - px/mm horizontale en verticale gevoeligheid
- Peninstellingen
  - Drukafhankelijke toewijzing van de penpunt
  - ExpressKeys toewijzen
  - Pen knoppen toewijzen
  - Muis knoppen toewijzen
  - Toetsenbordtoetsen toewijzen
  - Instellingen voor externe plugins
- Instelingen opslaan en laden
  - OpenTabletDriver laadt automatisch de gebruikersinstellingen via `settings.json` in de `%localappdata%` van de actieve gebruiker of uit de `.config` in de hoofdmap van de instellingen.
- Configuratie-editor
  - Laat u configuraties maken, bewerken en verwijderen.
  - Genereer configuraties voor zichtbare HID apparaten
- Plugins
  - Filters
  - Uitvoer modi
  - Hulpmiddelen

# Bijdragen aan OpenTabletDriver

Bekijk de [issue tracker](https://github.com/OpenTabletDriver/OpenTabletDriver/issues) als u wilt bijdragen aan OpenTabletDriver. Volg de richtlijnen zoals benoemd in onze [richtlijnen voor bijdragen](https://github.com/OpenTabletDriver/OpenTabletDriver/blob/master/CONTRIBUTING.md) wanneer u een pull request maakt.

[Maak een issue ticket](https://github.com/OpenTabletDriver/OpenTabletDriver/issues/new/choose) als u problemen of suggesties hebt. Vul de template in met relevante informatie. Zowel bug meldingen als tablets om te ondersteunen zijn welkom. Meestal is het ondersteunen van een tablet vrij eenvoudig.

Voor problemen en PRs met betrekking tot de verpakking van OpenTabletDriver, bekijk alstublieft [hier](https://github.com/OpenTabletDriver/OpenTabletDriver.Packaging) de repository. 

Voor problemen en PRs met betrekking tot de webpagina van OpenTabletDriver, bekijk alstublieft [hier](https://github.com/OpenTabletDriver/OpenTabletDriver.Web) de repository. 

### Een nieuwe tablet ondersteunen

Als u ondersteuning wilt toevoegen voor een nieuwe tablet, open dan een issue of join onze [Discord](https://discord.gg/9bcMaPkVAR), waar u om ondersteuning kunt vragen. *Over het algemeen is Discord handiger voor het ondersteunen van een tablet, om communicatie makkelijker te maken.*

Wij zullen een aantal dingen van u vragen, zoals het opnemen van de data die door uw tablet wordt verstuurd met behulp van het ingebouwde tablet debugging programma en wij zullen u vragen om de functies van de tablet (knoppen op de tablet en pen, druk van de pen, etc.) te testen met verschillende configuraties die wij u sturen om te proberen.

U mag natuurlijk ook zelf een PR openen om ondersteuning toe te voegen, indien u een goed begrip heeft over de werking ervan.

Over het algemeen is het proces relatief eenvoudig, vooral als het voor een tablet fabrikant is waarvoor wij al ondersteuning hebben bij andere tablets.
