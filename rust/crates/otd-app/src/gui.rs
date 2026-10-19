#![windows_subsystem = "windows"]

fn main() {
    if let Err(error) = otd_app::run() {
        let title = otd_platform::wide(std::ffi::OsStr::new("OpenTabletDriver Rust"));
        let text = otd_platform::wide(std::ffi::OsStr::new(&error));
        // SAFETY: terminated strings remain live for the modal error dialog.
        unsafe {
            windows_sys::Win32::UI::WindowsAndMessaging::MessageBoxW(
                std::ptr::null_mut(),
                text.as_ptr(),
                title.as_ptr(),
                0x10,
            );
        }
        std::process::exit(1);
    }
}
