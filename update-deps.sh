#!/usr/bin/env bash

REPO_DIR="$(dirname $(realpath ${BASH_SOURCE[0]}))"
DEPS_PATH="$REPO_DIR/deps.nix"

script="$(nix build $REPO_DIR#opentabletdriver.fetch-deps --no-link --print-out-paths)"
eval "$script" "$DEPS_PATH"
