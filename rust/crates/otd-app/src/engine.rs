use otd_core::{Mapper, Report, Settings, Variant, parser::parse};
use otd_platform::{PointerOutput, TabletDevice};
use serde::{Deserialize, Serialize};
use std::{
    path::PathBuf,
    sync::{
        Arc, Mutex,
        mpsc::{self, Receiver, SyncSender},
    },
    thread::{self, JoinHandle},
    time::{Duration, Instant},
};

/// UI-independent commands. A future HTML bridge can use this versioned model.
#[derive(Debug, Serialize, Deserialize)]
#[serde(tag = "command", content = "settings", rename_all = "snake_case")]
pub enum Command {
    Detect,
    Start,
    Stop,
    Apply(Settings),
    Save(Settings),
    Shutdown,
}

#[derive(Clone, Debug, Serialize)]
pub struct Snapshot {
    pub protocol_version: u32,
    pub running: bool,
    pub simulation: bool,
    pub connected: bool,
    pub tablet: String,
    pub message: String,
    pub reports: u64,
    pub malformed: u64,
    pub reports_per_second: f64,
    pub processing_us: f64,
    pub max_processing_us: f64,
    pub x: f32,
    pub y: f32,
    pub pressure: f32,
    pub settings: Settings,
    pub settings_revision: u64,
}

pub struct Engine {
    pub commands: SyncSender<Command>,
    pub snapshot: Arc<Mutex<Snapshot>>,
    worker: Option<JoinHandle<()>>,
}

impl Engine {
    pub fn spawn(
        settings: Settings,
        path: PathBuf,
        simulation: bool,
        initial_message: String,
    ) -> Self {
        let snapshot = Snapshot {
            protocol_version: 1,
            running: simulation,
            simulation,
            connected: simulation,
            tablet: if simulation {
                "VEIKK S640 (synthetic input)"
            } else {
                "VEIKK S640 / S640 V2"
            }
            .into(),
            message: if simulation {
                "Synthetic 1000 Hz input; no USB initialization or pointer injection".into()
            } else {
                initial_message
            },
            reports: 0,
            malformed: 0,
            reports_per_second: 0.0,
            processing_us: 0.0,
            max_processing_us: 0.0,
            x: 0.0,
            y: 0.0,
            pressure: 0.0,
            settings: settings.clone(),
            settings_revision: 0,
        };
        let shared = Arc::new(Mutex::new(snapshot.clone()));
        let (tx, rx) = mpsc::sync_channel(16);
        let clone = shared.clone();
        let worker = thread::Builder::new()
            .name("otd-input".into())
            .stack_size(256 * 1024)
            .spawn(move || run(rx, clone, snapshot, path))
            .expect("Cannot start tablet input thread");
        Self {
            commands: tx,
            snapshot: shared,
            worker: Some(worker),
        }
    }
    pub fn read(&self) -> Snapshot {
        self.snapshot
            .lock()
            .unwrap_or_else(|e| e.into_inner())
            .clone()
    }
    pub fn command(&self, command: Command) -> Result<(), String> {
        self.commands.try_send(command).map_err(|e| e.to_string())
    }
}

impl Drop for Engine {
    fn drop(&mut self) {
        // Bounded channel and <=100 ms native read wait; drain commands before each read.
        let _ = self.commands.send(Command::Shutdown);
        if let Some(worker) = self.worker.take() {
            let _ = worker.join();
        }
    }
}

fn publish(shared: &Mutex<Snapshot>, current: &Snapshot) {
    *shared.lock().unwrap_or_else(|e| e.into_inner()) = current.clone();
}

fn make_mapper(settings: Settings) -> Result<Mapper, &'static str> {
    let c = Variant::S640
        .configuration()
        .map_err(|_| "Invalid embedded S640 configuration")?;
    Mapper::new(
        settings,
        c.specifications.digitizer,
        c.specifications.pen.max_pressure,
    )
}

