#!/usr/bin/env bash

# This is a generic packaging script intended to be used by package maintainers

PKG_FILE="files"
output="$(readlink -f "${1}")"
PREFIX="${PREFIX:-usr}"

if ["${BUILD}" == "true"]; then
  move_to_nested "${output}" "${output}/${PREFIX}/lib/opentabletdriver"
else
  mkdir -p "${output}/${PREFIX}"
fi

copy_generic_files "${output}/${PREFIX}"
mkdir -p "${output}/${PREFIX}/share/doc/opentabletdriver"
cp -v "${REPO_ROOT}/LICENSE" "${output}/${PREFIX}/share/doc/opentabletdriver/LICENSE"

copy_pixmap_assets "${output}/${PREFIX}/share/pixmaps"
copy_manpage "${output}/${PREFIX}/share/man"

if [ "${PREFIX}" == "usr" ]; then
  generate_rules "${output}/usr/lib/udev/rules.d/90-opentabletdriver.rules"
else
  generate_rules "${output}/etc/udev/rules.d/90-opentabletdriver.rules"
fi

generate_desktop_file "${output}/${PREFIX}/share/applications/opentabletdriver.desktop"

if [ "${PREFIX}" != "usr" ]; then
  echo "Patching wrapper scripts to point to '/${PREFIX}/bin/opentabletdriver'..."
  for exe in "${output}/${PREFIX}/bin"/*; do
    sed -i "s|/usr/lib|/${PREFIX}/lib|" "${exe}"
  done

  sed -i "s|/usr/share|/${PREFIX}/share|" "${output}/${PREFIX}/share/applications/opentabletdriver.desktop"
fi

move_to_nested "${output}" "${output}/files"
