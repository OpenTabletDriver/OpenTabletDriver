#!/usr/bin/env bash

set -eu
. "$(dirname "${BASH_SOURCE[0]}")/lib.sh"

PACKAGE_GEN=""
PROJECTS=(
  "OpenTabletDriver.Daemon"
  "OpenTabletDriver.Console"
)

MOVE_RULES_TO_ETC="false"

### Argument parsing

print_help() {
  echo "Usage: ${BASH_SOURCE[0]} [OPTIONS]..."
  print_common_arg_help
  echo
  echo "Platform-specific options:"
  echo "  --package <package_type>      Package generation script to run after build"
  echo "                                (see eng/bash/* for available package types)"
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
    extra_args=("${remaining_args[@]}")
    break
  fi

  case "${remaining_args[0]}" in
    --package=*)
      PACKAGE_GEN="${remaining_args[0]#*=}"
      ;;
    --package)
      PACKAGE_GEN="${remaining_args[1]}"
      shift_arr "remaining_args"
      ;;
    --)
      is_extra_args=true
      ;;
    *)
      echo "Unknown option: ${remaining_args[0]}" >&2
      print_help
      exit 1
      ;;
  esac
  shift_arr "remaining_args"
done

if [ -z "${NET_RUNTIME:-}" ]; then
  if is_musl_based_distro; then # is this command even portable?
    NET_RUNTIME="linux-musl-x64"
  else
    NET_RUNTIME="linux-x64"
  fi
  echo "WARN: You must specify a runtime! Falling back to '${NET_RUNTIME}'"
fi

### Set defaults

if [[ "${NET_RUNTIME}" =~ ^win-.*$ ]]; then
  # the following vars are imported from old packaging script
  SINGLE_FILE="true"

  PACKAGE_GEN="${PACKAGE_GEN:-"windows"}"
  PROJECTS+=('OpenTabletDriver.UX.Wpf')
fi

if [[ "${NET_RUNTIME}" =~ ^osx-.*$ ]]; then
  # the following vars are imported from old packaging script
  SINGLE_FILE="false"
  SELF_CONTAINED="true"
  extra_args=("-p:MacBuildBundle=false ${extra_args[@]}")

  PACKAGE_GEN=${PACKAGE_GEN:-"macos"}
  PROJECTS+=('OpenTabletDriver.UX.MacOS')
fi

if [[ "${NET_RUNTIME}" =~ ^linux-.*$ ]]; then
  PROJECTS+=('OpenTabletDriver.UX.Gtk')
fi

### Build, if necessary

cd "${REPO_ROOT}"

prepare_build
if ! [ -e "${LIB_SCRIPT_ROOT}/${PACKAGE_GEN:-BinaryTarBall}/no-build" ]; then
  build "PROJECTS" "extra_args"
fi

### Package, if necessary

if [ -n "${PACKAGE_GEN}" ]; then
  echo -e "\nCreating package with type '${PACKAGE_GEN}'..."
  package_script="${LIB_SCRIPT_ROOT}/${PACKAGE_GEN}/package.sh"
  if [ ! -f "${package_script}" ]; then
    exit_with_error "Could not find package generation script: ${package_script}"
  fi

  echo -e "Running package generation script: ${package_script}\n"

  # child package.sh should expect to be run from the repo root and should write
  # the filename of the package generated to PKG_FILE
  . "${package_script}" "${OUTPUT}"

  echo -e "\nPackaging finished! Package created at '${OUTPUT}/${PKG_FILE}'"

  # output information to CI
  if [ -n "${GITHUB_OUTPUT:-}" ]; then
    printf 'output-file=%s\n' "$PKG_FILE" >> $GITHUB_OUTPUT
    printf 'version=%s\n' "$OTD_VERSION" >> $GITHUB_OUTPUT

    # allow packaging scripts to display a custom artifact name on CI
    if [ -n "${PKG_FILE_DISPLAY_NAME:-}" ]; then
      display_name="${PKG_FILE_DISPLAY_NAME}"
    else
      display_name="${PKG_FILE}"
    fi

    printf 'output-file-display-name=%s\n' "$display_name" >> $GITHUB_OUTPUT

    echo "Set values in GITHUB_OUTPUT"
  fi
fi
