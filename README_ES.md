[![Actions Status](https://github.com/OpenTabletDriver/OpenTabletDriver/workflows/.NET%20Core/badge.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/actions) [![CodeFactor](https://www.codefactor.io/repository/github/OpenTabletDriver/OpenTabletDriver/badge/master)](https://www.codefactor.io/repository/github/OpenTabletDriver/OpenTabletDriver/overview/master) [![Total Download Count](https://img.shields.io/github/downloads/OpenTabletDriver/OpenTabletDriver/total.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest)

# OpenTabletDriver

[English](README.md) | [한국어](README_KO.md) | Español | [Русский](README_RU.md) | [简体中文](README_CN.md) | [Français](README_FR.md)

OpenTabletDriver es un driver de tabletas multiplataforma, open-source y en modo de usuario. El objetivo de OpenTabletDriver es ser lo más multiplataforma posible con la mayor compatibilidad en una interfaz de usuario fácil de configurar.

<p align="middle">
  <img src="https://i.imgur.com/XDYf62e.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/jBW8NpU.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/ZLCy6wz.png" width="410" align="middle"/>
</p>

# Tabletas soportadas

Los estados de las tabletas que son soportadas, no probadas y planeadas para ser soportadas se pueden encontrar aquí. Las soluciones a los problemas más comunes pueden ser encontradas en la wiki de su plataforma.

- [Tabletas compatibles](https://opentabletdriver.net/Tablets)

# Instalación

- [Windows](https://opentabletdriver.net/Wiki/Install/Windows)
- [Linux](https://opentabletdriver.net/Wiki/Install/Linux)
- [MacOS](https://opentabletdriver.net/Wiki/Install/MacOS)

# Ejecución de los binarios de OpenTabletDriver

OpenTabletDriver funciona como dos procesos separados que se coordinan a la perfección. El programa activo que se encarga de manejar todos los datos de la tableta es `OpenTabletDriver.Daemon`, mientras que la interfaz de usuario es `OpenTabletDriver.UX.*`, donde `*` depende de su plataforma<sup>1</sup>. El servicio debe de iniciarse para que todo funcione, sin embargo, la interfaz de usuario es innecesaria. Si tiene configuraciones existentes, deberían de aplicarse cuando se inicie el servicio.

> <sup>1</sup>Windows usa `Wpf`, Linux usa `Gtk` y MacOS usa `MacOS` respectivamente. Esto, en su mayor parte, puede ser ignorado si no lo compilas desde el código fuente, ya que sólo se proporcionará la versión correcta.

## Compilación de OpenTabletDriver desde el código fuente

Los requisitos para compilar OpenTabletDriver son consistentes en todas las plataformas. La ejecución de OpenTabletDriver en cada plataforma requiere diferentes dependencias.

### Todas las plataformas

- .NET 6 SDK

#### Windows

No hay otras dependencias.

#### Linux

- libx11
- libxrandr
- libevdev2
- GTK+3

#### MacOS [Experimental]

No hay otras dependencias.

# Características

- Interfaz de usuario totalmente nativa de la plataforma
  - Windows: `Windows Presentation Foundation`
  - Linux: `GTK+3`
  - MacOS: `MonoMac`
- Herramienta de consola completa
  - Añada, cambie, cargue y guarde rapidamente las configuraciones
  - Soporte de scripts (Salida en json)
- Posicionamiento absoluto del cursor
  - Área de la pantalla y área de la tableta
  - Compensaciones ancladas al centro
  - Rotación precisa de área
- Posicionamiento relativo del cursor
  - Sensibilidad horizontal y vertical en px/mm
- Asignaciones al lápiz
  - Asignaciones por la presión de la punta
  - Asignaciones a las ExpressKey
  - Asignaciones a los botones del lápiz
  - Asignaciones a los botones del ratón
  - Asignaciones al teclado
  - Asignaciones a plugins externos
- Guarda y carga las configuraciones
  - Carga automáticamente la configuración del usuario a través de `settings.json` en `%localappdata%` del usuario activo o en `.config` de la carpeta raíz de configuraciones.
- Editor de configuraciones
  - Permite crear, modificar y eliminar configuraciones
  - Genera configuraciones a partir de dispositivos HID visibles
- Plugins
  - Filtros
  - Modos de salida
  - Herramientas

# Contribuyendo a OpenTabletDriver

Si desea contribuir a OpenTabletDriver, revise en el [rastreador de propuestas](https://github.com/OpenTabletDriver/OpenTabletDriver/issues).

Si tiene algún problema o sugerencia, [abra un ticket de propuesta](https://github.com/OpenTabletDriver/OpenTabletDriver/issues/new/choose).
