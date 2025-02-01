#!/usr/bin/env bash

# Simple bash script to easily build to verify functionality.
#
# Usage of --runtime is preferred, but the arguments 'windows', 'macos' or
# 'linux' can be used as a shorthand to specify an x64 runtime directly

output="bin"
config="Release"
options=()

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
    windows)
      options+=("--runtime" "win-x64")
      ;;
    macos)
      options+=("--runtime" "osx-x64")
      ;;
    linux)
      options+=("--runtime" "linux-x64")
      ;;
    --)
      shift
      options+=("$@")
      break
      ;;
    *)
      options+=("$1")
      ;;
  esac
  shift
done

# provide defaults, then pass everything else as-is
. "$(dirname ${BASH_SOURCE[0]})"/eng/bash/package.sh -o "${output}" -c "${config}" "${options[@]}"
