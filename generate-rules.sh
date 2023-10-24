#!/usr/bin/env bash
# dependencies: git, jq, tr (coreutils), awk (gawk), sed (gnused)

for c in git jq tr awk sed; do
  command -v $c > /dev/null
  if [[ $? > 0 ]]; then
    echo "Error: Command $c not found in \$PATH."
    exit 1
  fi
done

tohex() {
  printf $1 | awk '{ printf("%04x", $1) }'
}

shopt -s globstar
set -eu

OTD_CONFIGURATIONS="${OTD_CONFIGURATIONS:="$(git rev-parse --show-toplevel)/OpenTabletDriver.Configurations/Configurations"}"

script='[
  .[] | { Name:.Name, libinput:(.Attributes.libinputoverride // "0") } + (.DigitizerIdentifiers[] | { VendorID:.VendorID, ProductID:.ProductID })
] | unique | sort_by(.VendorID,.ProductID) | group_by(.VendorID, .ProductID) |
  map({ Names: (map(.Name) | join(",")), libinput: (map(.libinput) | max), VendorID: .[0].VendorID, ProductID: .[0].ProductID})
| .[] | "\(.Names):\(.VendorID):\(.ProductID):\(.libinput)"'

configs_arr=$(jq -s "$script" $OTD_CONFIGURATIONS/**/**.json | tr -d '"')

echo \# OpenTabletDriver udev rules \(https://github.com/OpenTabletDriver/OpenTabletDriver\)
echo KERNEL==\"uinput\", SUBSYSTEM==\"misc\", OPTIONS+=\"static_node=uinput\", TAG+=\"uaccess\", TAG+=\"udev-acl\"
echo KERNEL==\"js[0-9]*\", SUBSYSTEM==\"input\", ATTRS{name}==\"OpenTabletDriver Virtual Tablet\", RUN+=\"/usr/bin/env rm %E{DEVNAME}\"

IFS=':'
while read s; do
  read -r names vid pid libinput <<< $s

  vid=$(tohex $vid)
  pid=$(tohex $pid)

  echo \# $(echo $names | sed 's/,/\n# /g')
  echo KERNEL==\"hidraw*\", ATTRS{idVendor}==\"$vid\", ATTRS{idProduct}==\"$pid\", TAG+=\"uaccess\", TAG+=\"udev-acl\"
  echo SUBSYSTEM==\"usb\", ATTRS{idVendor}==\"$vid\", ATTRS{idProduct}==\"$pid\", TAG+=\"uaccess\", TAG+=\"udev-acl\"

  if [[ $libinput > 0 ]]; then
    echo SUBSYSTEM==\"input\", ATTRS{idVendor}==\"$vid\", ATTRS{idProduct}==\"$pid\", ENV{LIBINPUT_IGNORE_DEVICE}=\"$libinput\"
  fi
done <<< $configs_arr
