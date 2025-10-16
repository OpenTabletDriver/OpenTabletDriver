#!/usr/bin/env bash

# This is a packaging script is intended to just provide the barebones files
#
# At the time of writing the script (Avalonia), the following files should be
# emitted: Apps and local dependencies (e.g. Avalonia has Skia), as well as the
# local README and udev rules

PKG_FILE="${OTD_LNAME}-${OTD_VERSION}_${NET_RUNTIME}_simple.tar.gz"
folder_name="${OTD_LNAME}-Simple"

output="$(readlink -f "${1}")"

# strip last slash if present
output="${output%/}"

packager_folder="$(dirname "${BASH_SOURCE[0]}")"

[ "${BUILD}" != "true" ] && exit_with_error "ERR: This package needs build files(?)"

echo "Moving build output to subfolder"
move_to_nested "${output}" "${output}/bin"

cp -v -- "${REPO_ROOT}/LICENSE" "${output}/"
cp -v -- "${packager_folder}/README-SimplePackage.md" "${output}/"

generate_rules "${output}/70-opentabletdriver.rules"
move_to_nested "${output}" "${output}/${folder_name}"
create_binary_tarball "${output}/${folder_name}" "${output}/${PKG_FILE}"
