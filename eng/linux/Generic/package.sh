#!/usr/bin/env bash

# This is a generic packaging script intended to be used by package maintainers

PKG_FILE="files"
output="$(readlink -f "${1}")"

move_to_nested "${output}" "${output}/usr/lib/opentabletdriver"

copy_generic_files "${output}"
generate_rules "${output}/usr/lib/udev/rules.d/90-opentabletdriver.rules"
generate_desktop_file "${output}/usr/share/applications/opentabletdriver.desktop"
copy_pixmap_assets "${output}/usr/share/pixmaps"
copy_manpage "${output}/usr/share/man/man8"

move_to_nested "${output}" "${output}/files"
