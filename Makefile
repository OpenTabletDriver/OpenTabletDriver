#!/usr/bin/make -f
SHELL=/bin/bash

ifdef DESTDIR
	PREFIX = $(DESTDIR)/usr
else
	PREFIX = /usr
endif

PKG_VERSION = $(shell [[ $$(cat Directory.Build.props) =~ \<VersionBase\>(.+?)\<\/VersionBase\> ]] && echo $${BASH_REMATCH[1]})
VERSION_REGEX = ^Version=.\+$$
VERSION_REPLACE_REGEX = Version=$(PKG_VERSION)

all: build

build:
	./build.sh

install:
	mkdir -p $(PREFIX)/share/OpenTabletDriver
	cp -r bin/* $(PREFIX)/share/OpenTabletDriver
	find $(PREFIX)/share/OpenTabletDriver -name "*.pdb" -type f -exec rm {} ';'
	
	cp -r Common/Linux/* $(PREFIX)
	
	mkdir -p $(PREFIX)/share/man/man8
	gzip -c docs/manpages/opentabletdriver.8 > $(PREFIX)/share/man/man8/opentabletdriver.8.gz

	mkdir -p $(PREFIX)/share/pixmaps
	cp -v OpenTabletDriver.UX/Assets/* $(PREFIX)/share/pixmaps

	mkdir -p $(PREFIX)/lib/udev/rules.d
	./generate-rules.sh -v OpenTabletDriver.Configurations/Configurations $(PREFIX)/lib/udev/rules.d/99-opentabletdriver.rules

	sed -i "s/$(VERSION_REGEX)/$(VERSION_REPLACE_REGEX)/g" "$(PREFIX)/share/applications/OpenTabletDriver.desktop"
	
uninstall:
	rm -r $(PREFIX)/share/OpenTabletDriver
	cd Common/Linux; find . -type f -exec rm $(PREFIX)/{} ';'
	rm $(PREFIX)/share/man/man8/opentabletdriver.8.gz
	rm $(PREFIX)/share/pixmaps/otd.{ico,png}
	rm $(PREFIX)/lib/udev/rules.d/99-opentabletdriver.rules
	
clean:
	rm -rf bin
	
	
.PHONY: all build install uninstall clean
