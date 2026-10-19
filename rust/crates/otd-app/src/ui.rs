//! Native control shell. Layout is intentionally provisional until the HTML mockup.
//! No browser engine/GPU rendering loop; telemetry is refreshed at 4 Hz.
use crate::engine::{Command, Engine};
use otd_core::{DisplayRect, MouseBinding, Settings};
use std::{
    ffi::OsStr,
    io,
    mem::zeroed,
    path::PathBuf,
    ptr::{null, null_mut},
    time::{Duration, Instant},
};
use windows_sys::Win32::{
    Foundation::*,
    Graphics::Gdi::*,
    System::LibraryLoader::*,
    UI::{
        Controls::{
            DRAWITEMSTRUCT, EM_LIMITTEXT, ICC_BAR_CLASSES, INITCOMMONCONTROLSEX,
            InitCommonControlsEx, ODS_DISABLED, ODS_FOCUS, ODS_SELECTED, ODT_BUTTON, TBM_SETPOS,
            TBM_SETRANGE, TBS_HORZ, TBS_NOTICKS,
        },
        HiDpi::*,
        Input::KeyboardAndMouse::EnableWindow,
        Shell::*,
        WindowsAndMessaging::*,
    },
};

const DETECT: usize = 100;
const START: usize = 101;
const STOP: usize = 102;
const APPLY: usize = 103;
const SAVE: usize = 104;
const PRIMARY: usize = 105;
const DESKTOP: usize = 106;
const ASPECT: usize = 107;
const DEFAULTS: usize = 108;
const SETTINGS: usize = 109;
const FULL_AREA: usize = 110;
const AREA_SCALE: usize = 111;
const CLOSE_TO_TRAY: usize = 302;
const SETTINGS_DONE: usize = 303;
const TRAY_OPEN: usize = 304;
const TRAY_EXIT: usize = 305;
const CLIP: usize = 300;
const LIMIT: usize = 301;
const TRAY_CALLBACK: u32 = WM_APP + 1;
const TRAY_ICON_ID: u32 = 1;
const TBM_GETPOS: u32 = WM_USER;

struct Ui {
    engine: Engine,
    main_window: HWND,
    status: HWND,
    telemetry: HWND,
    footer: HWND,
    edits: Vec<HWND>,
    scale_slider: HWND,
    scale_readout: HWND,
    updating_scale: bool,
    bindings: Vec<HWND>,
    clip: HWND,
    limit: HWND,
    start: HWND,
    stop: HWND,
    scale: f32,
    settings: Settings,
    settings_window: HWND,
    close_to_tray: HWND,
    tray_added: bool,
    background_brush: HBRUSH,
    input_brush: HBRUSH,
    close_at: Option<Instant>,
    self_test: Option<u8>,
    test_error: Option<String>,
    screenshot: Option<PathBuf>,
}

fn w(s: &str) -> Vec<u16> {
    otd_platform::wide(OsStr::new(s))
}
fn text(hwnd: HWND, s: &str) {
    // SAFETY: live child control and terminated UTF-16 passed only for this call.
    unsafe {
        SetWindowTextW(hwnd, w(s).as_ptr());
    }
}
fn read_text(hwnd: HWND) -> String {
    let mut buffer = [0u16; 128];
    // SAFETY: stack buffer length is passed correctly; edit text is limited to 64 characters.
    let len = unsafe { GetWindowTextW(hwnd, buffer.as_mut_ptr(), buffer.len() as i32) };
    String::from_utf16_lossy(&buffer[..len.max(0) as usize])
}

impl Ui {
    fn scale_dimensions(&self, percent: f32) -> (f32, f32) {
        let width = read_text(self.edits[8]).parse::<f32>().unwrap_or(1920.0);
        let height = read_text(self.edits[9]).parse::<f32>().unwrap_or(1080.0);
        let ratio = if width > 0.0 && height > 0.0 {
            width / height
        } else {
            16.0 / 9.0
        };
        let mut max_width = 152.4;
        let mut max_height = max_width / ratio;
        if max_height > 101.6 {
            max_height = 101.6;
            max_width = max_height * ratio;
        }
        let scale = percent.clamp(1.0, 100.0) / 100.0;
        (max_width * scale, max_height * scale)
    }

    fn update_scale_control(&self) {
        if self.scale_slider.is_null() || self.edits.len() < 10 {
            return;
        }
        let width = read_text(self.edits[0]).parse::<f32>().ok();
        let height = read_text(self.edits[1]).parse::<f32>().ok();
        let Some((width, height)) = width.zip(height) else {
            text(self.scale_readout, "Custom");
            return;
        };
        let (max_width, max_height) = self.scale_dimensions(100.0);
        let percent = (width / max_width).min(height / max_height) * 100.0;
        let (expected_width, expected_height) = self.scale_dimensions(percent);
        if percent.is_finite()
            && (width - expected_width).abs() < 0.2
            && (height - expected_height).abs() < 0.2
        {
            let rounded = percent.round().clamp(1.0, 100.0) as u32;
            text(self.scale_readout, &format!("{rounded}%"));
            unsafe { SendMessageW(self.scale_slider, TBM_SETPOS, 1, rounded as isize) };
        } else {
            text(self.scale_readout, "Custom");
        }
    }

    fn apply_area_scale(&mut self, percent: u32) {
        let percent = percent.clamp(1, 100);
        let (width, height) = self.scale_dimensions(percent as f32);
        self.updating_scale = true;
        text(self.edits[0], &format!("{width:.1}"));
        text(self.edits[1], &format!("{height:.1}"));
        text(self.edits[2], "76.2");
        text(self.edits[3], "50.8");
        text(self.scale_readout, &format!("{percent}%"));
        unsafe {
            SendMessageW(self.scale_slider, TBM_SETPOS, 1, percent as isize);
            InvalidateRect(self.main_window, null(), 0);
        }
        self.updating_scale = false;
    }

