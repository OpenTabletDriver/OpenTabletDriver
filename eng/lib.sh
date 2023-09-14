#!/usr/bin/env bash

# This script contains variables and functions that are shared between systems
# using bash shell. Should be sourced by other scripts early on.

set -eu

### Input Environment Variables

VERSION_SUFFIX=${VERSION_SUFFIX:-}

### Global variables

PREV_PATH=${PWD}
ENG_SCRIPT_ROOT="$(readlink -f $(dirname "${BASH_SOURCE[0]}"))"
REPO_ROOT="$(readlink -f "${ENG_SCRIPT_ROOT}/../")"

### Build Requirements

DOTNET_VERSION="7.0"

# could do away with declare -g, but did it anyway for all of them for consistency
# with NET_RUNTIME (a global variable without initial value in lib.sh)
declare -g OUTPUT="dist"
declare -g CONFIG="Release"
declare -g FRAMEWORK="net7.0"
declare -g NET_RUNTIME
declare -g DOG_FOOD="true"
declare -g BUILD="true"
declare -g PORTABLE="false"
declare -g SINGLE_FILE="true"
declare -g SELF_CONTAINED="false"

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

### Automatically handle errors and exit

handle_error() {
  echo "Build failed!"
  cd "${PREV_PATH}"
  exit 1
}

handle_exit() {
  cd "${PREV_PATH}"
}

trap handle_error ERR
trap handle_exit EXIT

### Helper functions

exit_with_error() {
  echo "$1"
  handle_error
}

parse_build_args() {
  local -n _args="$1"
  local args=("${_args[@]}") # copy args to local array
  local -n remaining_options="$2"

  while [ ${#args[@]} -gt 0 ]; do
    case "${args[0]}" in
      -o=*|--output=*)
        OUTPUT="${args[0]#*=}"
        ;;
      -o|--output)
        OUTPUT="${args[1]}"
        shift_arr "args"
        ;;
      -c=*|--configuration=*)
        CONFIG="${args[0]#*=}"
        ;;
      -c|--configuration)
        CONFIG="${args[1]}"
        shift_arr "args"
        ;;
      -f=*|--framework=*)
        FRAMEWORK="${args[0]#*=}"
        ;;
      -f|--framework)
        FRAMEWORK="${args[1]}"
        shift_arr "args"
        ;;
      -r=*|--runtime=*)
        NET_RUNTIME="${args[0]#*=}"
        ;;
      -r|--runtime)
        NET_RUNTIME="${args[1]}"
        shift_arr "args"
        ;;
      --dog-food=*)
        DOG_FOOD="${args[0]#*=}"
        ;;
      --dog-food)
        DOG_FOOD="${args[1]}"
        shift_arr "args"
        ;;
      --build=*)
        BUILD="${args[0]#*=}"
        ;;
      --build)
        BUILD="${args[1]}"
        shift_arr "args"
        ;;
      --portable=*)
        PORTABLE="${args[0]#*=}"
        ;;
      --portable)
        PORTABLE="${args[1]}"
        shift_arr "args"
        ;;
      --single-file=*)
        SINGLE_FILE="${args[0]#*=}"
        ;;
      --single-file)
        SINGLE_FILE="${args[1]}"
        shift_arr "args"
        ;;
      --self-contained=*)
        SELF_CONTAINED="${args[0]#*=}"
        ;;
      --self-contained)
        SELF_CONTAINED="${args[1]}"
        shift_arr "args"
        ;;
      *)
        remaining_options+=("${args[0]}")
        ;;
    esac
    shift_arr "args"
  done
}

print_common_arg_help() {
  echo "Options:"
  echo "  -o, --output <path>           Output directory for build artifacts (default: ${OUTPUT})"
  echo "  -c, --configuration <config>  Build configuration (default: ${CONFIG})"
  echo "  -f, --framework <framework>   Target framework (default: ${FRAMEWORK})"
  echo "  -r, --runtime <runtime>       Target runtime (default: ${NET_RUNTIME:-})"
  echo "  --dog-food <bool>             Whether to output dogfood binaries (default: ${DOG_FOOD})"
  echo "  --build <bool>                Whether to build binaries (default: ${BUILD})"
  echo "  --portable <bool>             Whether to build portable binaries (default: ${PORTABLE})"
  echo "  --single-file <bool>          Whether to build single-file binaries (default: ${SINGLE_FILE})"
  echo "  --self-contained <bool>       Whether to build self-contained binaries (default: ${SELF_CONTAINED})"
  echo "  -h, --help                    Print this help message"
}