fn run(rx: Receiver<Command>, shared: Arc<Mutex<Snapshot>>, mut status: Snapshot, path: PathBuf) {
    let mut mapper = match make_mapper(status.settings.clone()) {
        Ok(m) => m,
        Err(e) => {
            status.message = e.into();
            publish(&shared, &status);
            return;
        }
    };
    let mut device: Option<TabletDevice> = None;
    let mut pointer = PointerOutput::default();
    let mut packet = [0u8; 64];
    let mut next_detection = Instant::now();
    let mut next_tick = Instant::now();
    let mut last_publish = Instant::now();
    let mut reports_since_publish = 0u64;
    let mut processing_ns = 0u128;
    let mut processed_since_publish = 0u64;
    let mut sequence = 0u64;
    let mut pending_command = None;
    loop {
        for command in pending_command.take().into_iter().chain(rx.try_iter()) {
            let save = matches!(&command, Command::Save(_));
            match command {
                Command::Shutdown => {
                    let _ = pointer.reset();
                    status.running = false;
                    status.connected = false;
                    publish(&shared, &status);
                    return;
                }
                Command::Stop => {
                    status.running = false;
                    status.connected = false;
                    status.pressure = 0.0;
                    device = None;
                    let _ = pointer.reset();
                    status.message = "Stopped; all mouse buttons released".into();
                }
                Command::Start => {
                    status.running = true;
                    next_detection = Instant::now();
                    next_tick = Instant::now();
                    status.message = if status.simulation {
                        "Synthetic input; OS pointer output disabled"
                    } else {
                        "Searching for S640; close other tablet drivers if opening fails"
                    }
                    .into();
                }
                Command::Detect => {
                    status.message = match otd_platform::detect_s640() {
                        Ok(devices) if devices.is_empty() => {
                            "No supported S640 HID collection detected".into()
                        }
                        Ok(devices) => format!(
                            "Detected {} supported HID collection(s): {:?}",
                            devices.len(),
                            devices[0].variant
                        ),
                        Err(e) => format!("Detection failed: {e}"),
                    };
                }
                Command::Apply(candidate) | Command::Save(candidate) => {
                    // Validate and persist before changing the live mapper. No recursive recovery.
                    match make_mapper(candidate.clone()) {
                        Err(e) => status.message = format!("Settings rejected: {e}"),
                        Ok(next) => {
                            let result = if save {
                                crate::persistence::save(&path, &candidate)
                            } else {
                                Ok(())
                            };
                            match result {
                                Err(e) => {
                                    status.message =
                                        format!("Save failed; previous settings remain active: {e}")
                                }
                                Ok(()) => {
                                    let _ = pointer.reset();
                                    pointer.refresh_desktop();
                                    mapper = next;
                                    status.settings = candidate;
                                    status.settings_revision += 1;
                                    status.message = if save {
                                        "Applied and saved successfully"
                                    } else {
                                        "Applied; not saved for next startup"
                                    }
                                    .into();
                                }
                            }
                        }
                    }
                }
            }
            publish(&shared, &status);
        }
        if !status.running {
            status.reports_per_second = 0.0;
            publish(&shared, &status);
            match rx.recv_timeout(Duration::from_secs(1)) {
                Ok(command) => pending_command = Some(command),
                Err(mpsc::RecvTimeoutError::Disconnected) => return,
                Err(mpsc::RecvTimeoutError::Timeout) => {}
            }
            continue;
        }
        if !status.simulation && device.is_none() {
            if Instant::now() >= next_detection {
                let found = otd_platform::detect_s640();
                match found {
                    Ok(devices) if devices.is_empty() => {
                        status.message =
                            "Waiting for VEIKK S640; reconnect USB or press Detect".into()
                    }
                    Ok(devices) => {
                        for info in devices {
                            match TabletDevice::open(&info) {
                                Ok(opened) => {
                                    status.tablet = format!("VEIKK {:?}", info.variant);
                                    device = Some(opened);
                                    status.connected = true;
                                    status.message = "Connected: absolute mouse output (drawing pressure injection is not implemented)".into();
                                    break;
                                }
                                Err(e) => status.message = format!("Cannot initialize S640: {e}"),
                            }
                        }
                    }
                    Err(e) => status.message = format!("HID detection failed: {e}"),
                }
                next_detection = Instant::now() + Duration::from_secs(1);
                publish(&shared, &status);
            }
            if device.is_none() {
                match rx.recv_timeout(next_detection.saturating_duration_since(Instant::now())) {
                    Ok(command) => pending_command = Some(command),
                    Err(mpsc::RecvTimeoutError::Disconnected) => return,
                    Err(mpsc::RecvTimeoutError::Timeout) => {}
                }
                continue;
            }
        }
        let (variant, len) = if status.simulation {
            if next_tick > Instant::now() {
                thread::sleep(next_tick - Instant::now());
            }
            next_tick += Duration::from_millis(1);
            if next_tick < Instant::now() {
                next_tick = Instant::now();
            }
            sequence += 1;
            packet[..9].copy_from_slice(&synthetic_packet(sequence));
            (Variant::S640, 9)
        } else {
            let Some(opened) = device.as_mut() else {
                continue;
            };
            match opened.read_timeout(&mut packet, 100) {
                Ok(0) => {
                    if last_publish.elapsed() >= Duration::from_millis(250) {
                        status.reports_per_second = 0.0;
                        publish(&shared, &status);
                    }
                    continue;
                }
                Ok(len) => (opened.variant, len),
                Err(e) => {
                    let _ = pointer.reset();
                    device = None;
                    status.connected = false;
                    status.pressure = 0.0;
                    status.message = format!("Tablet disconnected or read failed: {e}");
                    publish(&shared, &status);
                    continue;
                }
            }
        };
        let start = Instant::now();
        status.reports += 1;
        reports_since_publish += 1;
        let mut limited = false;
        let output = match parse(variant, &packet[..len]) {
            Ok(Report::Pen(pen)) => {
                let mapped = mapper.map(pen);
                limited = mapped.is_none();
                mapped
            }
            Ok(Report::OutOfRange) => {
                status.pressure = 0.0;
                let _ = pointer.reset();
                None
            }
            Ok(_) => None,
            Err(_) => {
                status.malformed += 1;
                None
            }
        };
        let elapsed = start.elapsed().as_nanos();
        processing_ns += elapsed;
        processed_since_publish += 1;
        status.max_processing_us = status.max_processing_us.max(elapsed as f64 / 1000.0);
        if let Some(output) = output {
            status.x = output.x;
            status.y = output.y;
            status.pressure = output.pressure;
            if !status.simulation
                && let Err(e) = pointer.emit(output)
            {
                let _ = pointer.reset();
                device = None;
                status.running = false;
                status.connected = false;
                status.message = format!("Pointer output failed; driver stopped: {e}");
                publish(&shared, &status);
            }
        } else if limited {
            // Leaving a limited area must never leave a tip/button held down.
            let _ = pointer.reset();
        }
        if last_publish.elapsed() >= Duration::from_millis(250) {
            status.reports_per_second =
                reports_since_publish as f64 / last_publish.elapsed().as_secs_f64();
            status.processing_us =
                processing_ns as f64 / processed_since_publish.max(1) as f64 / 1000.0;
            publish(&shared, &status);
            reports_since_publish = 0;
            processing_ns = 0;
            processed_since_publish = 0;
            last_publish = Instant::now();
        }
    }
}

