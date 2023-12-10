#!/usr/bin/env bash

# Simple bash script to easily build on linux to verify functionality.
# Uses the same commands as those found in the PKGBUILD for the AUR
# package.

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
    *)
      options+=("$1")
      ;;
  esac
  shift
done

# provide defaults, then pass everything else as-is
. "$(dirname ${BASH_SOURCE[0]})"/eng/linux/package.sh -o "${output}" -c "${config}" "${options[@]}"
