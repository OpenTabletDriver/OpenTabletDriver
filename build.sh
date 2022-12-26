#!/usr/bin/env bash

# Simple bash script to easily build on linux to verify functionality.
# Uses the same commands as those found in the PKGBUILD for the AUR
# package.

prev_pwd=$(pwd)
runtime=${1:-linux-x64}
config="Release"

shift

options=(--configuration "$config" --verbosity=quiet --self-contained='false' --output='./bin' /p:PublishSingleFile=true /p:DebugType=embedded \
    /p:SuppressNETCoreSdkPreviewMessage=true /p:PublishTrimmed=false --runtime=$runtime -p:SourceRevisionId=$(git rev-parse --short HEAD))

# change dir to script root, in case people run the script outside of the folder
cd "$(dirname "$0")"

# sanity check
if [ ! -d OpenTabletDriver ]; then
    echo "Could not find OpenTabletDriver folder! Please put this script from the root of the OpenTabletDriver repository."
    exit 1
fi

echo "Cleaning old build dirs"
if [ -d ./bin ]; then
    for dir in ./bin/*; do
        if [ "$dir" != "./bin/userdata" ]; then
            rm -rf "$dir"
        fi
    done
fi

dotnet clean --configuration "$config" --verbosity=quiet

echo "Building OpenTabletDriver with runtime $runtime."
mkdir -p ./bin

echo -e "\nBuilding Daemon...\n"
dotnet publish OpenTabletDriver.Daemon ${options[@]} $@ || exit 1

echo -e "\nBuilding Console...\n"
dotnet publish OpenTabletDriver.Console ${options[@]} $@ || exit 2

echo -e "\nBuilding GTK UX...\n"
dotnet publish OpenTabletDriver.UX.Gtk ${options[@]} $@ || exit 3

echo -e "\nBuild finished successfully. Binaries created in ./bin\n"

if [ ! -f /etc/udev/rules.d/??-opentabletdriver.rules ] && [ ! -f /usr/lib/udev/rules.d/??-opentabletdriver.rules ]; then
    echo "Udev rules don't seem to be installed in /etc/udev/rules.d or /usr/lib/udev/rules.d."
    echo "If your distribution installs them elsewhere, ignore this message."
    echo "If not, generate them by running generate-rules.sh, then follow the prompts."
fi

cd "$prev_pwd"
