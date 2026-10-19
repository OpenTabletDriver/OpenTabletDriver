fn main() {
    if let Err(error) = otd_app::run() {
        eprintln!("OpenTabletDriver Rust: {error}");
        std::process::exit(1);
    }
}