    fn install_tray_icon(&mut self, parent: HWND, install: bool) -> Result<(), String> {
        // SAFETY: NOTIFYICONDATAW is zero-initialized and all borrowed strings and
        // handles remain valid for the duration of Shell_NotifyIconW.
        unsafe {
            let mut data: NOTIFYICONDATAW = zeroed();
            data.cbSize = std::mem::size_of::<NOTIFYICONDATAW>() as u32;
            data.hWnd = parent;
            data.uID = TRAY_ICON_ID;
            if install {
                data.uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP;
                data.uCallbackMessage = TRAY_CALLBACK;
                data.hIcon = LoadIconW(null_mut(), IDI_APPLICATION);
                let tip: Vec<u16> = w("OpenTabletDriver Rust — VEIKK S640")
                    .into_iter()
                    .take(data.szTip.len() - 1)
                    .collect();
                data.szTip[..tip.len()].copy_from_slice(&tip);
                if Shell_NotifyIconW(NIM_ADD, &data) == 0 {
                    return Err(io::Error::last_os_error().to_string());
                }
                self.tray_added = true;
            } else if self.tray_added {
                Shell_NotifyIconW(NIM_DELETE, &data);
                self.tray_added = false;
            }
        }
        Ok(())
    }

    fn open_settings(&mut self, parent: HWND) -> Result<(), String> {
        // Reuse an already open preferences window instead of creating duplicates.
        unsafe {
            if !self.settings_window.is_null() && IsWindow(self.settings_window) != 0 {
                ShowWindow(self.settings_window, SW_SHOWNORMAL);
                SetForegroundWindow(self.settings_window);
                return Ok(());
            }
            let class = w("OpenTabletDriverRustPanel");
            let window = CreateWindowExW(
                WS_EX_TOOLWINDOW,
                class.as_ptr(),
                w("Settings").as_ptr(),
                WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU,
                CW_USEDEFAULT,
                CW_USEDEFAULT,
                (340.0 * self.scale) as i32,
                (170.0 * self.scale) as i32,
                parent,
                null_mut(),
                GetModuleHandleW(null()),
                null(),
            );
            if window.is_null() {
                return Err(io::Error::last_os_error().to_string());
            }
            self.settings_window = window;
            SetWindowLongPtrW(window, GWLP_USERDATA, self as *mut Ui as isize);
            self.control(
                window,
                "STATIC",
                "Application behavior",
                0,
                0,
                [18, 16, 280, 22],
            );
            self.close_to_tray = self.control(
                window,
                "BUTTON",
                "Close to system tray",
                WS_TABSTOP | BS_AUTOCHECKBOX as u32,
                CLOSE_TO_TRAY,
                [18, 48, 280, 25],
            );
            self.control(
                window,
                "STATIC",
                "Keep the driver running when this window closes.",
                0,
                0,
                [38, 75, 275, 22],
            );
            self.control(
                window,
                "BUTTON",
                "Done",
                WS_TABSTOP,
                SETTINGS_DONE,
                [220, 105, 90, 28],
            );
            SendMessageW(
                self.close_to_tray,
                BM_SETCHECK,
                usize::from(self.settings.close_to_tray),
                0,
            );
            ShowWindow(window, SW_SHOW);
            SetForegroundWindow(window);
        }
        Ok(())
    }

    fn test_step(&mut self, hwnd: HWND) -> Result<(), String> {
        let Some(step) = self.self_test else {
            return Ok(());
        };
        let status = self.engine.read();
        // Queue normal button notifications; the real window procedure handles them.
        let click = |id| unsafe {
            PostMessageW(hwnd, WM_COMMAND, id, 0);
        };
        match step {
            0 => {
                text(self.edits[0], "100");
                click(APPLY);
            }
            1 => {
                if status.settings_revision == 0 {
                    return Ok(());
                }
                if status.settings.area.width_mm != 100.0 {
                    return Err("UI Apply did not update the engine".into());
                }
                text(self.edits[0], "0");
                if self.collect().is_ok() {
                    return Err("UI accepted an invalid tablet area".into());
                }
                text(self.edits[0], "100");
                click(STOP);
            }
            2 => {
                if status.running {
                    return Ok(());
                }
                click(START);
            }
            3 => {
                if !status.running {
                    return Ok(());
                }
                click(SAVE);
            }
            4 => {
                if status.settings_revision < 2 || !status.message.contains("saved successfully") {
                    return Ok(());
                }
                unsafe {
                    SendMessageW(self.scale_slider, TBM_SETPOS, 1, 50);
                    PostMessageW(hwnd, WM_HSCROLL, 0, self.scale_slider as LPARAM);
                }
            }
            5 => {
                let width = read_text(self.edits[0]).parse::<f32>().unwrap_or_default();
                let height = read_text(self.edits[1]).parse::<f32>().unwrap_or_default();
                if (width - 76.2).abs() > 0.1 || (height - 42.9).abs() > 0.1 {
                    return Err(format!(
                        "50% scale produced an unexpected area: {width:.1} by {height:.1} mm"
                    ));
                }
                self.close_at = Some(Instant::now() + Duration::from_millis(500));
            }
            _ => return Ok(()),
        }
        self.self_test = Some(step + 1);
        Ok(())
    }

