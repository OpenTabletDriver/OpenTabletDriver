#!/bin/bash

# Simple bash script to easily build on linux to verify functionality.
# Uses the same commands as those found in the PKGBUILD for the AUR
# package.

echo "Building OpenTabletDriver..."

dotnet publish OpenTabletDriver.Daemon        \
  --configuration   Release                   \
  --framework       net5                      \
  --runtime         linux-x64                 \
  --self-contained  false                     \
  --output          "./$_pkgname/out"         \
  /p:SuppressNETCoreSdkPreviewMessage=true    \
  /p:PublishTrimmed=false

dotnet publish OpenTabletDriver.Console       \
  --configuration   Release                   \
  --framework       net5                      \
  --runtime         linux-x64                 \
  --self-contained  false                     \
  --output          "./$_pkgname/out"         \
  /p:SuppressNETCoreSdkPreviewMessage=true    \
  /p:PublishTrimmed=false

dotnet publish OpenTabletDriver.UX.Gtk        \
  --configuration   Release                   \
  --framework       net5                      \
  --runtime         linux-x64                 \
  --self-contained  false                     \
  --output          "./$_pkgname/out"         \
  /p:SuppressNETCoreSdkPreviewMessage=true    \
  /p:PublishTrimmed=false
