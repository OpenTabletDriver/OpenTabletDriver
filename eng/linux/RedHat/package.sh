#!/usr/bin/env bash

# increment this when releasing a new package of the same upstream version
# where the only changes are to the packaging itself
PKG_VER="1"
PKG_FILE="${OTD_LNAME}-${OTD_VERSION}-${PKG_VER}-x64.rpm"

redhat_src="$(readlink -f $(dirname "${BASH_SOURCE[0]}"))"

output="$(readlink -f "${1}")"

echo "RPMizing..."
mkdir -p "${output}"/{BUILD,RPMS,SOURCES,SPECS,SRPMS}

echo "Making a source tarball..."
create_source_tarball "${OTD_LNAME}-${OTD_VERSION}" | gzip -c > "${output}/SOURCES/${OTD_LNAME}-${OTD_VERSION}.tar.gz"

echo "Generating ${OTD_LNAME}.spec..."
cat << EOF > "${output}/SPECS/${OTD_LNAME}.spec"
Name: ${OTD_LNAME}
Version: ${OTD_VERSION}
Release: ${PKG_VER}%{?dist}
Summary: A ${OTD_DESC}

Source0: ${OTD_LNAME}-${OTD_VERSION}.tar.gz

License: LGPLv3
URL: ${OTD_UPSTREAM_URL}

BuildRequires: dotnet-sdk-${DOTNET_VERSION}
Requires: dotnet-runtime-${DOTNET_VERSION}
Requires: gtk3
Requires: udev
Requires(post): grep
Suggests: libX11
Suggests: libXrandr

# libevdev is libevdev2 on SUSE, and libevdev on RHEL/Fedora...
Requires: libevdev.so.2()(64bit)

%description
${OTD_LONG_DESC}

${OTD_LONG_DESC2}

%global __requires_exclude_from ^/usr/lib/${OTD_LNAME}/.*$

# No debug symbols
%global debug_package %{nil}

# No stripping
%global __os_install_post %{nil}

%prep
%autosetup

%build
./build.sh

%install
export DONT_STRIP=1
PREFIX="%{_prefix}" ./eng/linux/Generic/package.sh "%{buildroot}"
mkdir -p "%{buildroot}/%{_prefix}/lib/"
cp -r bin "%{buildroot}/%{_prefix}/lib/opentabletdriver"

%post -f eng/linux/Generic/postinst

%postun -f eng/linux/Generic/postrm

%files
%defattr(-,root,root)
%dir %{_prefix}/lib/opentabletdriver
%dir %{_prefix}/share/doc/opentabletdriver
%{_bindir}/otd
%{_bindir}/otd-daemon
%{_bindir}/otd-gui
%{_prefix}/lib/modprobe.d/99-opentabletdriver.conf
%{_prefix}/lib/opentabletdriver/*
%{_prefix}/lib/systemd/user/opentabletdriver.service
%{_prefix}/lib/udev/rules.d/90-opentabletdriver.rules
%{_prefix}/share/applications/opentabletdriver.desktop
%{_prefix}/share/man/man8/opentabletdriver.8.gz
%{_prefix}/share/doc/opentabletdriver/LICENSE
%{_prefix}/share/pixmaps/otd.ico
%{_prefix}/share/pixmaps/otd.png

%changelog
EOF

moved_output="${output}/opentabletdriver"
move_to_nested "${output}" "${moved_output}"
rpmbuild -D "_topdir ${moved_output}" -bb "${moved_output}/SPECS/${OTD_LNAME}.spec"
PKG_FILE="$(basename "$(ls "${moved_output}/RPMS/x86_64/${OTD_LNAME}"*.rpm)")"
mv "${moved_output}/RPMS/x86_64/${PKG_FILE}" "${output}"
