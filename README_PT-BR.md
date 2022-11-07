[![Actions Status](https://github.com/OpenTabletDriver/OpenTabletDriver/workflows/.NET%20Core/badge.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/actions) [![CodeFactor](https://www.codefactor.io/repository/github/OpenTabletDriver/OpenTabletDriver/badge/master)](https://www.codefactor.io/repository/github/OpenTabletDriver/OpenTabletDriver/overview/master) [![Total Download Count](https://img.shields.io/github/downloads/OpenTabletDriver/OpenTabletDriver/total.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest)

# OpenTabletDriver

[English](README.md)| [한국어](README_KO.md) | [Español](README_ES.md) | [Русский](README_RU.md) | [简体中文](README_CN.md) |[Français](README_FR.md) | [Português Brasileiro](README_PT-BR.md)

O OpenTabletDriver é um driver para mesas digitalizadoras em modo de usuário, código-aberto e multiplataforma. O objetivo do OpenTabletDriver é ser multiplataforma com a maior compatibilidade em uma interface gráfica configurável e fácil de usar. 
<p align="middle">
  <img src="https://i.imgur.com/XDYf62e.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/jBW8NpU.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/ZLCy6wz.png" width="410" align="middle"/>
</p>

# Mesas [Tablets] Suportadas

Todos os estados das mesas que estão suportadas, não testado, e planejado a ter suporte podem ser encontrados aqui. Soluções para problemas comuns podem ser encontrados na wiki para sua plataforma. 
- [Mesas Suportadas](https://opentabletdriver.net/Tablets)

# Instalação

- [Windows](https://opentabletdriver.net/Wiki/Install/Windows)
- [Linux](https://opentabletdriver.net/Wiki/Install/Linux)
- [MacOS](https://opentabletdriver.net/Wiki/Install/MacOS)

# Executando os binários do OpenTabletDriver

O OpenTabletDriver funciona como dois processos separados que se interagem entre si perfeitamente. O programa ativo que cuida de toda a informação da mesa é o `OpenTabletDriver.Daemon`, enquanto a interface é `OpenTabletDriver.UX.*`, onde `*` depende de sua plataforma<sup>1</sup>. O Daemon deve ser iniciado para tudo funcionar, mas a interface é desnecessária. Se você tem configurações existentes, elas vão se aplicar assim que o daemon iniciar.

> <sup>1</sup>Windows usa o `Wpf`, Linux usa o `Gtk`, e MacOS usa  `MacOS` respectivamente. Essa parte pode ser ignorada se você não o compilar do código fonte, já que a versão correta será fornecida na instalação.

## Compilando o OpenTabletDriver do código fonte

Os requerimentos para compilar o OpenTabletDriver são consistente entre todas as plataformas, A execução do OpenTabletDriver em cada plataforma requer dependências diferentes.

### Todas as plataformas

- .NET 5 SDK

#### Windows

Nenhuma outra dependência.

#### Linux

- libx11
- libxrandr
- libevdev2
- GTK+3

#### MacOS [Experimental]

Nenhuma outra depedência.

# Features

- Interface Totalmente nativa á sua plataforma
  - Windows: `Windows Presentation Foundation`
  - Linux: `GTK+3`
  - MacOS: `MonoMac`
- Ferramenta de console totalmente desenvolvida
  - Adquira, altere, carregue ou salve configurações rapidamente
  - Suporte a scripts (arquivo/saída json)
- Posicionamento absoluto do cursor
  - Área da tela e área da mesa
  - *Offsets* ancorados no centro
  - Precisa rotação de área
- Posicionamento relativo do cursor
  - Sensibilidade horizontal e vertical de px / mm
- Configuração da caneta
  - Ponta por ligações de pressão
  - Configuração dos atalhos de teclado expressos
  - Configurações dos botões da caneta
  - Configuração do botão do mouse
  - Configuração do teclado
  - Configuração de plug-ins externos
- Salvando e carregando configurações
  - Automaticamente carrega as configurações do usuário usando `settings.json` no usuário ativo `%localappdata%` ou `.config` no diretório raiz.
- Editor de configurações
  - Permite você á criar, modificar, e deletar configurações.
  - Gerar configurações de dispositivos HID visíveis
- Plugins
  - Filtros
  - Modos de saída (*Output*)
  - Ferramentas

# Contribuindo ao OpenTabletDriver

Se você deseja contribuir ao OpenTabletDriver, confira o [Rastreador de Problemas](https://github.com/OpenTabletDriver/OpenTabletDriver/issues).

Se você tiver algum problema ou sugestão, [abra um tíquete de problema](https://github.com/OpenTabletDriver/OpenTabletDriver/issues/new/choose).
