#!/usr/bin/env bash

PREFIX="usr/local"

output="${1}"

. "${GENERIC_FILES}/package.sh" "${output}"

PKG_FILE="${OTD_LNAME}-${OTD_VERSION}-x64.tar.gz"

echo "Creating binary tarball '${output}/${PKG_FILE}'..."
mv "${output}/files" "${output}/${OTD_LNAME}"
create_binary_tarball "${output}/${OTD_LNAME}" "${output}/${PKG_FILE}"
