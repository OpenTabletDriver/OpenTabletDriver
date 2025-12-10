{ lib
, buildDotnetModule
, fetchFromGitHub
, fetchurl
, gtk3
, libX11
, libXrandr
, libappindicator
, libevdev
, libnotify
, udev
, copyDesktopItems
, makeDesktopItem
, nixosTests
, wrapGAppsHook3
, dpkg
, dotnetCorePackages

, strace

, git
, jq
, coreutils
, gawk
, gnused
, bash
}:

buildDotnetModule rec {
  pname = "OpenTabletDriver";
  version = "0.6.6.2";

  src = ./.;

  dotnet-sdk = dotnetCorePackages.sdk_10_0;
  dotnet-runtime = dotnetCorePackages.runtime_10_0;

  dotnetInstallFlags = [ "--framework=net10.0" ];

  projectFile = [ "OpenTabletDriver.Console" "OpenTabletDriver.Daemon" "OpenTabletDriver.UX.Gtk" ];
  nugetDeps = ./deps.json;

  executables = [ "OpenTabletDriver.Console" "OpenTabletDriver.Daemon" "OpenTabletDriver.UX.Gtk" ];

  nativeBuildInputs = [
    copyDesktopItems
    wrapGAppsHook3
    dpkg
    jq
    coreutils
    gawk
    gnused
    bash
  ];

  runtimeDeps = [
    gtk3
    libX11
    libXrandr
    libappindicator
    libevdev
    libnotify
    udev
  ];

  buildInputs = runtimeDeps;

  postFixup = ''
    # Give a more "*nix" name to the binaries
    mv $out/bin/OpenTabletDriver.Console $out/bin/otd
    mv $out/bin/OpenTabletDriver.Daemon $out/bin/otd-daemon
    mv $out/bin/OpenTabletDriver.UX.Gtk $out/bin/otd-gui

    install -Dm644 $src/OpenTabletDriver.UX/Assets/otd.png -t $out/share/pixmaps

    # Generate udev rules from source
    export OTD_CONFIGURATIONS="$src/OpenTabletDriver.Configurations/Configurations"

    mkdir -p $out/lib/udev/rules.d
    bash $src/generate-rules.sh \
      | sed 's@/usr/bin/env rm@${lib.getExe' coreutils "rm"}@' \
      > $out/lib/udev/rules.d/70-opentabletdriver.rules
  '';

  desktopItems = [
    (makeDesktopItem {
      desktopName = "OpenTabletDriver";
      name = "OpenTabletDriver";
      exec = "otd-gui";
      icon = "otd";
      comment = "Open source, cross-platform, user-mode tablet driver";
      categories = [ "Utility" ];
    })
  ];
}
