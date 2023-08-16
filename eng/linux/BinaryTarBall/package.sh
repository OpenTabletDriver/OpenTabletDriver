#!/usr/bin/env bash

PKG_FILE="${OTD_LNAME}-${OTD_VERSION}-x64.tar.gz"

output="${1}"

move_to_nested "${output}" "${output}/usr/local/lib/opentabletdriver"

copy_generic_files "${output}"
mv "${output}/usr/lib"/* "${output}/usr/local/lib"
mkdir -p "${output}/etc/"
mv "${output}/usr/local/lib/systemd" "${output}/etc"

echo "Patching wrapper scripts to point to '/usr/local/lib/opentabletdriver'..."
mkdir -p "${output}/usr/local/bin"
for exe in "${output}/usr/bin"/*; do
  sed -i "s|/usr/lib|/usr/local/lib|" "${exe}"
  mv "${exe}" "${output}/usr/local/bin/${exe##*/}"
done

echo "Removing unused directories..."
rmdir "${output}/usr/bin"
rmdir "${output}/usr/lib"

generate_rules "${output}/etc/udev/rules.d/99-opentabletdriver.rules"
generate_desktop_file "${output}/usr/local/share/applications/opentabletdriver.desktop"
sed -i "s|/usr/share|/usr/local/share|" "${output}/usr/local/share/applications/opentabletdriver.desktop"
copy_pixmap_assets "${output}/usr/local/share/pixmaps"

echo "Creating binary tarball '${output}/${PKG_FILE}'..."

move_to_nested "${output}" "${output}/${OTD_LNAME}"
create_binary_tarball "${output}/${OTD_LNAME}" "${output}/${PKG_FILE}"