shift_arr() {
  local -n arr="$1"
  local shift="${2:-1}"

  arr=("${arr[@]:$shift}")
}

# need to call `parse_build_args` before calling this function
prepare_build() {
  if [ ! -d "OpenTabletDriver" ]; then
    exit_with_error "Could not find OpenTabletDriver folder!"
  fi

  if [ -d "${OUTPUT}" ]; then
    echo "Cleaning old build outputs..."
    for dir in "${OUTPUT}"/*; do
      dir_name=$(basename "${dir}")
      if [ "${dir_name}" != "userdata" ]; then
        if ! rm -rf "${dir}" 2>/dev/null; then
          exit_with_error "Could not clean old build dirs. Please manually remove contents of ${OUTPUT} folder."
        fi
      fi
    done
  fi

  mkdir -p "${OUTPUT}"
}

# need to call `parse_build_args` before calling this function
build() {
  if [ "${BUILD}" != "true" ]; then
    echo "Skipping build..."
    return
  fi

  local -n projects="$1"
  local -n extra_options="$2"
  local options=(
    --configuration "${CONFIG}"
    --runtime "${NET_RUNTIME}"
    --framework "${FRAMEWORK}"
    --self-contained "${SELF_CONTAINED}"
    --output "${OUTPUT}"
    /p:PublishTrimmed=false
    /p:DebugType=embedded
    /p:SuppressNETCoreSdkPreviewMessage=true
    /p:VersionSuffix=${VERSION_SUFFIX}
  )

  if [ "${DOG_FOOD}" != "true" ]; then
    options+=( /p:SourceRevisionId=$(git rev-parse --short HEAD) )
  fi
  if [ "${SINGLE_FILE}" == "true" ]; then
    options+=( /p:PublishSingleFile=true )
  fi

  if [ ${#extra_options[@]} -gt 0 ]; then
    options+=("${extra_options[@]}")
  fi

  # this initial restore is needed to make clean work properly on cases where
  # the projects changed dependencies (e.g. added a new nuget package)
  echo "Restoring packages..."
  dotnet restore --runtime "${NET_RUNTIME}" --verbosity quiet

  echo "Running dotnet clean..."
  dotnet clean --configuration "${CONFIG}" --verbosity quiet

  if [ "${PORTABLE}" = "true" ]; then
    mkdir -p "${OUTPUT}/userdata"
  fi

  for project in "${projects[@]}"; do
    echo -e "\nBuilding ${project}...\n"
    dotnet publish "${project}" "${options[@]}"
  done

  echo -e "\nBuild finished! Binaries created in ${OUTPUT}"
}

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
  echo "Moving ${source} to ${nested}..."
  mkdir -p "${nested}"
  mv ${contents} "${nested}"
}

copy_pixmap_assets() {
  local output_folder="${1}"

  echo "Copying pixmap assets to '${output_folder}'..."
  mkdir -p "${output_folder}"
  cp "${REPO_ROOT}/OpenTabletDriver.UX/Assets"/* "${output_folder}"
}

copy_manpage() {
  local output_folder="${1}"

  echo "Copying manpage(s) to '${output_folder}'..."
  pushd "${REPO_ROOT}/docs/manpages" > /dev/null
  for manpage in *; do
    local index="$(echo $manpage | rev | cut -d. -f1)"
    mkdir -p "${output_folder}/man${index}"
    gzip -c $manpage > "${output_folder}/man${index}/${manpage}.gz"
  done
  popd > /dev/null
}

create_source_tarball() {
  local prefix="${1}"
  git archive --format=tar --prefix="${prefix}/" HEAD
}

create_source_tarball_gz() {
  local prefix="${1}"
  git archive --format=tar.gz --prefix="${prefix}/" HEAD
}
