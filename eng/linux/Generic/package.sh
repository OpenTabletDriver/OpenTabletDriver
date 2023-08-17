#!/usr/bin/env bash

. "$(dirname "${BASH_SOURCE[0]}")/../lib.sh"

# This is a generic packaging script intended to be used by package maintainers

output="$(readlink -f "${1}")"
PREFIX="${PREFIX:-/usr/local/}"

mkdir -p "${output}/${PREFIX}"
copy_generic_files "${output}/${PREFIX}"
mkdir -p "${output}/${PREFIX}/share/doc/opentabletdriver"
cp -v "${REPO_ROOT}/LICENSE" "${output}/${PREFIX}/share/doc/opentabletdriver/LICENSE"

generate_rules "${output}/${PREFIX}/lib/udev/rules.d/90-opentabletdriver.rules"
generate_desktop_file "${output}/${PREFIX}/share/applications/opentabletdriver.desktop"
copy_pixmap_assets "${output}/${PREFIX}/share/pixmaps"
copy_manpage "${output}/${PREFIX}/share/man"
