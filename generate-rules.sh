#!/usr/bin/env bash

SRC_ROOT=$(readlink -f $(dirname ${BASH_SOURCE[0]}))
[ ! -d "${SRC_ROOT}" ] && exit 100;

PROJECT="${SRC_ROOT}/OpenTabletDriver.Tools.udev"
FRAMEWORK="net6.0"

TABLET_CONFIGURATIONS="${SRC_ROOT}/OpenTabletDriver.Configurations/Configurations"
RULES_FILE="${SRC_ROOT}/bin/99-opentabletdriver.rules"

if [ "$#" -gt 0 ]; then
  # Pass arguments to utility instead of using defaults
  dotnet_args=($@)
else
  [ ! -d "${TABLET_CONFIGURATIONS}" ] && exit 101;
  dotnet_args=("-v" "${TABLET_CONFIGURATIONS}" "${RULES_FILE}")
fi

echo "Generating udev rules..."

dotnet run --project "${PROJECT}" -f "${FRAMEWORK}" -- ${dotnet_args[@]}

echo "\nRule file generated. Please move the generated rule file './bin/99-opentabletdriver.rules' to /etc/udev/rules.d, then run"
echo "sudo udevadm control --reload-rules"
echo "or simply reboot your PC."
