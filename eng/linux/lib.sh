#!/usr/bin/env bash

. "$(dirname "${BASH_SOURCE[0]}")/../lib.sh"

### Global variables

PKG_SCRIPT_ROOT="$(readlink -f $(dirname "${BASH_SOURCE[0]}"))"
GENERIC_FILES="$(readlink -f "${PKG_SCRIPT_ROOT}/Generic")"

### Helper functions

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
Version=1.5
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
