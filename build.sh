#!/usr/bin/env bash

# Simple bash script to easily build on linux to verify functionality.
# Uses the same commands as those found in the PKGBUILD for the AUR
# package.

runtime=${1:-linux-x64}

options=(--configuration='Release' --framework='net5' --self-contained='false' --output='./bin' /p:SuppressNETCoreSdkPreviewMessage=true /p:PublishTrimmed=false --runtime=$runtime)

echo "Building OpenTabletDriver with runtime $runtime."
mkdir ./bin

echo -e "\nBuilding Daemon...\n"
dotnet publish OpenTabletDriver.Daemon ${options[@]}

echo -e "\nBuilding Console...\n"
dotnet publish OpenTabletDriver.Console ${options[@]}

echo -e "\nBuilding GTK UX...\n"
dotnet publish OpenTabletDriver.UX.Gtk ${options[@]}

echo "Build finished. Binaries created in ./bin"