    fn control(
        &self,
        parent: HWND,
        class: &str,
        title: &str,
        style: u32,
        id: usize,
        bounds: [i32; 4],
    ) -> HWND {
        let [x, y, width, height] = bounds.map(|n| (n as f32 * self.scale).round() as i32);
        // SAFETY: controls are parent-owned, classes are registered system controls;
        // fixed-size text buffers remain live through CreateWindowExW/SendMessageW.
        unsafe {
            let button_style = if class == "BUTTON" && style & BS_AUTOCHECKBOX as u32 == 0 {
                style | BS_OWNERDRAW as u32
            } else {
                style
            };
            let h = CreateWindowExW(
                0,
                w(class).as_ptr(),
                w(title).as_ptr(),
                WS_CHILD | WS_VISIBLE | button_style,
                x,
                y,
                width,
                height,
                parent,
                id as *mut _,
                null_mut(),
                null(),
            );
            SendMessageW(h, WM_SETFONT, GetStockObject(DEFAULT_GUI_FONT) as usize, 1);
            h
        }
    }

    fn initialize(&mut self, hwnd: HWND) -> Result<(), String> {
        self.control(
            hwnd,
            "STATIC",
            "OpenTabletDriver  ·  ALPHA BUILD",
            0,
            0,
            [24, 18, 670, 24],
        );
        self.control(
            hwnd,
            "STATIC",
            "VEIKK S640  |  Absolute tablet mapping",
            0,
            0,
            [24, 47, 670, 22],
        );
        self.control(
            hwnd,
            "BUTTON",
            "Settings",
            WS_TABSTOP,
            SETTINGS,
            [600, 16, 96, 28],
        );
        self.status = self.control(hwnd, "STATIC", "Ready", 0, 0, [24, 80, 680, 50]);
        self.control(
            hwnd,
            "BUTTON",
            "Detect",
            WS_TABSTOP,
            DETECT,
            [24, 138, 105, 30],
        );
        self.start = self.control(
            hwnd,
            "BUTTON",
            "Start driver",
            WS_TABSTOP,
            START,
            [140, 138, 120, 30],
        );
        self.stop = self.control(
            hwnd,
            "BUTTON",
            "Stop",
            WS_TABSTOP,
            STOP,
            [270, 138, 105, 30],
        );
        self.telemetry = self.control(hwnd, "STATIC", "", 0, 0, [24, 184, 680, 60]);

        self.control(
            hwnd,
            "STATIC",
            "TABLET AREA  ·  millimeters",
            0,
            0,
            [24, 250, 670, 22],
        );
        let fields = [
            ("Width", 374, 270),
            ("Height", 535, 270),
            ("X Offset", 374, 307),
            ("Y Offset", 535, 307),
            ("Rotation °", 374, 344),
            ("Tip click %", 535, 344),
            ("Screen X", 24, 489),
            ("Screen Y", 195, 489),
            ("Width px", 366, 489),
            ("Height px", 537, 489),
        ];
        for (i, (label, x, y)) in fields.into_iter().enumerate() {
            let label_width = if i < 6 { 76 } else { 72 };
            self.control(hwnd, "STATIC", label, 0, 0, [x, y + 4, label_width, 24]);
            let h = self.control(
                hwnd,
                "EDIT",
                "",
                WS_TABSTOP | WS_BORDER | ES_AUTOHSCROLL as u32,
                200 + i,
                [x + label_width, y, if i < 6 { 78 } else { 90 }, 25],
            );
            // SAFETY: EM_LIMITTEXT uses only an integer count.
            unsafe {
                SendMessageW(h, EM_LIMITTEXT, 64, 0);
            }
            self.edits.push(h);
        }
        self.control(
            hwnd,
            "BUTTON",
            "Reset to full area",
            WS_TABSTOP,
            FULL_AREA,
            [374, 389, 128, 29],
        );
        self.control(
            hwnd,
            "BUTTON",
            "Conform to display ratio",
            WS_TABSTOP,
            ASPECT,
            [510, 389, 190, 29],
        );
        self.control(hwnd, "STATIC", "Tablet scale", 0, 0, [374, 427, 110, 20]);
        self.scale_readout = self.control(hwnd, "STATIC", "50%", 0, 0, [634, 427, 62, 20]);
        self.scale_slider = self.control(
            hwnd,
            "msctls_trackbar32",
            "",
            WS_TABSTOP | TBS_HORZ | TBS_NOTICKS,
            AREA_SCALE,
            [374, 446, 326, 28],
        );
        // Trackbar range is 1–100%, with the half-area default selected.
        unsafe {
            SendMessageW(
                self.scale_slider,
                TBM_SETRANGE,
                1,
                ((100u32 << 16) | 1) as isize,
            );
            SendMessageW(self.scale_slider, TBM_SETPOS, 1, 50);
        }
        self.clip = self.control(
            hwnd,
            "BUTTON",
            "Clip to mapped area",
            WS_TABSTOP | BS_AUTOCHECKBOX as u32,
            CLIP,
            [24, 526, 210, 24],
        );
        self.limit = self.control(
            hwnd,
            "BUTTON",
            "Ignore outside area",
            WS_TABSTOP | BS_AUTOCHECKBOX as u32,
            LIMIT,
            [255, 526, 210, 24],
        );
        self.control(
            hwnd,
            "BUTTON",
            "Primary display",
            WS_TABSTOP,
            PRIMARY,
            [24, 556, 145, 29],
        );
        self.control(
            hwnd,
            "BUTTON",
            "All displays",
            WS_TABSTOP,
            DESKTOP,
            [180, 556, 135, 29],
        );
        self.control(
            hwnd,
            "BUTTON",
            "Defaults",
            WS_TABSTOP,
            DEFAULTS,
            [326, 556, 120, 29],
        );

        for (i, label) in ["Tip", "Pen button 1", "Pen button 2"]
            .into_iter()
            .enumerate()
        {
            let x = 24 + i as i32 * 224;
            self.control(hwnd, "STATIC", label, 0, 0, [x, 610, 108, 24]);
            let combo = self.control(
                hwnd,
                "COMBOBOX",
                "",
                WS_TABSTOP | CBS_DROPDOWNLIST as u32 | WS_VSCROLL,
                400 + i,
                [x + 105, 605, 108, 140],
            );
            for option in ["Disabled", "Left", "Right", "Middle"] {
                // SAFETY: CB_ADDSTRING copies the terminated string before returning.
                unsafe {
                    SendMessageW(combo, CB_ADDSTRING, 0, w(option).as_ptr() as isize);
                }
            }
            self.bindings.push(combo);
        }
        self.control(
            hwnd,
            "BUTTON",
            "Apply",
            WS_TABSTOP,
            APPLY,
            [24, 660, 115, 32],
        );
        self.control(
            hwnd,
            "BUTTON",
            "Apply and save",
            WS_TABSTOP,
            SAVE,
            [150, 660, 150, 32],
        );
        self.footer = self.control(hwnd, "STATIC", "", 0, 0, [320, 653, 385, 52]);
        if self
            .edits
            .iter()
            .chain(self.bindings.iter())
            .any(|h| h.is_null())
            || self.status.is_null()
            || self.telemetry.is_null()
            || self.scale_slider.is_null()
            || self.scale_readout.is_null()
        {
            return Err("Could not create Windows controls".into());
        }
        self.fill();
        self.refresh();
        Ok(())
    }

