[![GitHub Actions Status](https://github.com/OpenTabletDriver/OpenTabletDriver/actions/workflows/dotnet.yml/badge.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/actions/workflows/dotnet.yml) [![Total Download Count](https://img.shields.io/github/downloads/OpenTabletDriver/OpenTabletDriver/total.svg)](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/latest)

# OpenTabletDriver

English | [한국어](docs/README_KO.md) | [Español](docs/README_ES.md) | [Русский](docs/README_RU.md) | [简体中文](docs/README_CN.md) | [Français](docs/README_FR.md) | [Deutsch](docs/README_DE.md) | Português-BR

OpenTabletDriver é um programa de código aberto, multi-plataforma, driver de mesas digitalizadoras (tablets) em modo de usuário. O objetivo do OpenTabletDriver é ser compatível em múltiplas plataformas em uma interface gráfica amigável e de fácil utilização para o usuário.

<p align="middle">
  <img src="https://i.imgur.com/XDYf62e.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/jBW8NpU.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/ZLCy6wz.png" width="410" align="middle"/>
</p>

# Tablets compatíveis

Todos os tablets que são compatíveis, não testados ou que estão em planejamento para testes podem ser vistos aqui.

- [Tablets Compatíveis](https://opentabletdriver.net/Tablets)

# Instalação

- [Windows](https://opentabletdriver.net/Wiki/Install/Windows)
- [Linux](https://opentabletdriver.net/Wiki/Install/Linux)
- [MacOS](https://opentabletdriver.net/Wiki/Install/MacOS)

# Soluções de problemas

Por favor, consulte a [wiki oficial](https://opentabletdriver.net/Wiki) para problemas comuns e outras peculiaridades.

# Executando OpenTabletDriver

O OpenTabletDriver funciona como dois processos separados que comunicam-se entre si para poder funcionar perfeitamente. O programa ativo que lida com todo o manuseio de dados é `OpenTabletDriver.Daemon`, enquanto o GUI (interface) é `OpenTabletDriver.UX.*`, onde `*` depende da plataforma<sup>1</sup>. O daemon deve ser inicializado para que tudo possa rodar sem problemas, enquanto o GUI não é necessário. Se você possui uma configuração já pronta, elas devem ser aplicadas quando o daemon iniciar.

> <sup>1</sup>Windows usa `Wpf`, Linux usa `Gtk`, e MacOS usa `MacOS`. O que pode ser ignorado por grande parte da aplicação se você não compilar a partir da fonte, já que apenas a versão correta será fornecida.

## Compilando OpenTabletDriver a partir do codigo-fonte

Os requisitos para compilar o OpenTabletDriver são referentes à todas as plataformas. Cada plataforma requer dependências diferentes.

### Todas as plataformas

- .NET 10 SDK (pode ser obtida [aqui](https://dotnet.microsoft.com/download/dotnet/10.0) - Você precisa do SDK para sua plataforma, usuários Linux devem instalar via gerenciador de pacotes se possível)

#### Windows

Execute `build.sh windows` para produzir as compilações binárias na pasta 'bin'. Estas compilações estão executadas no modo portátil por padrão.

Caso você nao tenha WSL ou alguma outra forma de acessar o BASH com uma instalacao funcional do dotnet, o script de compilação legado do windows ainda existe em `build.ps1`.

#### Linux

Pacotes Necessários (alguns pacotes podem vir pre-instalados de acordo com sua distribuição):

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

#### MacOS

Sem outras dependências.

# Funcionalidades

- Interface totalmente nativa
  - Windows: `Windows Presentation Foundation`
  - Linux: `GTK+3`
  - MacOS: `MonoMac`
- Suporte a múltiplos tablets
  - lida com múltiplos tablets de uma vasta seleção de modelos e fabricantes, cada um com seu próprio fluxo de plugins e configurações
- Especificações de tablets validadas, garantindo a transição mais suave possível ao trocar de dispositivo
- Ferramenta de console completa
  - Obtenha, altere, carregue ou salve configurações rapidamente
  - Suporte a scripts (saída em JSON)
- Posicionamento do cursor absoluto
  - Área da tela e área do tablet
  - Deslocamentos (offsets) ancorados ao centro
  - Rotação de área precisa
- Posicionamento relativo do cursor
  - Sensibilidade horizontal e vertical em px/mm
- Compatibilidade com recursos avançados de tablet
  - Sensibilidade à pressão
    - Windows: Necessita do plugin Windows Ink OpenTabletDriver para o modo de saída Windows Ink e o driver de sistema VMulti
    - Linux: Suportado nativamente com o modo de saída "Linux Artist Mode"
    - MacOS: Compatível nativamente em todos os modos de saída
  - Inclinação da caneta
  - Rodas e seletores da mesa digitalizadora
  - Teclas Auxiliares/Express
- Atalhos da caneta
  - Atalho da ponta por pressão
  - Atalhos de teclas Express ("Aux") 
  - Atalhos dos botões da caneta
  - Atalhos de botões do mouse
  - Atalhos de scroll do mouse
  - Atalhos de teclado
  - Predefinições de atalhos
  - Atalhos de plugins externos
- Salvando e carregando configurações
  - Configurações persistentes
  - Predefinições para acesso rápido a configurações salvas anteriormente
- Plugins
  - Gerenciador de Plugins (via
    [Repositório de Plugins](https://github.com/OpenTabletDriver/Plugin-Repository))
  - Filtros, incluindo filtros assíncronos (interpoladores)
  - Modos de saída
- Ferramentas de depuração de dispositivo
  - Analisador de dados da mesa digitalizadora ("Tablet Debugger")
  - Leitura de informações (strings) do dispositivo USB
- Notificação automática de atualização de versão
  - Pode ser desativado com a flag de linha de comando `--skipupdate`
- Conversão de áreas de drivers de fabricantes
  - Suporta a transformação da sua área de mesas Wacom / XP-Pen / Huion / Gaomon / VEIKK
- Daemon independente, para sistemas de baixo desempenho ou sem interface gráfica (headless)

# Contribuindo com o OpenTabletDriver

Se você deseja contribuir com o OpenTabletDriver, dê uma olhada nas [issues](https://github.com/OpenTabletDriver/OpenTabletDriver/issues). Ao criar Pull Requests, siga as orientações descritas em nossas
[diretrizes de contribuição](CONTRIBUTING.md).

Se você tiver qualquer problema ou sugestão, [abra uma issue
](https://github.com/OpenTabletDriver/OpenTabletDriver/issues/new/choose)
e preencha o modelo com as informações relevantes. Aceitamos tanto relatos de bugs quanto suporte para novos tablets.
Em muitos casos, adicionar suporte para um novo tablet é bem fácil.

Para issues e PRs relacionados à [página web](https://opentabletdriver.net) do OpenTabletDriver, veja o repositório [aqui](https://github.com/OpenTabletDriver/opentabletdriver.github.io).

### Adicionando suporte para um novo tablet

Se você quiser que adicionemos suporte para um novo tablet, abra uma issue ou entre no nosso
[discord](https://discord.gg/9bcMaPkVAR)  pedindo ajuda. Geralmente preferimos que o suporte para novos tablets seja feito via Discord,
devido à necessidade de troca de informações.

Nós pediremos para você fazer algumas coisas,
como gravar os dados enviados pelo seu tablet usando nossa ferramenta de depuração interna e testar recursos da mesa (botões físicos, botões da caneta, pressão, etc.) com diferentes configurações que enviaremos para você testar.

Você também é bem-vindo para abrir um PR adicionando o suporte por conta própria, caso tenha uma boa noção do que está envolvido.

Geralmente, esse processo é relativamente fácil, especialmente se for de um fabricante que já suportamos em outros modelos.
