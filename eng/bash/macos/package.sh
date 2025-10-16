#!/usr/bin/env bash

[ "${BUILD}" != "true" ] && exit_with_error "Must build to package MacOS"

pkg_script_root="$(readlink -f $(dirname "${BASH_SOURCE[0]}"))"

echo -e "\nPreparing package..."

PKG_FILE="${OTD_NAME}-${OTD_VERSION}_${NET_RUNTIME}.tar.gz"

pkg_root="${OUTPUT}/${OTD_NAME}.app"

move_to_nested "${OUTPUT}" "${pkg_root}/Contents/MacOS"

echo "Copying MacOS assets..."
mkdir -p "${pkg_root}/Contents/Resources"
cp "${pkg_script_root}/Icon.icns" "${pkg_root}/Contents/Resources/"
cp "${pkg_script_root}/Info.plist" "${pkg_root}/Contents/"

echo "Creating tarball..."
create_binary_tarball "${pkg_root}" "${OUTPUT}/${PKG_FILE}"

echo -e "\nPackaging finished! Package created at '${OUTPUT}/${PKG_FILE}'"
