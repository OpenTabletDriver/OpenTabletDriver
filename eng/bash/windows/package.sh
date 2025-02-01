#!/usr/bin/env bash

[ "${BUILD}" != "true" ] && exit_with_error "Must build to package MacOS"

pkg_script_root="$(readlink -f $(dirname "${BASH_SOURCE[0]}"))"

echo -e "\nPreparing package..."

PKG_FILE='*'

PKG_FILE_DISPLAY_NAME="${OTD_NAME}-${OTD_VERSION}_${NET_RUNTIME}"
pkg_root="${OUTPUT}/${PKG_FILE_DISPLAY_NAME}"

move_to_nested "${OUTPUT}" "${pkg_root}"

echo "Copying Windows assets"
cp "${pkg_script_root}/convert_to_portable.bat" "${pkg_root}/"

echo "Packaging finished! Loose file contents ready in ${pkg_root}"
