#!/usr/bin/env bash

set -eo pipefail

oldifs="$IFS"
IFS=$'\n'

cd "$(dirname ${BASH_SOURCE[0]})"/..

if [ ! -z "$GET_CONFIGS_FROM_CI" ]; then
  # git diff command uses a github action runner quirk where the diff between
  # HEAD^ and HEAD is equal to diffing commit merge base with pr tip
  mapfile -t configs < <(git diff --name-only --diff-filter=AM HEAD^ HEAD | grep '^OpenTabletDriver\.Configurations/Configurations/.*\.json')
fi

if [ -z "$configs" ]; then
  # if still undefined, grab all configurations
  mapfile -t configs < <(find OpenTabletDriver.Configurations/Configurations/ -type f -iname '*.json')
fi

TABLETS="${TABLETS:-TABLETS.md}"

unset failed

if [ ${#configs[@]} -ne 0 ]; then
  for conf in "${configs[@]}"; do
    name=$(cat $conf| jq -r '.Name')
    if ! grep -q "| $name " $TABLETS; then
      echo "Failure: '$name' was not found in $TABLETS - was it spelled correctly?"
      failed=y
    else
      if [ ! -z "$VERBOSE" ]; then
        echo "Pass: '$name'"
      fi
    fi
  done
fi

IFS="$oldifs"
unset oldifs

if [ ! -z "$failed" ]; then
  exit 1
fi
