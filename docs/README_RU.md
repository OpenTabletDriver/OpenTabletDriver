[![Actions Status](https://github.com/OpenTabletDriver/OpenTabletDriver/workflows/.NET%20CI/badge.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/actions) [![Total Download Count](https://img.shields.io/github/downloads/OpenTabletDriver/OpenTabletDriver/total.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest)

# OpenTabletDriver

[English](../README.md) | [Español](README_ES.md) | [Français](README_FR.md) | [Deutsch](README_DE.md) | [Português-BR](README_PTBR.md) | [Nederlands](README_NL.md) | [한국어](README_KO.md) | Русский | [简体中文](README_CN.md) | [繁體中文](README_TW.md) | [Ελληνικά](README_EL.md) | [Magyar](README_HU.md)

OpenTabletDriver — кроссплатформенный драйвер с открытым исходным кодом для графических планшетов. Целью проекта является поддержка максимального числа устройств и платформ, а также создание простого и удобного интерфейса для настройки драйвера.

<p align="middle">
  <img src="https://i.imgur.com/XDYf62e.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/jBW8NpU.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/ZLCy6wz.png" width="410" align="middle"/>
</p>

# Поддерживаемые планшеты

Статус поддержки различных моделей можно найти по [этой ссылке](https://opentabletdriver.net/Tablets).

Возможные типы статуса поддержки:

- Supported — полностью поддерживается;
- Missing Features — поддерживается основной функционал, некоторые доп. функции не поддерживаются;
- Untested — есть конфигурация, возможно работает, но не проверялся;
- Broken — не поддерживается на текущий момент;
- Has Quirks — поддерживается, но имеет особенности в работе.

Решения возможных типичных проблем указаны в разделе вики используемой платформы.

# Установка

- [Windows](https://opentabletdriver.net/Wiki/Install/Windows)
- [Linux](https://opentabletdriver.net/Wiki/Install/Linux)
- [MacOS](https://opentabletdriver.net/Wiki/Install/MacOS)

# Запуск и использование OpenTabletDriver

OpenTabletDriver работает в двух процессах, взаимодействующих друг с другом. `OpenTabletDriver.Daemon` — часть драйвера, которая находится в фоне, обрабатывает сигнал с планшета и перемещает курсор. Графическим интерфейсом для настройки является `OpenTabletDriver.UX.*`, где `*` зависит от вашей платформы<sup>1</sup>. При желании можно использовать `Daemon` без `UX`, последние сохраненные настройки будут загружены автоматически.

> <sup>1</sup>В Windows используется `Wpf`, в Linux — `Gtk`, а в MacOS — `MacOS`. Если не собирать драйвер самостоятельно, об этом не стоит беспокоиться — в архиве будет только подходящая версия `UX`.

## Сборка OpenTabletDriver из исходников

Требования к сборке OpenTabletDriver одинаковы для всех платформ, но для запуска на разных платформах OpenTabletDriver может иметь дополнительные зависимости.

### Все платформы

- .NET 8 SDK

#### Windows

Нет дополнительных зависимостей.

#### Linux

- libx11
- libxrandr
- libevdev2
- GTK+3

#### MacOS [Экспериментальная поддержка]

Нет дополнительных зависимостей.

# Функции и особенности

- Полностью нативный графический интерфейс
  - Windows: `Windows Presentation Foundation`
  - Linux: `GTK+3`
  - MacOS: `MonoMac`
- Консольная утилита
  - Быстро считывайте, изменяйте, загружайте или сохраняйте настройки
  - Поддержка скриптинга (вывод в json)
- Абсолютное позиционирование курсора
  - Установка областей на экране и планшете
  - Отступы от центра области
  - Точное вращение области
- Относительное позиционирование курсора (режим мыши)
  - раздельная горизонтальная и вертикальная чувствительность в пикс./мм
- Привязки (бинды) на:
  - Касание поверхности стилусом
  - Кнопки на планшете
  - Кнопки на стилусе
  - Кнопки мыши
  - Кнопки на клавиатуре
  - Внешние бинды плагинов
- Сохранение и загрузка настроек
  - Автозагрузка настроек из файла `settings.json` в `%localappdata%` или `.config` пользователя
- Плагины
  - Фильтры
  - Режимы вывода
  - Инструменты

# Помощь в разработке OpenTabletDriver

Если вам хочется помочь с разработкой драйвера, то актуальные проблемы можно найти [здесь](https://github.com/OpenTabletDriver/OpenTabletDriver/issues).

Если у вас возникли проблемы, появились пожелания или предложения, откройте [новый тикет](https://github.com/OpenTabletDriver/OpenTabletDriver/issues/new/choose).
