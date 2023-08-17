#!/usr/bin/env bash

PKG_FILE="${OTD_LNAME}-${OTD_VERSION}-x64.tar.gz"

output="${1}"

move_to_nested "${output}" "${output}/build"

env PREFIX="/usr/local" "${GENERIC_FILES}/package.sh" "${output}"
mkdir -p "${output}/usr/local/lib"
mv "${output}/build" "${output}/usr/local/lib/opentabletdriver"

echo "Patching wrapper scripts to point to '/usr/local/lib/opentabletdriver'..."
for exe in "${output}/usr/local/bin"/*; do
  sed -i "s|/usr/lib|/usr/local/lib|" "${exe}"
done

mkdir -p "${output}/etc/udev/rules.d/"
mv "${output}/usr/local/lib/udev/rules.d/90-opentabletdriver.rules" "${output}/etc/udev/rules.d/90-opentabletdriver.rules"
rm -r "${output}/usr/local/lib/udev/"
sed -i "s|/usr/share|/usr/local/share|" "${output}/usr/local/share/applications/opentabletdriver.desktop"

echo "Creating binary tarball '${output}/${PKG_FILE}'..."

move_to_nested "${output}" "${output}/${OTD_LNAME}"
create_binary_tarball "${output}/${OTD_LNAME}" "${output}/${PKG_FILE}"
