#!/usr/bin/env bash

set -eu

output="dist"
config="Release"
framework="net6.0"
netRuntime="linux-x64"
isRelease="true"
packageGen=""
isPortable="false"
singleFile="false"
selfContained="false"

print_help() {
  echo "Usage: $0 [OPTIONS]..."
  echo "Options:"
  echo "  -o, --output <path>           Output directory for build artifacts (default: ./dist/)"
  echo "  -c, --configuration <config>  Build configuration (default: Release)"
  echo "  -f, --framework <framework>   Target framework (default: net6.0)"
  echo "  -r, --runtime <runtime>       Target runtime (default: linux-x64)"
  echo "  --release <bool>              Whether to output production binaries (default: true)"
  echo "  --package <package>           Package generation script to run after build"
  echo "  --portable <bool>             Whether to build portable binaries (default: false)"
  echo "  --single-file <bool>          Whether to build single-file binaries (default: false)"
  echo "  --self-contained <bool>       Whether to build self-contained binaries, implies --single-file (default: false)"
  echo "  -h, --help                    Print this help message"
}

while [ $# -gt 0 ]; do
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
    *)
      echo "Unknown option: $1"
      print_help
      exit 1
      ;;
  esac
  shift
done

script_root="$(readlink -f $(dirname "${BASH_SOURCE[0]}"))"
prev_path=$(pwd)

projects=(
  "OpenTabletDriver.Daemon" \
  "OpenTabletDriver.Console" \
  "OpenTabletDriver.UX.Gtk" \
)

options=(
  --configuration "${config}" \
  --runtime "${netRuntime}" \
  --framework "${framework}" \
  --self-contained "${selfContained}" \
  --output "${output}" \
  /p:PublishTrimmed=false \
  /p:DebugType=embedded \
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
. "$(dirname "$0")/lib.sh"

# Change dir to repo root
cd "${REPO_ROOT}"

# Sanity check
if [ ! -d "OpenTabletDriver" ]; then
  exit_with_error "Could not find OpenTabletDriver folder!"
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

echo "Restoring packages..."
dotnet restore --verbosity quiet > /dev/null

echo "Running dotnet clean..."
dotnet clean --configuration "${config}" --verbosity quiet > /dev/null

mkdir -p "${output}"
if [ "${isPortable}" = "true" ]; then
  mkdir -p "${output}/userdata"
fi

for project in "${projects[@]}"; do
  echo -e "\nBuilding ${project}...\n"
  dotnet publish "${project}" "${options[@]}"
done

echo -e "\nBuild finished! Binaries created in ${output}\n"

if [ -n "${packageGen}" ]; then
  package_script="${script_root}/${packageGen}/package.sh"
  if [ ! -f "${package_script}" ]; then
    exit_with_error "Could not find package generation script: ${package_script}"
  fi

  echo -e "Running package generation script: ${package_script}\n"

  # child package.sh should expect to be run from the repo root and should write
  # the filename of the package generated to PKG_FILE
  . "${package_script}" "${output}" "${netRuntime}"

  echo -e "\nPackaging finished! Package created at '${output}/${PKG_FILE}'"
fi

cd "${prev_path}"
