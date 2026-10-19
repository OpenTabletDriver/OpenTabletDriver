mod capture;
mod engine;
mod persistence;
mod ui;

use otd_core::{Mapper, Report, Settings, Variant, parser::parse};
use std::{
    alloc::{GlobalAlloc, Layout, System},
    hint::black_box,
    path::PathBuf,
    sync::atomic::{AtomicBool, AtomicU64, Ordering},
    time::{Duration, Instant},
};

struct MeasuredAllocator;
static TRACK_ALLOCATIONS: AtomicBool = AtomicBool::new(false);
static ALLOCATIONS: AtomicU64 = AtomicU64::new(0);
// SAFETY: delegates every allocation/deallocation to System unchanged. Tracking only
// increments an atomic, without allocating or unwinding inside allocator callbacks.
unsafe impl GlobalAlloc for MeasuredAllocator {
    unsafe fn alloc(&self, layout: Layout) -> *mut u8 {
        if TRACK_ALLOCATIONS.load(Ordering::Relaxed) {
            ALLOCATIONS.fetch_add(1, Ordering::Relaxed);
        }
        unsafe { System.alloc(layout) }
    }
    unsafe fn dealloc(&self, ptr: *mut u8, layout: Layout) {
        unsafe { System.dealloc(ptr, layout) }
    }
    unsafe fn realloc(&self, ptr: *mut u8, layout: Layout, size: usize) -> *mut u8 {
        if TRACK_ALLOCATIONS.load(Ordering::Relaxed) {
            ALLOCATIONS.fetch_add(1, Ordering::Relaxed);
        }
        unsafe { System.realloc(ptr, layout, size) }
    }
    unsafe fn alloc_zeroed(&self, layout: Layout) -> *mut u8 {
        if TRACK_ALLOCATIONS.load(Ordering::Relaxed) {
            ALLOCATIONS.fetch_add(1, Ordering::Relaxed);
        }
        unsafe { System.alloc_zeroed(layout) }
    }
}
#[global_allocator]
static ALLOCATOR: MeasuredAllocator = MeasuredAllocator;

#[derive(Default)]
struct Options {
    ui_self_test: bool,
    screenshot: Option<PathBuf>,
    probe: bool,
    headless: bool,
    simulation: bool,
    run: bool,
    hidden: bool,
    devices: bool,
    benchmark: bool,
    seconds: Option<u64>,
    iterations: Option<usize>,
    report: Option<PathBuf>,
    settings: Option<PathBuf>,
}

fn options() -> Result<Option<Options>, String> {
    let mut o = Options::default();
    let mut args = std::env::args().skip(1);
    while let Some(arg) = args.next() {
        match arg.as_str() {
            "--help" | "-h" => {
                println!(
                    "OpenTabletDriver Rust / S640 migration\n\nNo arguments: native control panel, driver stopped until Start.\n--simulate          1000 Hz synthetic input; never injects pointer events\n--headless          No UI (requires --simulate or --run)\n--run               Start physical S640 input and absolute mouse output\n--seconds N         Exit cleanly after N seconds\n--devices           List matching HID collections without initialization\n--probe             Read-only observation; no initialization or pointer output\n--benchmark         Measure parsing/mapping only; no hardware or output\n--iterations N      Benchmark count (default 1000000)\n--report PATH       Write machine-readable results\n--settings PATH     Use a separate Rust settings file\n--hidden            Render UI offscreen without taking focus\n--ui-self-test      Exercise UI controls (requires --simulate and --settings)\n--snapshot PATH     Render this app's window to a BMP file"
                );
                return Ok(None);
            }
            "--headless" => o.headless = true,
            "--simulate" => o.simulation = true,
            "--run" => o.run = true,
            "--hidden" => o.hidden = true,
            "--devices" => o.devices = true,
            "--probe" => o.probe = true,
            "--ui-self-test" => o.ui_self_test = true,
            "--snapshot" => {
                o.screenshot = Some(args.next().ok_or("Missing screenshot path")?.into())
            }
            "--benchmark" => o.benchmark = true,
            "--seconds" => {
                o.seconds = Some(
                    args.next()
                        .ok_or("Missing seconds")?
                        .parse()
                        .map_err(|_| "Invalid seconds")?,
                )
            }
            "--iterations" => {
                o.iterations = Some(
                    args.next()
                        .ok_or("Missing iteration count")?
                        .parse()
                        .map_err(|_| "Invalid iteration count")?,
                )
            }
            "--report" => o.report = Some(args.next().ok_or("Missing report path")?.into()),
            "--settings" => o.settings = Some(args.next().ok_or("Missing settings path")?.into()),
            _ => return Err(format!("Unknown argument: {arg}; use --help")),
        }
    }
    if o.ui_self_test && (!o.simulation || o.settings.is_none() || o.headless) {
        return Err("UI self-test requires --simulate, --settings PATH, and the UI".into());
    }
    if o.simulation && o.run {
        return Err("Choose --simulate or --run, not both".into());
    }
    if o.hidden && o.run {
        return Err("Offscreen UI tests cannot enable physical pointer output".into());
    }
    Ok(Some(o))
}

