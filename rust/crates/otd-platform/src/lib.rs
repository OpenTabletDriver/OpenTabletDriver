//! Small native boundary. Hardware handles remain owned by the processing thread.
#[cfg(not(windows))]
compile_error!("This migration milestone implements Windows only; platform parity is pending.");

mod windows;
pub use windows::*;
