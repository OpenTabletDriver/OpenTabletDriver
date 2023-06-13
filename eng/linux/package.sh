#!/usr/bin/env bash

set -eu

output="dist"
config="Release"
framework="net6.0"
netRuntime="linux-x64"
isRelease="true"
build="true"
packageGen=""
isPortable="false"
singleFile="true"
selfContained="false"

print_help() {
  echo "Usage: ${BASH_SOURCE[0]} [OPTIONS]..."
  echo "Options:"
  echo "  -o, --output <path>           Output directory for build artifacts (default: ./dist/)"
  echo "  -c, --configuration <config>  Build configuration (default: Release)"
  echo "  -f, --framework <framework>   Target framework (default: net6.0)"
  echo "  -r, --runtime <runtime>       Target runtime (default: linux-x64)"
  echo "  --release <bool>              Whether to output production binaries (default: true)"
  echo "  --package <package>           Package generation script to run after build"
  echo "  --build <bool>                Whether to build binaries (default: true)"
  echo "  --portable <bool>             Whether to build portable binaries (default: false)"
  echo "  --single-file <bool>          Whether to build single-file binaries (default: true)"
  echo "  --self-contained <bool>       Whether to build self-contained binaries, implies --single-file (default: false)"
  echo "  -h, --help                    Print this help message"
  echo
  echo "Remarks:"
  echo "  Anything after '--', if it is specified, will be passed to dotnet publish as-is."
}

is_extra_args=false
extra_options=()

while [ $# -gt 0 ]; do
  if $is_extra_args; then
    extra_options=("$@")
    break
  fi

  case "$1" in
    -o=*|--output=*)
      output="${1#*=}"
      ;;
    -o|--output)
      output="$2"
      shift
      ;;
    -c=*|--configuration=*)
      config="${1#*=}"
      ;;
    -c|--configuration)
      config="$2"
      shift
      ;;
    -f=*|--framework=*)
      framework="${1#*=}"
      ;;
    -f|--framework)
      framework="$2"
      shift
      ;;
    -r=*|--runtime=*)
      netRuntime="${1#*=}"
      ;;
    -r|--runtime)
      netRuntime="$2"
      shift
      ;;
    --release=*)
      isRelease="${1#*=}"
      ;;
    --release)
      isRelease="$2"
      shift
      ;;
    --build=*)
      build="${1#*=}"
      ;;
    --build)
      build="$2"
      shift
      ;;
    --package=*)
      packageGen="${1#*=}"
      ;;
    --package)
      packageGen="$2"
      shift
      ;;
    --portable=*)
      isPortable="${1#*=}"
      ;;
    --portable)
      isPortable="$2"
      shift
      ;;
    --single-file=*)
      singleFile="${1#*=}"
      ;;
    --single-file)
      singleFile="$2"
      shift
      ;;
    --self-contained=*)
      selfContained="${1#*=}"
      ;;
    --self-contained)
      selfContained="$2"
      shift
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
  shift
done

prev_path=${PWD}

projects=(
  "OpenTabletDriver.Daemon"
  "OpenTabletDriver.Console"
  "OpenTabletDriver.UX.Gtk"
)

options=(
  --configuration "${config}"
  --runtime "${netRuntime}"
  --framework "${framework}"
  --self-contained "${selfContained}"
  --output "${output}"
  /p:PublishTrimmed=false
  /p:DebugType=embedded
  /p:SuppressNETCoreSdkPreviewMessage=true
)

if [ "${isRelease}" != "true" ]; then
  options+=( /p:SourceRevisionId=$(git rev-parse --short HEAD) )
fi
if [ "${selfContained}" == "true" ] || [ "${singleFile}" == "true" ]; then
  options+=( /p:PublishSingleFile=true )
fi

handle_error() {
  echo "Build failed!"
  cd "${prev_path}"
  exit 1
}

exit_with_error() {
  echo "$1"
  handle_error
}

trap handle_error ERR

# Import helper functions
. "$(dirname "${BASH_SOURCE[0]}")/lib.sh"

# Change dir to repo root
cd "${REPO_ROOT}"

# Sanity checks
if [ ! -d "OpenTabletDriver" ]; then
  exit_with_error "Could not find OpenTabletDriver folder!"
fi

if [ -n "${packageGen}" ] && [ "${isPortable}" = "true" ]; then
  exit_with_error "Cannot build portable binaries and generate packages at the same time!"
fi

if [ -d "${output}" ]; then
  echo "Cleaning old build outputs..."
  for dir in "${output}"/*; do
    dir_name=$(basename "${dir}")
    if [ "${dir_name}" != "userdata" ]; then
      if ! rm -rf "${dir}" 2>/dev/null; then
        exit_with_error "Could not clean old build dirs. Please manually remove contents of ${output} folder."
      fi
    fi
  done
fi

mkdir -p "${output}"

if [ "${build}" = "true" ]; then
  echo "Restoring packages..."
  dotnet restore --verbosity quiet > /dev/null

  echo "Running dotnet clean..."
  dotnet clean --configuration "${config}" --verbosity quiet > /dev/null

  if [ "${isPortable}" = "true" ]; then
    mkdir -p "${output}/userdata"
  fi

  for project in "${projects[@]}"; do
    echo -e "\nBuilding ${project}...\n"
    dotnet publish "${project}" "${options[@]}"
  done

  echo -e "\nBuild finished! Binaries created in ${output}\n"
fi

if [ -n "${packageGen}" ]; then
  package_script="${PKG_SCRIPT_ROOT}/${packageGen}/package.sh"
  if [ ! -f "${package_script}" ]; then
    exit_with_error "Could not find package generation script: ${package_script}"
  fi

  echo -e "Running package generation script: ${package_script}\n"

  # child package.sh should expect to be run from the repo root and should write
  # the filename of the package generated to PKG_FILE
  . "${package_script}" "${output}"

  echo -e "\nPackaging finished! Package created at '${output}/${PKG_FILE}'"
fi

cd "${prev_path}"