    fn fill(&self) {
        let s = &self.settings;
        let values = [
            s.area.width_mm.to_string(),
            s.area.height_mm.to_string(),
            s.area.center_x_mm.to_string(),
            s.area.center_y_mm.to_string(),
            s.area.rotation_deg.to_string(),
            s.tip_threshold_percent.to_string(),
            s.display.x.to_string(),
            s.display.y.to_string(),
            s.display.width.to_string(),
            s.display.height.to_string(),
        ];
        for (h, value) in self.edits.iter().zip(values) {
            text(*h, &value);
        }
        self.update_scale_control();
        // SAFETY: control-specific messages with integer arguments only.
        unsafe {
            SendMessageW(self.clip, BM_SETCHECK, usize::from(s.clip_to_area), 0);
            SendMessageW(
                self.limit,
                BM_SETCHECK,
                usize::from(s.ignore_outside_area),
                0,
            );
            for (h, binding) in
                self.bindings
                    .iter()
                    .zip([s.tip_binding, s.pen_bindings[0], s.pen_bindings[1]])
            {
                let i = match binding {
                    MouseBinding::Disabled => 0,
                    MouseBinding::Left => 1,
                    MouseBinding::Right => 2,
                    MouseBinding::Middle => 3,
                };
                SendMessageW(*h, CB_SETCURSEL, i, 0);
            }
        }
    }

    fn collect(&self) -> Result<Settings, String> {
        let v: Vec<String> = self.edits.iter().map(|h| read_text(*h)).collect();
        let float = |i: usize| {
            v[i].trim()
                .parse::<f32>()
                .map_err(|_| format!("Invalid numeric value: {}", v[i]))
        };
        let integer = |i: usize| {
            v[i].trim()
                .parse::<i32>()
                .map_err(|_| format!("Expected integer: {}", v[i]))
        };
        let mut s = self.settings.clone();
        s.area.width_mm = float(0)?;
        s.area.height_mm = float(1)?;
        s.area.center_x_mm = float(2)?;
        s.area.center_y_mm = float(3)?;
        s.area.rotation_deg = float(4)?;
        s.tip_threshold_percent = float(5)?;
        s.display = DisplayRect {
            x: integer(6)?,
            y: integer(7)?,
            width: u32::try_from(integer(8)?).map_err(|_| "Display width must be positive")?,
            height: u32::try_from(integer(9)?).map_err(|_| "Display height must be positive")?,
        };
        // SAFETY: BM_GETCHECK and CB_GETCURSEL return integer values.
        unsafe {
            s.clip_to_area = SendMessageW(self.clip, BM_GETCHECK, 0, 0) != 0;
            s.ignore_outside_area = SendMessageW(self.limit, BM_GETCHECK, 0, 0) != 0;
            for (i, h) in self.bindings.iter().enumerate() {
                let binding = match SendMessageW(*h, CB_GETCURSEL, 0, 0) {
                    1 => MouseBinding::Left,
                    2 => MouseBinding::Right,
                    3 => MouseBinding::Middle,
                    _ => MouseBinding::Disabled,
                };
                if i == 0 {
                    s.tip_binding = binding;
                } else {
                    s.pen_bindings[i - 1] = binding;
                }
            }
        }
        s.validate().map_err(str::to_owned)?;
        Ok(s)
    }

