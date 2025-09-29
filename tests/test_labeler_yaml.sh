#!/bin/bash

PROJECT_ROOT="$(dirname "${BASH_SOURCE[0]}")/.."
LABELER_YML_PATH="${LABELER_YML_PATH:="$PROJECT_ROOT"/.github/labeler.yml}"
unset fail

shopt -s globstar

yq_output="$(yq -r '[.[].[]."changed-files".[]."any-glob-to-any-file" | select(.).[]] | sort | unique | .[]' $LABELER_YML_PATH)"

IFS=$'\n'
cd "$PROJECT_ROOT"
for glob in $yq_output; do
  if [ ! -e "$glob" ]; then
    echo "FAIL: No matches on glob '$glob'"
    fail=y
  fi
done

if [ ! -z "$fail" ]; then
  echo "Errors occured"
  exit 1
fi
