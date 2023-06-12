#!/usr/bin/env bash

PKG_FILE="${OTD_LNAME}-${OTD_VERSION}-x64.tar.gz"

script_root="$(readlink -f $(dirname "${BASH_SOURCE[0]}"))"

output="${1}"
netRuntime="${2}"

move_to_nested "${output}" "${output}/lib/opentabletdriver"

echo "Copying generic files..."
cp -R "${GENERIC_FILES}"/* "${output}/"
mkdir -p "${output}/etc/"
mv "${output}/usr/lib"/* "${output}/etc/"

echo "Patching wrapper scripts to point to '/lib/opentabletdriver'..."
mkdir -p "${output}/bin"
for exe in "${output}/usr/bin"/*; do
  sed -i "s|#!/usr/bin/env sh|#!/bin/sh|" "${exe}"
  sed -i "s|/usr/lib|/lib|" "${exe}"
  mv "${exe}" "${output}/bin/${exe##*/}"
done

echo "Removing unused directories..."
rmdir "${output}/usr/bin"
rmdir "${output}/usr/lib"
rmdir "${output}/usr"

generate_rules "${output}/etc/udev/rules.d/99-opentabletdriver.rules"
generate_desktop_file "${output}/share/applications/opentabletdriver.desktop"
sed -i "s|/usr/share|/share|" "${output}/share/applications/opentabletdriver.desktop"
copy_pixmap_assets "${output}/share/pixmaps"

echo "Creating binary tarball '${output}/${PKG_FILE}'..."

move_to_nested "${output}" "${output}/${OTD_LNAME}"
create_binary_tarball "${output}/${OTD_LNAME}" "${output}/${PKG_FILE}"
