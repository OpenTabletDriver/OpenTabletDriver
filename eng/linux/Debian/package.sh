#!/usr/bin/env bash

# increment this when releasing a new package of the same upstream version
# where the only changes are to the packaging itself
PKG_VER="1"
PKG_FILE="${OTD_LNAME}-${OTD_VERSION}-${PKG_VER}-x64.deb"

debian_src="$(readlink -f $(dirname "${BASH_SOURCE[0]}"))"

output="${1}"

echo "Copying source files..."
create_source_tarball "${OTD_LNAME}-${OTD_VERSION}" | tar -xf - -C "${output}"

echo "Debianizing..."
cp -R "${debian_src}/debian" "${output}/${OTD_LNAME}-${OTD_VERSION}"
cp "${GENERIC_FILES}/postinst" "${output}/${OTD_LNAME}-${OTD_VERSION}/debian/${OTD_LNAME}.postinst"
cp "${GENERIC_FILES}/postrm" "${output}/${OTD_LNAME}-${OTD_VERSION}/debian/${OTD_LNAME}.postrm"

echo "Generating debian/control..."
cat << EOF > "${output}/${OTD_LNAME}-${OTD_VERSION}/debian/control"
Source: ${OTD_LNAME}
Priority: optional
Build-Depends: dotnet-sdk-${DOTNET_VERSION}
Maintainer: ${OTD_MAINTAINERS[1]}

Package: opentabletdriver
Section: base
Architecture: amd64
Pre-Depends: udev
Depends: libevdev2, libgtk-3-0, dotnet-runtime-${DOTNET_VERSION}, \${shlibs:Depends}
Recommends: libx11-6, libxrandr2
Description: A ${OTD_DESC}
 $(echo "${OTD_LONG_DESC}" | sed '2,$s/^/ /')
 .
 $(echo "${OTD_LONG_DESC2}" | sed '2,$s/^/ /')
Homepage: ${OTD_UPSTREAM_URL}
EOF

echo "Generating debian/changelog..."
cat <<EOF > "${output}/${OTD_LNAME}-${OTD_VERSION}/debian/changelog"
${OTD_LNAME} (${OTD_VERSION}-${PKG_VER}) unstable; urgency=low

  * New version: ${OTD_VERSION}-${PKG_VER}

 -- InfinityGhost <infinityghostgit@gmail.com>  `LANG=C date +"%a, %d %b %Y %X %z"`

EOF

PREV_DIR="${PWD}"
echo "Creating '${PKG_FILE}'..."
cd "${output}/${OTD_LNAME}-${OTD_VERSION}"
# TODO: fix --no-sign
dpkg-buildpackage --no-sign -b
cd "${PREV_DIR}"
mv "${output}/${OTD_LNAME}_${OTD_VERSION}-${PKG_VER}_amd64.deb" "${output}/${PKG_FILE}"
