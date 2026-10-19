# HTML UI preview

Open `index.html` directly in a browser. This is a static visual prototype for the future Rust UI. Controls are illustrative and telemetry is simulated; the page does not connect to a tablet or send input to Windows.

The preview uses inline vector drawings and local CSS/JavaScript without network requests. It does not add a browser runtime to the Rust driver or affect its memory measurements. The production UI should keep telemetry bounded and refresh at the existing 4 Hz snapshot cadence. A production webview counts toward the 50 MB total process budget and needs its own measurement before adoption.

The current visual theme uses a black background, white primary controls and foreground accents, and neutral gray borders and secondary labels.

The tablet area preview has Landscape and Portrait presets plus independent width and height coverage sliders. Both percentages default to 50%; changing them updates the preview rectangle and millimeter values.
