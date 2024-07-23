[![Actions Status](https://github.com/OpenTabletDriver/OpenTabletDriver/workflows/.NET%20CI/badge.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/actions) [![Total Download Count](https://img.shields.io/github/downloads/OpenTabletDriver/OpenTabletDriver/total.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest)

# OpenTabletDriver

[English](../README.md) | Español | [Français](README_FR.md) | [Deutsch](README_DE.md) | [Português-BR](README_PTBR.md) | [Nederlands](README_NL.md) | [한국어](README_KO.md) | [Русский](README_RU.md) | [简体中文](README_CN.md) | [繁體中文](README_TW.md) | [Ελληνικά](README_EL.md) | [Magyar](README_HU.md)

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

- .NET 8 SDK (Puede ser obtenido desde [aquí](https://dotnet.microsoft.com/download/dotnet/8.0) - Usted necesita el SDK compatible con su plataforma, los usuarios de Linux deben de instalarlo a través del gestor de paquetes siempre que sea posible)

#### Windows

No hay otras dependencias.

#### Linux

Paquetes necesarios (algunos paquetes pueden venir preinstalados en su distribución)

- libx11
- libxrandr
- libevdev2
- GTK+3

Para compilarlo en Linux, ejecute el archivo 'build.sh' proporcionado. Esto ejecutará los mismos comandos 'dotnet publish' utilizados para compilar el paquete AUR, y producirá binarios utilizables en 'OpenTabletDriver/bin'.

Para compilarlo en Linux en ARM, ejecute el archivo 'build.sh' proporcionado con el runtime apropiado como argumento. Para arm64, esto sería 'linux-arm64'.

Nota: Si se compila por primera vez, ejecute el script 'generate-rules.sh' incluido. Esto generará un conjunto de reglas udev en 'OpenTabletDriver/bin', llamado '99-opentabletdriver.rules'. Este archivo debe de ser movido a `/etc/udev/rules.d/`:

```
sudo mv ./bin/99-opentabletdriver.rules /etc/udev/rules.d/
```

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
- Plugins
  - Filtros
  - Modos de salida
  - Herramientas

# Contribuyendo a OpenTabletDriver

Si desea contribuir a OpenTabletDriver, revise en el [rastreador de propuestas](https://github.com/OpenTabletDriver/OpenTabletDriver/issues). Cuando cree una solicitud de extracción, siga las pautas indicadas en nuestras [guías de contribución](https://github.com/OpenTabletDriver/OpenTabletDriver/blob/master/CONTRIBUTING.md).

Si tiene algún problema o sugerencia, [abra un ticket de propuesta](https://github.com/OpenTabletDriver/OpenTabletDriver/issues/new/choose) y rellene la plantilla con la información pertinente. Agradecemos tanto los informes de errores, como las nuevas tabletas a las que añadir compatibilidad. En muchos casos, añadir compatibilidad a una nueva tableta es bastante fácil.

Para propuestas y solicitudes de extracción relacionados con la página web de OpenTabletDriver, vea el repositorio de [aquí](https://github.com/OpenTabletDriver/opentabletdriver.github.io).

### Soporte para una nueva tableta

Si le gustaría que añadiéramos soporte para una nueva tableta, abra una propuesta o únase a nuestro servidor de [discord](https://discord.gg/9bcMaPkVAR) solicitando soporte. *Generalmente preferimos que añadir soporte para una tableta se haga a través de discord, debido a las idas y venidas que se producen*.

Le pediremos que haga algunas cosas, como hacer una grabación de los datos enviados por su tableta usando nuestra herramienta de depuración incorporada, probar las características de la tableta (Botones de la tableta, botones del lápiz, presión del lápiz, etc.) con diferentes configuraciones que le enviaremos para que las pruebe.

Por supuesto, también puede abrir una solicitud de extracción añadiendo soporte usted mismo, si tienes un buen conocimiento de lo que implica.

Por lo general este proceso es relativamente fácil, especialmente si se trata de un fabricante para el que ya tenemos soporte en otras tabletas.
