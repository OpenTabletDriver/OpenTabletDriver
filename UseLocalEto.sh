#!/usr/bin/env bash
# TODO: macOS is not converted yet, Eto macOS doesn't build on non-macOS

## CONFIG START

ETO_SRC_PATH="../Eto/src"

OTD_UXNAME="OpenTabletDriver.UX"
OTD_GTKNAME="${OTD_UXNAME}.Gtk"
OTD_WPFNAME="${OTD_UXNAME}.Wpf"
OTD_MACOSNAME="${OTD_UXNAME}.MacOS"
OTD_SLN="OpenTabletDriver.sln"

## CONFIG END

# $1 = name
genCsProj() {
  printf "%s/%s.csproj" "$1" "$1"
}

UX_CSPROJ="$(genCsProj "$OTD_UXNAME")"
UX_GTK_CSPROJ="$(genCsProj "$OTD_GTKNAME")"
UX_WPF_CSPROJ="$(genCsProj "$OTD_WPFNAME")"
UX_MACOS_CSPROJ="$(genCsProj "$OTD_MACOSNAME")"

dotnet remove "$UX_CSPROJ" reference Eto.Forms
dotnet remove "$UX_GTK_CSPROJ" reference Eto.Platform.Gtk
dotnet remove "$UX_WPF_CSPROJ" reference Eto.Platform.Wpf
#dotnet remove "$UX_MACOS_CSPROJ" reference Eto.Platform.Mac64

ETO_CSPROJ="${ETO_SRC_PATH}/Eto/Eto.csproj"
ETO_GTK_CSPROJ="${ETO_SRC_PATH}/Eto.Gtk/Eto.Gtk.csproj"
ETO_WPF_CSPROJ="${ETO_SRC_PATH}/Eto.Wpf/Eto.Wpf.csproj"
#ETO_MACOS_CSPROJ="${ETO_SRC_PATH}/Eto.Mac/Eto.Mac64.csproj"

dotnet sln $OTDSLN add "$ETO_CSPROJ" "$ETO_GTK_CSPROJ" "$ETO_WPF_CSPROJ" # "$ETO_MACOS_CSPROJ"

dotnet add "$UX_CSPROJ" reference "$ETO_CSPROJ"
dotnet add "$UX_GTK_CSPROJ" reference "$ETO_GTK_CSPROJ"
dotnet add "$UX_WPF_CSPROJ" reference "$ETO_WPF_CSPROJ"
#dotnet add "$UX_MACOS_CSPROJ" reference "$ETO_MACOS_CSPROJ"
