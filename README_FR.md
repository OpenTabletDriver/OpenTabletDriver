[![Actions Status](https://github.com/OpenTabletDriver/OpenTabletDriver/workflows/.NET%20Core/badge.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/actions) [![CodeFactor](https://www.codefactor.io/repository/github/OpenTabletDriver/OpenTabletDriver/badge/master)](https://www.codefactor.io/repository/github/OpenTabletDriver/OpenTabletDriver/overview/master) [![Total Download Count](https://img.shields.io/github/downloads/OpenTabletDriver/OpenTabletDriver/total.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest)

# OpenTabletDriver

[English](README.md) | [한국어](README_KO.md) | [Español](README_ES.md) | [Русский](README_RU.md) | [简体中文](README_CN.md) | Français

OpenTabletDriver est un driver de tablette en mode utilisateur, open source et multiplateforme. Le but d'OpenTabletDriver est d'être compatible avec le plus de plateforme possibles, et ce avec une interface graphique utilisateur facilement configurable.

<p align="middle">
  <img src="https://i.imgur.com/XDYf62e.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/jBW8NpU.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/ZLCy6wz.png" width="410" align="middle"/>
</p>

# Tablettes supportées

Tous les modèles de tablettes supportés, non testées, et prévus pour être supportés peuvent-être trouvés ici. Des solutions alternatives peuvent-être trouvées sur le wiki pour votre plateforme.

- [Tablettes supportées](https://github.com/OpenTabletDriver/OpenTabletDriver/blob/master/TABLETS.md)

# Installation

- [Guide d'installation](https://github.com/OpenTabletDriver/OpenTabletDriver/wiki/Installation-Guide)

# Exécuter un binaire OpenTabletDriver

Le fonctionnement d'OpenTabletDriver est basé sur l'utilisation de deux processus séparés qui interagissent parfaitement entre eux. Le programme actif qui permet le traitement des données est `OpenTabletDriver.Daemon`, tandis que l'interface graphique est `OpenTabletDriver.UX.*`, où `*` dépend de votre plateforme<sup>1</sup>. Pour que tout fonctionne correctement, Le programme actif daemon doit être exécuté. Si vous avez des paramètres existants, ils vont s'appliquer lors de son exécution.


> <sup>1</sup>Windows utilise `Wpf`, Linux utilise `Gtk`, et MacOS utilise `MacOS` respectivement. Celà peut-être ignoré dans la plupart des cas si vous ne tentez pas de build à partir de la source, car seule la bonne version sera fournie.
## Build OpenTabletDriver à partir de la source

Les exigences pour build OpenTabletDriver sont cohérentes sur toutes les plateformes. L'éxécution d'OpenTabletDriver requière des dépendances différentes.

### Toutes les plateformes

- .NET 5 SDK

#### Windows

Aucune autre dépendance.

#### Linux

- libx11
- libxrandr
- libevdev2
- GTK+3

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


  - Tip by pressure bindings // A TRAD
  - Express key bindings // A TRAD



  - Raccourcis boutons stylet
  - Raccourcis boutons souris
  - Raccourcis clavier
  - Plugin de raccourcis externe
- Sauvegarder et charger des paramètres
  - Charge automatiquement les paramètres utilisateur via `settings.json` dans l'utilisateur actif `%localappdata%` ou le `.config` du dossier répertoire racine des paramètres.
- Éditeur de configuration
  - Vous permets de créer, modifier ou supprimer des configurations.
  - Génère des configurations venant des appareils HID visibles
- Plugins
  - Filtres
  - Modes de sorties
  - Outils

# Contribuer à OpenTabletDriver

Si vous souhaitez contribuer à OpenTabletDriver, regardez le [Traqueur d'incidents](https://github.com/OpenTabletDriver/OpenTabletDriver/issues).

Si vous avez des problèmes ou des suggestions, [ouvrez un ticket](https://github.com/OpenTabletDriver/OpenTabletDriver/issues/new/choose).