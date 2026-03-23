About
=====

This packaging script is mostly intended for use with testing and
with our CI/CD scripts to allow for easy testing of PR's

If you are testing a new configuration, you likely need new udev rules as well.
These are included as `70-opentabletdriver.rules` and can be placed in
/etc/udev/rules.d/. You will need to reload udev rules after putting the file
there.


Requirements
============

- Existing OpenTabletDriver install
or
- Having permissions to `uinput` and `hidraw`