    fn action(&mut self, id: usize) -> Result<(), String> {
        match id {
            SETTINGS => self.open_settings(self.main_window)?,
            CLOSE_TO_TRAY => {
                // The preference is saved immediately so the next launch behaves the same.
                let enabled = unsafe { SendMessageW(self.close_to_tray, BM_GETCHECK, 0, 0) != 0 };
                let previous = self.settings.close_to_tray;
                let mut candidate = self.settings.clone();
                candidate.close_to_tray = enabled;
                self.install_tray_icon(self.main_window, enabled)?;
                if let Err(error) = self.engine.command(Command::Save(candidate.clone())) {
                    let _ = self.install_tray_icon(self.main_window, previous);
                    unsafe {
                        SendMessageW(self.close_to_tray, BM_SETCHECK, usize::from(previous), 0);
                    }
                    return Err(error);
                }
                self.settings = candidate;
                text(
                    self.status,
                    if enabled {
                        "Close to system tray is enabled. This preference was saved."
                    } else {
                        "Close to system tray is disabled. This preference was saved."
                    },
                );
            }
            SETTINGS_DONE => unsafe {
                if !self.settings_window.is_null() {
                    DestroyWindow(self.settings_window);
                }
            },
            FULL_AREA => {
                self.settings = self.collect()?;
                self.settings.area.width_mm = 152.4;
                self.settings.area.height_mm = 101.6;
                self.settings.area.center_x_mm = 76.2;
                self.settings.area.center_y_mm = 50.8;
                self.settings.area.rotation_deg = 0.0;
                self.fill();
                text(self.status, "Tablet area reset to the full S640 surface.");
            }
            DETECT => self.engine.command(Command::Detect)?,
            START => self.engine.command(Command::Start)?,
            STOP => self.engine.command(Command::Stop)?,
            APPLY | SAVE => {
                let candidate = self.collect()?;
                self.engine.command(if id == SAVE {
                    Command::Save(candidate.clone())
                } else {
                    Command::Apply(candidate.clone())
                })?;
                self.settings = candidate;
            }
            PRIMARY | DESKTOP | ASPECT => {
                self.settings = self.collect()?;
                if id == PRIMARY {
                    self.settings.display = otd_platform::primary_display();
                }
                if id == DESKTOP {
                    self.settings.display = otd_platform::virtual_desktop();
                }
                if id == ASPECT {
                    let ratio =
                        self.settings.display.width as f32 / self.settings.display.height as f32;
                    let mut width = 152.4;
                    let mut height = width / ratio;
                    if height > 101.6 {
                        height = 101.6;
                        width = height * ratio;
                    }
                    self.settings.area.width_mm = width;
                    self.settings.area.height_mm = height;
                    self.settings.area.center_x_mm = 76.2;
                    self.settings.area.center_y_mm = 50.8;
                    self.apply_area_scale(100);
                }
                self.fill();
                text(
                    self.status,
                    "Editor changed. Press Apply or Apply and save to use these values.",
                );
            }
            DEFAULTS => {
                self.settings = Settings {
                    display: otd_platform::primary_display(),
                    ..Settings::default()
                };
                self.fill();
                text(
                    self.status,
                    "Defaults loaded into editor. Press Apply to use them.",
                );
            }
            _ => {}
        }
        Ok(())
    }

    fn refresh(&self) {
        let s = self.engine.read();
        text(
            self.status,
            &format!(
                "{} | {}\r\n{}",
                if s.running { "RUNNING" } else { "STOPPED" },
                s.tablet,
                s.message
            ),
        );
        text(
            self.telemetry,
            &format!(
                "Input: {:.0} reports/s   |   Received: {}   |   Malformed: {}\r\nCore processing: {:.3} us avg / {:.3} us max   |   Pressure: {:.1}%   |   Position: {:.0}, {:.0}",
                s.reports_per_second,
                s.reports,
                s.malformed,
                s.processing_us,
                s.max_processing_us,
                s.pressure * 100.0,
                s.x,
                s.y
            ),
        );
        if let Ok(memory) = otd_platform::memory_usage() {
            text(
                self.footer,
                &format!(
                    "Whole process incl. UI: {:.2} MB working set / {:.2} MB private (target < 50 MB)\r\nMouse output only. Drawing pressure injection and original .NET plugins are pending.",
                    memory.working_set_bytes as f64 / 1_000_000.0,
                    memory.private_bytes as f64 / 1_000_000.0
                ),
            );
        }
        // SAFETY: valid child HWNDs; only adjusts enabled state.
        unsafe {
            EnableWindow(self.start, i32::from(!s.running));
            EnableWindow(self.stop, i32::from(s.running));
        }
    }
}

unsafe fn draw_tablet_preview(hwnd: HWND, ui: &Ui) {
    // SAFETY: BeginPaint provides a live DC that is paired with EndPaint.
    unsafe {
        let mut paint: PAINTSTRUCT = zeroed();
        let dc = BeginPaint(hwnd, &mut paint);
        draw_tablet_geometry(dc, ui, POINT { x: 0, y: 0 });
        EndPaint(hwnd, &paint);
    }
}

