[![Actions Status](https://github.com/OpenTabletDriver/OpenTabletDriver/workflows/.NET%20CI/badge.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/actions) [![Total Download Count](https://img.shields.io/github/downloads/OpenTabletDriver/OpenTabletDriver/total.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest)

# OpenTabletDriver

[English](../README.md) | [Español](README_ES.md) | [Français](README_FR.md) | [Deutsch](README_DE.md) | Português-BR | [Nederlands](README_NL.md) | [한국어](README_KO.md) | [Русский](README_RU.md) | [简体中文](README_CN.md) | [繁體中文](README_TW.md) | [Ελληνικά](README_EL.md) | [Magyar](README_HU.md)

OpenTabletDriver é um driver de código aberto multi-plataforma para mesas digitalizadoras (tablets) configurado pelo usuário.
O objetivo do OpenTabletDriver é ser compatível em múltiplas plataformas e ter uma interface gráfica amigável de fácil utilização para o usuário.

<p align="middle">
  <img src="https://i.imgur.com/XDYf62e.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/jBW8NpU.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/ZLCy6wz.png" width="410" align="middle"/>
</p>

# Tablets compatíveis

Todos os tablets que são compatíveis, não testados, ou que estão em planejamento para testes estão [aqui](https://opentabletdriver.net/Tablets).
Soluções de problemas comuns podem ser encontrados na wiki.

# Instalação

- [Windows](https://opentabletdriver.net/Wiki/Install/Windows)
- [Linux](https://opentabletdriver.net/Wiki/Install/Linux)
- [MacOS](https://opentabletdriver.net/Wiki/Install/MacOS)

# Executando OpenTabletDriver

O OpenTabletDriver funciona como dois processos separados que comunicam-se entre si para poder funcionar perfeitamente.
O programa ativo que lida com o manuseio de dados é `OpenTabletDriver.Daemon`,
enquanto a interface de usuário é lidada atráves do `OpenTabletDriver.UX`.
O daemon (processo em segundo) deve ser inicializado para que tudo possa rodar sem problemas, enquanto a interface não é necessária.
Se você possui alguma configuração já pronta, ela deve ser aplicada quando o daemon iniciar.

## Compilando OpenTabletDriver do código-fonte

Os requisitos para compilar o OpenTabletDriver são referentes à todas as plataformas. Já para executar OpenTabletDriver em cada plataforma requer dependências diferentes.

### Todas as plataformas

- .NET 8 SDK (pode ser obtida [aqui](https://dotnet.microsoft.com/download/dotnet/8.0) - Você precisa do SDK para sua plataforma, usuários Linux devem instalar via gerenciador de pacotes se possível)

#### Windows

Execute `build.ps1` para produzir os binários na pasta 'bin'. Estas compilações seram executadas no modo portátil por padrão.

#### Linux

Pacotes necessários (alguns pacotes já podem estar pré-instalados na sua distribuição)

- libx11
- libxrandr
- libevdev2
- GTK+3

Execute `./eng/linux/package.sh`. Se um "pacote de instalação" precisa ser gerado, há suporte oficial para os seguintes formatos de empacotamentos:

| Formato de empacotamento | Comando |
| --- | --- |
| Binário genérico `.tar.gz` | `./eng/linux/package.sh --package BinaryTarBall` |
| Pacote Debian `.deb` | `./eng/linux/package.sh --package Debian` |
| Pacote Red Hat `.rpm` | `./eng/linux/package.sh --package RedHat` |

O binário genérico deve ser extraído ao diretório raiz.

#### MacOS [Experimental]

Execute `./eng/macos/package.sh --package true`.

# Funcionalidades

- Interface totalmente nativa
  - Windows: `Windows Presentation Foundation`
  - Linux: `GTK+3`
  - MacOS: `MonoMac`
- Ferramenta de console completa
  - Rápida a criação, modificação, carregamento, e/ou salvamento de configurações
  - Suporte a scripts (saídas json)
- Posicionamento absoluto do cursor
  - Área da tela e área do tablet
  - Fixação do offset central
  - Rotação precisa da área
- Posicionamento relativo do cursor
  - px/mm sensibilidade horizontal e vertical
- Vinculações de teclas...
  - ...por pressão da ponta da caneta
  - ...nos botões do tablet
  - ...nos botões da caneta
  - ...nos botões do mouse
  - ...nas teclas do teclado
  - ...de plugins externos
- Salvando e carregando configurações
  - Carregamento automático das configurações de usuário atráves de `settings.json` na pasta `%localappdata%` do usuário ou na raiz do diretório `.config`.
- Plugins
  - Filtros
  - Modos de saída
  - Ferramentas

# Contribuindo ao OpenTabletDriver

Se você deseja contribuir para o OpenTabletDriver, confira a aba de [problemas](https://github.com/OpenTabletDriver/OpenTabletDriver/issues).
Quando criar pull requests, siga as orientações descritas em nosso [guia de contribuição](https://github.com/OpenTabletDriver/OpenTabletDriver/blob/master/CONTRIBUTING.md).

Se você tiver algum problema ou sugestão, [relate um problema](https://github.com/OpenTabletDriver/OpenTabletDriver/issues/new/choose) e preencha o template com informações relevantes.
Somos gratos aos relatos de bugs quanto aos pedidos de novos tablets para adicionar suporte. Em alguns casos, adicionar um novo tablet pode ser fácil.

Para issues e PRs relacionados ao site do OpenTabletDriver [página web](https://opentabletdriver.net), veja este repositório [aqui](https://github.com/OpenTabletDriver/opentabletdriver.github.io).

### Adicionando suporte a um novo tablet

Se você gostaria de adicionar suporte a um novo tablet, abra uma issue ou entre em nosso
[discord](https://discord.gg/9bcMaPkVAR) solicitando o suporte. *Nós geralmente preferimos as solicitações por um novo tablet sejam feitas via discord, devido as trocas de ideias envolvidas*.

Nós precisaremos que você faça algumas coisas como gravar os dados enviados pelo seu tablet utilizando nossa ferramenta de debug integrada,
testando funcionalidades do tablet (botões do tablet, botões da caneta, pressão da caneta, etc) com uma configuração diferente quenós iremos enviar a você para testar.

Você também, claro, é bem vindo a abrir uma PR adicionando o suporte ao tablet por você mesmo, caso tenha uma boa compreensão do que está sendo desenvolvido.

Geralmente esse processo é relativamente fácil, especialmente se a fabricante do seu tablet já exisitr em algum outro tablet que já tenha suporte.
