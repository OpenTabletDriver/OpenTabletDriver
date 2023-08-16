#!/usr/bin/env bash

# no SRPM for now

# increment this when releasing a new package of the same upstream version
# where the only changes are to the packaging itself
PKG_VER="1"
PKG_FILE="${OTD_LNAME}-${OTD_VERSION}-${PKG_VER}-x64.rpm"

redhat_src="$(readlink -f $(dirname "${BASH_SOURCE[0]}"))"

output="$(readlink -f "${1}")"

move_to_nested "${output}" "${output}/tmp"

echo "RPMizing..."
mkdir -p "${output}"/{BUILD,RPMS,SOURCES,SPECS,SRPMS}

build_src="${output}/BUILD"
move_to_nested "${output}/tmp" "${build_src}/usr/lib/${OTD_LNAME}"
rm -r "${output}/tmp"

copy_generic_files "${build_src}"

echo "Copying RedHat files..."
cp -R "${redhat_src}/usr" "${build_src}"

generate_rules "${build_src}/usr/lib/udev/rules.d/99-${OTD_LNAME}.rules"
generate_desktop_file "${build_src}/usr/share/applications/${OTD_LNAME}.desktop"
copy_pixmap_assets "${build_src}/usr/share/pixmaps"

echo "Generating ${OTD_LNAME}.spec..."
cat << EOF > "${output}/${OTD_LNAME}.spec"
Name: ${OTD_LNAME}
Obsoletes: ${OTD_NAME}
Version: ${OTD_VERSION}
Release: ${PKG_VER}%{?dist}
Summary: A ${OTD_DESC}
BuildArch: x86_64

License: LGPLv3
URL: ${OTD_UPSTREAM_URL}

BuildRequires: systemd-rpm-macros
Requires: dotnet-runtime-6.0
Requires: libevdev.so.2()(64bit)
Requires: gtk3
Suggests: libX11
Suggests: libXrandr
Requires(post): udev
Requires(post): grep

%if 0%{?suse_version}
Requires(post): /usr/bin/lsmod
%endif

%description
${OTD_LONG_DESC}

${OTD_LONG_DESC2}

%global __requires_exclude_from ^/usr/lib/${OTD_LNAME}/.*$

# No debug symbols
%global debug_package %{nil}

# No stripping
%global __os_install_post %{nil}

%clean

%prep

%build

%install
export DONT_STRIP=1

cp -r %{_builddir}/* %{buildroot}

%pre

%post
BOLD_YELLOW='\033[1;33m'
RESET='\033[0m'

if lsmod | grep hid_uclogic > /dev/null; then
    rmmod hid_uclogic || true
fi

if lsmod | grep wacom > /dev/null; then
    rmmod wacom || true
fi

if udevadm control --reload; then
    udevadm trigger --settle || true
    udevadm trigger --name-match=uinput --settle || true
fi

printf "\${BOLD_YELLOW}Run the daemon by invoking 'otd-daemon', or by enabling opentabletdriver.service\${RESET}"

%preun
%systemd_user_preun opentabletdriver.service

%postun
%systemd_user_postun opentabletdriver.service

if udevadm control --reload; then
    udevadm trigger --settle || true
fi

%files
%defattr(-,root,root)
%{_bindir}/otd
%{_bindir}/otd-daemon
%{_bindir}/otd-gui
%{_prefix}/lib/modprobe.d/99-opentabletdriver.conf
%{_prefix}/lib/opentabletdriver/*
%{_prefix}/lib/systemd/user/opentabletdriver.service
%{_prefix}/lib/udev/rules.d/99-opentabletdriver.rules
%{_prefix}/share/applications/opentabletdriver.desktop
%{_prefix}/share/licenses/opentabletdriver/LICENSE
%{_prefix}/share/pixmaps/otd.ico
%{_prefix}/share/pixmaps/otd.png

%changelog
EOF

moved_output="${output}/opentabletdriver"
move_to_nested "${output}" "${moved_output}"
rpmbuild -D "_topdir ${moved_output}" -bb "${moved_output}/${OTD_LNAME}.spec"
PKG_FILE="$(basename "$(ls "${moved_output}/RPMS/x86_64/${OTD_LNAME}"*.rpm)")"
mv "${moved_output}/RPMS/x86_64/${PKG_FILE}" "${output}"
