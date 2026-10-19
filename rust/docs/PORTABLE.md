# OpenTabletDriver Rust / S640 Windows prototype

`otd-rust-ui.exe` opens the native control panel. The driver starts stopped.

`Launch-Demo.cmd` opens the panel with synthetic input. This mode never initializes the tablet or moves/clicks the OS cursor.

For physical use, close the existing tablet driver normally first, launch `otd-rust-ui.exe`, check the mapping, and press Start. Stop or closing the panel releases buttons and closes the device. This prototype does not install itself or change the original driver settings.

This Windows pre-release has been tested only with the VEIKK S640. Although the source includes an S640 V2 definition, that model is not validated for this build. Absolute mouse output is implemented; artist pressure injection, original .NET plugins, relative mode, other tablets, and platform parity remain pending. Live read-only S640 input has been verified. Independent initialization and physical pointer/button injection have not yet been exercised.

`otd-rust.exe` is the console diagnostics executable; run it with `--help`. `PERFORMANCE.md` documents measured memory and timing scope. Memory measurements apply to this prototype's current feature set, not future parity.

Source: https://github.com/susenayw/OpenTabletDriver/tree/codex/rust-s640-low-memory/rust

AI assistance was used to create this migration. See the source repository for the retained upstream code, license, tests, and migration notes. The final UI design will follow a later HTML mockup.
