[![Actions Status](https://github.com/OpenTabletDriver/OpenTabletDriver/workflows/.NET%20CI/badge.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/actions) [![Total Download Count](https://img.shields.io/github/downloads/OpenTabletDriver/OpenTabletDriver/total.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest)

# OpenTabletDriver

[English](../README.md) | [Español](README_ES.md) | Français | [Deutsch](README_DE.md) | [Português-BR](README_PTBR.md) | [Nederlands](README_NL.md) | [한국어](README_KO.md) | [Русский](README_RU.md) | [简体中文](README_CN.md) | [繁體中文](README_TW.md) | [Ελληνικά](README_EL.md) | [Magyar](README_HU.md)

OpenTabletDriver est un driver de tablette en mode utilisateur, open source et multiplateforme. Le but d'OpenTabletDriver est d'être compatible avec le plus de plateforme possibles, et ce grâce à une interface graphique utilisateur facilement configurable.

<p align="middle">
  <img src="https://i.imgur.com/XDYf62e.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/jBW8NpU.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/ZLCy6wz.png" width="410" align="middle"/>
</p>

# Tablettes supportées

Tous les modèles de tablettes supportés, non testées, et prévus pour être supportés peuvent-être trouvés ici. Des solutions alternatives peuvent-être trouvées sur le wiki pour votre plateforme.

- [Tablettes supportées](https://opentabletdriver.net/Tablets)

# Installation

- [Windows](https://opentabletdriver.net/Wiki/Install/Windows)
- [Linux](https://opentabletdriver.net/Wiki/Install/Linux)
- [MacOS](https://opentabletdriver.net/Wiki/Install/MacOS)

# Exécuter OpenTabletDriver

Le fonctionnement d'OpenTabletDriver est basé sur l'utilisation de deux processus séparés qui interagissent parfaitement entre eux. Le programme actif qui permet le traitement des données est `OpenTabletDriver.Daemon`, tandis que l'interface graphique est `OpenTabletDriver.UX.*`, où `*` dépend de votre plateforme<sup>1</sup>. Pour que tout fonctionne correctement, Le programme actif daemon doit être exécuté. Si vous avez des paramètres existants, ils vont s'appliquer lors de son exécution.


> <sup>1</sup>Windows utilise `Wpf`, Linux utilise `Gtk`, et MacOS utilise `MacOS` respectivement. Celà peut-être ignoré dans la plupart des cas si vous ne tentez pas de build à partir de la source, car seule la bonne version sera fournie.
## Build OpenTabletDriver à partir de la source

Les exigences pour build OpenTabletDriver sont cohérentes sur toutes les plateformes. L'exécution d'OpenTabletDriver requière des dépendances différentes.

### Toutes les plateformes

- .NET 8 SDK (peut-être obtenu [Ici](https://dotnet.microsoft.com/download/dotnet/8.0) - Prendre le SDK pour votre plateforme, les utilisateurs Linux doivent installer via un gestionnaire de paquets qui fournit le paquet .NET 7)

#### Windows

Aucune autre dépendance.

#### Linux

Paquets requis (certains paquets peuvent-être pré-installés pour votre distribution)

- libx11
- libxrandr
- libevdev2
- GTK+3

Pour pouvoir build sur Linux, exécutez le 'build.sh' fourni. Ceci va également exécuter
la command 'dotnet publish' utilisée pour build le paquet AUR,
cela va également produire des exécutables utilisables dans 'OpenTabletDriver/bin'.

Pour build sur ARM Linux, exécutez le 'build.sh' fourni
avec les arguments d'exécution appropriés. Pour arm64, c'est
'linux-arm64'.

Note: Si vous buildez pour la première fois,
exécutez le script generate-rules.sh inclu.
Cela va générer plusieurs règles udev
dans OpenTabletDriver/bin appelées '99-opentabletdriver.rules'.
Ce fichier doit-être déplacé dans `/etc/udev/rules.d/`:

```
sudo mv ./bin/99-opentabletdriver.rules /etc/udev/rules.d/
```

#### MacOS [Expérimental]

Aucune autre dépendance.

# Fonctionnalités

- GUI entièrement natif pour toutes les plateformes
  - Windows: `Windows Presentation Foundation`
  - Linux: `GTK+3`
  - MacOS: `MonoMac`
- Outil de console à part entière
  - Obtient, modifie, charge ou sauvegarde rapidement les paramètres
  - Support de script (sortie json)
- Positionnement absolu du curseur
  - Zone de l'écran ainsi que la zone de la tablette
  - Décalages par rapport au centre
  - Rotations précises de la zone
- Positionnement relatif du curseur
  - Sensibilité horizontal et vertical (px/mm)
- Raccourcis du stylet
  - Raccourcis de pression de la pointe
  - Raccourcis touches express
  - Raccourcis boutons stylet
  - Raccourcis boutons souris
  - Raccourcis clavier
  - Plugin de raccourcis externe
- Sauvegarder et charger des paramètres
  - Charge automatiquement les paramètres utilisateur via `settings.json` dans l'utilisateur actif `%localappdata%` ou le `.config` du dossier répertoire racine des paramètres.
- Plugins
  - Filtres
  - Modes de sorties
  - Outils

# Contribuer à OpenTabletDriver

Si vous souhaitez contribuer à OpenTabletDriver, regardez le [Traqueur d'incidents](https://github.com/OpenTabletDriver/OpenTabletDriver/issues).

Si vous avez des problèmes ou des suggestions, [ouvrez un ticket](https://github.com/OpenTabletDriver/OpenTabletDriver/issues/new/choose).
