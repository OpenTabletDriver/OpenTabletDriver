#!/usr/bin/env bash

set -eu

SRC_ROOT=$(readlink -f $(dirname ${BASH_SOURCE[0]}))
PROJECT="${SRC_ROOT}/OpenTabletDriver.Tools.udev"

if [ $# -eq 2 ]; then
    TABLET_CONFIGURATIONS="${1}"
    RULES_FILE="${2}"
elif [ $# -eq 0 ]; then
    TABLET_CONFIGURATIONS="${SRC_ROOT}/OpenTabletDriver.Configurations/Configurations"
    RULES_FILE="-"
elif [ $# -ne 2 ]; then
    echo "Usage: ${0} <configuration folder> <output file>"
    exit 1
fi

mkdir -p "$(dirname "${RULES_FILE}")"

if [ "${RULES_FILE}" = "-" ]; then
    RULES_FILE="/dev/stdout"
fi

dotnet run --project "${PROJECT}" -- "${TABLET_CONFIGURATIONS}" > "${RULES_FILE}"
