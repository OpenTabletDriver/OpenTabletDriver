#!/usr/bin/env bash

set -eu
. "$(dirname "${BASH_SOURCE[0]}")/lib.sh"

NET_RUNTIME="linux-x64"

PACKAGE_GEN=""
PROJECTS=(
  "OpenTabletDriver.Daemon"
  "OpenTabletDriver.Console"
  "OpenTabletDriver.UX.Gtk"
)

print_help() {
  echo "Usage: ${BASH_SOURCE[0]} [OPTIONS]..."
  print_common_arg_help
  echo
  echo "Platform-specific options:"
  echo "  --package <package_type>      Package generation script to run after build"
  echo "                                (see eng/linux/* for available package types)"
  echo
  echo "Remarks:"
  echo "  Anything after '--', if it is specified, will be passed to dotnet publish as-is."
}

args=("$@")          # copy args
remaining_args=()    # remaining args after parsing
is_extra_args=false  # whether we've reached '--'
extra_args=()        # args after '--'

parse_build_args "args" "remaining_args"

while [ ${#remaining_args[@]} -gt 0 ]; do
  if $is_extra_args; then
    extra_args=("$@")
    break
  fi

  case "$1" in
    --package=*)
      PACKAGE_GEN="${1#*=}"
      ;;
    --package)
      PACKAGE_GEN="$2"
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

if [ -n "${PACKAGE_GEN}" ]; then
  echo -e "\nCreating package with type '${PACKAGE_GEN}'..."
  package_script="${PKG_SCRIPT_ROOT}/${PACKAGE_GEN}/package.sh"
  if [ ! -f "${package_script}" ]; then
    exit_with_error "Could not find package generation script: ${package_script}"
  fi

  echo -e "Running package generation script: ${package_script}\n"

  # child package.sh should expect to be run from the repo root and should write
  # the filename of the package generated to PKG_FILE
  . "${package_script}" "${OUTPUT}"

  echo -e "\nPackaging finished! Package created at '${OUTPUT}/${PKG_FILE}'"
fi
