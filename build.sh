#!/usr/bin/env bash

# Simple bash script to easily build on linux to verify functionality.
# Uses the same commands as those found in the PKGBUILD for the AUR
# package.

runtime=${1:-linux-x64}
shift


config=(--configuration='Release')

options=(${config} --framework='net7.0' --self-contained='false' --verbosity=quiet --output='./bin' \
    /p:SuppressNETCoreSdkPreviewMessage=true /p:PublishTrimmed=false --runtime=$runtime -p:SourceRevisionId=$(git rev-parse --short HEAD))

# change dir to script root, in case people run the script outside of the folder
cd "$(dirname "$0")"

# sanity check
if [ ! -d OpenTabletDriver ]; then
    echo "Could not find OpenTabletDriver folder! Chickening out..."
    exit 1
fi

echo "Cleaning old build dirs"
if [ -d ./bin ]; then
    rm -r ./bin
fi
dotnet clean ${config[@]}

echo "Building OpenTabletDriver with runtime $runtime."
mkdir -p ./bin

echo -e "\nBuilding Daemon...\n"
dotnet publish OpenTabletDriver.Daemon ${options[@]} $@ || exit 1

echo -e "\nBuilding Console...\n"
dotnet publish OpenTabletDriver.Console ${options[@]} $@ || exit 2

echo -e "\nBuilding GTK UX...\n"
dotnet publish OpenTabletDriver.UX.Gtk ${options[@]} $@ || exit 3

echo "Build finished successfully. Binaries created in ./bin"

if [ ! -f /etc/udev/rules.d/??-opentabletdriver.rules ] && [ ! -f /usr/lib/udev/rules.d/??-opentabletdriver.rules ]; then
    echo "\nUdev rules don't seem to be installed in /etc/udev/rules.d or /usr/lib/udev/rules.d."
    echo "If your distribution installs them elsewhere, ignore this message."
    echo "If not, generate them by running generate-rules.sh, then follow the prompts."
fi
