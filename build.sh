#!/bin/bash

# Simple bash script to easily build on linux to verify functionality.
# Uses the same commands as those found in the PKGBUILD for the AUR
# package.

runtime=${1:-linux-x64}

options="--configuration Release --framework net5 --self-contained false --output \"./bin\" /p:SuppressNETCoreSdkPreviewMessage=true /p:PublishTrimmed=false"

echo "Building OpenTabletDriver with runtime $runtime."
mkdir ./bin

echo -e "\nBuilding Daemon...\n"
dotnet publish OpenTabletDriver.Daemon $options --runtime $runtime

echo -e "\nBuilding Console...\n"
dotnet publish OpenTabletDriver.Console $options --runtime $runtime

echo -e "\nBuilding GTK UX...\n"
dotnet publish OpenTabletDriver.UX.Gtk $options --runtime $runtime

echo "Build finished. Binaries created in './bin'"

chmod +x ./bin/OpenTabletDriver.Daemon
chmod +x ./bin/OpenTabletDriver.Console
chmod +x ./bin/OpenTabletDriver.UX.Gtk

echo "Binaries made executable. [DONE]"