unsafe fn draw_tablet_geometry(dc: HDC, ui: &Ui, offset: POINT) {
    // SAFETY: the caller provides a live paint/print DC; all GDI objects are restored
    // and released before returning.
    unsafe {
        let scaled = |value: i32| (value as f32 * ui.scale).round() as i32;
        let tablet = RECT {
            left: scaled(42) + offset.x,
            top: scaled(286) + offset.y,
            right: scaled(333) + offset.x,
            bottom: scaled(480) + offset.y,
        };
        let surface = CreateSolidBrush(0x001b1b1b);
        let border = CreateSolidBrush(0x00424242);
        FillRect(dc, &tablet, surface);
        SetBkMode(dc, TRANSPARENT as i32);
        SetTextColor(dc, 0x00eeeeee);
        let label = w("VEIKK S640");
        let mut label_box = RECT {
            left: scaled(51) + offset.x,
            top: scaled(266) + offset.y,
            right: scaled(180) + offset.x,
            bottom: scaled(284) + offset.y,
        };
        DrawTextW(
            dc,
            label.as_ptr(),
            -1,
            &mut label_box,
            DT_LEFT | DT_SINGLELINE,
        );

        let values: Vec<f32> = ui
            .edits
            .iter()
            .take(5)
            .map(|h| read_text(*h).parse().unwrap_or(0.0))
            .collect();
        if values.len() == 5 && values[0] > 0.0 && values[1] > 0.0 {
            let sx = (tablet.right - tablet.left) as f32 / 152.4;
            let sy = (tablet.bottom - tablet.top) as f32 / 101.6;
            let cx = tablet.left as f32 + values[2] * sx;
            let cy = tablet.top as f32 + values[3] * sy;
            let half_w = values[0].min(152.4) * sx * 0.5;
            let half_h = values[1].min(101.6) * sy * 0.5;
            let angle = values[4].to_radians();
            let (sin, cos) = angle.sin_cos();
            let corners = [
                (-half_w, -half_h),
                (half_w, -half_h),
                (half_w, half_h),
                (-half_w, half_h),
            ];
            let mut points: [POINT; 4] = corners.map(|(x, y)| POINT {
                x: (cx + x * cos - y * sin).round() as i32,
                y: (cy + x * sin + y * cos).round() as i32,
            });
            let active = CreateSolidBrush(0x00ffffff);
            let pen = CreatePen(PS_SOLID, scaled(1), 0x00ffffff);
            let old_brush = SelectObject(dc, active);
            let old_pen = SelectObject(dc, pen);
            Polygon(dc, points.as_mut_ptr(), points.len() as i32);
            SelectObject(dc, old_brush);
            SelectObject(dc, old_pen);
            DeleteObject(active);
            DeleteObject(pen);

            let ratio_text = format!("{:.2}", values[0] / values[1]);
            let ratio = w(&ratio_text);
            let mut ratio_box = RECT {
                left: (cx - half_w).round() as i32,
                top: (cy - half_h).round() as i32,
                right: (cx + half_w).round() as i32,
                bottom: (cy + half_h).round() as i32,
            };
            SetTextColor(dc, 0x00000000);
            DrawTextW(
                dc,
                ratio.as_ptr(),
                -1,
                &mut ratio_box,
                DT_CENTER | DT_VCENTER | DT_SINGLELINE,
            );
        }
        FrameRect(dc, &tablet, border);
        DeleteObject(surface);
        DeleteObject(border);
    }
}

unsafe fn draw_button(item: &DRAWITEMSTRUCT, scale: f32) {
    // SAFETY: DRAWITEMSTRUCT is supplied by Windows for WM_DRAWITEM; all GDI objects
    // are restored and deleted before returning.
    unsafe {
        let dc = item.hDC;
        let rect = item.rcItem;
        let primary = item.CtlID == START as u32 || item.CtlID == SAVE as u32;
        let disabled = item.itemState & ODS_DISABLED != 0;
        let pressed = item.itemState & ODS_SELECTED != 0;
        let filled = (primary || pressed) && !disabled;
        let background = if filled { 0x00ffffff } else { 0x00101010 };
        let foreground = if filled {
            0x00000000
        } else if disabled {
            0x00777777
        } else {
            0x00ffffff
        };
        let outline = if disabled { 0x00525252 } else { 0x00ffffff };
        let brush = CreateSolidBrush(background);
        let pen = CreatePen(PS_SOLID, scale.max(1.0).round() as i32, outline);
        let old_brush = SelectObject(dc, brush);
        let old_pen = SelectObject(dc, pen);
        RoundRect(
            dc,
            rect.left,
            rect.top,
            rect.right,
            rect.bottom,
            (8.0 * scale) as i32,
            (8.0 * scale) as i32,
        );
        SelectObject(dc, old_brush);
        SelectObject(dc, old_pen);
        SetBkMode(dc, TRANSPARENT as i32);
        SetTextColor(dc, foreground);
        let mut buffer = [0u16; 128];
        let length = GetWindowTextW(item.hwndItem, buffer.as_mut_ptr(), buffer.len() as i32);
        let mut text_rect = rect;
        let inset = (6.0 * scale) as i32;
        text_rect.left += inset;
        text_rect.right -= inset;
        DrawTextW(
            dc,
            buffer.as_ptr(),
            length,
            &mut text_rect,
            DT_CENTER | DT_VCENTER | DT_SINGLELINE,
        );
        if item.itemState & ODS_FOCUS != 0 {
            let mut focus = rect;
            InflateRect(&mut focus, -(3.0 * scale) as i32, -(3.0 * scale) as i32);
            DrawFocusRect(dc, &focus);
        }
        DeleteObject(brush);
        DeleteObject(pen);
    }
}

unsafe fn show_tray_menu(hwnd: HWND) {
    // SAFETY: this short-lived native menu owns no borrowed strings after AppendMenuW
    // copies them and is destroyed immediately after TrackPopupMenu returns.
    unsafe {
        let menu = CreatePopupMenu();
        if menu.is_null() {
            return;
        }
        AppendMenuW(
            menu,
            MF_STRING,
            TRAY_OPEN,
            w("Open OpenTabletDriver Rust").as_ptr(),
        );
        AppendMenuW(menu, MF_SEPARATOR, 0, null());
        AppendMenuW(menu, MF_STRING, TRAY_EXIT, w("Exit").as_ptr());
        let mut point: POINT = zeroed();
        GetCursorPos(&mut point);
        SetForegroundWindow(hwnd);
        let selected = TrackPopupMenu(
            menu,
            TPM_RETURNCMD | TPM_NONOTIFY,
            point.x,
            point.y,
            0,
            hwnd,
            null(),
        );
        DestroyMenu(menu);
        if selected == TRAY_OPEN as i32 {
            PostMessageW(hwnd, TRAY_CALLBACK + 1, 0, 0);
        } else if selected == TRAY_EXIT as i32 {
            PostMessageW(hwnd, TRAY_CALLBACK + 2, 0, 0);
        }
        PostMessageW(hwnd, WM_NULL, 0, 0);
    }
}