fn write_result(value: &serde_json::Value, path: Option<&PathBuf>) -> Result<(), String> {
    let json = serde_json::to_string_pretty(value).map_err(|e| e.to_string())?;
    if let Some(path) = path {
        std::fs::write(path, &json).map_err(|e| e.to_string())?;
    }
    println!("{json}");
    Ok(())
}

pub fn run() -> Result<(), String> {
    let Some(o) = options()? else {
        return Ok(());
    };
    otd_platform::initialize_dpi_awareness();
    if o.probe {
        return probe(o.seconds.unwrap_or(15), o.report.as_ref());
    }
    if o.benchmark {
        return benchmark(o.iterations.unwrap_or(1_000_000), o.report.as_ref());
    }
    if o.devices {
        let devices = otd_platform::detect_s640().map_err(|e| e.to_string())?;
        let list: Vec<_> = devices.iter().map(|d| serde_json::json!({"variant":d.variant,"input_length":d.input_len,"output_length":d.output_len})).collect();
        return write_result(&serde_json::json!({"devices": list}), o.report.as_ref());
    }
    if o.headless && !o.simulation && !o.run {
        return Err("Headless mode requires --simulate or --run".into());
    }
    let _instance = otd_platform::single_instance().map_err(|e| e.to_string())?;
    let path = o.settings.unwrap_or_else(persistence::settings_path);
    let (settings, message) = if path.exists() {
        match persistence::load(&path) {
            Ok(settings) => (
                settings,
                "Rust settings loaded; click Start to enable physical output".into(),
            ),
            Err(e) => (
                Settings {
                    display: otd_platform::primary_display(),
                    ..Settings::default()
                },
                format!("Settings not loaded; file preserved: {e}"),
            ),
        }
    } else {
        (
            Settings {
                display: otd_platform::primary_display(),
                ..Settings::default()
            },
            "Ready. Detect is read-only; Start initializes the tablet and enables mouse output."
                .into(),
        )
    };
    let engine = engine::Engine::spawn(settings, path, o.simulation, message);
    if o.run {
        engine.command(engine::Command::Start)?;
    }
    let snapshot = engine.snapshot.clone();
    if o.headless {
        otd_platform::install_console_stop_handler().map_err(|e| e.to_string())?;
        let start = Instant::now();
        while !otd_platform::console_stop_requested()
            && !o
                .seconds
                .is_some_and(|seconds| start.elapsed() >= Duration::from_secs(seconds))
        {
            std::thread::sleep(Duration::from_millis(50));
        }
        drop(engine);
    } else {
        ui::run(
            engine,
            o.seconds.map(Duration::from_secs),
            o.hidden,
            o.ui_self_test,
            o.screenshot,
        )?;
    }
    let final_state = snapshot.lock().unwrap_or_else(|e| e.into_inner()).clone();
    let memory = otd_platform::memory_usage().map_err(|e| e.to_string())?;
    write_result(
        &serde_json::json!({
            "snapshot": final_state,
            "memory": {"working_set_bytes":memory.working_set_bytes,"private_bytes":memory.private_bytes,"peak_working_set_bytes":memory.peak_working_set_bytes},
            "measurement_scope":"One process: Rust engine + native UI when enabled. Synthetic input does not exercise USB or SendInput."
        }),
        o.report.as_ref(),
    )
}

