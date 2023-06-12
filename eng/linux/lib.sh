#!/usr/bin/env bash

set -eu

### Input Environment Variables

VERSION_SUFFIX=${VERSION_SUFFIX:-}

### Global variables

REPO_ROOT="$(readlink -f "$(dirname "${BASH_SOURCE[0]}")/../../")"
GENERIC_FILES="$(readlink -f "$(dirname "${BASH_SOURCE[0]}")/Generic")"

### Global Descriptors

OTD_NAME="OpenTabletDriver"
OTD_LNAME="opentabletdriver"

OTD_DESC="cross-platform open-source tablet driver"

OTD_LONG_DESC="${OTD_NAME} has the highest number of supported tablets with great
compatibility across multiple platforms, packaged in an easy-to-use graphical
user interface."

OTD_LONG_DESC2="${OTD_NAME} has support for multiple tablets from the following (non-exhaustive) OEMs:
 * Wacom
 * Huion
 * XP-Pen
 * XenceLabs
 * Gaomon
 * Veikk"

OTD_MAINTAINERS=(
  "InfinityGhost <infinityghostgit@gmail.com>" \
  "X9VoiD <oscar.silvestrexx@gmail.com>" \
)
OTD_UPSTREAM_URL="https://opentabletdriver.net"
OTD_REPO_URL="https://github.com/OpenTabletDriver/OpenTabletDriver"
OTD_GIT="https://github.com/OpenTabletDriver/OpenTabletDriver.git"
OTD_VERSION_BASE="$(sed -n 's|.*<VersionBase>\(.*\)</VersionBase>.*|\1|p' "${REPO_ROOT}/Directory.Build.props")"
OTD_VERSION="${OTD_VERSION_BASE}${VERSION_SUFFIX}"

OTD_DAEMON="otd-daemon"
OTD_GUI="otd-gui"
OTD_CLI="otd"

### Helper functions

create_binary_tarball() {
  local source="${1}"
  local output="${2}"

  local last_pwd="${PWD}"

  output="$(readlink -f "${output}")"
  cd "${source}/.."
  tar -czf "${output}" "$(basename ${source})"

  cd "${last_pwd}"
}

move_to_nested() {
  local source="${1}"
  local nested="${2}"

  local contents="$(echo "${source}"/*)"
  mkdir -p "${nested}"
  mv ${contents} "${nested}"
}

generate_rules() {
  local output_file="${1}"

  echo "Generating udev rules..."
	"${REPO_ROOT}/generate-rules.sh" -v "${REPO_ROOT}/OpenTabletDriver.Configurations/Configurations" "${output_file}" > /dev/null
}

generate_desktop_file() {
  local output="${1}"

  mkdir -p "$(dirname "${output}")"
  cat << EOF > "${output}"
[Desktop Entry]
Version=${OTD_VERSION}
Name=${OTD_NAME}
Comment=A ${OTD_DESC}
Exec=${OTD_GUI}
Icon=/usr/share/pixmaps/otd.png
Terminal=false
Type=Application
Categories=Settings;
StartupNotify=true
EOF
}

copy_pixmap_assets() {
  local output_folder="${1}"

  echo "Copying pixmap assets to '${output_folder}'..."
  mkdir -p "${output_folder}"
  cp "${REPO_ROOT}/OpenTabletDriver.UX/Assets"/* "${output_folder}"
}