unsafe extern "system" fn window_proc(
    hwnd: HWND,
    message: u32,
    wparam: WPARAM,
    lparam: LPARAM,
) -> LRESULT {
    // SAFETY: pointer is installed before showing the window and cleared before Box drop.
    unsafe {
        let pointer = GetWindowLongPtrW(hwnd, GWLP_USERDATA) as *mut Ui;
        if !pointer.is_null() {
            let ui = &mut *pointer;
            if hwnd == ui.main_window && message == TRAY_CALLBACK {
                match lparam as u32 {
                    WM_LBUTTONUP | WM_LBUTTONDBLCLK => {
                        PostMessageW(hwnd, TRAY_CALLBACK + 1, 0, 0);
                    }
                    WM_RBUTTONUP | WM_CONTEXTMENU => show_tray_menu(hwnd),
                    _ => {}
                }
                return 0;
            }
            if hwnd == ui.main_window && message == TRAY_CALLBACK + 1 {
                ShowWindow(hwnd, SW_RESTORE);
                SetForegroundWindow(hwnd);
                return 0;
            }
            if hwnd == ui.main_window && message == TRAY_CALLBACK + 2 {
                if !ui.settings_window.is_null() && IsWindow(ui.settings_window) != 0 {
                    DestroyWindow(ui.settings_window);
                }
                let _ = ui.install_tray_icon(hwnd, false);
                DestroyWindow(hwnd);
                return 0;
            }
            if hwnd == ui.main_window && message == WM_PRINT {
                let result = DefWindowProcW(hwnd, message, wparam, lparam);
                let mut client_origin = POINT { x: 0, y: 0 };
                let mut window_rect: RECT = zeroed();
                ClientToScreen(hwnd, &mut client_origin);
                GetWindowRect(hwnd, &mut window_rect);
                draw_tablet_geometry(
                    wparam as HDC,
                    ui,
                    POINT {
                        x: client_origin.x - window_rect.left,
                        y: client_origin.y - window_rect.top,
                    },
                );
                return result;
            }
            match message {
                WM_COMMAND if wparam >> 16 == 0 => {
                    if let Err(e) = ui.action(wparam & 0xffff) {
                        MessageBoxW(
                            hwnd,
                            w(&e).as_ptr(),
                            w("Settings / driver error").as_ptr(),
                            MB_OK | MB_ICONERROR,
                        );
                    }
                    return 0;
                }
                WM_COMMAND => {
                    if !ui.updating_scale {
                        ui.update_scale_control();
                    }
                    InvalidateRect(hwnd, null(), 0);
                }
                WM_HSCROLL if lparam as HWND == ui.scale_slider => {
                    let percent = SendMessageW(ui.scale_slider, TBM_GETPOS, 0, 0) as u32;
                    ui.apply_area_scale(percent);
                    text(
                        ui.status,
                        "Tablet scale updated. Press Apply or Apply and save to use it.",
                    );
                    return 0;
                }
                WM_ERASEBKGND => {
                    let mut client: RECT = zeroed();
                    GetClientRect(hwnd, &mut client);
                    FillRect(wparam as HDC, &client, ui.background_brush);
                    return 1;
                }
                WM_CTLCOLORSTATIC | WM_CTLCOLOREDIT | WM_CTLCOLORLISTBOX | WM_CTLCOLORBTN => {
                    let dc = wparam as HDC;
                    SetTextColor(dc, 0x00eeeeee);
                    let edit = message == WM_CTLCOLOREDIT || message == WM_CTLCOLORLISTBOX;
                    SetBkColor(dc, if edit { 0x00141414 } else { 0x00000000 });
                    return if edit {
                        ui.input_brush as isize
                    } else {
                        ui.background_brush as isize
                    };
                }
                WM_DRAWITEM => {
                    let item = &mut *(lparam as *mut DRAWITEMSTRUCT);
                    if !item.hwndItem.is_null() && item.CtlType == ODT_BUTTON {
                        draw_button(item, ui.scale);
                        return 1;
                    }
                }
                WM_PAINT if hwnd == ui.main_window => {
                    draw_tablet_preview(hwnd, ui);
                    return 0;
                }
                WM_TIMER => {
                    ui.refresh();
                    if let Err(error) = ui.test_step(hwnd) {
                        ui.test_error = Some(error);
                        PostMessageW(hwnd, WM_CLOSE, 0, 0);
                    }
                    if (ui.self_test.is_none() || ui.self_test == Some(6))
                        && let Some(path) = ui.screenshot.take()
                        && let Err(error) = crate::capture::window(hwnd, &path)
                    {
                        ui.test_error = Some(error.to_string());
                        PostMessageW(hwnd, WM_CLOSE, 0, 0);
                    }
                    if ui
                        .close_at
                        .is_some_and(|deadline| Instant::now() >= deadline)
                    {
                        PostMessageW(hwnd, WM_CLOSE, 0, 0);
                    }
                    return 0;
                }
                WM_CLOSE if hwnd != ui.main_window => {
                    DestroyWindow(hwnd);
                    return 0;
                }
                WM_CLOSE => {
                    if ui.settings.close_to_tray && ui.close_at.is_none() && ui.self_test.is_none()
                    {
                        ShowWindow(hwnd, SW_HIDE);
                        return 0;
                    }
                    let _ = ui.install_tray_icon(hwnd, false);
                    if !ui.settings_window.is_null() && IsWindow(ui.settings_window) != 0 {
                        DestroyWindow(ui.settings_window);
                    }
                    DestroyWindow(hwnd);
                    return 0;
                }
                WM_DESTROY => {
                    if hwnd == ui.main_window {
                        let _ = ui.install_tray_icon(hwnd, false);
                        if !ui.settings_window.is_null() && IsWindow(ui.settings_window) != 0 {
                            DestroyWindow(ui.settings_window);
                        }
                        KillTimer(hwnd, 1);
                        DeleteObject(ui.background_brush);
                        DeleteObject(ui.input_brush);
                        PostQuitMessage(0);
                    } else if hwnd == ui.settings_window {
                        ui.settings_window = null_mut();
                        ui.close_to_tray = null_mut();
                    }
                    return 0;
                }
                _ => {}
            }
        }
        DefWindowProcW(hwnd, message, wparam, lparam)
    }
}

