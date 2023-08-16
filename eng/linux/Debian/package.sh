#!/usr/bin/env bash

# in the future if we want to be part of official debian repos, we need to
# provide a manpage for the binaries in /usr/bin, and add a source package
# where the binaries could be built from.

# increment this when releasing a new package of the same upstream version
# where the only changes are to the packaging itself
PKG_VER="1"
PKG_FILE="${OTD_LNAME}-${OTD_VERSION}-${PKG_VER}-x64.deb"

debian_src="$(readlink -f $(dirname "${BASH_SOURCE[0]}"))"

output="${1}"

move_to_nested "${output}" "${output}/usr/lib/opentabletdriver"
copy_generic_files "${output}"

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
cp -R "${debian_src}/DEBIAN" "${output}"
cp -R "${debian_src}/usr" "${output}"

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
 $(echo "${OTD_LONG_DESC}" | sed '2,$s/^/ /')
 .
 $(echo "${OTD_LONG_DESC2}" | sed '2,$s/^/ /')
Homepage: ${OTD_UPSTREAM_URL}
Vcs-Browser: ${OTD_REPO_URL}
Vcs-Git: ${OTD_GIT}
EOF

echo "Creating '${PKG_FILE}'..."
move_to_nested "${output}" "${output}/opentabletdriver"
dpkg-deb --build --root-owner-group "${output}/opentabletdriver" "${output}/${PKG_FILE}"
