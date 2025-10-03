#!/usr/bin/env bash

redhat_src="$(readlink -f $(dirname "${BASH_SOURCE[0]}"))"

output="$(readlink -f "${1}")"

if [ -n "$VERSION_SUFFIX" ]; then
  # see https://docs.fedoraproject.org/en-US/packaging-guidelines/Versioning/
  if [[ "$VERSION_SUFFIX" == -* ]]; then
    # use prerelease versioning
    version_to_use="${OTD_VERSION_BASE}~${VERSION_SUFFIX:1}"
  elif [[ "$VERSION_SUFFIX" == +* ]]; then
    version_to_use="${OTD_VERSION_BASE}${VERSION_SUFFIX}"
  else
    # some other variant, use underscore as visual seperator as it probably isn't a version component
    version_to_use="${OTD_VERSION_BASE}_${VERSION_SUFFIX}"
  fi
else
  version_to_use="${OTD_VERSION}"
fi

version_to_use="${version_to_use//+/^}" # replace commit info with preferred format

echo "Determined version $version_to_use"

echo "RPMizing..."
mkdir -p "${output}"/{BUILD,RPMS,SOURCES,SPECS,SRPMS}

echo "Making a source tarball..."
create_source_tarball_gz "${OTD_LNAME}-${version_to_use}" > "${output}/SOURCES/${OTD_LNAME}-${version_to_use}.tar.gz"

echo "Generating ${OTD_LNAME}.spec..."
cat << EOF > "${output}/SPECS/${OTD_LNAME}.spec"
Name: ${OTD_LNAME}
Version: ${version_to_use}
Release: 1
Summary: A ${OTD_DESC}

Source0: ${OTD_LNAME}-${version_to_use}.tar.gz

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
VERSION_SUFFIX=${VERSION_SUFFIX} ./eng/bash/package.sh --output bin

%install
export DONT_STRIP=1
PREFIX="%{_prefix}" ./eng/bash/package.sh --package Generic --build false
mkdir -p "%{buildroot}"
mv ./dist/files/* "%{buildroot}"/
rm -rf ./dist
mkdir -p "%{buildroot}/%{_prefix}/lib/"
cp -r bin "%{buildroot}/%{_prefix}/lib/opentabletdriver"

%post -f eng/bash/Generic/postinst

%postun -f eng/bash/Generic/postrm

%files
%defattr(-,root,root)
%dir %{_prefix}/lib/opentabletdriver
%dir %{_prefix}/share/doc/opentabletdriver
%{_bindir}/otd
%{_bindir}/otd-daemon
%{_bindir}/otd-gui
%{_prefix}/lib/modprobe.d/99-opentabletdriver.conf
%{_prefix}/lib/modules-load.d/opentabletdriver.conf
%{_prefix}/lib/opentabletdriver/*
%{_prefix}/lib/systemd/user/opentabletdriver.service
%{_prefix}/lib/udev/rules.d/70-opentabletdriver.rules
%{_prefix}/share/applications/opentabletdriver.desktop
%{_prefix}/share/man/man8/opentabletdriver.8.gz
%{_prefix}/share/doc/opentabletdriver/LICENSE
%{_prefix}/share/pixmaps/otd.ico
%{_prefix}/share/pixmaps/otd.png
%{_prefix}/share/libinput/30-vendor-opentabletdriver.quirks

%changelog
EOF

moved_output="${output}/opentabletdriver"
move_to_nested "${output}" "${moved_output}"
rpmbuild -D "_topdir ${moved_output}" -bb "${moved_output}/SPECS/${OTD_LNAME}.spec"
PKG_FILE="$(basename "$(ls "${moved_output}/RPMS/x86_64/${OTD_LNAME}"*.rpm)")"
mv "${moved_output}/RPMS/x86_64/${PKG_FILE}" "${output}"
