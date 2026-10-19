# Roadmap to broad tablet support

The first release remains an S640-only Windows pre-release. The next goal is to support every tablet model represented by this repository's upstream configuration set on Windows, while keeping the Rust process below the 50 MB target. Linux and macOS need separate input/output backends after the engine stops depending on Windows APIs.

## What is hard-coded today

The Rust prototype has two S640 report variants. HID discovery filters to the S640 VID/PID and report sizes, the engine always selects the S640 digitizer geometry and pressure range, initialization sends one S640-specific report, and the UI labels the connected device as an S640. The mapping math is already generic once it receives a tablet's digitizer dimensions and pressure maximum.

The configuration tree contains 360 tablet JSON files, 537 device identifiers, and 50 distinct report-parser names. Four parser implementations account for 259 identifiers (48.2%): XP-Pen (103), UCLogic tilt (59), Huion Giano (51), and UCLogic (46). These are useful first parser targets, but identifier counts are not the same as hardware validation or full feature coverage.

## Staged implementation

1. **Create a configuration catalog.** Generate a build-time index of the existing upstream JSON without copying or manually maintaining 360 files. Match an attached HID collection using VID/PID, report lengths, interface usage, and configured device strings. Parse and retain only the selected tablet profile at runtime. Detection should report recognized-but-unsupported devices without opening them for writing.
2. **Define a parser contract.** Each Rust parser takes a bounded input report and returns a small normalized event: pen coordinates, pressure, pen buttons, tilt, auxiliary buttons, wheel, out-of-range, or ignored. Parser selection must use the exact upstream `ReportParser` name; there is no safe universal byte layout. Keep the input loop allocation-free and give each parser family golden fixtures generated from its upstream C# implementation.
3. **Port parser families by coverage.** Start with XP-Pen, UCLogic tilt, Huion Giano, and UCLogic, then prioritize Wacom families and the remaining parser names by model demand. A model is enabled only when its parser, report lengths, digitizer geometry, pressure scale, and initialization sequence are implemented and tested. Unknown parser names remain visibly unsupported.
4. **Make profiles drive the engine.** Pass the selected profile into the mapper and HID session. Read coordinate range, physical dimensions, pressure range, input/output/feature report lengths, and initialization reports from that profile. Allocate a bounded report buffer once per connected device instead of assuming 64 bytes. Persist settings by stable tablet identity so changing tablets cannot apply an invalid area or initialization sequence.
5. **Make the panel model-neutral.** Replace the S640 label with detected model selection and status, derive area limits and defaults from the selected profile, and keep the current mapping controls shared across models. Preserve explicit Apply/Save behavior and require a deliberate Start before sending initialization or pointer events.
6. **Verify each support claim.** Compare decoded reports against upstream fixtures for every parser family, validate config-to-parser compatibility across all 537 identifiers, and test at least one real device per parser family before marking a model supported. Run the same memory gate with the largest reports, reconnects, and UI open.

## Memory and safety constraints

- Keep configuration JSON embedded as static data, but deserialize only profiles that match attached devices.
- Index by device identifiers during the build; do not allocate a large parsed catalog at startup.
- Bound HID report length and reject profiles that exceed the supported limit before opening a device.
- Use profile-provided initialization bytes only after the user selects Start and the full device identity matches.
- Preserve a read-only detection path and never guess a parser from VID/PID alone when multiple configurations match.
- Measure the whole process, including UI, selected profile, input buffer, and platform output.

## First engineering milestone

The first code change should replace `detect_s640()` and `Variant`-based selection with a configuration-backed device profile while keeping the S640 behavior unchanged. Then add one non-S640 parser family end to end, including its config match, initialization behavior, mapper settings, golden fixtures, UI name, and a hardware verification checklist. This proves the abstraction before expanding parser coverage in bulk.

The word “universal” should be reserved for broad model coverage, not for a generic HID fallback. The app can expose the supported model count and clearly label recognized devices whose parser is still pending.
