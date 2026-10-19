# HTML UI design reference

Open `index.html` directly in a browser to view the design reference. Its controls and telemetry are illustrative; the page does not connect to a tablet or send input to Windows. The Rust app uses native Windows controls and draws its tablet mapping preview directly, keeping the tablet driver and UI in one lightweight process.

The reference uses local CSS/JavaScript without network requests. The application does not embed a browser runtime. The production UI keeps telemetry bounded and refreshes at the existing 4 Hz snapshot cadence.

The current visual theme uses a black background, white primary controls and foreground accents, and neutral gray borders and secondary labels.

The tablet area reference shows the tablet bounds, mapped rectangle, center offsets, rotation, aspect ratio, lock, width and height controls, plus reset and conform actions. The live Rust app connects its native area controls to the existing settings engine and redraws the mapped region as values change.
