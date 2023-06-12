#!/usr/bin/env bash

# in the future if we want to be part of official debian repos, we need to
# provide a manpage for the binaries in /usr/bin, and add a source package
# where the binaries could be built from.

PKG_VER="1"
PKG_FILE="${OTD_LNAME}-${OTD_VERSION}-${PKG_VER}-x64.deb"

script_root="$(readlink -f $(dirname "${BASH_SOURCE[0]}"))"

output="${1}"
netRuntime="${2}"

move_to_nested "${output}" "${output}/usr/lib/opentabletdriver"

echo "Copying generic files..."
cp -R "${GENERIC_FILES}"/* "${output}/"

echo "Debianizing..."

mkdir -p "${output}/debian"
touch "${output}/debian/control"

echo "Generating shlibdeps..."
last_cwd="${PWD}"
cd "${output}"
shlibdeps="$(dpkg-shlibdeps -O "usr/lib/opentabletdriver/OpenTabletDriver.Daemon" 2> /dev/null)"
shlibdeps="${shlibdeps#shlibs:Depends=}"
cd "${last_cwd}"
mv "${output}"/{debian,DEBIAN}

echo "Copying Debian files..."
cp -R "${script_root}/DEBIAN" "${output}"
cp -R "${script_root}/usr" "${output}"

generate_rules "${output}/usr/lib/udev/rules.d/99-opentabletdriver.rules"
generate_desktop_file "${output}/usr/share/applications/opentabletdriver.desktop"
copy_pixmap_assets "${output}/usr/share/pixmaps"

echo "Generating DEBIAN/control..."
cat << EOF > "${output}/DEBIAN/control"
Package: ${OTD_LNAME}
Version: ${OTD_VERSION}-${PKG_VER}
Section: misc
Priority: optional
Architecture: amd64
Installed-Size: $(du -s "${output}" | cut -f1)
Pre-Depends: udev
Depends: libevdev2, libgtk-3-0, dotnet-runtime-6.0, ${shlibdeps}
Recommends: libx11-6, libxrandr2
Conflicts: ${OTD_NAME}
Replaces: ${OTD_NAME}
Maintainer: ${OTD_MAINTAINERS[1]}
Description: A ${OTD_DESC}
 ${OTD_NAME} has the highest number of supported tablets with great
 compatibility across multiple platforms, packaged in an easy-to-use graphical
 user interface.
 .
 ${OTD_NAME} has support for multiple tablets from the following (non-exhaustive) OEMs:
  * Wacom
  * Huion
  * XP-Pen
  * XenceLabs
  * Gaomon
  * Veikk
Homepage: ${OTD_UPSTREAM_URL}
Vcs-Browser: ${OTD_REPO_URL}
Vcs-Git: ${OTD_GIT}
EOF

echo "Creating '${PKG_FILE}'..."

move_to_nested "${output}" "${output}/opentabletdriver"
dpkg-deb --build --root-owner-group "${output}/opentabletdriver" "${output}/${PKG_FILE}"
