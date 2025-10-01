#!/usr/bin/env bash

# This script contains variables and functions that are shared between systems
# using bash shell. Should be sourced by other scripts early on.

set -eu

### Input Environment Variables

VERSION_SUFFIX=${VERSION_SUFFIX:-}

### Global variables

PREV_PATH=${PWD}
LIB_SCRIPT_ROOT="$(readlink -f $(dirname "${BASH_SOURCE[0]}"))"
REPO_ROOT="$(readlink -f "${LIB_SCRIPT_ROOT}/../../")"
GENERIC_FILES="$(readlink -f "${LIB_SCRIPT_ROOT}/Generic")"

# The below regex supports the following backrefs:
# \1  Full version, e.g. '0.7.0.0alpha-rc1'
# \2  Primary version, e.g. '0.7.0.0'
# \3  unused (sed doesn't support non-capturing groups)
# \4  Secondary version, e.g. 'alpha-rc1'
# \5  unused
# \6  Release candidate, if any, e.g. 'rc1'
# \7  Suffix of 'git describe', e.g. '1234-g1337f00d-dirty'
# \8  Distance from tag, e.g. '1234'
# \9  Short-SHA of commit with trailing 'g' trimmed, e.g. '1337f00d'
# \10 Remainder, e.g. '-dirty'
#
# Please tag project with any of the following formats only:
# v0.7.0.0
# v9.9.9.9
# v9.9.9.99
# v0.7.0.0alpha
# v0.7.0.0z
# v0.7.0.0rc1
# v0.7.0.0-rc99
# v1
# v1.42
# v10.42.123
GIT_TAG_REGEX='^v(([0-9]+(\.[0-9]+)*)([^-\r\n]*(-?(rc[0-9]+))?))-?(([0-9]+)-g([a-f0-9]{8})(-.*)?)?$'

# if suffix unset, autodetect suffix from git
if [ -z "$VERSION_SUFFIX" ]; then
  # limit git repo discovery to project root
  export GIT_CEILING_DIRECTORIES="$(realpath ${REPO_ROOT}/../)"

  if hash git &>/dev/null && \
  hash sed &>/dev/null && \
  git rev-parse --is-inside-work-tree &>/dev/null
  then
    GIT_DESCRIBE="$(git describe --long --tags --dirty)"

    # don't set suffix if this is a tagged commit
    COMMIT_DISTANCE_FROM_TAG="$(sed -E s/"${GIT_TAG_REGEX}"/\\8/ <<< "$GIT_DESCRIBE")"
    if [ "$COMMIT_DISTANCE_FROM_TAG" -gt 0 ]; then
      # use git describe as suffix
      # commit distance from tag should not be used as a part before suffix
      #   as that might not accurately represent version (e.g. commit distance 11
      #   isn't necessarily newer than commit distance 5 if they're from 2 separate PR's)
      VERSION_SUFFIX="$(sed -E s/"${GIT_TAG_REGEX}"/\\7/ <<< "$GIT_DESCRIBE")"
      #echo "DEBUG: commit distance: '$COMMIT_DISTANCE_FROM_TAG'"
      dont_set_dirty=y
    elif [ "$COMMIT_DISTANCE_FROM_TAG" -eq 0 ]; then
      # use secondary version ('alpha-rc1' from '0.7alpha-rc1')
      VERSION_SUFFIX="$(sed -E s/"${GIT_TAG_REGEX}"/\\4/ <<< "$GIT_DESCRIBE")"
    else
      echo "WARN: Unable determine commit distance from tag"
    fi

    describe_remainder="$(sed -E s/"${GIT_TAG_REGEX}"/\\10/ <<< "$GIT_DESCRIBE")"
    if [ -z "$dont_set_dirty" ] && [[ $describe_remainder =~ ^.*dirty.*$ ]]; then
      # tag dirty if dirty
      VERSION_SUFFIX="${VERSION_SUFFIX:-}"-dirty
    fi
  else
    echo "WARN: VERSION_SUFFIX unset and git or sed not found, VERSION_SUFFIX remains unset!"
  fi
  if [ -n "$VERSION_SUFFIX" ]; then
    echo "Autodetected version suffix: '${VERSION_SUFFIX}'"
  fi
fi

### Build Requirements

DOTNET_VERSION="8.0"

# could do away with declare -g, but did it anyway for all of them for consistency
# with NET_RUNTIME (a global variable without initial value in lib.sh)
declare -g OUTPUT="dist"
declare -g CONFIG="Release"
declare -g FRAMEWORK="net$DOTNET_VERSION"
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
  echo "Build failed!" >&2
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
  echo "$1" >&2
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
    -p:PublishTrimmed=false
    -p:DebugType=embedded
    -p:SuppressNETCoreSdkPreviewMessage=true
    -p:VersionSuffix=${VERSION_SUFFIX}
  )

  if [ "${DOG_FOOD}" != "true" ]; then
    options+=( -p:SourceRevisionId=$(git rev-parse --short HEAD) )
  fi
  if [ "${SINGLE_FILE}" == "true" ]; then
    options+=( -p:PublishSingleFile=true )
  fi

  if [ ${#extra_options[@]} -gt 0 ]; then
    options+=("${extra_options[@]}")
  fi

  # this initial restore is needed in cases projects changed dependencies
  # (e.g. added a new nuget package)
  echo "Restoring packages..."
  dotnet restore --runtime "${NET_RUNTIME}" --verbosity quiet

  if [ "${PORTABLE}" = "true" ]; then
    mkdir -p "${OUTPUT}/userdata"
  fi

  for project in "${projects[@]}"; do
    echo -e "\nBuilding ${project}...\n"
    dotnet publish "${project}" "${options[@]}"
  done

  echo -e "\nBuild finished! Binaries created in ${OUTPUT}"
}

# always creates a subfolder by changing directory to the parent folder of source ($1)
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

### Linux Helper functions

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