pub fn run(
    engine: Engine,
    close_after: Option<Duration>,
    hidden: bool,
    self_test: bool,
    screenshot: Option<PathBuf>,
) -> Result<(), String> {
    let settings = engine.read().settings;
    // SAFETY: class callback remains valid for the process lifetime; the boxed state
    // stays alive through the entire message loop. Windows owns child controls.
    unsafe {
        let common_controls = INITCOMMONCONTROLSEX {
            dwSize: std::mem::size_of::<INITCOMMONCONTROLSEX>() as u32,
            dwICC: ICC_BAR_CLASSES,
        };
        if InitCommonControlsEx(&common_controls) == 0 {
            return Err(io::Error::last_os_error().to_string());
        }
        let scale = GetDpiForSystem() as f32 / 96.0;
        let instance = GetModuleHandleW(null());
        let class = w("OpenTabletDriverRustPanel");
        let wc = WNDCLASSW {
            lpfnWndProc: Some(window_proc),
            hInstance: instance,
            hCursor: LoadCursorW(null_mut(), IDC_ARROW),
            hbrBackground: (COLOR_WINDOW + 1) as HBRUSH,
            lpszClassName: class.as_ptr(),
            ..zeroed()
        };
        if RegisterClassW(&wc) == 0 {
            return Err(io::Error::last_os_error().to_string());
        }
        let hwnd = CreateWindowExW(
            0,
            class.as_ptr(),
            w("OpenTabletDriver Rust — S640 alpha").as_ptr(),
            WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX,
            CW_USEDEFAULT,
            CW_USEDEFAULT,
            (740.0 * scale) as i32,
            (760.0 * scale) as i32,
            null_mut(),
            null_mut(),
            instance,
            null(),
        );
        if hwnd.is_null() {
            return Err(io::Error::last_os_error().to_string());
        }
        let mut ui = Box::new(Ui {
            engine,
            main_window: hwnd,
            status: null_mut(),
            telemetry: null_mut(),
            footer: null_mut(),
            edits: Vec::with_capacity(10),
            scale_slider: null_mut(),
            scale_readout: null_mut(),
            updating_scale: false,
            bindings: Vec::with_capacity(3),
            clip: null_mut(),
            limit: null_mut(),
            start: null_mut(),
            stop: null_mut(),
            scale,
            settings,
            settings_window: null_mut(),
            close_to_tray: null_mut(),
            tray_added: false,
            background_brush: CreateSolidBrush(0x00000000),
            input_brush: CreateSolidBrush(0x00141414),
            close_at: close_after.map(|d| Instant::now() + d),
            self_test: self_test.then_some(0),
            test_error: None,
            screenshot,
        });
        SetWindowLongPtrW(hwnd, GWLP_USERDATA, &mut *ui as *mut Ui as isize);
        if let Err(e) = ui.initialize(hwnd) {
            SetWindowLongPtrW(hwnd, GWLP_USERDATA, 0);
            DestroyWindow(hwnd);
            return Err(e);
        }
        if ui.settings.close_to_tray
            && let Err(e) = ui.install_tray_icon(hwnd, true)
        {
            SetWindowLongPtrW(hwnd, GWLP_USERDATA, 0);
            DestroyWindow(hwnd);
            return Err(format!("Could not create system tray icon: {e}"));
        }
        if SetTimer(hwnd, 1, 250, None) == 0 {
            SetWindowLongPtrW(hwnd, GWLP_USERDATA, 0);
            DestroyWindow(hwnd);
            return Err("Unable to create UI refresh timer".into());
        }
        if hidden {
            // Realize/paint all controls without taking focus or covering the user's work.
            SetWindowPos(
                hwnd,
                null_mut(),
                -20000,
                -20000,
                0,
                0,
                SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE,
            );
        }
        ShowWindow(hwnd, if hidden { SW_SHOWNOACTIVATE } else { SW_SHOW });
        let mut msg: MSG = zeroed();
        loop {
            match GetMessageW(&mut msg, null_mut(), 0, 0) {
                0 => break,
                -1 => {
                    DestroyWindow(hwnd);
                    return Err(io::Error::last_os_error().to_string());
                }
                _ => {
                    if IsDialogMessageW(hwnd, &msg) == 0 {
                        TranslateMessage(&msg);
                        DispatchMessageW(&msg);
                    }
                }
            }
        }
        // WM_DESTROY has already destroyed hwnd; the pointer is no longer reachable.
        let result = if let Some(error) = ui.test_error.take() {
            Err(error)
        } else if self_test && ui.self_test != Some(6) {
            Err("UI self-test timed out".into())
        } else {
            Ok(())
        };
        drop(ui);
        result
    }
}
