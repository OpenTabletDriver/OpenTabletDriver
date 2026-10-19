# UI contract / version 1

The current native panel and engine live in one process. `Command` and `Snapshot` in `crates/otd-app/src/engine.rs` are separate from Windows controls. There is **no HTTP server, IPC endpoint, or HTML interface** in this milestone.

The future HTML mockup should cover:

- Connected tablet / disconnected / stopped / running / failed states.
- Detect, Start, and Stop with clear output status.
- A tablet-area editor: width, height, center, rotation, clipping, limiting, aspect ratio.
- Monitor mapping with negative monitor origins and physical pixels.
- Tip threshold and tip/pen mouse bindings.
- Apply separately from Apply and save, with failure feedback and unsaved-change state.
- Read-only pressure/position test visualization, clearly separated from pressure injection support.
- Memory, rate, processing telemetry, and bounded diagnostic history.
- Keyboard navigation, proper labels, scaling, and readable error messages.

Commands serialize as tagged objects:

```json
{"command":"detect"}
{"command":"start"}
{"command":"stop"}
{"command":"apply","settings":{"schema_version":1}}
{"command":"save","settings":{"schema_version":1}}
```

The settings objects above are abbreviated: all fields in `otd-core/settings.rs` are required. Unknown fields/schemas and invalid finite-number/range constraints are rejected. UI adapters must validate the complete payload before enqueueing commands. A future external bridge must add authentication, payload bounds, protocol negotiation, and lifecycle rules; these local enum messages are not by themselves a remotely exposed API.

Snapshots contain protocol version, running/connected/simulation state, tablet and last message, report/malformed counts, report rate, core processing times, pressure/position, accepted settings, and a settings revision counter. Snapshots publish at most 4 Hz during active input; raw input packets never pass through the UI command channel. Memory is read from the whole process by the current panel.

HTML can be used to prototype the layout without committing to a browser-based runtime. The existing native panel is a functional test shell, not the final visual design.
