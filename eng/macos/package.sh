#!/usr/bin/env bash

set -eu
. "$(dirname "${BASH_SOURCE[0]}")/../lib.sh"

NET_RUNTIME="osx-x64"
SINGLE_FILE="false"
SELF_CONTAINED="true"

PKG_SCRIPT_ROOT="$(readlink -f $(dirname "${BASH_SOURCE[0]}"))"
PACKAGE="false"
PROJECTS=(
  "OpenTabletDriver.Daemon"
  "OpenTabletDriver.Console"
  "OpenTabletDriver.UX.MacOS"
)

print_help() {
  echo "Usage: ${BASH_SOURCE[0]} [OPTIONS]..."
  print_common_arg_help
  echo
  echo "Platform-specific options:"
  echo "  --package <bool>      Whether to generate a package after build"
  echo
  echo "Remarks:"
  echo "  Anything after '--', if it is specified, will be passed to dotnet publish as-is."
}

args=("$@")          # copy args
remaining_args=()    # remaining args after parsing
is_extra_args=false  # whether we've reached '--'
extra_args=()        # args after '--'

parse_build_args "args" "remaining_args"
PKG_FILE="${OTD_LNAME}-${OTD_VERSION}-${NET_RUNTIME}.app.tar.gz"

while [ ${#remaining_args[@]} -gt 0 ]; do
  if $is_extra_args; then
    extra_args=("$@")
    break
  fi

  case "$1" in
    --package=*)
      PACKAGE="${1#*=}"
      ;;
    --package)
      PACKAGE="$2"
      shift_arr "remaining_args"
      ;;
    --)
      is_extra_args=true
      ;;
    *)
      echo "Unknown option: $1"
      print_help
      exit 1
      ;;
  esac
  shift_arr "remaining_args"
done

cd "${REPO_ROOT}"

prepare_build
build "PROJECTS" "extra_args"

if [ "${PACKAGE}" = "true" ]; then
  echo -e "\nPreparing package..."

  pkg_file="${OTD_NAME}-${OTD_VERSION}-${NET_RUNTIME}.tar.gz"
  pkg_root="${OUTPUT}/${OTD_NAME}.app"

  move_to_nested "${OUTPUT}" "${pkg_root}/Contents/MacOS"

  echo "Copying MacOS assets..."
  mkdir -p "${pkg_root}/Contents/Resources"
  cp "${PKG_SCRIPT_ROOT}/Icon.icns" "${pkg_root}/Contents/Resources/"
  cp "${PKG_SCRIPT_ROOT}/Info.plist" "${pkg_root}/Contents/"

  echo "Creating tarball..."
  create_binary_tarball "${pkg_root}" "${OUTPUT}/${pkg_file}"

  echo -e "\nPackaging finished! Package created at '${OUTPUT}/${pkg_file}'"
fi