pub fn synthetic_packet(sequence: u64) -> [u8; 9] {
    let x = ((sequence * 37) % 30481) as u16;
    let y = ((sequence * 17) % 20321) as u16;
    let pressure = ((sequence * 13) % 8192) as u16;
    [
        8,
        0x41,
        0xa0,
        x as u8,
        (x >> 8) as u8,
        y as u8,
        (y >> 8) as u8,
        pressure as u8,
        (pressure >> 8) as u8,
    ]
}

#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn command_contract_is_versionable_and_rejects_bad_payloads() {
        let command = Command::Apply(Settings::default());
        let json = serde_json::to_string(&command).unwrap();
        assert!(matches!(
            serde_json::from_str::<Command>(&json).unwrap(),
            Command::Apply(_)
        ));
        assert!(serde_json::from_str::<Command>("{\"command\":\"apply\"}").is_err());
    }
    #[test]
    fn simulator_stops_and_invalid_settings_do_not_replace_active_configuration() {
        let path = std::env::temp_dir().join("otd-rust-unused-test-settings.json");
        let engine = Engine::spawn(Settings::default(), path, true, "test".into());
        thread::sleep(Duration::from_millis(280));
        assert!(engine.read().reports > 0);
        let mut bad = Settings::default();
        bad.area.width_mm = 0.0;
        engine.command(Command::Apply(bad)).unwrap();
        engine.command(Command::Stop).unwrap();
        thread::sleep(Duration::from_millis(150));
        let s = engine.read();
        assert!(!s.running);
        assert_eq!(s.settings, Settings::default());
    }
}
