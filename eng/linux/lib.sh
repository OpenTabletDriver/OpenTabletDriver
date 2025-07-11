#!/usr/bin/env bash

. "$(dirname "${BASH_SOURCE[0]}")/../lib.sh"

### Global variables

PKG_SCRIPT_ROOT="$(readlink -f $(dirname "${BASH_SOURCE[0]}"))"
GENERIC_FILES="$(readlink -f "${PKG_SCRIPT_ROOT}/Generic")"

### Helper functions

# From https://github.com/dotnet/install-scripts/blob/main/src/dotnet-install.sh
is_musl_based_distro() {
  (ldd --version 2>&1 || true) | grep -q musl
}

copy_generic_files() {
  local output="${1}"

  echo "Copying generic files..."
  cp -Rv "${GENERIC_FILES}/usr/"* "${output}"
  echo
}

test_rules() {
  if ! hash udevadm 2>/dev/null; then
    echo "INFO: test_rules: Cannot test rules without program 'udevadm'. Passing."
    return 0
  fi

  if ! udevadm verify --help >/dev/null; then
    echo "INFO: test_rules: Your udevadm does not support 'udevadm verify'. Passing."
    return 0
  fi

  if [ ! -f "${1}" ]; then
    echo "test_rules: Not a file '${1}'" >&2
    return 1
  fi

  udevadm verify "${1}"
}

generate_rules() {
  local output_file="${1}"

  echo "Generating udev rules to ${output_file}..."
  mkdir -p $(dirname "$output_file")
  "${REPO_ROOT}/generate-rules.sh" > "${output_file}"

  test_rules "$output_file"
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
StartupWMClass=OpenTabletDriver.UX
EOF
}