fn probe(seconds: u64, path: Option<&PathBuf>) -> Result<(), String> {
    use serde_json::json;
    let devices = otd_platform::detect_s640().map_err(|e| e.to_string())?;
    let info = devices.first().ok_or("No S640 detected")?;
    let mut device = otd_platform::TabletDevice::observe(info).map_err(|e| e.to_string())?;
    let start = Instant::now();
    let mut buffer = [0; 64];
    let mut received = 0u64;
    let mut pens = 0u64;
    let mut errors = 0u64;
    let mut samples = Vec::with_capacity(64);
    while start.elapsed() < Duration::from_secs(seconds.min(120)) {
        let len = device
            .read_timeout(&mut buffer, 100)
            .map_err(|e| e.to_string())?;
        if len == 0 {
            continue;
        }
        received += 1;
        match parse(device.variant, &buffer[..len]) {
            Ok(report) => {
                if matches!(report, Report::Pen(_)) {
                    pens += 1;
                }
                if samples.len() < 64 {
                    samples.push(json!({"packet":buffer[..len],"decoded":report}));
                }
            }
            Err(_) => errors += 1,
        }
    }
    let memory = otd_platform::memory_usage().map_err(|e| e.to_string())?;
    write_result(
        &json!({"scope":"Read-only S640 observation. No initialization reports or OS input injection.","variant":device.variant,"seconds":start.elapsed().as_secs_f64(),"received":received,"pen_reports":pens,"malformed":errors,"samples":samples,"working_set_bytes":memory.working_set_bytes,"private_bytes":memory.private_bytes}),
        path,
    )
}

fn benchmark(iterations: usize, path: Option<&PathBuf>) -> Result<(), String> {
    if !(1_000..=10_000_000).contains(&iterations) {
        return Err("Iterations must be between 1000 and 10000000".into());
    }
    let config = Variant::S640.configuration().map_err(|e| e.to_string())?;
    let mapper = Mapper::new(
        Settings::default(),
        config.specifications.digitizer,
        config.specifications.pen.max_pressure,
    )
    .map_err(str::to_owned)?;
    let mut samples = Vec::with_capacity(iterations.min(100_000));
    for i in 0..10_000 {
        let p = engine::synthetic_packet(i);
        black_box(parse(Variant::S640, black_box(&p))).ok();
    }
    ALLOCATIONS.store(0, Ordering::Relaxed);
    TRACK_ALLOCATIONS.store(true, Ordering::Relaxed);
    let throughput_start = Instant::now();
    for i in 0..iterations {
        let packet = black_box(engine::synthetic_packet(i as u64));
        if let Ok(Report::Pen(pen)) = parse(Variant::S640, black_box(&packet)) {
            black_box(mapper.map(black_box(pen)));
        }
    }
    let throughput_elapsed = throughput_start.elapsed();
    let allocations = ALLOCATIONS.load(Ordering::Relaxed);
    TRACK_ALLOCATIONS.store(false, Ordering::Relaxed);
    for i in 0..iterations.min(100_000) {
        let packet = black_box(engine::synthetic_packet(i as u64));
        let start = Instant::now();
        if let Ok(Report::Pen(pen)) = parse(Variant::S640, black_box(&packet)) {
            black_box(mapper.map(black_box(pen)));
        }
        samples.push(start.elapsed().as_nanos() as u64);
    }
    samples.sort_unstable();
    let percentile = |p: usize| samples[(samples.len() - 1) * p / 100];
    let memory = otd_platform::memory_usage().map_err(|e| e.to_string())?;
    let value = serde_json::json!({
        "iterations":iterations,"elapsed_ms":throughput_elapsed.as_secs_f64()*1000.0,
        "reports_per_second":iterations as f64/throughput_elapsed.as_secs_f64(),
        "mean_ns":throughput_elapsed.as_nanos() as f64/iterations as f64,
        "p50_ns":percentile(50),"p95_ns":percentile(95),"p99_ns":percentile(99),
        "hot_path_allocations":allocations,
        "memory":{"working_set_bytes":memory.working_set_bytes,"private_bytes":memory.private_bytes},
        "scope":"Synthetic S640 parse + affine map + pressure/button mapping. Excludes USB, OS injection, scheduling, UI, and display latency. Timing samples include clock overhead."
    });
    write_result(&value, path)?;
    if allocations != 0 {
        return Err("Report hot path allocated memory".into());
    }
    Ok(())
}
