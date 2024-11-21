[![Actions Status](https://github.com/OpenTabletDriver/OpenTabletDriver/workflows/.NET%20CI/badge.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/actions) [![Total Download Count](https://img.shields.io/github/downloads/OpenTabletDriver/OpenTabletDriver/total.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest)

# OpenTabletDriver

[English](../README.md) | [Español](README_ES.md) | [Français](README_FR.md) | [Deutsch](README_DE.md) | Português-BR | [Nederlands](README_NL.md) | [한국어](README_KO.md) | [Русский](README_RU.md) | [简体中文](README_CN.md) | [繁體中文](README_TW.md) | [Ελληνικά](README_EL.md) | [Magyar](README_HU.md)

OpenTabletDriver é um programa de código aberto, multi-plataforma, driver de mesas digitalizadoras (tablets) configurado pelo usuário. O objetivo do OpenTabletDriver é ser compatível em múltiplas plataformas em uma interface gráfica amigável e de fácil utilização para o usuário.

<p align="middle">
  <img src="https://i.imgur.com/XDYf62e.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/jBW8NpU.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/ZLCy6wz.png" width="410" align="middle"/>
</p>

# Tablets compatíveis

Todos os tablets que são compatíveis, não testados ou que estão em planejamento para testes estão [aqui](https://opentabletdriver.net/Tablets). Soluções de problemas comuns podem ser encontrados na wiki.

# Instalação

- [Windows](https://opentabletdriver.net/Wiki/Install/Windows)
- [Linux](https://opentabletdriver.net/Wiki/Install/Linux)
- [MacOS](https://opentabletdriver.net/Wiki/Install/MacOS)

# Executando OpenTabletDriver

O OpenTabletDriver funciona como dois processos separados que comunicam-se entre si para poder funcionar perfeitamente. O programa ativo que lida com todos o manuseio de dados é `OpenTabletDriver.Daemon`, enquanto o GUI (interface) é `OpenTabletDriver.UX.*`, onde `*` depende da plataforma<sup>1</sup>. O daemon deve ser inicializado para que tudo possa rodar sem problemas, enquanto o GUI não é necessário. Se você possui uma configuração já pronta, elas devem ser aplicadas quando o daemon iniciar.

> <sup>1</sup>Windows usa `Wpf`, Linux usa `Gtk`, e MacOS usa `MacOS`. O que pode ser ignorado por grande parte da aplicação se você não compilar a partir da fonte, já que apenas a versão correta será fornecida.

## Buildando OpenTabletDriver da fonte

Os requisitos para buildar o OpenTabletDriver são referentes à todas as plataformas. Cada plataforma requer dependências diferentes.

### Todas as plataformas

- .NET 8 SDK (pode ser obtida [aqui](https://dotnet.microsoft.com/download/dotnet/8.0) - Você precisa do SDK para sua plataforma, usuários Linux devem instalar via gerenciador de pacotes se possível)

#### Windows

Execute `build.ps1` para produzir as compilações binárias na pasta 'bin'. Estas compilações estão executadas no modo portátil por padrão.

#### Linux

Pacotes requeridos (alguns pacotes podem já estar pré-instalados na sua distro)

- libx11
- libxrandr
- libevdev2
- GTK+3

Para compilar no Linux, execute o arquivo fornecido chamado 'build.sh'. Irá executar o equivalente ao comando 'dotnet publish'
usado para compilação do pacote AUR, e irá produzir as compilações binárias em 'OpenTabletDriver/bin'.

Para compilar no ARM linux, execute o arquivo fornecido 'build.sh' passando o runtime apropriado como argumento. Por exemplo, para o arm64 seria: 'linux-arm64'.

Nota: Se você está compilando pela primeira vez, execute junto o script generate-rules.sh. Isso irá gerar um pacote de regras de udev em OpenTabletDriver/bin,
chamado '99-opentabletdriver.rules'. Esse arquivo deve ser movido para `/etc/udev/rules.d/`:

```
sudo mv ./bin/99-opentabletdriver.rules /etc/udev/rules.d/
```

#### MacOS [Experimental]

Sem outras dependências.

# Funcionalidades

- Interface totalmente nativa
  - Windows: `Windows Presentation Foundation`
  - Linux: `GTK+3`
  - MacOS: `MonoMac`
- Ferramenta do console completa
  - Rápida criação, alteração, carregamento, ou salvando configurações
  - Suporte a scripts (saídas de json)
- Posicionamento absoluto do cursor
  - Área da tela e área do tablet
  - Fixação do offset central
  - Rotação precisa da área
- Posicionamento relativo do cursor
  - px/mm sensibilidade horizontal e vertical
- Vinculações de teclas (binding)
  - Binding na pressão da ponta da caneta
  - Binding nas Express Key
  - Binding nos botões da caneta
  - Binding nos botões do mouse
  - Binding nas teclas do teclado
  - Binding de plugins externos
- Salvando e carregando informações
  - Carregamento automático pelas configurações do usuário via `settings.json` no usuário ativo no diretório raiz de configurações `%localappdata%` ou `.config`
- Plugins
  - Filtros
  - Modos de saída
  - Ferramentas

# Contribuindo ao OpenTabletDriver

Se você deseja contribuir para o OpenTabletDriver, confira a aba de [problemas](https://github.com/OpenTabletDriver/OpenTabletDriver/issues). Quando criar pull requests, siga as orientações descritas em nosso [guia de contribuição](https://github.com/OpenTabletDriver/OpenTabletDriver/blob/master/CONTRIBUTING.md).

Se você tiver algum problema ou sugestão, [relate um problema](https://github.com/OpenTabletDriver/OpenTabletDriver/issues/new/choose) e preencha o template com informações relevantes.
Somos gratos aos relatos de bugs quanto aos pedidos de novos tablets para adicionar suporte. Em alguns casos, adicionar um novo tablet pode ser fácil.


Para issues e PRs relacionados ao site do OpenTabletDriver [página web](https://opentabletdriver.net), veja este repositório [aqui](https://github.com/OpenTabletDriver/opentabletdriver.github.io).

### Adicionando suporte a um novo tablet

Se você gostaria de adicionar suporte a um novo tablet, abra uma issue ou entre em nosso
[discord](https://discord.gg/9bcMaPkVAR) solicitando o suporte. *Nós geralmente preferimos as solicitações por um novo tablet sejam feitas via discord, devido as trocas de ideias envolvidas*.

Nós precisaremos que você faça algumas coisas como gravar os dados enviados pelo seu tablet utilizando nossa ferramenta de debug integrada, testando funcionalidades do tablet (botões do tablet, botões da caneta, pressão da caneta, etc) com uma configuração diferente quenós iremos enviar a você para testar.

Você também, claro, é bem vindo a abrir uma PR adicionando o suporte a tablet por você mesmo,caso tenha uma boa compreensão do que está desenvolvido.

Geralmente esse processo é relativamente fácil, especialmente se a fabricante do seu tablet já exisitr em algum outro tablet que já tenha suporte.
