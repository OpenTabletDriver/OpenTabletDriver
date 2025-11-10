[![Actions Status](https://github.com/OpenTabletDriver/OpenTabletDriver/workflows/.NET%20CI/badge.svg)](https://github.com/OpenTabletDriver/OpenTableDriver/actions) [![Total Download Count](https://img.shields.io/github/downloads/OpenTabletDriver/OpenTabletDriver/total.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest)

# OpenTabletDriver

[English](../README.md) | [Español](README_ES.md) | [Français](README_FR.md) | [Deutsch](README_DE.md) | Português-BR | [Nederlands](README_NL.md) | [한국어](README_KO.md) | [Русский](README_RU.md) | [简体中文](README_CN.md) | [繁體中文](README_TW.md) | [Ελληνικά](README_EL.md) | [Magyar](README_HU.md)

OpenTabletDriver é um driver multi-plataforma de código aberto para mesas digitalizadoras, configurável pelo usuário.

O objetivo é ser compatível com múltiplas plataformas, possuir uma interface gráfica amigável, e ter uma fácil utilização.

<p align="middle">
  <img src="https://i.imgur.com/XDYf62e.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/jBW8NpU.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/ZLCy6wz.png" width="410" align="middle"/>
</p>

# Tablets compatíveis

Todos os tablets que são compatíveis, não testados ou que estão em planejamento para testes estão [aqui](https://opentabletdriver.net/Tablets).
Soluções de problemas comuns podem ser encontrados na wiki.

# Instalação

- [Windows](https://opentabletdriver.net/Wiki/Install/Windows)
- [Linux](https://opentabletdriver.net/Wiki/Install/Linux)
- [MacOS](https://opentabletdriver.net/Wiki/Install/MacOS)

# Executando OpenTabletDriver

OpenTabletDriver funciona como dois processos separados em que comunicam entre si para poder funcionar perfeitamente.
O processo ativo, em que lida com o manuseio de dados, é chamado de `OpenTabletDriver.Daemon`,
enquanto o processo de interface gráfica é chamado de `OpenTabletDriver.UI`.
O processo ativo deve ser inicializado para que tudo possa rodar sem problemas,
já o processo da interface gráfica não é necessário.
Se você já possui uma configuração, elas devem ser carregadas quando o processo ativo iniciar.

## Compilando OpenTabletDriver usando o código-fonte

Os requisitos para compilar OpenTabletDriver são referentes à todas as plataformas. Cada plataforma requer dependências diferentes.

### Todas as plataformas

- .NET 8 SDK (pode ser obtida [aqui](https://dotnet.microsoft.com/download/dotnet/8.0) - Você precisará do SDK para sua plataforma, usuários de Linux devem instalar usando o gerenciador de pacotes nativo, se possível)

#### Windows

Execute `build.ps1` para produzir os binários, enviadas para a pasta 'bin'. Estes binários serão executadas no modo portátil por padrão.

#### Linux

Pacotes necessários (alguns pacotes já podem estarem pré-instalados no seu sistema)

- libx11
- libxrandr
- libevdev2
- GTK+3

Execute `./eng/linux/package.sh`. Se algum "pacote de instalação" é designado,
temos suporte para os seguintes formatos de empacotamento:

| Formato do pacote | Comando|
| --- | --- |
| Generic binary tarball (`.tar.gz`) | `./eng/linux/package.sh --package BinaryTarBall` |
| Debian package (`.deb`) | `./eng/linux/package.sh --package Debian` |
| Red Hat package (`.rpm`) | `./eng/linux/package.sh --package RedHat` |

O 'Generic binary tarball' é feito para ser extraído ao diretório raiz.

#### MacOS [Experimental]

Execute `./eng/macos/package.sh --package true`.

# Funcionalidades

- Interface gráfica totalmente nativa
  - Windows: `Windows Presentation Foundation`
  - Linux: `GTK+3`
  - MacOS: `MonoMac`
- Ferramenta de console completa
  - Crie, altere, carregue, ou salve configurações rapidamente
  - Suporte à scripts (saídas `json`)
- Posicionamento absoluto do cursor
  - Área da tela e área da mesa digitalizadora
  - Offsets centrais
  - Rotação precisa da área
- Posicionamento relativo do cursor
  - px/mm sensibilidade horizontal e vertical
- Vinculações de teclas (binding)
  - Bindings na pressão da ponta da caneta
  - Bindings nas Express Key
  - Bindings nos botões da caneta
  - Bindings nos botões do mouse
  - Bindings nas teclas do teclado
  - Bindings de plugins externos
- Salvando e carregando informações
  - Carregamento automático das configurações do usuário via `settings.json` no usuário ativo no diretório raiz de configurações `%localappdata%` ou `.config`
- Plugins
  - Filtros
  - Modos de saída
  - Ferramentas

# Contribuindo ao OpenTabletDriver

Se você deseja contribuir ao projeto OpenTabletDriver,
confira a aba de [problemas](https://github.com/OpenTabletDriver/OpenTabletDriver/issues).
Quando criar pull requests, siga as orientações descritas
em nosso [guia de contribuição](https://github.com/OpenTabletDriver/OpenTabletDriver/blob/master/CONTRIBUTING.md).

Se você tiver algum problema ou sugestão, [abra um ticket](https://github.com/OpenTabletDriver/OpenTabletDriver/issues/new/choose)
e preencha o template com informações relevantes.
Somos gratos aos relatos de bugs, quanto aos pedidos de novas mesas digitalizadoras.
Em muitos casos, adicionar uma nova mesa digitalizora tende ser mais fácil.

Para issues e pull requests relacionados à [página web](https://opentabletdriver.net), verifique este repositório [aqui](https://github.com/OpenTabletDriver/opentabletdriver.github.io).

### Adicionando suporte à uma nova mesa digitalizadora

Se você gostaria de adicionar suporte a um novo tablet, abra uma issue ou entre em nosso
[discord](https://discord.gg/9bcMaPkVAR) solicitando o suporte. *Nós geralmente preferimos as solicitações por um novo tablet sejam feitas via discord, devido as trocas de ideias envolvidas*.

Nós precisaremos que você faça algumas coisas como gravar os dados enviados pelo seu tablet utilizando nossa ferramenta de debug integrada, testando funcionalidades do tablet (botões do tablet, botões da caneta, pressão da caneta, etc) com uma configuração diferente quenós iremos enviar a você para testar.

Você também, claro, é bem vindo a abrir uma PR adicionando o suporte a tablet por você mesmo,caso tenha uma boa compreensão do que está desenvolvido.

Geralmente esse processo é relativamente fácil, especialmente se a fabricante do seu tablet já exisitr em algum outro tablet que já tenha suporte.
