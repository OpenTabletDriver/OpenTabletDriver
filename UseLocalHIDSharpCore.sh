#!/usr/bin/env bash

## CONFIG START

HIDSHARPCORE_SRC_PATH="../HIDSharpCore"

OTD_NAME="OpenTabletDriver"

## CONFIG END

# $1 = name
genCsProj() {
  printf "%s/%s.csproj" "$1" "$1"
}

CORE_NAME="$(genCsProj "$OTD_NAME")"

dotnet remove "$CORE_NAME" reference HidSharpCore

HIDSHARPCORE_CSPROJ="${HIDSHARPCORE_SRC_PATH}/$(genCsProj HidSharp)"

dotnet sln $OTDSLN add "$HIDSHARPCORE_CSPROJ"

dotnet add "$CORE_NAME" reference "$HIDSHARPCORE_CSPROJ"
