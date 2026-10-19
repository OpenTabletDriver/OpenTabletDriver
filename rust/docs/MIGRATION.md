# Migration scope and next gates

The current pre-release prioritizes the user's VEIKK S640 and the 50 MB process budget. It does not claim to have converted the entire upstream repository. The configuration tree contains 360 tablet definitions but uses 50 report-parser families, so broad model support requires a profile-driven device registry and explicit parser ports. See the [roadmap to broad tablet support](UNIVERSAL-ROADMAP.md).

| Upstream responsibility | Rust implementation | Status |
| --- | --- | --- |
| S640 and S640 V2 tablet definitions | `otd-core/config.rs` embeds original JSON | Done for two variants |
| VEIKK V1/V2 packet decoding | `otd-core/parser.rs` | 2,048 C# fixture comparisons pass; V1 hardware read verified |
| Absolute mapping, rotation, clip/limit | `otd-core/mapping.rs` | Implemented; numeric tests |
| Pressure threshold and mouse bindings | `otd-core/mapping.rs` | Implemented; pressure remapping preserves integer truncation |
| HID enumeration / report sizes | `otd-platform/windows.rs` SetupAPI + HID caps | Verified on connected S640 |
| Read/init/dispose | Stable overlapped IO + RAII | Read verified; write/init and suspend/reconnect need hardware checks |
| Windows absolute pointer/buttons | `SendInput` with virtual-desktop normalization | Implemented; not enabled during this session |
| Settings application/persistence | `otd-app/engine.rs`, `persistence.rs` | Validated replacement; failed save preserves prior state |
| UI | Native Win32 panel, bounded commands, 4 Hz snapshots | Apply/Stop/Start/Save/validation smoke test passes |
| CLI and measurements | Console executable + PowerShell gate | Implemented |
| Original .NET plugin compatibility | None | Pending; a pure Rust build cannot load existing plugins directly |
| Other tablets, relative output, artist pressure output | None | Pending |
| Linux/macOS and packaging/service/updater | None | Pending |

Before using this as the active driver:

1. Close the existing driver normally, preserving any unsaved profile changes.
2. Test initialization, hover, tip click, both side buttons, Stop/close releasing buttons, unplug/replug, and sleep/resume on the S640. Reopen the existing driver if the prototype does not work.
3. Compare physical pen-to-pointer behavior and latency with the same monitor, mapping, and polling conditions. The core benchmark is insufficient for that claim.
4. Run at least a 30-minute hardware-driven memory test, including UI edits and reconnects. Require both memory metrics to remain under 50 MB.

Subsequent work should port relative mode, finish settings import, and define a bounded plugin API before expanding tablet coverage. Each new parser needs raw-packet golden fixtures and hardware verification. Plugin/recording queues must have explicit capacity limits.

UI design stays independent: build the HTML mockup later, agree on the layout/flows, and implement it against the documented command/state model. A native implementation currently provides the measured memory margin. If an embedded browser is chosen, include all renderer/helper processes in a fresh budget measurement.
